$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$keep = @(
  '.git',
  '.github',
  '.gitignore',
  'StreamVault.md',
  'streamvault-backend',
  'streamvault-admin-dashboard',
  'docker-compose.yml',
  'docker-compose-simple.yml',
  'monitoring',
  'README.md',
  'README-PRODUCTION.md',
  'SETUP.md'
)

$legacyDir = Join-Path $root '_legacy'
New-Item -ItemType Directory -Force -Path $legacyDir | Out-Null

$items = Get-ChildItem -Force -LiteralPath $root
foreach ($item in $items) {
  if ($keep -contains $item.Name) { continue }
  if ($item.Name -eq '_legacy') { continue }

  $dest = Join-Path $legacyDir $item.Name

  if (Test-Path $dest) {
    Remove-Item -Recurse -Force $dest
  }

  try {
    Move-Item -Force -LiteralPath $item.FullName -Destination $dest
  } catch {
    Write-Warning "Skipping '$($item.Name)' (in use / locked). Close any running processes using it and re-run cleanup. Error: $($_.Exception.Message)"
    continue
  }
}

Write-Host "Done. All non-canonical files were moved to: $legacyDir"
Write-Host "If everything works, you can delete _legacy later."
