# DesktopLife 开发与打包指南

本文说明从源码运行、生成自带运行环境的 EXE、制作分享用 ZIP，以及如何验证发布结果。命令均在仓库根目录的 PowerShell 中执行。

## 1. 开发环境

- Windows x64；应用面向 Windows 10/11 x64。
- .NET 10 SDK。本仓库 `global.json` 指定 `10.0.204`，允许同一补丁系列更新。
- Git，用于获取源码和生成打包版本标识。
- 首次还原需要访问 NuGet。开发机安装 SDK；使用自包含版本的朋友无需安装 SDK 或 .NET。

检查环境：

```powershell
git --version
dotnet --info
dotnet --list-sdks
```

获取并运行：

```powershell
git clone https://github.com/JonGates/DesktopLife.git
cd DesktopLife
dotnet restore DesktopLife.sln
dotnet build DesktopLife.sln -c Release
dotnet test DesktopLife.sln -c Release
dotnet run --project src/DesktopLife.App -- --settings
```

已经运行的实例会继续复用；`--settings` 打开设置窗口。开始发布验证前，先从托盘退出已有实例，否则新进程会因为单实例限制而退出。

## 2. 发布形式

| 形式 | 朋友需要安装 .NET | 分发内容 | 用途 |
| --- | --- | --- | --- |
| 普通发布目录 | 需要 .NET 10 Desktop Runtime x64 | 整个目录，含 EXE、DLL 和配置文件 | 开发机或已有运行环境的电脑 |
| 自包含单文件 EXE | 不需要 | 自带运行环境的 EXE；分发时附上运行环境授权说明 | 直接启动、免安装运行 |
| 便携 ZIP | 不需要 | EXE、启动入口、中英文说明、构建信息、授权说明 | 发给朋友或上传 GitHub Releases |

单文件 EXE 已嵌入托管程序集、苍蝇素材和运行环境。运行时会将必要的本机组件解包到用户可写的缓存目录，首次启动可能稍慢。

“便携”指程序和 .NET 无需安装。个人配置仍保存在 `%AppData%\DesktopLife`，不会跟随 EXE 移动到另一台电脑。

## 3. 推荐：一条命令生成便携包

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Package-Portable.ps1 -Version 0.4.0
```

脚本会输出 JSON，包含：

- `Archive`：`artifacts/DesktopLife-Portable-win-x64-v<发布版本>.zip`。
- `PublishedDirectory`：本次独立暂存目录，其中的 `DesktopLife.exe` 就是可独立运行的自包含 EXE。
- `Bytes`：ZIP 字节数。
- `SHA256`：ZIP 校验值；同时写入旁边的 `.zip.sha256` 文件。

每次使用新的暂存目录，防止普通发布的旧 DLL 混入便携包。相同发布版本的 ZIP 已存在时，脚本会在构建前拒绝覆盖；请递增 -Version，或先重命名旧包。

发布版本由 -Version 显式指定，也支持 v0.4.0、0.4.0-beta.1。EXE 版本属性和 build-info.json 同步记录此版本；SourceRevision 单独保留 Git 提交用于追溯。GitHub Release 标签应对应 v0.4.0。构建使用当前工作区，正式分享前应先提交应用代码：

```powershell
git status --short
git rev-parse --short HEAD
```

脚本默认打入 `.NET 10.0.12`，这是此次实际验证的运行环境版本。更新运行环境时显式指定版本，并重新验证：

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Package-Portable.ps1 -Version 0.4.0 -RuntimeVersion 10.0.12
```

朋友完整解压 ZIP 后，双击 `Start-DesktopLife.cmd` 即可启动并打开设置；双击 `DesktopLife.exe` 也能启动，设置窗口可通过托盘图标打开。

## 4. 只生成自包含 EXE

需要固定输出位置时，可直接使用下面的命令。`artifacts/exe` 应使用新目录，或确认没有其他发布方式留下的旧文件；不要与正在运行的发布目录混用。

```powershell
dotnet publish src/DesktopLife.App/DesktopLife.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:RuntimeFrameworkVersion=10.0.12 `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:DisableTransitiveFrameworkReferenceDownloads=true `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  -o artifacts/exe
```

