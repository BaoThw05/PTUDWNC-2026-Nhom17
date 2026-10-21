param(
    [string]$EnvFile = (Join-Path $PSScriptRoot '../.env'),
    [string]$ComposeFile = (Join-Path $PSScriptRoot '../docker-compose.yml'),
    [string]$ProjectName = 'culinaryblog',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot ('../backups/' + (Get-Date -Format 'yyyyMMdd-HHmmss')))
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$envPath = (Resolve-Path $EnvFile).Path
$composePath = (Resolve-Path $ComposeFile).Path
$output = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw "Thư mục backup đã tồn tại: $output" }
New-Item -ItemType Directory -Path $output | Out-Null

function Compose([string[]]$Arguments) {
    & docker compose --project-directory $repo --project-name $ProjectName --env-file $envPath -f $composePath @Arguments
    if ($LASTEXITCODE -ne 0) { throw "docker compose $($Arguments[0]) thất bại" }
}

function InvokeDockerCli([string[]]$Arguments) {
    & docker @Arguments
    if ($LASTEXITCODE -ne 0) { throw "docker $($Arguments[0]) thất bại" }
}

$started = @()
$dumpInContainer = '/tmp/culinary-backup-' + [guid]::NewGuid().ToString('N') + '.dump'
try {
    foreach ($service in @('web', 'api', 'minio')) {
        $id = (& docker compose --project-directory $repo --project-name $ProjectName --env-file $envPath -f $composePath ps --status running -q $service)
        if ($LASTEXITCODE -ne 0) { throw "Không kiểm tra được $service" }
        if ($id) { $started += $service }
    }
    $postgres = (& docker compose --project-directory $repo --project-name $ProjectName --env-file $envPath -f $composePath ps --status running -q postgres)
    if ($LASTEXITCODE -ne 0 -or -not $postgres) { throw 'PostgreSQL chưa chạy' }
    $minio = (& docker compose --project-directory $repo --project-name $ProjectName --env-file $envPath -f $composePath ps -q minio)
    if ($LASTEXITCODE -ne 0 -or -not $minio) { throw 'Container MinIO chưa được tạo' }

    # Dừng ghi từ ứng dụng trước khi chụp cả database và object storage.
    if ($started.Count) { Compose (@('stop') + $started) | Out-Null }
    $mounts = (& docker inspect --format '{{json .Mounts}}' $minio | ConvertFrom-Json)
    if ($LASTEXITCODE -ne 0) { throw 'Không kiểm tra được volume MinIO' }
    $volume = ($mounts | Where-Object { $_.Destination -eq '/data' } | Select-Object -First 1).Name
    if (-not $volume) { throw 'Không tìm thấy volume miniodata trong Compose' }

    Compose @('exec', '-T', 'postgres', 'sh', '-c', ('pg_dump -Fc -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f ' + $dumpInContainer)) | Out-Null
    InvokeDockerCli @('cp', "${postgres}:${dumpInContainer}", (Join-Path $output 'postgres.dump')) | Out-Null
    InvokeDockerCli @('run', '--rm', '--mount', "type=volume,source=$volume,target=/data,readonly", '--mount', "type=bind,source=$output,target=/backup", 'alpine:3.20.7', 'tar', '-czf', '/backup/minio.tar.gz', '-C', '/data', '.') | Out-Null

    $files = @('postgres.dump', 'minio.tar.gz')
    foreach ($file in $files) {
        if ((Get-Item -LiteralPath (Join-Path $output $file)).Length -eq 0) { throw "Backup rỗng: $file" }
    }
    $manifest = [ordered]@{
        format = 1
        createdUtc = (Get-Date).ToUniversalTime().ToString('o')
        project = $ProjectName
        files = [ordered]@{}
    }
    foreach ($file in $files) {
        $manifest.files[$file] = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $output $file)).Hash.ToLowerInvariant()
    }
    $manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $output 'manifest.json') -Encoding UTF8
    Write-Host "Đã tạo backup: $output"
}
finally {
    if ($postgres) { & docker exec $postgres rm -f $dumpInContainer 2>$null | Out-Null }
    if ($started.Count) {
        # MinIO cần sẵn sàng trước khi API nhận request trở lại.
        foreach ($service in @('minio', 'api', 'web')) {
            if ($started -contains $service) { Compose @('start', $service) | Out-Null }
        }
    }
}
