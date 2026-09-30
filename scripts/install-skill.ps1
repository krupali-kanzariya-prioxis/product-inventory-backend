$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$source = Join-Path $repoRoot 'skills\product-planning'
$target = Join-Path $repoRoot '.claude\skills\product-planning'

if (-not (Test-Path $source)) {
	throw "Skill source folder not found: $source"
}

New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
if (Test-Path $target) {
	Remove-Item -Path $target -Recurse -Force
}

Copy-Item -Path $source -Destination $target -Recurse -Force
Write-Host "Installed skill to $target"
