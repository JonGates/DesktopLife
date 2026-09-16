param(
    [ValidatePattern('^v?\d+\.\d+\.\d+(-[0-9A-Za-z]+(\.[0-9A-Za-z]+)*)?$')]
    [string]$Version = '0.4.0',
    [string]$RuntimeVersion = '10.0.12',
    [string]$NuGetSource
)
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$releaseVersion = $Version.TrimStart('v')
$revision = (git -C $repository rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Cannot determine source revision.' }
$name = "DesktopLife-ScreenSaver-win-x64-v$releaseVersion"
$output = Join-Path $repository ('artifacts/screensaver-stage-' + [Guid]::NewGuid().ToString('N') + '/' + $name)
$archive = Join-Path $repository "artifacts/$name.zip"
if (Test-Path -LiteralPath $archive) { throw "Archive already exists: $archive. Choose a new version." }
$arguments = @('publish', (Join-Path $repository 'src/DesktopLife.ScreenSaver/DesktopLife.ScreenSaver.csproj'), '-c', 'Release', '-r', 'win-x64',
    '--self-contained', 'true', "-p:RuntimeFrameworkVersion=$RuntimeVersion", "-p:Version=$releaseVersion", '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true', '-p:DisableTransitiveFrameworkReferenceDownloads=true', '-p:DebugType=None', '-p:DebugSymbols=false', '-o', $output, '--nologo')
if ($NuGetSource) { $arguments += @('--source', $NuGetSource) }
& dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw 'Screen saver publish failed.' }
$exe = Join-Path $output 'DesktopLife.ScreenSaver.exe'
if (!(Test-Path -LiteralPath $exe)) { throw 'Published executable missing.' }
# The self-contained bundle is a native Windows executable; Windows recognizes .scr as a screen saver.
Rename-Item -LiteralPath $exe -NewName 'DesktopLife.scr'
@'
@echo off
start "" rundll32.exe desk.cpl,InstallScreenSaver "%~dp0DesktopLife.scr"
'@ | Set-Content -LiteralPath (Join-Path $output 'Install-ScreenSaver.cmd') -Encoding ASCII
@'
@echo off
start "" "%~dp0DesktopLife.scr" /c
'@ | Set-Content -LiteralPath (Join-Path $output 'Configure-ScreenSaver.cmd') -Encoding ASCII
@'
@echo off
start "" "%~dp0DesktopLife.scr" /s
'@ | Set-Content -LiteralPath (Join-Path $output 'Preview-FullScreen.cmd') -Encoding ASCII
Copy-Item -LiteralPath (Join-Path $repository 'docs/SCREENSAVER.md') -Destination (Join-Path $output 'README.md')
@{ Version=$releaseVersion; SourceRevision=$revision; RuntimeVersion=$RuntimeVersion; RuntimeIdentifier='win-x64'; SelfContained=$true; BuiltUtc=[DateTime]::UtcNow.ToString('o') } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'build-info.json') -Encoding UTF8
$cache = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget/packages' }
foreach ($package in @('microsoft.netcore.app.runtime.win-x64', 'microsoft.windowsdesktop.app.runtime.win-x64')) {
    $root = Join-Path $cache "$package/$RuntimeVersion"
    $notices = @(Get-ChildItem -LiteralPath $root -File | Where-Object { $_.Name -match '^(LICENSE|THIRD-PARTY)' })
    if ($notices.Count -eq 0) { throw "Runtime notices missing: $package" }
    foreach ($notice in $notices) { Copy-Item -LiteralPath $notice.FullName -Destination (Join-Path $output "$package-$($notice.Name)") }
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($output, $archive, [IO.Compression.CompressionLevel]::Optimal, $true)
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
"$hash  $([IO.Path]::GetFileName($archive))" | Set-Content -LiteralPath "$archive.sha256" -Encoding ASCII
[pscustomobject]@{ Archive=$archive; Bytes=(Get-Item -LiteralPath $archive).Length; SHA256=$hash; PublishedDirectory=$output } | ConvertTo-Json
