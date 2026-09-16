# DesktopLife v0.4.2 — Spiders & Longer Jumps / 蜘蛛与跳跃优化

One version, two self-contained Windows x64 programs: desktop pets and screen saver.
同一版本提供桌面宠物与 Windows 屏保两个子程序，均自带 .NET 运行环境。

## What's new / 更新

- **Spider / 蜘蛛**：新增八足爬行蜘蛛，写实与可爱外观，支持数量、尺寸范围、鼠标避让和跨屏移动。默认关闭，在“生物配置”中把蜘蛛数量设为大于 0 即可体验。
- **Longer jumps / 更远的跳跃**：100% 尺寸、无遮挡时，蟋蟀普通跳距 160–260 像素，蚱蜢 300–450 像素；受惊跳距增加 30%。跳距按起跳时的尺寸缩放，腾空高度提高，落点不受帧率影响。屏幕边界与空隙仍限制位移。
- **Ladybug wings / 瓢虫翅膀**：透明后翅改为窄根、圆润末端，拍翅时加入俯视宽度变化，红色鞘翅展开角度收小。
- 桌面与屏保同步支持以上变化，两种外观均适用；旧配置保持兼容。蜘蛛采用地栖爬行表现，暂不模拟结网、捕食或跳跃。

Spiders have eight articulated legs and realistic/cute appearances, with population and size controls. Cricket and grasshopper jumps now scale with body size, gain extra range when startled, and land consistently across frame rates. Ladybug wings have a narrower silhouette and a foreshortened stroke. Both programs include all changes; existing settings remain compatible.

## Downloads / 下载

- `DesktopLife-Portable-win-x64-v0.4.2.zip` — extract fully, then run `Start-DesktopLife.cmd` / 完整解压后运行。
- `DesktopLife-ScreenSaver-win-x64-v0.4.2.zip` — configure with `Configure-ScreenSaver.cmd`, install with `Install-ScreenSaver.cmd` / 配置、安装屏保。
- `SHA256SUMS.txt` — one checksum list for both ZIPs / 两个 ZIP 的统一校验清单。

Windows 10/11 x64. No separate .NET installation needed. Exit the old desktop app from its tray menu before launching the new version. Desktop and screen saver settings remain separate.

升级前先从托盘退出旧程序。桌面与屏保分别保存配置。GitHub 自动提供的 Source code ZIP / tar.gz 是源码，不是可直接运行的便携包。

## Validation / 验证

196 automated tests passed. Additional checks cover both visual styles, transparent rendering, 100%/125%/150% DPI motion poses, desktop/screen saver settings, and the packaged screen saver preview lifecycle.

This remains a preview release / 本版本仍为预览版。