输出：`artifacts/exe/DesktopLife.exe`。打开设置窗口：

```powershell
& .\artifacts\exe\DesktopLife.exe --settings
```

关键参数：

| 参数 | 作用 |
| --- | --- |
| `-c Release` | 发布构建，关闭 Debug HUD |
| `-r win-x64` | 为 Windows x64 生成应用入口和运行环境 |
| `--self-contained true` | 将 .NET 运行环境包含在发布结果中 |
| `PublishSingleFile=true` | 将应用和运行环境组合到单个 EXE |
| `IncludeNativeLibrariesForSelfExtract=true` | 将 WPF 等本机组件纳入单文件，运行时解包 |
| `RuntimeFrameworkVersion` | 固定打入包内的运行环境版本 |
| `DisableTransitiveFrameworkReferenceDownloads=true` | 对本项目仅下载所需框架包，避免预下载未使用的 ASP.NET Core 运行环境 |
| `DebugType=None` / `DebugSymbols=false` | 不生成分发用调试符号 |

不要对当前 WPF 项目自行加入裁剪或 AOT 参数；这些模式需要独立兼容性验证。

直接运行只需要这个自包含 EXE。分享时应保留 .NET 运行环境的授权和第三方声明；第 3 节的脚本会自动复制，推荐直接分发它生成的完整 ZIP。

## 5. 普通发布目录

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Publish.ps1
```

输出到 `artifacts/publish`。这个默认模式依赖电脑上已安装的 .NET 10 Desktop Runtime x64，必须保留整个目录，不能只拷贝其中的 EXE。

旧脚本也接受 `-SelfContained`，但复用同一个输出目录，且默认使用 SDK 选择的运行环境版本。正式分发请优先使用第 3、4 节，明确版本并隔离输出。

## 6. 发布验证

应用代码变化后，先编译并运行测试。仅文档或文件复制变化无需重复全部单元测试。

```powershell
dotnet build DesktopLife.sln -c Release
dotnet test DesktopLife.sln -c Release
```

对实际准备分享的 EXE 检查窗口和单实例行为；路径按自己的输出修改：

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Test-Overlay.ps1 -Executable .\artifacts\exe\DesktopLife.exe
powershell -ExecutionPolicy Bypass -File scripts/Test-SettingsInstance.ps1 -Executable .\artifacts\exe\DesktopLife.exe
```

这两个脚本会启动并关闭测试进程。先退出已有 DesktopLife；如快捷键被其他程序占用，应用会弹出设置提示，应先处理冲突再执行要求后台启动的测试。

分享 ZIP 前：

1. 将实际 ZIP 解压到新的目录，并从该目录启动。
2. 检查设置窗口、中英文、数量保存、快捷键、托盘退出。
3. 检查多屏方向、跨屏移动、点击穿透、苍蝇停落和动画。
4. 检查说明文件中文显示正常，包内没有个人配置或日志。
5. 用 `Get-FileHash` 与旁边的 SHA256 文件对比，确认传输完整。

```powershell
Get-FileHash '.\artifacts\DesktopLife-Portable-win-x64-<版本>.zip' -Algorithm SHA256
```

中间构建目录（`src/DesktopLife.App/bin/Release/net10.0-windows/win-x64`）的 `DesktopLife.runtimeconfig.json` 中，自包含版本应使用 `includedFrameworks`，包含 `Microsoft.NETCore.App` 和 `Microsoft.WindowsDesktop.App`。单文件打包后该配置嵌入 EXE，因此最终目录没有外置 runtimeconfig 文件是正常的。

彻底验证未安装 .NET 的使用场景，应在干净的 Windows 虚拟机上运行。此次另做了指定空 .NET 根目录、观察实际加载模块的检查，详见 [便携包验证记录](PORTABLE_PACKAGE_VERIFICATION.md)。

## 7. 下载或打包失败排查

### SDK 不匹配

检查 `dotnet --list-sdks` 与 `global.json`。更新 SDK 要有意识地修改版本约束，再重新编译验证。

### NuGet 下载慢或还原失败

