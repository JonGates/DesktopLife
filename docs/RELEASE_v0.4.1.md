# DesktopLife v0.4.1 — Jump, Fly & Cute / 跳跃、飞行与可爱外观

同一版本提供桌面宠物与 Windows 屏保两个子程序，均自带 .NET 运行环境。

## 更新

- **蟋蟀、蚱蜢会跳跃**：准备蓄力、蹬腿、空中收腿、伸腿落地与缓冲。鼠标靠近可提前触发，落地后有冷却间隔。
- **瓢虫会飞行**：先展翅再离地，鞘翅与膜翅分开绘制，飞行时拍翅，落地后收翅。
- **12 种昆虫均支持可爱外观**，桌面与屏保都能切换，保留数量、位置和尺寸。
- 蟑螂、蚂蚁、毛毛虫与新增昆虫统一为“数量 / 最小 % / 最大 %”配置表。
- 修正瓢虫过长的腿与触角、螳螂的捕捉足关节和步行足比例。
- 多屏相接边缘可通行；动作不会穿过屏幕布局中的空隙，暂停时冻结运动和拍翅。

已有配置保持兼容。新增种类默认关闭，在“昆虫配置”中把数量设为大于 0 即可体验。蚱蜢飞行、捕食与蜕皮尚未模拟。

## 下载

- `DesktopLife-Portable-win-x64-v0.4.1.zip`：完整解压后运行 `Start-DesktopLife.cmd`。
- `DesktopLife-ScreenSaver-win-x64-v0.4.1.zip`：运行 `Configure-ScreenSaver.cmd` 配置，`Install-ScreenSaver.cmd` 安装。
- `SHA256SUMS.txt`：两个 ZIP 的统一校验清单。

Windows 10/11 x64 预览版。更新桌面程序前从托盘退出旧版本。屏保包安装后需保留解压目录。

## Validation

174 automated tests pass. Both visual styles pass motion-stage and transparent rendering checks at 100%, 125% and 150% DPI. Screen saver preview parenting, resizing and cleanup are verified.

Crickets and grasshoppers now jump, and ladybugs take off, fly and land. All 12 insects support cute and realistic styles. This release also unifies population controls and corrects ladybug and mantis legs. Both self-contained ZIPs belong to the same version and share one checksum manifest.
