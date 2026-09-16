# DesktopLife 昆虫屏保 · Windows 10/11 Insect Screensaver (.scr)

[中文项目首页](https://github.com/JonGates/DesktopLife#readme) · [English guide](https://github.com/JonGates/DesktopLife/blob/main/README.en.md) · **[下载屏保 / Download screensaver](https://github.com/JonGates/DesktopLife/releases/download/v0.4.2/DesktopLife-ScreenSaver-win-x64-v0.4.2.zip)**

Windows 10/11 x64 屏保，自带 .NET 运行环境。独立于桌面宠物模式，可切换深色／浅色背景，支持多显示器、昆虫总数量设置、自动飞行和停落。

## 使用

### 从主程序设置（0.5 开发预览）

主程序设置分为「生物 / 屏保 / 偏好」。在「屏保」页：

- **预览屏保**：立即全屏运行，移动鼠标或按键退出；预览期间暂停桌面生物，退出后恢复此前的运行状态。
- **配置屏保内容**：选择森林／海洋、深浅背景以及生物数量与尺寸。这些配置独立于桌面场景。
- **打开 Windows 屏保设置**：将自包含程序复制成固定路径的 `DesktopLife.scr`，然后打开系统屏保设置。选择 DesktopLife、等待分钟数并点击“应用”，之后由 Windows 空闲计时触发。

安装副本位于 `%LocalAppData%\DesktopLife\ScreenSaver\<内容哈希>\DesktopLife.scr`，移动便携版原文件夹不会破坏这份副本。更新主程序后再点击此按钮，可让 Windows 使用新版本。只有点击该按钮才准备安装；打开主程序或预览不会更改 Windows 自动屏保设置。

主程序自身也支持 Windows 标准 `/s`、`/c`、`/p HWND` 参数，屏保启动入口独立于桌面单实例检查。因此主程序正在运行或已退出时，Windows 均可启动已配置的屏保。

### 配置界面（0.5 开发预览）

屏保与主程序共用卡片、按钮和输入框样式，窗口可调整大小。顶部选择背景和生物风格，森林／海洋标签下以统一表格设置数量、最小尺寸和最大尺寸；底部保存按钮始终可见。

右上角可切换简体中文／English，首次使用跟随主程序语言，保存后独立记忆。切换语言或场景不会丢失未保存的输入；点击“保存屏保设置”才保存两组配置，直接关闭则放弃本次修改。输入错误时自动显示对应场景并定位到该字段。

### 使用独立屏保包

**自动触发需要先在 Windows 中选中 DesktopLife 并应用等待时间。** 仅运行桌面宠物 EXE 或双击 SCR 打开配置，不会启用系统屏保。完成下面第 4 步后，由 Windows 在空闲达到设定时间时自动启动，无需保持桌面宠物程序运行。

源码预览版还支持「森林 / 海洋」场景；海洋中小绿龟使用自动模拟的鼠标目标。真实键鼠输入仍用于退出屏保，桌面模式的点击缩壳交互不会阻止退出。

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
- 屏保配置支持中英文切换（0.5 开发预览；旧版为中英文并列）。程序异常时记录到 `%LocalAppData%\DesktopLife\screensaver-error.log` 并退出。

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
powershell -ExecutionPolicy Bypass -File scripts/Package-ScreenSaver.ps1 -Version 0.4.2
```

输出：`artifacts/DesktopLife-ScreenSaver-win-x64-v0.4.2.zip` 和 `.zip.sha256`。原桌面模式继续使用 `Package-Portable.ps1` 打包。

源码 / Source: https://github.com/JonGates/DesktopLife

Microsoft: [屏保参数](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ms686421(v=vs.85)) · [安装入口](https://learn.microsoft.com/en-us/windows/win32/devnotes/scrnsave-exe)

昆虫风格 / Creature style：配置窗口可选择写实或可爱外观，独立于深浅色背景；下次启动屏保或系统预览时生效。
