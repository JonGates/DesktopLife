# DesktopLife v0.6.0 · Five Languages / 五种语言

Windows 10/11 x64 · 正式版 / Stable · 自带 .NET 运行环境

## 下载 / Download

- **DesktopLife-Portable-win-x64-v0.6.0.zip**：主程序便携包，包含桌面生物、雨窗和屏保功能。解压后运行 `Start-DesktopLife.cmd` 或 `DesktopLife.exe`。
- **SHA256SUMS.txt**：便携 ZIP 的校验值。

One portable application includes all desktop scenes and the integrated screen saver. The .NET runtime is included. GitHub's Source code archives are for developers.

## 更新内容

- 新增 **繁體中文、日本語、한국어**，连同简体中文和 English 共五种界面语言。
- 覆盖主程序、托盘菜单及数量摘要、屏保配置、生物名称、雨量选项、帮助与校验提示。
- 主程序语言即时生效；屏保使用自己的语言设置。首次配置屏保可继承桌面语言。
- 切换语言保留尚未保存的输入；错误信息中的数量范围会随语言一起更新。
- 文案内置于便携程序，不需要联网或安装系统语言包。现有中英文配置继续兼容。
- 新增共享翻译资源和维护文档，方便后续扩展语言。

## What's new

- Added Traditional Chinese, Japanese and Korean; five interface languages in total.
- Localized desktop and screen saver settings, tray menus, creature names, rain levels, help and validation messages.
- Independent language preferences for desktop and screen saver; unsaved inputs survive language switching.
- Bundled translations require no network connection or extra language packs. Existing settings remain compatible.

## 升级 / Upgrade

先从托盘退出旧版，再启动新包，原有个人配置保留。在设置右上角选择语言。屏保语言需点击保存后生效。

升级后进入「屏保 → 打开 Windows 屏保设置」更新系统使用的屏保副本，然后应用等待时间。

Exit the old app before launching the new package. Existing settings are retained. Choose a language at the top right; save screen saver settings to retain its language. Reopen Windows screen saver settings through the main app after upgrading to refresh the installed copy.

## 验证 / Validation

245 项自动化测试；五种语言的资源完整性、格式参数、语言保存与回退检查；主程序和屏保共 60 张不同语言与窗口尺寸的界面检查。保留所有森林、海洋、雨窗功能。

245 automated tests plus translation coverage, format arguments, persistence and fallback checks. UI checks cover five languages, two window sizes and 60 rendered views across desktop and screen saver settings.
