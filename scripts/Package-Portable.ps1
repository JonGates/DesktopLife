param(
    [ValidatePattern('^v?\d+\.\d+\.\d+(-[0-9A-Za-z]+(\.[0-9A-Za-z]+)*)?$')]
    [string]$Version = '0.4.0',
    [string]$RuntimeVersion = '10.0.12',
    [string]$NuGetSource
)
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$revision = (git -C $repository rev-parse --short HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Cannot determine source revision.' }
$releaseVersion = $Version.TrimStart('v')
$name = "DesktopLife-Portable-win-x64-v$releaseVersion"
$work = Join-Path $repository ('artifacts/portable-stage-' + [Guid]::NewGuid().ToString('N'))
$output = Join-Path $work $name
$archive = Join-Path $repository "artifacts/$name.zip"
if (Test-Path -LiteralPath $archive) { throw "Archive already exists: $archive. Choose a new version or rename the old archive." }
$arguments = @('publish', (Join-Path $repository 'src/DesktopLife.App/DesktopLife.App.csproj'), '-c', 'Release', '-r', 'win-x64',
    '--self-contained', 'true', "-p:RuntimeFrameworkVersion=$RuntimeVersion", "-p:Version=$releaseVersion", '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true', '-p:DisableTransitiveFrameworkReferenceDownloads=true', '-p:DebugType=None', '-p:DebugSymbols=false', '-o', $output, '--nologo')
if ($NuGetSource) { $arguments += @('--source', $NuGetSource) }
& dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw 'Portable publish failed.' }
if (!(Test-Path (Join-Path $output 'DesktopLife.exe'))) { throw 'Published executable missing.' }
@'
@echo off
start "" "%~dp0DesktopLife.exe" --settings
'@ | Set-Content -LiteralPath (Join-Path $output 'Start-DesktopLife.cmd') -Encoding ASCII
$instructions = @'
DesktopLife - Windows x64 portable edition / 便携版

中文
1. 将整个 ZIP 解压到普通文件夹。
2. 双击 Start-DesktopLife.cmd 启动并打开设置。
3. 已自带 .NET 运行环境，不需要安装 .NET、SDK 或 Visual Studio。
4. 右上角切换语言；调整昆虫数量与尺寸；“更多昆虫”中可开启新增 8 种，点击“保存数量与尺寸”。
5. 苍蝇围绕鼠标飞行，左键点击指定落点，停留 3 秒后继续飞行。
6. 默认 Ctrl+Alt+S 启动/恢复，Ctrl+Alt+P 暂停；可在设置中修改。
7. 关闭设置后继续运行。双击托盘图标重开设置；托盘右键“退出”结束程序。
8. 快捷键只在程序运行期间有效。多屏排列自动跟随 Windows 显示设置。

支持 Windows 10/11 x64。无需管理员权限，无需登录。
设置保存在当前用户的 %AppData%\DesktopLife，升级程序时仍可保留。
“便携”指免安装程序与 .NET；个人设置不会写入此分发文件夹。

English
Extract the complete ZIP, then double-click Start-DesktopLife.cmd.
The .NET runtime is included; no SDK, Visual Studio or .NET installation is needed.
Choose English at the top right. Save population changes with Save population.
Default shortcuts: Ctrl+Alt+S starts/resumes; Ctrl+Alt+P pauses. Both are configurable.
Closing settings keeps the app running. Double-click its tray icon to reopen settings;
use the tray Exit command to stop. Shortcuts only work while the app is running.
Windows 10/11 x64. Preferences are stored in %AppData%\DesktopLife, outside this folder.

Source: https://github.com/JonGates/DesktopLife
'@
[IO.File]::WriteAllText((Join-Path $output 'README.txt'), $instructions, [Text.UTF8Encoding]::new($true))
@{ Version=$releaseVersion; SourceRevision=$revision; RuntimeVersion=$RuntimeVersion; RuntimeIdentifier='win-x64'; SelfContained=$true; BuiltUtc=[DateTime]::UtcNow.ToString('o') } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'build-info.json') -Encoding UTF8
# Preserve the redistribution notices supplied by the runtime NuGet packages.
$cache = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget/packages' }
foreach ($package in @('microsoft.netcore.app.runtime.win-x64', 'microsoft.windowsdesktop.app.runtime.win-x64')) {
    $root = Join-Path $cache "$package/$RuntimeVersion"
    $notices = @(Get-ChildItem -LiteralPath $root -File | Where-Object { $_.Name -match '^(LICENSE|THIRD-PARTY)' })
    if ($notices.Count -eq 0) { throw "Runtime notices missing: $package" }
    foreach ($notice in $notices) { Copy-Item -LiteralPath $notice.FullName -Destination (Join-Path $output "$package-$($notice.Name)") }
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
if (Test-Path -LiteralPath $archive) { throw "Archive already exists: $archive. Rename it before packaging again." }
[IO.Compression.ZipFile]::CreateFromDirectory($output, $archive, [IO.Compression.CompressionLevel]::Optimal, $true)
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
"$hash  $([IO.Path]::GetFileName($archive))" | Set-Content -LiteralPath "$archive.sha256" -Encoding ASCII
[pscustomobject]@{ Archive=$archive; Bytes=(Get-Item -LiteralPath $archive).Length; SHA256=$hash; PublishedDirectory=$output } | ConvertTo-Json
