# Scripts/allure-report.ps1
# Run tests and generate Allure report

param(
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path $PSScriptRoot -Parent
$ResultsDir = Join-Path $ProjectRoot "allure-results"
$ReportDir = Join-Path $ProjectRoot "allure-report"

# Refresh PATH to find allure
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

Write-Host "=== Allure Report Generator ===" -ForegroundColor Cyan

# Step 1: Run tests
if (-not $SkipTests) {
    Write-Host "[1/3] Running tests..." -ForegroundColor Yellow
    if (Test-Path $ResultsDir) {
        Remove-Item -Recurse -Force $ResultsDir
    }
    dotnet test "$ProjectRoot/Api/Api.csproj" --settings "$ProjectRoot/Api/.runsettings" --verbosity minimal
    dotnet test "$ProjectRoot/Ui/Ui.csproj" --settings "$ProjectRoot/Ui/.runsettings" --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Tests had failures, but continuing to generate report..." -ForegroundColor DarkYellow
    }
} else {
    Write-Host "[1/3] Skipping tests (used -SkipTests)" -ForegroundColor DarkGray
}

# Step 1b: Copy categories
$CategoriesSource = Join-Path $PSScriptRoot "allure-categories.json"
if (Test-Path $CategoriesSource) {
    Copy-Item $CategoriesSource -Destination (Join-Path $ResultsDir "categories.json") -Force
}

# Step 2: Generate report
Write-Host "[2/3] Generating Allure report..." -ForegroundColor Yellow
if (-not (Test-Path $ResultsDir) -or (Get-ChildItem $ResultsDir).Count -eq 0) {
    Write-Host "No allure-results found. Make sure AllureNUnit attribute is applied to test classes." -ForegroundColor Red
    exit 1
}
allure generate $ResultsDir -o $ReportDir --clean

# Step 2b: Override behaviors.json with NUnit categories
$BehaviorsScript = Join-Path $PSScriptRoot "generate-behaviors.ps1"
if (Test-Path $BehaviorsScript) {
    & $BehaviorsScript -ResultsDir $ResultsDir -ReportDataDir (Join-Path $ReportDir "data")
}

# Step 3: Serve report and open in browser
Write-Host "[3/3] Starting Allure server on port 9090..." -ForegroundColor Yellow
Stop-Process -Name "java" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
Start-Process allure -ArgumentList "open",$ReportDir,"--port","9090" -WindowStyle Hidden
Start-Sleep -Seconds 3
Write-Host "Report opened at http://localhost:9090" -ForegroundColor Green

Write-Host "=== Done ===" -ForegroundColor Cyan
