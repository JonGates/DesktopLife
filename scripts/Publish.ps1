param([switch]$SelfContained)
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '../src/DesktopLife.App/DesktopLife.App.csproj'
$output = Join-Path $PSScriptRoot '../artifacts/publish'
if ($SelfContained) {
    dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $output --nologo
} else {
    dotnet publish $project -c Release --self-contained false -o $output --nologo
}
if ($LASTEXITCODE -ne 0) { throw "Publish failed ($LASTEXITCODE)" }
Write-Host "Published: $output/DesktopLife.exe"
