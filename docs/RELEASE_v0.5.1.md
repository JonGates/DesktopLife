# DesktopLife v0.5.1 · Rain Window / 雨窗

Windows 10/11 x64 · 正式版 / Stable · 自带 .NET 运行环境

## 下载 / Download

- **DesktopLife-Portable-win-x64-v0.5.1.zip**：主程序便携包，包含桌面模式及屏保功能。解压后运行 `Start-DesktopLife.cmd` 或 `DesktopLife.exe`。
- **SHA256SUMS.txt**：ZIP 的 SHA256 校验值。

One portable application includes desktop scenes and the integrated screen saver. No separate .NET installation is needed. GitHub's Source code archives are for developers.

## 更新内容

- 新增「雨窗」场景，支持鼠标碰触滑落、沿途吞并和点击玻璃裂纹；裂纹约 3 秒消失。
- 五档雨量：毛毛雨、小雨、中雨、大雨、暴雨。桌面和屏保分别保存；总雨滴上限为 100 / 220 / 600 / 900 / 1,200。
- 静止水滴使用多种稳定轮廓；滑落采用圆润水团、伸缩水颈和渐淡水痕。大滴留下较宽水痕，修复水痕接缝黑圈。
- 限制吞并后的水滴尺寸，小水滴滑落更慢，持续滑落不再短暂停顿。
- 屏保支持背景图片、预先柔焦和水滴内的近似弯曲倒像折射。
- 修复设置标签边缘样式，保留森林、海洋、双语及快捷键设置。

## What's new

Rain Window adds five intensity levels, pointer-released drops, merging, temporary glass fractures, varied bead silhouettes and size-dependent sliding speeds. Wet trails scale with drop size and fade without dark circular seams. Screen saver images support soft focus and approximate curved refraction.

## 升级 / Upgrade

先从托盘退出旧版，再解压启动新包；现有个人配置保留。升级后进入「屏保 → 打开 Windows 屏保设置」，更新系统使用的屏保副本，并应用等待时间。桌面和屏保配置独立；真实键鼠输入会退出屏保。

Exit the previous app before launching this version. Settings are retained. Reopen Windows screen saver settings through the main app after upgrading to refresh the installed copy, then apply your idle timeout.

## 验证与限制 / Validation and limits

通过 245 项自动化测试、雨窗渲染、白底水痕接缝检查及设置界面检查。雨窗为简化视觉模拟，桌面透明模式不抓取背景；图片折射仅用于屏保。多屏、高分辨率与暴雨档的实际性能取决于设备。

Rain is a simplified visual simulation. Desktop overlays do not capture or refract the desktop; image refraction is available in the screen saver. Performance varies by resolution, display count and hardware.
