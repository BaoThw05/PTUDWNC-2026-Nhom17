param(
    [string]$InputPath = "docs/data/recipe-image-prompts.jsonl",
    [string]$OutputDirectory = "src/Backend/CulinaryBlog.Infrastructure/RecipeSearch/seed-images"
)

$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($env:CLOUDFLARE_ACCOUNT_ID) -or [string]::IsNullOrWhiteSpace($env:CLOUDFLARE_API_TOKEN)) {
    throw "Set CLOUDFLARE_ACCOUNT_ID and CLOUDFLARE_API_TOKEN in the environment first."
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$inputFile = Join-Path $repoRoot $InputPath
$outputPath = Join-Path $repoRoot $OutputDirectory
if (-not (Test-Path -LiteralPath $inputFile)) { throw "Prompt file not found: $inputFile" }
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

$jobs = @(Get-Content -LiteralPath $inputFile -Encoding utf8 | Where-Object { $_.Trim() } | ForEach-Object { $_ | ConvertFrom-Json })
if ($jobs.Count -ne 50) { throw "Expected 50 prompts, found $($jobs.Count)." }
$endpoint = "https://api.cloudflare.com/client/v4/accounts/$env:CLOUDFLARE_ACCOUNT_ID/ai/run/@cf/black-forest-labs/flux-1-schnell"

for ($index = 0; $index -lt $jobs.Count; $index++) {
    $job = $jobs[$index]
    $fileName = [IO.Path]::ChangeExtension([IO.Path]::GetFileName($job.out), ".jpg")
    $filePath = Join-Path $outputPath $fileName
    if (Test-Path -LiteralPath $filePath) {
        Write-Output ("[{0}/50] exists, skipping {1}" -f ($index + 1), $fileName)
        continue
    }

    $body = @{ prompt = [string]$job.prompt } | ConvertTo-Json -Compress
    $created = $false
    for ($attempt = 1; $attempt -le 3 -and -not $created; $attempt++) {
        try {
            $response = Invoke-RestMethod -Uri $endpoint -Method Post `
                -Headers @{ Authorization = "Bearer $env:CLOUDFLARE_API_TOKEN" } `
                -ContentType "application/json" -Body $body -TimeoutSec 180
            $encodedImage = [string]$response.result.image
            if ([string]::IsNullOrWhiteSpace($encodedImage)) { throw "Cloudflare response did not contain result.image." }
            $imageBytes = [Convert]::FromBase64String($encodedImage)
            [IO.File]::WriteAllBytes($filePath, $imageBytes)
            $created = $true
            Write-Output ("[{0}/50] saved {1} ({2:N0} bytes)" -f ($index + 1), $fileName, $imageBytes.Length)
        }
        catch {
            $statusCode = 0
            if ($_.Exception.Response) { $statusCode = [int]$_.Exception.Response.StatusCode }
            if ($statusCode -ge 400 -and $statusCode -lt 500) {
                throw "Cloudflare rejected image $($index + 1)/50 with HTTP $statusCode. Fix the account/token/quota before resuming."
            }
            if ($attempt -eq 3) { throw "Image $($index + 1)/50 failed after 3 attempts: $($_.Exception.Message)" }
            Start-Sleep -Seconds ([Math]::Pow(2, $attempt))
        }
    }
    Start-Sleep -Milliseconds 300
}

Write-Output "All prompts processed. Images are in $outputPath"
