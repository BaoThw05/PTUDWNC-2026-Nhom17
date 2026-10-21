param(
    [Parameter(Mandatory = $true)][string]$BackupDirectory,
    [string]$EnvFile = (Join-Path $PSScriptRoot '../.env'),
    [string]$ComposeFile = (Join-Path $PSScriptRoot '../docker-compose.yml'),
    [string]$ProjectName = 'culinaryblog',
    [switch]$ReplaceExisting,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
if (-not $Force) { throw 'Khôi phục sẽ thay thế database. Chạy lại với -Force sau khi kiểm tra đúng project và backup.' }
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$envPath = (Resolve-Path $EnvFile).Path
$composePath = (Resolve-Path $ComposeFile).Path
$backup = (Resolve-Path $BackupDirectory).Path
$manifest = Get-Content -LiteralPath (Join-Path $backup 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ($manifest.format -ne 1) { throw 'Định dạng manifest không được hỗ trợ' }
foreach ($file in @('postgres.dump', 'minio.tar.gz')) {
    $actual = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $backup $file)).Hash.ToLowerInvariant()
    if ($actual -ne $manifest.files.$file) { throw "Checksum không khớp: $file" }
}

function Compose([string[]]$Arguments) {
    & docker compose --project-directory $repo --project-name $ProjectName --env-file $envPath -f $composePath @Arguments
    if ($LASTEXITCODE -ne 0) { throw "docker compose $($Arguments[0]) thất bại" }
}

function InvokeDockerCli([string[]]$Arguments) {
    & docker @Arguments
    if ($LASTEXITCODE -ne 0) { throw "docker $($Arguments[0]) thất bại" }
}

$dumpInContainer = '/tmp/culinary-restore-' + [guid]::NewGuid().ToString('N') + '.dump'
try {
    Compose @('up', '-d', '--wait', 'postgres') | Out-Null
    Compose @('create', 'minio') | Out-Null
    $minio = (& docker compose --project-directory $repo --project-name $ProjectName --env-file $envPath -f $composePath ps -a -q minio)
    if ($LASTEXITCODE -ne 0 -or -not $minio) { throw 'Container MinIO chưa được tạo' }
    $mounts = (& docker inspect --format '{{json .Mounts}}' $minio | ConvertFrom-Json)
    if ($LASTEXITCODE -ne 0) { throw 'Không kiểm tra được volume MinIO' }
    $volume = ($mounts | Where-Object { $_.Destination -eq '/data' } | Select-Object -First 1).Name
    if (-not $volume) { throw 'Không tìm thấy volume miniodata trong Compose' }

    $labels = (& docker volume inspect --format '{{json .Labels}}' $volume | ConvertFrom-Json)
    if ($LASTEXITCODE -ne 0 -or $labels.'com.docker.compose.project' -ne $ProjectName -or $labels.'com.docker.compose.volume' -ne 'miniodata') {
        throw "Volume $volume không thuộc project $ProjectName/miniodata"
    }
    InvokeDockerCli @('run', '--rm', '--mount', "type=bind,source=$backup,target=/backup,readonly", 'alpine:3.20.7', 'tar', '-tzf', '/backup/minio.tar.gz') | Out-Null

    # Mặc định chỉ khôi phục ảnh vào volume trống để không lẫn file cũ.
    $entries = InvokeDockerCli @('run', '--rm', '--mount', "type=volume,source=$volume,target=/data,readonly", 'alpine:3.20.7', 'sh', '-c', 'find /data -mindepth 1 -print -quit')
    if ($entries -and -not $ReplaceExisting) { throw "Volume $volume không trống. Dùng -ReplaceExisting -Force nếu cần thay thế dữ liệu ảnh hiện có." }

    $postgres = (& docker compose --project-directory $repo --project-name $ProjectName --env-file $envPath -f $composePath ps --status running -q postgres)
    if ($LASTEXITCODE -ne 0 -or -not $postgres) { throw 'PostgreSQL chưa chạy' }
    InvokeDockerCli @('cp', (Join-Path $backup 'postgres.dump'), "${postgres}:${dumpInContainer}") | Out-Null
    Compose @('exec', '-T', 'postgres', 'sh', '-c', ('pg_restore --list ' + $dumpInContainer + ' >/dev/null')) | Out-Null

    $services = @(Compose @('config', '--services'))
    $appServices = @('web', 'api', 'minio') | Where-Object { $services -contains $_ }
    if ($appServices.Count) { Compose (@('stop') + $appServices) | Out-Null }
    if ($entries) {
        InvokeDockerCli @('run', '--rm', '--mount', "type=volume,source=$volume,target=/data", 'alpine:3.20.7', 'sh', '-c', 'find /data -mindepth 1 -maxdepth 1 -exec rm -rf -- {} +') | Out-Null
    }
    Compose @('exec', '-T', 'postgres', 'sh', '-c', ('pg_restore --clean --if-exists --no-owner --no-acl -U "$POSTGRES_USER" -d "$POSTGRES_DB" ' + $dumpInContainer)) | Out-Null
    InvokeDockerCli @('run', '--rm', '--mount', "type=volume,source=$volume,target=/data", '--mount', "type=bind,source=$backup,target=/backup,readonly", 'alpine:3.20.7', 'tar', '-xzf', '/backup/minio.tar.gz', '-C', '/data') | Out-Null
    Write-Host "Đã khôi phục database và MinIO cho project $ProjectName. Chạy docker compose up -d để mở lại ứng dụng."
}
finally {
    if ($postgres) { & docker exec $postgres rm -f $dumpInContainer 2>$null | Out-Null }
}
