# DesktopLife v0.5.0 · Forest & Ocean

Windows 10/11 x64 · 正式版 / Stable release · 自带 .NET 运行环境 / Runtime included

## 下载哪个？ / Which download?

- **推荐：`DesktopLife-Portable-win-x64-v0.5.0.zip`** — 主程序已包含桌面宠物、屏保预览、内容配置和 Windows 屏保设置入口。解压后运行 `Start-DesktopLife.cmd`。
- **`SHA256SUMS.txt`** — 主程序 ZIP 的校验值。

Recommended: the Portable ZIP includes desktop pets **and screen saver functionality**. Extract it and run `Start-DesktopLife.cmd`. One application package includes the runtime; no separate .NET installation is required. GitHub's Source code archives are for developers.

## 更新内容

- **海洋场景：** 小绿龟与 12 种鱼，支持写实／可爱风格。森林与海洋共 26 种生物，场景分别保存数量和尺寸。
- **小绿龟互动：** 与苍蝇共用跟随速度及移动逻辑；点击后到达落点再缩壳停留 3 秒，随后恢复跟随。
- **动作改进：** 鱼头保持朝向实际游动方向，改进蜘蛛逃跑时的丝线与丝网表现。
- **统一设置：** 主程序分为「生物／屏保／偏好」；屏保配置使用相同卡片、紧凑表格和底部保存按钮，支持中英文与窗口缩放。
- **屏保集成：** 主程序可预览、配置屏保并准备固定路径的 `.scr` 副本；应用 Windows 等待时间后，系统自动触发。

## What's new

- Forest / Ocean scenes with independent populations; one green turtle and 12 fish species, with realistic and cute styles.
- Cursor-following turtle travels to a clicked spot, withdraws into its shell for three seconds, then resumes following.
- Fish face their swimming direction; spider escape silk and webs look more natural.
- Redesigned Creatures / Screen saver / Preferences pages and matching bilingual screen saver configuration.
- The main app now previews, configures and prepares a native Windows screen saver copy.

## 升级与自动屏保 / Upgrade and screen saver setup

先从托盘退出旧版，再运行新包，现有个人配置可保留。进入「屏保」→「打开 Windows 屏保设置」，选择 DesktopLife、设置等待时间并点击“应用”。升级后重复此步骤以更新系统使用的屏保副本；只启动主程序或预览不会启用自动屏保。

Exit the old desktop app from its tray menu before upgrading. Preferences are retained. Open the main app's Screen saver page, open Windows settings, select DesktopLife and **Apply** an idle timeout. Repeat this after upgrading to use the new screen saver copy. Windows can start it even when the desktop app is closed.

屏保已集成在主程序，不再单独提供屏保包。桌面和屏保配置独立，真实鼠标或键盘输入会退出屏保。

## 验证与限制

通过 227 项自动化测试，以及双语、窄窗口、配置保存与输入校验检查。发布包包含版本和源码提交信息；上传时核对文件大小与 SHA256。本版本为正式版；实际混合 DPI、不同 GPU 和高数量性能仍需更多设备验证。

[中文说明](https://github.com/JonGates/DesktopLife/blob/main/README.zh-CN.md) · [English README](https://github.com/JonGates/DesktopLife#readme) · [反馈 / Issues](https://github.com/JonGates/DesktopLife/issues)
