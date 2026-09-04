<#
.SYNOPSIS
    Wrapper around `gh pr create` that auto-triggers PR review after creation.
.DESCRIPTION
    Runs gh pr create with all passed arguments, then triggers /review-pr on the new PR.
.PARAMETER Title
    PR title (required)
.PARAMETER Body
    PR body/description (optional)
.PARAMETER Base
    Target branch (default: main)
.PARAMETER Head
    Source branch (default: current branch)
.PARAMETER Draft
    Create as draft PR
.PARAMETER WhatIf
    Shows what would happen without actually creating the PR
.PARAMETER Confirm
    Prompts for confirmation before creating the PR
.EXAMPLE
    .\gh-pr-create.ps1 -Title "Add product tests" -Body "Added 5 API tests"
    .\gh-pr-create.ps1 -Title "Fix naming" -Draft
    .\gh-pr-create.ps1 -Title "Fix naming" -WhatIf
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory=$true)]
    [string]$Title,
    [string]$Body = "",
    [string]$Base = "main",
    [string]$Head = "",
    [switch]$Draft
)

$ErrorActionPreference = "Stop"

# Build gh pr create command
$createArgs = @("pr", "create", "--title", $Title, "--base", $Base)

if ($Head) {
    $createArgs += "--head", $Head
}

if ($Body) {
    $createArgs += "--body", $Body
}

if ($Draft) {
    $createArgs += "--draft"
}

$prDescription = "PR '$Title' into $Base"

if ($PSCmdlet.ShouldProcess($prDescription, "Create PR")) {
    Write-Host "Creating PR..." -ForegroundColor Cyan

    # Create the PR
    $output = & gh @createArgs 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create PR: $output"
        exit 1
    }

    Write-Host $output -ForegroundColor Green

    # Extract PR number from output (URL format: https://github.com/Owner/Repo/pull/123)
    if ($output -match "pull/(\d+)") {
        $prNumber = $Matches[1]
        Write-Host ""
        Write-Host "PR #$prNumber created successfully!" -ForegroundColor Green
        Write-Host "Run '/review-pr $prNumber' to review, or the review will trigger automatically." -ForegroundColor Yellow
    } else {
        Write-Host ""
        Write-Host "PR created, but could not extract PR number from output." -ForegroundColor Yellow
        Write-Host "Output: $output"
    }
} else {
    Write-Host "WhatIf: Would create PR '$Title' into $Base" -ForegroundColor Yellow
}