检查官方源连接。优先重试还原；不要关闭包签名或完整性校验。已有完整且可信的本地包源时，可使用：

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Package-Portable.ps1 -Version 0.4.0 -NuGetSource D:\packages\verified-feed
```

本地源需包含指定版本所需的包，或依赖已有 NuGet 缓存。此前的 `artifacts/portable-nuget-feed` 只是本机临时缓存，不会随仓库上传。

### 输出文件被占用

先退出托盘中的程序，再重新发布；或者使用新的输出目录。不要把构建失败后的目录作为成品发出。

### 中文说明乱码

`Package-Portable.ps1` 包含中文并通过 Windows PowerShell 5.1 执行，必须保留 UTF-8 BOM。说明文件同样使用 UTF-8 BOM 写入。

### 双击后没有设置窗口

默认 EXE 在托盘运行。双击托盘图标，或使用 `--settings` 参数；ZIP 中的启动脚本已带这个参数。

## 8. 仓库与分发

DesktopLife 使用统一项目版本：桌面应用与屏保是同一 Release 下的两个子程序，不分别维护版本。发布时在同一个干净的版本标签检出目录执行：

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Package-Portable.ps1 -Version 0.4.0
powershell -ExecutionPolicy Bypass -File scripts/Package-ScreenSaver.ps1 -Version 0.4.0
```

打包脚本各自生成 `.zip.sha256` 供本地检查；发布时将校验值合并为一个清单，只上传两个 ZIP 和一个 `SHA256SUMS.txt` 到同一个 `v0.4.0` Release。检查两个包的 `build-info.json` 中 Version 一致，SourceRevision 解析到同一个 Git 提交（可用 `git rev-parse <SourceRevision>` 比较完整哈希）。后续版本同步更新两条命令的版本号；保留旧 Release 作为历史记录。

生成统一校验清单（每行是一个 ZIP 的 SHA256 和文件名）：

```powershell
$releaseVersion = '0.4.0'
$packageNames = @(
    "DesktopLife-Portable-win-x64-v$releaseVersion.zip",
    "DesktopLife-ScreenSaver-win-x64-v$releaseVersion.zip"
)
$checksumLines = foreach ($packageName in $packageNames) {
    $packageHash = (Get-FileHash -LiteralPath "artifacts/$packageName" -Algorithm SHA256 -ErrorAction Stop).Hash
    "$packageHash  $packageName"
}
$checksumLines | Set-Content -LiteralPath artifacts/SHA256SUMS.txt -Encoding ASCII
```

GitHub 自动提供的 Source code ZIP / tar.gz 是源码附件；我们维护的发布附件共三个。Tags 页保留各历史版本，不用于展示这三个程序下载附件。

- 提交源码、打包脚本、开发文档和验证记录。
- `artifacts`、`bin`、`obj` 已被 Git 忽略，构建产物不会随 `git push` 上传。
- 直接将 ZIP 发给朋友，或另外创建 GitHub Release 并上传 ZIP 和 SHA256 文件。提交源码不会自动生成 Release 下载附件。
- 当前流程生成免安装运行的程序；如需带安装向导、桌面快捷方式和卸载入口的安装器，需要另外增加安装器构建步骤。

## 屏保版补充

新增独立项目 `src/DesktopLife.ScreenSaver`，复用引擎与素材，生成标准 Windows `.scr`。桌面模式打包入口保持 `Package-Portable.ps1`。

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Package-ScreenSaver.ps1 -Version 0.4.0
```

输出 `artifacts/DesktopLife-ScreenSaver-win-x64-v0.4.0.zip`，包含自带运行环境的 `DesktopLife.scr`、安装/配置/全屏体验脚本；SHA256 校验文件位于 ZIP 旁。完整使用、配置位置与标准参数见 [屏保说明](SCREENSAVER.md)。

验证命令：

```powershell
dotnet run --project tools/DesktopLife.Diagnostics -- --screensaver-check
dotnet run --project tools/DesktopLife.Diagnostics -- --screensaver-fullscreen-check
dotnet run --project tools/DesktopLife.Diagnostics -- --screensaver-layout-check
```

后两项会短暂打开全屏窗口并自动关闭；分别验证当前真实显示器和包含负坐标、错位边缘的模拟双屏布局。
