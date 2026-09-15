# DesktopLife

Windows 10/11 桌面生物原型，C# / .NET 10 / WPF / Win32。

当前交付：**苍蝇 + 蟑螂，多屏支持**。每个扩展桌面屏幕独立生成默认 1 只苍蝇、20 只蟑螂，数量随屏幕数自动变化。

| 屏幕数 | 苍蝇 | 蟑螂 |
| --- | --- | --- |
| 1 | 1 | 20 |
| 2 | 2 | 40 |
| 3 | 3 | 60 |

按 Windows 提供的逻辑显示器计数；复制显示模式共享同一桌面。鼠标只触发所在屏幕的生物。支持负坐标、上下排列、不同分辨率；插拔和布局变化时同步窗口和数量，未变化的屏幕保留生物状态，位置或尺寸改变的屏幕重新生成。暂停期间接入新屏仍保持暂停。

## 直接运行

双击根目录 **`Run-DesktopLife.cmd`**，或者 **`artifacts/publish/DesktopLife.exe`**。

- 当前发布包使用本机已有的 **.NET 10 Desktop Runtime x64**，无需重新安装开发环境。
- 请保留 `artifacts/publish/` 内所有文件；不要只复制 exe。
- 移动鼠标后稍等，苍蝇会从屏幕左侧进入；静止后会飞出屏幕。
- 启动后几秒内，蟑螂从屏幕四周陆续爬出。鼠标靠近时会四散，逃到边缘后可藏起来，安全时再出现。
- 右下角系统托盘（可能在折叠区域中）找到 **DesktopLife**，菜单显示屏幕数和总生物数；**暂停 / 恢复 / 退出**同时作用于所有屏幕。
- 只允许运行一个实例；重复打开不会再生成一只苍蝇。
- Release 没有 HUD。Debug 右上角显示输入状态、更新频率、逻辑更新时间、Fly 状态与生物数量。
- 目前没有全局快捷键；`Ctrl + Alt + D` 随后续产品化阶段实现。

## 开发与验证

需要 .NET 10 SDK（本次环境为 10.0.204；`global.json` 固定 SDK 补丁系列）。

源码仓库：[JonGates/DesktopLife](https://github.com/JonGates/DesktopLife)。首次获取源码：

```powershell
git clone https://github.com/JonGates/DesktopLife.git
cd DesktopLife
```

仓库包含源码、测试、脚本和开发文档；`artifacts/`、`bin/`、`obj/` 是本地生成内容，不纳入版本管理。首次克隆后可直接运行根目录启动脚本（需要 SDK），或执行下面的发布脚本生成 `artifacts/publish/DesktopLife.exe`。

```powershell
dotnet build DesktopLife.sln
dotnet test DesktopLife.sln
dotnet run --project src/DesktopLife.App

# 原生窗口样式、焦点与命中测试；先退出已运行的 DesktopLife
powershell -ExecutionPolicy Bypass -File scripts/Test-Overlay.ps1

# 实际 WPF 离屏渲染：100% / 125% / 150%，校验物理坐标与透明度
dotnet run --project tools/DesktopLife.Diagnostics -- artifacts/render-check

# 使用真实 App 生命周期，验证群体可见、暂停停止更新、恢复和退出；先退出现有实例
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --live artifacts/live-probe

# 使用生产窗口管理器与模拟屏幕布局，验证插拔、负坐标、暂停中接入、分辨率改变
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --displays artifacts/display-probe

# Release 发布：默认使用已安装的 Desktop Runtime
powershell -ExecutionPolicy Bypass -File scripts/Publish.ps1

# 可选：自带运行时单文件，需下载 Microsoft 官方运行时 NuGet 包
powershell -ExecutionPolicy Bypass -File scripts/Publish.ps1 -SelfContained
```

脚本的 ExecutionPolicy 参数只影响本次 PowerShell 进程，不修改系统策略。

## 架构

| 项目 | 职责 |
| --- | --- |
| `DesktopLife.Engine` | MouseTracker、GameLoop、WorldBounds、固定种子随机源、CreatureManager、SimulationWorld；不引用 WPF / Win32 |
| `DesktopLife.Creatures` | Fly 与 Cockroach 状态机、每屏群体创建与布局同步、邻近避让；仅依赖 Engine |
| `DesktopLife.Windows` | Win32 鼠标采样、显示器枚举与物理矩形、窗口扩展样式 |
| `DesktopLife.Rendering` | 根据 CreatureKind 在一个 DrawingContext 中批量绘制；两种生物各两帧矢量素材在启动时缓存并 Freeze |
| `DesktopLife.App` | DesktopHost 统一管理每屏 Overlay、单一 CompositionTarget.Rendering 主循环及鼠标采样、托盘、Debug HUD、异常日志 |

模拟坐标为屏幕物理像素，Renderer 显式减去显示器原点，再除以当前 WPF DPI 比例。Manifest 使用 PerMonitorV2。

Overlay 在显示前配置 `WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW`，去除 `WS_EX_APPWINDOW`；同时使用 `ShowActivated=false`、`SWP_NOACTIVATE` 与消息处理防止激活。穿透依赖原生 layered window 样式，不仅是 WPF 命中开关。技术依据：[Microsoft Window Features](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features)。

状态转换：`Offscreen → Approach → Orbit → Panic → Approach`；静止超过阈值时转为 `Depart → Offscreen`，恢复移动可打断离开。逻辑步长限制为 50ms；鼠标速度与空闲时间使用真实采样间隔。暂停时取消渲染事件订阅，恢复时重新建立鼠标采样基线。

蟑螂使用 `Hidden → Emerge → Crawl → Panic → Flee` 状态机；到边缘后 Hidden，安全后可恢复 Crawl。个体速度、游走转向、反应时长和逃跑偏角使用可固定种子的随机源。CreatureManager 将只读邻居集合交给行为层，24px 范围内的可见蟑螂互相避让。没有每只生物的定时器或控件。

## 范围与限制

- 支持 Windows x64 多显示器，素材为程序绘制的占位图形。
- 已通过 Debug / Release 构建和 56 项 xUnit 测试；本机双屏各 1920×1080、100% 缩放的窗口尺寸、穿透、不抢焦点和全局暂停/恢复检查通过。详见 `docs/MULTI_DISPLAY_VERIFICATION.md`；之前的里程碑记录保留在 `docs/MILESTONE1_VERIFICATION.md` 和 `docs/MILESTONE2_VERIFICATION.md`。
- 本次双屏群体运行采样约 27–32 次更新/秒，未达到稳定 60 FPS。后续仍需性能优化与长时间验收。
- 125% / 150% 已通过离屏渲染检查，插拔与拓扑变化已通过模拟布局的真实窗口检查；实际混合系统缩放、物理拔插、浏览器点击体验和不同 GPU 尚未完整验收。
- 暂无蚂蚁、密度菜单、物种开关、设置保存、开机自启、安装器。下一阶段是 Ant / Pheromone。
- 此里程碑没有 `settings.json`。后续配置路径预定为 `%AppData%\DesktopLife\settings.json`。
- 异常日志：`%LocalAppData%\DesktopLife\logs\yyyy-MM-dd.log`。
- 自带运行时的发布在本次网络环境下载中断；本次实际交付的是轻量依赖运行时版本。

完整产品规格：`docs/DesktopLife_Codex_Development_Spec.md`；多屏计划：`docs/superpowers/plans/2026-09-15-multiple-displays.md`。
