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

# Per-service code coverage (lines covered by namespace)
Write-Host ""
Write-Host "=== Per-Service Code Coverage ===" -ForegroundColor Cyan
Write-Host ""

$serviceNamespaces = @{
    "FakeStore"      = @("Core/Models/FakeStore", "Api/FakeStore")
    "JsonPlaceholder" = @("Core/Models/JsonPlaceholder", "Api/JsonPlaceholder")
    "GitHub"         = @("Core/Models/GitHub", "Api/GitHub")
}

$serviceResults = @{}

foreach ($file in $coverageFiles) {
    [xml]$xml = Get-Content $file.FullName

    foreach ($package in $xml.coverage.packages.package) {
        foreach ($class in $package.classes.class) {
            $filename = $class.filename

            foreach ($service in $serviceNamespaces.Keys) {
                $matched = $false
                foreach ($ns in $serviceNamespaces[$service]) {
                    if ($filename -like "$ns*") {
                        $matched = $true
                        break
                    }
                }

                if ($matched) {
                    if (-not $serviceResults.ContainsKey($service)) {
                        $serviceResults[$service] = @{ Covered = 0; Total = 0 }
                    }
                    $totalLines = $class.lines.line.Count
                    $coveredLines = ($class.lines.line | Where-Object { [int]$_.hits -gt 0 }).Count
                    $serviceResults[$service].Covered += $coveredLines
                    $serviceResults[$service].Total += $totalLines
                }
            }
        }
    }
}

foreach ($service in @("FakeStore", "JsonPlaceholder", "GitHub")) {
    if ($serviceResults.ContainsKey($service)) {
        $covered = $serviceResults[$service].Covered
        $total = $serviceResults[$service].Total
        $pct = if ($total -gt 0) { [math]::Round($covered / $total * 100, 1) } else { 0 }
        $color = if ($pct -ge 70) { "Green" } elseif ($pct -ge 50) { "Yellow" } else { "Red" }
        Write-Host ("{0,-15} Lines: {1,-5} / {2,-5} ({3}%)" -f $service, $covered, $total, $pct) -ForegroundColor $color
    } else {
        Write-Host ("{0,-15} Lines: 0 / 0 (0%)" -f $service) -ForegroundColor Red
    }
}

# Generate per-service HTML report
$serviceHtmlPath = "$reportPath/service-coverage.html"
$html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Gredja — Per-Service Coverage</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 40px; background: #f5f5f5; }
        h1 { color: #333; }
        table { border-collapse: collapse; width: 100%; max-width: 800px; background: white; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        th, td { padding: 12px 16px; text-align: left; border-bottom: 1px solid #eee; }
        th { background: #2c3e50; color: white; }
        tr:hover { background: #f9f9f9; }
        .high { color: #27ae60; font-weight: bold; }
        .mid { color: #f39c12; font-weight: bold; }
        .low { color: #e74c3c; font-weight: bold; }
        .section { margin-top: 30px; }
        .section h2 { color: #555; border-bottom: 2px solid #3498db; padding-bottom: 5px; }
    </style>
</head>
<body>
    <h1>Gredja — Per-Service Coverage Report</h1>
    <p>Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")</p>

    <div class="section">
        <h2>Endpoint Coverage (tests per API endpoint)</h2>
        <table>
            <tr><th>Service</th><th>Endpoints</th><th>Tested</th><th>Coverage</th></tr>
"@

foreach ($service in @("FakeStore", "JsonPlaceholder", "GitHub")) {
    $endpointsFile = "Core/Config/${service}Endpoints.cs"
    if (-not (Test-Path $endpointsFile)) { continue }
    $content = Get-Content $endpointsFile -Raw
    $pattern = 'public const string (\w+) = "(/[^"]+)"'
    $endpoints = [regex]::Matches($content, $pattern).Count
    $testDir = "Api/$service/Tests"
    $testFiles = 0
    if (Test-Path $testDir) {
        $testFiles = (Get-ChildItem -Path $testDir -Recurse -Filter "*Tests.cs").Count
    }
    $pct = if ($endpoints -gt 0) { [math]::Round($testFiles / $endpoints * 100) } else { 0 }
    $cls = if ($pct -ge 70) { "high" } elseif ($pct -ge 50) { "mid" } else { "low" }
    $html += "            <tr><td>$service</td><td>$endpoints</td><td>$testFiles</td><td class=`"$cls`">$pct%</td></tr>`n"
}

$html += @"
        </table>
    </div>

    <div class="section">
        <h2>Per-Service Code Coverage (model lines)</h2>
        <table>
            <tr><th>Service</th><th>Lines Covered</th><th>Lines Total</th><th>Coverage</th></tr>
"@

foreach ($service in @("FakeStore", "JsonPlaceholder", "GitHub")) {
    if ($serviceResults.ContainsKey($service)) {
        $covered = $serviceResults[$service].Covered
        $total = $serviceResults[$service].Total
        $pct = if ($total -gt 0) { [math]::Round($covered / $total * 100, 1) } else { 0 }
    } else {
        $covered = 0; $total = 0; $pct = 0
    }
    $cls = if ($pct -ge 70) { "high" } elseif ($pct -ge 50) { "mid" } else { "low" }
    $html += "            <tr><td>$service</td><td>$covered</td><td>$total</td><td class=`"$cls`">$pct%</td></tr>`n"
}

$html += @"
        </table>
    </div>

    <div class="section">
        <h2>Overall Code Coverage (by module)</h2>
        <table>
            <tr><th>Module</th><th>Lines</th><th>Branches</th></tr>
"@

foreach ($file in $coverageFiles) {
    [xml]$xml = Get-Content $file.FullName
    $lineRate = [math]::Round([double]$xml.coverage.'line-rate' * 100, 1)
    $branchRate = [math]::Round([double]$xml.coverage.'branch-rate' * 100, 1)
    $linesCovered = $xml.coverage.'lines-covered'
    $linesValid = $xml.coverage.'lines-valid'
    $branchesCovered = $xml.coverage.'branches-covered'
    $branchesValid = $xml.coverage.'branches-valid'
    $html += "            <tr><td>Lines</td><td>$linesCovered / $linesValid ($lineRate%)</td><td>$branchesCovered / $branchesValid ($branchRate%)</td></tr>`n"
}

$html += @"
        </table>
    </div>
</body>
</html>
"@

$html | Out-File -FilePath $serviceHtmlPath -Encoding UTF8
Write-Host ""
Write-Host "Per-service report: $serviceHtmlPath" -ForegroundColor Cyan

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Green

# Open HTML reports in browser
$htmlReport = Join-Path $reportPath "index.html"
$serviceReport = Join-Path $reportPath "service-coverage.html"
if (Test-Path $htmlReport) {
    Start-Process $htmlReport
}
if (Test-Path $serviceReport) {
    Start-Process $serviceReport
}
