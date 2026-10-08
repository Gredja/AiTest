# Normalizes worktree line endings to what .gitattributes mandates (eol=crlf group).
# LF-only writes from AI tools trigger "LF will be replaced by CRLF" warnings on every
# `git add`; run this before staging. Worktree-only: the clean filter produces the same
# blob, so a correct run leaves `git status` empty.
$ErrorActionPreference = 'Stop'
Set-Location (git rev-parse --show-toplevel)

$crlfExtensions = 'cs', 'csx', 'csproj', 'props', 'sln', 'slnx', 'json', 'jsonc', 'ps1'
$extraFiles = '.editorconfig'
$converted = 0

$paths = @((git ls-files) + @(git ls-files --others --exclude-standard) + $extraFiles | Select-Object -Unique)
foreach ($path in $paths) {
    $extension = [IO.Path]::GetExtension($path).TrimStart('.').ToLower()
    $needsCrlf = ($crlfExtensions -contains $extension) -or ($extraFiles -contains $path)
    if (-not $needsCrlf -or -not (Test-Path $path)) {
        continue
    }

    $text = [IO.File]::ReadAllText($path)
    if ($text -notmatch "`r`n" -and $text -notmatch "(?<!`r)`n") {
        continue
    }

    $normalized = $text -replace "`r?`n", "`r`n"
    if ($normalized -ceq $text) {
        continue
    }

    [IO.File]::WriteAllText($path, $normalized, [Text.UTF8Encoding]::new($false))
    $converted++
}

Write-Host "EOL normalized: $converted file(s)"
