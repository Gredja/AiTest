$ErrorActionPreference = 'Stop'
$root = 'D:\Work\AI\curriculum-public\Gredja'

function Update-CsFile {
    param(
        [string]$RelPath,
        [int[]]$RemoveLines = @(),
        [hashtable]$ReplaceLines = @{},
        [hashtable]$InsertAfter = @{}
    )

    $path = Join-Path $root $RelPath
    $bytes = [System.IO.File]::ReadAllBytes($path)
    $hasBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $offset = if ($hasBom) { 3 } else { 0 }
    $text = [System.Text.Encoding]::UTF8.GetString($bytes, $offset, $bytes.Length - $offset)
    $encoding = [System.Text.UTF8Encoding]::new($hasBom)

    $lines = $text -split "`r`n"

    foreach ($lineNum in $RemoveLines) {
        if ($lineNum -lt 1 -or $lineNum -gt $lines.Count) { throw "$RelPath remove ${lineNum}: out of range" }
        if ($lines[$lineNum - 1] -notmatch '^\s*//') { throw "$RelPath remove ${lineNum}: not a comment: '$($lines[$lineNum - 1])'" }
    }
    foreach ($key in $ReplaceLines.Keys) {
        if ($key -lt 1 -or $key -gt $lines.Count) { throw "$RelPath replace ${key}: out of range" }
    }
    foreach ($key in $InsertAfter.Keys) {
        if ($key -lt 1 -or $key -gt $lines.Count) { throw "$RelPath insertAfter ${key}: out of range" }
    }

    $result = [System.Collections.Generic.List[string]]::new()
    for ($i = 1; $i -le $lines.Count; $i++) {
        if ($RemoveLines -contains $i) {
            # skip
        } elseif ($ReplaceLines.ContainsKey($i)) {
            foreach ($newLine in $ReplaceLines[$i]) { $result.Add($newLine) }
        } else {
            $result.Add($lines[$i - 1])
        }
        if ($InsertAfter.ContainsKey($i)) {
            foreach ($newLine in $InsertAfter[$i]) { $result.Add($newLine) }
        }
    }

    $newText = [string]::Join("`r`n", $result)
    [System.IO.File]::WriteAllText($path, $newText, $encoding)
    Write-Host "OK $RelPath"
}

# --- Ui ---
Update-CsFile 'Ui\Tests\Support\GitHubUiTestBase.cs' `
    -RemoveLines 25,26 `
    -ReplaceLines @{ 17 = @('    internal const int UiTimeoutMs = 15_000;') }

Update-CsFile 'Ui\Pages\GitHubStream\Repository\IssuesPage.cs' `
    -ReplaceLines @{ 30 = @(
        '    // The list is indexed asynchronously after a write: a page loaded right after a create',
        '    // can still miss the new issue — poll with a reload instead of a single instant count'
    ) }

Update-CsFile 'Ui\Pages\GitHubStream\Repository\NotFoundPage.cs' `
    -RemoveLines 9,17,18 `
    -ReplaceLines @{ 10 = @(
        '    private static readonly TimeSpan _messageTimeout =',
        '        TimeSpan.FromMilliseconds(GitHubUiTestBase.UiTimeoutMs);'
    ) } `
    -InsertAfter @{ 3 = @('using Ui.Tests.Support;') }

Update-CsFile 'Ui\Pages\GitHubStream\Issues\IssueDetailPage.cs' `
    -RemoveLines 23,30

Update-CsFile 'Ui\Pages\GitHubStream\Issues\IssueCreatePage.cs' `
    -RemoveLines 9,16,17 `
    -ReplaceLines @{
        10 = @(
            '    private static readonly TimeSpan _openTimeout =',
            '        TimeSpan.FromMilliseconds(GitHubUiTestBase.UiTimeoutMs);'
        )
        33 = @('        // accessible name includes the shortcut hint: "Create ( control enter )"')
    } `
    -InsertAfter @{ 3 = @('using Ui.Tests.Support;') }

Update-CsFile 'Ui\Pages\GitHubStream\Issues\CommentForm.cs' `
    -RemoveLines 9

Update-CsFile 'Ui\Pages\Common\HeaderPage.cs' `
    -RemoveLines 27

Update-CsFile 'Ui\AssemblyInfo.cs' `
    -RemoveLines 4

# --- Core ---
Update-CsFile 'Core\Helpers\GitHub\GitHubTestBase.cs' `
    -ReplaceLines @{ 63 = @(
        '    // A same-tree commit gives "No commits between" (OB §21) and an empty files list for the',
        '    // dynamic-max PR fixtures — the scratch file guarantees both divergence and ≥1 changed file.'
    ) }

Update-CsFile 'Core\Helpers\GitHub\GitHubCleanupHelper.cs' `
    -RemoveLines 10,11

Update-CsFile 'Core\Helpers\Assertions\AssertHelper.cs' `
    -RemoveLines 46,130

Update-CsFile 'Core\Helpers\Data\TestDataConstants.cs' `
    -RemoveLines 3,4

Update-CsFile 'Core\Helpers\GitHub\UiSandboxCleanupHelper.cs' `
    -RemoveLines 5,6

Update-CsFile 'Core\Config\GitHubErrors.cs' `
    -RemoveLines 5

Update-CsFile 'Core\Helpers\Data\DataGenerator.cs' `
    -RemoveLines 19

# --- Api / E2E ---
Update-CsFile 'Api\AssemblyInfo.cs' -RemoveLines 1,2,3
Update-CsFile 'E2E\AssemblyInfo.cs' -RemoveLines 1,2,3

Write-Host 'DONE'
