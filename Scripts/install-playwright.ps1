# Builds the Ui project and installs the Playwright Chromium browser (driver + binary).
# Run once per machine: ./Scripts/install-playwright.ps1
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    dotnet build Ui/Ui.csproj --nologo
    $playwrightScript = 'Ui/bin/Debug/net10.0/playwright.ps1'
    if (-not (Test-Path $playwrightScript)) {
        throw "playwright.ps1 not found at $playwrightScript — Microsoft.Playwright build targets did not emit it"
    }
    & $playwrightScript install chromium
    Write-Host "Playwright Chromium installed."
} finally {
    Pop-Location
}
