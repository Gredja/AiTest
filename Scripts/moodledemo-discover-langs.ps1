# One-off discovery for Ui/moodledemo/Fixtures/expected-langs.json:
# enumerates every ?lang= code exposed by the site switcher and records the actual
# <html lang/dir> + <title> of the home and login pages for each code.
# The external demo occasionally serves degraded responses (connection drops, HTTP 200
# with a foreign page) — each page fetch retries until a real site page is seen.
param(
    [string]$BaseUrl = 'https://school.moodledemo.net',
    [string]$OutFile = ''
)

$ErrorActionPreference = 'Stop'
$MaxAttempts = 3
$SiteTitleMarker = '| Mount Orange'

function Get-LangCodes {
    param([string]$Url)
    $response = Invoke-WebRequest -Uri $Url -UseBasicParsing
    [regex]::Matches($response.Content, '\?lang=([a-zA-Z_\-]+)') |
        ForEach-Object { $_.Groups[1].Value }
}

function Read-Locale {
    param([string]$Html)
    $htmlTag = [regex]::Match($Html, '<html[^>]*>')
    $lang = [regex]::Match($htmlTag.Value, '\slang="([^"]*)"')
    $dir = [regex]::Match($htmlTag.Value, '\sdir="([^"]*)"')
    $title = [regex]::Match($Html, '<title>([^<]*)</title>')
    [pscustomobject]@{
        Lang  = $lang.Groups[1].Value
        Dir   = $dir.Groups[1].Value
        Title = $title.Groups[1].Value
    }
}

function Get-PageEntry {
    param([string]$Url)
    $status = 0
    $locale = $null
    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $status = 0
        $content = ''
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing
            $status = [int]$response.StatusCode
            $content = $response.Content
        }
        catch { }

        if ($status -eq 200 -and $content -like "*$SiteTitleMarker*") {
            $locale = Read-Locale -Html $content
            if ($locale.Lang -ne '' -and $locale.Title -like "*$SiteTitleMarker*") {
                break
            }
            $locale = $null
        }
        if ($attempt -lt $MaxAttempts) { Start-Sleep -Seconds 1 }
    }
    if ($null -eq $locale) {
        Write-Warning "still dirty after $MaxAttempts attempts: $Url (status=$status)"
        $locale = [pscustomobject]@{ Lang = ''; Dir = ''; Title = '' }
    }
    [pscustomobject]@{ Lang = $locale.Lang; Dir = $locale.Dir; Title = $locale.Title }
}

$codes = @('en') + (Get-LangCodes "$BaseUrl/login/index.php") + (Get-LangCodes "$BaseUrl/")
$codes = $codes | Sort-Object -Unique
Write-Host "codes: $($codes.Count)"

$homeRows = [System.Collections.Generic.List[object]]::new()
$loginRows = [System.Collections.Generic.List[object]]::new()

foreach ($code in $codes) {
    $homeEntry = Get-PageEntry "$BaseUrl/?lang=$code"
    $loginEntry = Get-PageEntry "$BaseUrl/login/index.php?lang=$code"

    $note = ''
    if ($homeEntry.Lang -and $homeEntry.Lang -ne $code) { $note += " [lang=$($homeEntry.Lang)]" }
    if ($homeEntry.Lang -eq '') { $note += " [DIRTY]" }
    $shortTitle = $homeEntry.Title
    if ($shortTitle.Length -gt 40) { $shortTitle = $shortTitle.Substring(0, 40) }
    Write-Host ("{0,-12} {1} | {2}{3}" -f $code, $homeEntry.Dir, $shortTitle, $note)

    $homeRows.Add([pscustomobject]@{ code = $code; lang = $homeEntry.Lang; dir = $homeEntry.Dir; title = $homeEntry.Title })
    $loginRows.Add([pscustomobject]@{ code = $code; lang = $loginEntry.Lang; dir = $loginEntry.Dir; title = $loginEntry.Title })
}

$result = [pscustomobject]@{ home = $homeRows; loginPage = $loginRows }
$json = $result | ConvertTo-Json -Depth 4

if ($OutFile) {
    Set-Content -Path $OutFile -Value $json -Encoding UTF8
    Write-Host "written: $OutFile"
}
else {
    $json
}
