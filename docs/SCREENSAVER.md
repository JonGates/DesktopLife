# DesktopLife 昆虫屏保 · Windows 10/11 Insect Screensaver (.scr)

[中文项目首页](https://github.com/JonGates/DesktopLife#readme) · [English guide](https://github.com/JonGates/DesktopLife/blob/main/README.en.md) · **[下载主程序 / Download DesktopLife](https://github.com/JonGates/DesktopLife/releases/download/v0.5.0/DesktopLife-Portable-win-x64-v0.5.0.zip)**

Windows 10/11 x64 屏保，自带 .NET 运行环境。独立于桌面宠物模式，可切换深色／浅色背景，支持森林、海洋、雨窗三种场景，多显示器、生物数量设置和五档雨量。雨窗使用图片背景时支持柔焦与水滴内的近似折射。

## 使用

### 从主程序设置（v0.5.0）

主程序设置分为「生物 / 屏保 / 偏好」。在「屏保」页：

- **预览屏保**：立即全屏运行，移动鼠标或按键退出；预览期间暂停桌面生物，退出后恢复此前的运行状态。
- **配置屏保内容**：选择森林／海洋／雨窗、深浅或图片背景，以及生物数量与尺寸或五档雨量。这些配置独立于桌面场景。
- **打开 Windows 屏保设置**：将自包含程序复制成固定路径的 `DesktopLife.scr`，然后打开系统屏保设置。选择 DesktopLife、等待分钟数并点击“应用”，之后由 Windows 空闲计时触发。

安装副本位于 `%LocalAppData%\DesktopLife\ScreenSaver\<内容哈希>\DesktopLife.scr`，移动便携版原文件夹不会破坏这份副本。更新主程序后再点击此按钮，可让 Windows 使用新版本。只有点击该按钮才准备安装；打开主程序或预览不会更改 Windows 自动屏保设置。

主程序自身也支持 Windows 标准 `/s`、`/c`、`/p HWND` 参数，屏保启动入口独立于桌面单实例检查。因此主程序正在运行或已退出时，Windows 均可启动已配置的屏保。

### 配置界面（v0.5.0）

屏保与主程序共用卡片、按钮和输入框样式，窗口可调整大小。顶部选择背景和生物风格，森林／海洋标签下以统一表格设置数量、最小尺寸和最大尺寸，雨窗标签下选择五档雨量；底部保存按钮始终可见。

右上角可切换简体中文／English，首次使用跟随主程序语言，保存后独立记忆。切换语言或场景不会丢失未保存的输入；点击“保存屏保设置”才保存两组配置，直接关闭则放弃本次修改。输入错误时自动显示对应场景并定位到该字段。

### Windows 自动触发

在主程序中打开 Windows 屏保设置后，选择 DesktopLife、设定等待分钟数并点击“应用”。仅运行主程序或预览不会启用自动屏保。升级程序后重复此操作，更新系统使用的副本。

v0.5.0 仅发布主程序包，屏保已集成。Windows 调用的 `.scr` 副本由主程序自动准备，无需单独下载。主程序退出后，Windows 仍可启动已设置的屏保。

## 行为与配置

### 背景图片

屏保配置中的「背景图片」支持选择 PNG、JPG、BMP，并显示缩略图。保存时会将原图复制到配置目录的 `backgrounds` 子目录，移动原文件不影响屏保。每屏等比填充，超出边缘的内容裁切；Windows 小窗口预览同样显示图片。点击「恢复纯色」并保存即可使用深浅背景。图片丢失或无法读取时自动回退到纯色。

此功能尚未包含在已发布的 v0.5.0 中。

- 每个实际显示器都有完整背景，共享一批昆虫。昆虫可以沿相接边缘跨屏；屏幕发生插拔或排列改变时退出本次屏保，下次启动读取新排列。
- 屏保里苍蝇自动游走，定期落下搓足；真实鼠标和键盘用于退出屏保。
- 蟑螂和蚂蚁各 0–500，毛毛虫 0–100。默认数量 20 / 20 / 3；设为 0 可关闭该类爬行昆虫。
- 屏保设置独立保存到 `%AppData%\DesktopLife\screensaver.json`，不修改桌面宠物模式的数量、快捷键、语言或录屏排除设置。
- 无参数或 `/c` 打开配置；`/s` 全屏运行；`/p HWND` 在 Windows 提供的父窗口中预览。非法预览参数退出，不会意外全屏。
- 屏保配置支持中英文切换（v0.5.0；旧版为中英文并列）。程序异常时记录到 `%LocalAppData%\DesktopLife\screensaver-error.log` 并退出。

## English

Download the DesktopLife Portable ZIP, extract it, and start the main app. Open **Screen saver** to configure content or preview it. Open **Windows screen saver settings**, select DesktopLife, choose an idle timeout and click **Apply**. Repeat setup after upgrading to select the new copy. No separate screen saver download is needed.

## 开发与打包 / Build

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Package-Portable.ps1 -Version 0.5.0
dotnet run --project src/DesktopLife.App -- /c
dotnet run --project tools/DesktopLife.Diagnostics -- --screensaver-check
```

输出 `artifacts/DesktopLife-Portable-win-x64-v0.5.0.zip`，包含桌面与屏保功能。正式发布仅上传该 ZIP 与 `SHA256SUMS.txt`。[打包指南](DEVELOPMENT_AND_PACKAGING.md)。
