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

# Step 1: Clean previous results
if (Test-Path $ResultsDir) {
    Write-Host "[1/4] Cleaning previous results..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $ResultsDir
}

# Step 2: Run tests
if (-not $SkipTests) {
    Write-Host "[2/4] Running tests..." -ForegroundColor Yellow
    dotnet test $ProjectRoot --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Tests had failures, but continuing to generate report..." -ForegroundColor DarkYellow
    }
} else {
    Write-Host "[2/4] Skipping tests (used -SkipTests)" -ForegroundColor DarkGray
}

# Step 3: Generate report
Write-Host "[3/4] Generating Allure report..." -ForegroundColor Yellow
if (-not (Test-Path $ResultsDir) -or (Get-ChildItem $ResultsDir).Count -eq 0) {
    Write-Host "No allure-results found. Make sure AllureNUnit attribute is applied to test classes." -ForegroundColor Red
    exit 1
}
allure generate $ResultsDir -o $ReportDir --clean

# Step 4: Serve report and open in browser
Write-Host "[4/4] Starting Allure server on port 9090..." -ForegroundColor Yellow
Stop-Process -Name "java" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
Start-Process allure -ArgumentList "open",$ReportDir,"--port","9090" -WindowStyle Hidden
Start-Sleep -Seconds 3
Start-Process "http://localhost:9090"
Write-Host "Report opened at http://localhost:9090" -ForegroundColor Green

Write-Host "=== Done ===" -ForegroundColor Cyan
