# MILESTONE 1 — Fly Prototype 验证记录

日期：2026-09-15。此文档区分已实现、自动验证和仍需人工验收的内容。

## 环境

- Windows 11 x64，系统版本 10.0.26200。
- .NET SDK 10.0.204；Microsoft.WindowsDesktop.App 10.0.8 已安装。
- 主显示器 1920×1080，DPI 96（100%）。未改变用户系统缩放设置。
- 原始目录仅有规格书；新增五个 src 项目、两个 xUnit 项目，以及独立离屏渲染诊断工具。

## 构建与测试证据

| 检查 | 结果 |
| --- | --- |
| `dotnet build DesktopLife.sln` | 成功，0 警告、0 错误 |
| `dotnet test DesktopLife.sln` | 27 通过，0 失败：Engine 17，Creatures 10 |
| Release build / test | 成功，27 通过 |
| `scripts/Publish.ps1` | 成功，输出 `artifacts/publish/DesktopLife.exe` 与配套文件 |
| 发布版原生启动检查 | 通过，程序窗口确实创建；检查完成后关闭测试进程 |
| WPF 离屏渲染诊断 | 100% / 125% / 150% 均通过，每种比例 338 个着色像素、物理中心 (200,200)、背景透明 |

测试按阶段推进。输入、模拟、Fly 首先使用未实现版本运行，分别观察到 3、8、5 项失败，然后实现并转绿。暂停恢复缺陷另有两项测试先失败后修复。生成模板测试已移除，没有计入最终 27 项。

### xUnit 覆盖

- 初次采样不产生虚假移动；3-4-5 位移 / 50ms = 100px/s。
- 静止时间按实际间隔增加、移动清零、亚像素抖动、非法时间间隔。
- 向量归一化、负屏幕原点、三个 DPI 比例的坐标变换、固定种子随机。
- GameLoop 最大步长 50ms、暂停时钟重置、管理器只读集合、每实体每帧一次更新。
- Offscreen→Approach、接近后 Orbit、静止后 Depart→Offscreen、返回不瞬移、近距离快速鼠标触发 Panic 并恢复。
- 鼠标移出主屏时不召回苍蝇。
- 两个相同种子的世界运行十分钟模拟（36,000 帧）状态与位置一致，位置有限、速度受限。
- 暂停期间移动后恢复：屏外和可见苍蝇均不产生虚假的速度尖峰或 Panic。

## 原生窗口证据

`scripts/Test-Overlay.ps1` 对实际应用 HWND 的检查结果：

```text
Styles       0x80800A8
NoActivation true
ClickThrough true
Bounds       0,0,1920,1080
Dpi          96
```

检查了 layered、transparent、noactivate、toolwindow、topmost 标志；排除了 appwindow；启动前后前台 HWND 一致。`WindowFromPoint` 命中下层窗口而非 Overlay。阶段 1 使用了不透明测试圆点；最终 Debug 版本还检查 HUD 区域。原生样式使用方式符合 [Microsoft layered window 文档](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features)。

## 运行采样与性能调整

实际启动 Debug 版本采样 30 秒，进程存活，私有内存约 89.9–92.7MiB。发现屏外等待时仍每帧重绘整个透明窗口，因此限制为：Release 屏外无变化时停止重绘，Debug 屏外 HUD 每秒最多重绘四次；输入仍由统一循环采样。

调整后的发布版实际运行 60 秒：

- 进程全程存活。
- 60 秒新增 CPU 时间约 0.625 秒，相当于平均约 1.04% 的一个 CPU 核心；不是整机 CPU 百分比，也不是活跃追随时的性能。
- 工作集约 116.8–126.4MiB；私有内存约 55.8–65.5MiB，启动后回落，再缓慢增加至 58.9MiB。
- 句柄从 398 降至 392。
- 采样期间未注入鼠标输入；这是第一里程碑当次记录。通用脚本的 `artifacts/runtime-release.json` 会被后续版本采样覆盖。

这只是短时采样，不能证明长期没有内存增长；持续数小时、不同 GPU 和活跃追随负载仍需验收。HUD 的 FPS 表示主循环更新频率，Update 表示模拟更新时间，不是 GPU 帧耗时。

## 代码复查

独立 reviewer 发现暂停恢复会把暂停期间的位移除以一帧时间，误判为快速鼠标运动。已添加 `MouseTracker.Reset()`，在恢复时重建采样基线，并通过两项回归测试。复查未报告其他重要正确性问题。

## 发布说明

本次交付为 **framework-dependent** Release，依赖本机已有 .NET 10 Desktop Runtime x64。整个发布目录约 0.3MiB，需保留配套 DLL / JSON。

自带运行时单文件发布尝试被官方 NuGet 包下载中断阻塞（ResponseEnded）。未将不完整下载作为交付物。可在网络稳定时执行 `scripts/Publish.ps1 -SelfContained`。

## 未完成的人工验收

- 浏览器真实鼠标点击体验：浏览器工具策略拒绝 `file://` 本地测试页；未尝试绕过。原生 HWND 检查通过，不将其描述为浏览器实际点击已验收。
- 在 Windows 设置中切换到 125% / 150% 后的完整桌面表现。已完成算术测试与 WPF 实际离屏渲染，但不等价于切换系统 DPI。
- 视觉自然程度、快速甩动体验、不同桌面背景可辨识度。
- 实际托盘暂停/恢复/退出交互以及长期稳定性；逻辑、事件解绑和退出代码已实现，暂停采样逻辑有回归测试。
- Windows 10 真机运行。

因此，本次交付为第一里程碑的实现和自动验证结果；完整手工验收尚未全部完成。蟑螂、蚂蚁、设置、快捷键和安装器属于后续里程碑。
