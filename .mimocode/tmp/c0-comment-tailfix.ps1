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

Update-CsFile 'Core\Helpers\GitHub\GitHubTestBase.cs' -RemoveLines 65,66
Update-CsFile 'Ui\Pages\GitHubStream\Repository\IssuesPage.cs' -RemoveLines 32,33
Write-Host 'DONE'
