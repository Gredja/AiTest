[CmdletBinding()]
param(
    [string]$Project = "",
    [string]$Filter = "",
    [int]$ParallelWorkers = 8
)

# twin of Scripts/test-parallel-rerun.sh (CI container has no pwsh guarantee)
$ErrorActionPreference = "Continue"
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$resultsDir = Join-Path ([System.IO.Path]::GetTempPath()) "GredjaTestRun/two-pass-$stamp"
New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null
$script:LastTestExit = 0

function Invoke-DotnetTest {
    param([int]$Workers, [string]$TestFilter)

    $testArgs = @("test",
        "--verbosity", "minimal",
        "--results-directory", $resultsDir,
        "--logger", "trx")
    if ($Project) {
        $testArgs += $Project
    }
    if ($TestFilter) {
        $testArgs += @("--filter", $TestFilter)
    }
    $testArgs += @("--", "NUnit.NumberOfTestWorkers=$Workers")

    & dotnet @testArgs | Out-Host
    $script:LastTestExit = $LASTEXITCODE
}

function Get-FailedTests {
    $trxFiles = @(Get-ChildItem $resultsDir -Filter *.trx -ErrorAction SilentlyContinue)
    if ($trxFiles.Count -eq 0) {
        return $null
    }

    $definitions = @{}
    $failed = @()
    foreach ($file in $trxFiles) {
        [xml]$trx = Get-Content -LiteralPath $file.FullName
        foreach ($unitTest in $trx.TestRun.TestDefinitions.UnitTest) {
            $definitions[$unitTest.id] = $unitTest.name
        }
        foreach ($result in $trx.TestRun.Results.UnitTestResult) {
            if ($result.outcome -eq "Failed" -or $result.outcome -eq "Error") {
                $name = $definitions[$result.testId]
                if ($name) {
                    $failed += $name
                }
            }
        }
    }

    return @($failed | Sort-Object -Unique)
}

# VSTest --filter breaks on parentheses in parameterized FQNs (e.g. Method(en))
# and on an overly long OR chain; strip "(...)" so ~ matches the method base name.
function Get-RerunFilterTerms {
    param([string[]]$TestNames)
    return @($TestNames |
        ForEach-Object { $_ -replace '\([^()]*\)$', '' } |
        Sort-Object -Unique)
}

Write-Host "== PASS 1: workers=$ParallelWorkers $Project $Filter" -ForegroundColor Cyan
Invoke-DotnetTest -Workers $ParallelWorkers -TestFilter $Filter
$pass1Exit = $script:LastTestExit
if ($pass1Exit -eq 0) {
    Write-Host "PASS 1 green - nothing to rerun." -ForegroundColor Green
    exit 0
}

$failed = Get-FailedTests
if ($null -eq $failed -or $failed.Count -eq 0) {
    Write-Host "PASS 1 failed (exit $pass1Exit) but the trx files list no failed tests - cannot rerun." -ForegroundColor Red
    exit $pass1Exit
}

$filterTerms = Get-RerunFilterTerms -TestNames $failed
Write-Host "== PASS 2: rerun $($failed.Count) failed test(s) as $($filterTerms.Count) filter term(s) with 1 worker" -ForegroundColor Cyan
$failed | ForEach-Object { Write-Host "  - $_" }
$rerunFilter = ($filterTerms | ForEach-Object { "FullyQualifiedName~$_" }) -join "|"

Get-ChildItem $resultsDir -Filter *.trx | Remove-Item -Force
Invoke-DotnetTest -Workers 1 -TestFilter $rerunFilter
$pass2Exit = $script:LastTestExit
if ($pass2Exit -eq 0) {
    Write-Host "PASS 2 green - $($failed.Count) failure(s) recovered sequentially (flaky under $ParallelWorkers workers):" -ForegroundColor Yellow
    $failed | ForEach-Object { Write-Host "  ~ $_" }
    exit 0
}

$stillFailed = Get-FailedTests
if ($null -eq $stillFailed -or $stillFailed.Count -eq 0) {
    Write-Host "PASS 2 red - infrastructure/filter error (exit $pass2Exit), no Failed results in trx - not a recovery:" -ForegroundColor Red
    $failed | ForEach-Object { Write-Host "  ? $_" }
    exit 1
}

Write-Host "PASS 2 red - persistent failure(s):" -ForegroundColor Red
$stillFailed | ForEach-Object { Write-Host "  x $_" }
$recovered = @($failed | Where-Object { $stillFailed -notcontains $_ })
if ($recovered.Count -gt 0) {
    Write-Host "Recovered on rerun (flaky):" -ForegroundColor Yellow
    $recovered | ForEach-Object { Write-Host "  ~ $_" }
}
exit 1
