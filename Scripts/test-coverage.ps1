param(
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot/..

Write-Host "=== Gredja Test Coverage ===" -ForegroundColor Cyan

# Clean old results
Write-Host "`n[1/4] Cleaning old results..." -ForegroundColor Yellow
if (Test-Path "TestResults") {
    cmd /c "rmdir /s /q TestResults" 2>$null
}

# Run tests with coverlet
if (-not $SkipTests) {
    Write-Host "[2/4] Running tests with coverage collection..." -ForegroundColor Yellow
    dotnet test --verbosity minimal --collect:"XPlat Code Coverage" --results-directory "TestResults"
} else {
    Write-Host "[2/4] Skipping tests (using existing results)..." -ForegroundColor Yellow
}

# Generate HTML report
Write-Host "[3/4] Generating coverage report..." -ForegroundColor Yellow
$reportPath = "TestResults/Report"
$coverageFiles = Get-ChildItem -Path "TestResults" -Recurse -Filter "coverage.cobertura.xml"

if ($coverageFiles.Count -eq 0) {
    Write-Host "No coverage files found. Run tests first." -ForegroundColor Red
    exit 1
}

# Check if reportgenerator is installed
$rgExists = Get-Command reportgenerator -ErrorAction SilentlyContinue
if (-not $rgExists) {
    Write-Host "Installing dotnet-reportgenerator-globaltool..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
    $env:PATH = "$env:USERPROFILE\.dotnet\tools;$env:PATH"
}

$reports = ($coverageFiles | ForEach-Object { $_.FullName }) -join ";"
reportgenerator -reports:$reports -targetdir:$reportPath -reporttypes:Html | Out-Null

Write-Host "[4/4] Coverage summary:" -ForegroundColor Yellow
Write-Host ""

# Parse coverage XML for summary
foreach ($file in $coverageFiles) {
    [xml]$xml = Get-Content $file.FullName
    $lineRate = [math]::Round([double]$xml.coverage.'line-rate' * 100, 1)
    $branchRate = [math]::Round([double]$xml.coverage.'branch-rate' * 100, 1)
    $linesCovered = $xml.coverage.'lines-covered'
    $linesValid = $xml.coverage.'lines-valid'
    $branchesCovered = $xml.coverage.'branches-covered'
    $branchesValid = $xml.coverage.'branches-valid'

    Write-Host "Lines:   $linesCovered / $linesValid ($lineRate%)" -ForegroundColor $(if ($lineRate -ge 70) { "Green" } elseif ($lineRate -ge 50) { "Yellow" } else { "Red" })
    Write-Host "Branches: $branchesCovered / $branchesValid ($branchRate%)" -ForegroundColor $(if ($branchRate -ge 70) { "Green" } elseif ($branchRate -ge 50) { "Yellow" } else { "Red" })
}

Write-Host ""
Write-Host "HTML report: $reportPath/index.html" -ForegroundColor Cyan
Write-Host ""

# File coverage (endpoint → test mapping)
Write-Host "=== File Coverage (endpoint → test) ===" -ForegroundColor Cyan
Write-Host ""

$services = @("FakeStore", "JsonPlaceholder", "GitHub")

foreach ($service in $services) {
    $endpointsFile = "Core/Config/${service}Endpoints.cs"
    if (-not (Test-Path $endpointsFile)) { continue }

    $content = Get-Content $endpointsFile -Raw
    $endpoints = ([regex]::Matches($content, 'public const string (\w+) = "(/[^"]+)"')).Count

    $testDir = "Api/$service/Tests"
    $testFiles = 0
    if (Test-Path $testDir) {
        $testFiles = (Get-ChildItem -Path $testDir -Recurse -Filter "*Tests.cs").Count
    }

    $coverage = if ($endpoints -gt 0) { [math]::Round($testFiles / $endpoints * 100) } else { 0 }

    Write-Host ("{0,-15} Endpoints: {1,-3} Tested: {2,-3} Coverage: {3}%" -f $service, $endpoints, $testFiles, $coverage)
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Green

# Open HTML report in browser
$htmlReport = Join-Path $reportPath "index.html"
if (Test-Path $htmlReport) {
    Start-Process $htmlReport
}
