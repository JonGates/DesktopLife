# DesktopLife 昆虫屏保 · Windows 10/11 Insect Screensaver (.scr)

[中文项目首页](https://github.com/JonGates/DesktopLife#readme) · [English guide](https://github.com/JonGates/DesktopLife/blob/main/README.en.md) · **[下载屏保 / Download screensaver](https://github.com/JonGates/DesktopLife/releases/download/v0.4.1/DesktopLife-ScreenSaver-win-x64-v0.4.1.zip)**

Windows 10/11 x64 屏保，自带 .NET 运行环境。独立于桌面宠物模式，可切换深色／浅色背景，支持多显示器、昆虫总数量设置、自动飞行和停落。

## 使用

1. 完整解压屏保 ZIP 到**长期保留的文件夹**。
2. 双击 `Configure-ScreenSaver.cmd`，选择深色或浅色背景，设置各类昆虫的数量与尺寸范围后保存（新增 8 种默认关闭）。苍蝇固定 1 只。
3. 双击 `Preview-FullScreen.cmd` 体验全屏效果；启动约 0.75 秒后，移动鼠标、点击或按键会退出。
4. 双击 `Install-ScreenSaver.cmd`，打开 Windows“屏幕保护程序设置”。选择 DesktopLife，设置等待分钟数，再点击“应用”。可以在这个窗口直接预览动画或打开屏保设置。
5. 如需恢复时登录，勾选 Windows 的“在恢复时显示登录屏幕”；是否需要登录由 Windows 和组织策略管理。

安装入口使用 Windows 的 `desk.cpl,InstallScreenSaver`，不复制文件到系统目录。选择屏保后，请保留原文件路径；移动或删除文件会使 Windows 找不到它。禁用时在 Windows“屏幕保护程序设置”中选择“无”并应用。

## 行为与配置

- 每个实际显示器都有完整背景，共享一批昆虫。昆虫可以沿相接边缘跨屏；屏幕发生插拔或排列改变时退出本次屏保，下次启动读取新排列。
- 屏保里苍蝇自动游走，定期落下搓足；真实鼠标和键盘用于退出屏保。
- 蟑螂和蚂蚁各 0–500，毛毛虫 0–100。默认数量 20 / 20 / 3；设为 0 可关闭该类爬行昆虫。
- 屏保设置独立保存到 `%AppData%\DesktopLife\screensaver.json`，不修改桌面宠物模式的数量、快捷键、语言或录屏排除设置。
- 无参数或 `/c` 打开配置；`/s` 全屏运行；`/p HWND` 在 Windows 提供的父窗口中预览。非法预览参数退出，不会意外全屏。
- 屏保配置标签中英文并列。程序异常时记录到 `%LocalAppData%\DesktopLife\screensaver-error.log` 并退出。

## English

1. Extract the complete ZIP into a **permanent folder**. The .NET runtime is included.
2. Run `Configure-ScreenSaver.cmd` to choose **Dark / Light** and insect counts.
3. Run `Preview-FullScreen.cmd` to try it. After a brief startup grace period, move the mouse or press a key to exit.
4. Run `Install-ScreenSaver.cmd` to open Windows Screen Saver Settings. Select DesktopLife, choose an idle timeout, and click **Apply**.
5. Select **On resume, display logon screen** in Windows if desired. Windows and your organization's policies control sign-in behavior.

Keep the extracted folder in place after selecting the screen saver. To disable it, choose **None** in Windows Screen Saver Settings. Dark/light preferences and populations are separate from desktop mode. All monitors share one population; display changes end the current session so the next launch uses the updated layout.

## 开发与打包 / Build

在源码仓库根目录执行（Windows，.NET 10 SDK）：

```powershell
dotnet run --project src/DesktopLife.ScreenSaver -- /c
dotnet run --project src/DesktopLife.ScreenSaver -- /s
dotnet run --project tools/DesktopLife.Diagnostics -- --screensaver-check
powershell -ExecutionPolicy Bypass -File scripts/Package-ScreenSaver.ps1 -Version 0.4.1
```

输出：`artifacts/DesktopLife-ScreenSaver-win-x64-v0.4.1.zip` 和 `.zip.sha256`。原桌面模式继续使用 `Package-Portable.ps1` 打包。

源码 / Source: https://github.com/JonGates/DesktopLife

Microsoft: [屏保参数](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ms686421(v=vs.85)) · [安装入口](https://learn.microsoft.com/en-us/windows/win32/devnotes/scrnsave-exe)

昆虫风格 / Creature style：配置窗口可选择写实或可爱外观，独立于深浅色背景；下次启动屏保或系统预览时生效。
