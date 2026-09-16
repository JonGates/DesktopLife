# DesktopLife · Windows 昆虫屏保与桌面宠物

[English](README.md) · **简体中文** · [下载便携版 / Download](https://github.com/JonGates/DesktopLife/releases) · [反馈问题 / Report a bug](https://github.com/JonGates/DesktopLife/issues)

**让 Windows 桌面变成一个会动的小生态系统。**

苍蝇追随鼠标，蟑螂受惊四散，蚂蚁能跨屏爬行；电脑闲置时，同一群昆虫还可以变成原生 Windows 屏幕保护程序。

[**下载桌面宠物 v0.4.1**](https://github.com/JonGates/DesktopLife/releases/download/v0.4.1/DesktopLife-Portable-win-x64-v0.4.1.zip) · [**下载屏保版 v0.4.1**](https://github.com/JonGates/DesktopLife/releases/download/v0.4.1/DesktopLife-ScreenSaver-win-x64-v0.4.1.zip) · [全部版本](https://github.com/JonGates/DesktopLife/releases)

Windows 10/11 x64 · 12 种昆虫 · 写实/可爱风格 · 多显示器 · 自带运行环境

![DesktopLife Windows 桌面宠物实录：昆虫在桌面自由活动](docs/images/live-desktop.gif)

> 当前 v0.4.1 为预览版。如果它让你的桌面更有生命力——或者变得更糟了一点——欢迎点亮 ⭐，并在 [Issues](https://github.com/JonGates/DesktopLife/issues) 告诉我们下一种该加入什么生物。

## 共 12 种昆虫

新增 **瓢虫、步甲、蠼螋、衣鱼、蟋蟀、蚱蜢、螳螂、竹节虫**。设置中的“昆虫配置”可分别设置数量与最小／最大尺寸；新增种类默认关闭，尺寸默认 80%～120%。桌面和屏保均支持。

每种使用独立身体纹理和对应的足部、触角或尾部结构，体型从小瓢虫到细长竹节虫有明显差异。大小是参考常见体型关系的视觉比例，并非屏幕上的真实毫米。全部支持写实与可爱外观；蟋蟀和蚱蜢支持跳跃，瓢虫支持起飞与降落。暂不模拟捕食或蚱蜢飞行。[物种与比例说明](docs/INSECTS.md)。

![12 种昆虫的三倍细节与原尺寸，WPF 渲染示意而非录像](docs/images/insect-catalog.png)

## v0.4.1：可爱风格、跳跃与飞行

v0.4.1 已补齐新增 8 种的可爱外观，桌面和屏保均可切换。切换保留数量、尺寸与位置。蟋蟀和蚱蜢现在会蓄力跳跃、收腿腾空与缓冲落地；瓢虫会展翅起飞、飞行、降落收翅。鼠标靠近可触发有冷却间隔的跳跃，暂停会冻结全部动作。[动作说明](docs/LOCOMOTION.md)。桌面与屏保下载包均已包含这些更新。

![12 种可爱昆虫，WPF 渲染示意而非录像](docs/images/cute-insect-catalog.png)

## 写实昆虫：身体纹理与足部动作

桌面设置中的“昆虫风格”可立即切换外观，保留数量和位置。屏保设置也支持独立选择。苍蝇落地前减速、分段搓足；爬行昆虫平滑转向，步态随实际移动推进。

写实模式使用 AI 生成的微距风格身体纹理，配合程序驱动的足部与触角；素材不是实拍照片。蟑螂、蚂蚁与毛毛虫已更新纹理，蚂蚁会偶尔短暂停步，遇到鼠标时继续躲避。

![深浅背景下的写实昆虫，分别显示三倍细节和原尺寸，WPF 渲染示意非录像](docs/images/realistic-insects.png)

## 数量、尺寸与设置界面

- 每个物种可设置最小／最大尺寸百分比（10%～300%）；默认蟑螂 60%～180%、蚂蚁 60%～120%、毛毛虫 60%～140%。
- 保存尺寸立即生效，保留位置和数量；屏保独立保存。苍蝇尺寸不变。
- 更紧凑的灰绿色设置页；屏幕信息、快捷键和录屏选项默认折叠，底部固定操作按钮。

## 选择并下载

| 你想要的效果 | 下载 Windows x64 便携包 | 如何启动 |
| --- | --- | --- |
| 电脑闲置时自动显示昆虫，深色／浅色背景 | **[屏保版 v0.4.1](https://github.com/JonGates/DesktopLife/releases/download/v0.4.1/DesktopLife-ScreenSaver-win-x64-v0.4.1.zip)** | 解压后运行 `Install-ScreenSaver.cmd`，在 Windows 中设置等待时间。 |
| 工作时让昆虫在桌面活动，苍蝇跟随鼠标 | **[桌面宠物版 v0.4.1](https://github.com/JonGates/DesktopLife/releases/download/v0.4.1/DesktopLife-Portable-win-x64-v0.4.1.zip)** | 解压后运行 `Start-DesktopLife.cmd`。 |

DesktopLife v0.4.1 为预览版，同一版本提供桌面应用与屏保两个子程序；均自带 .NET 运行环境，分别保存配置。[屏保使用说明](docs/SCREENSAVER.md) · [全部版本与校验文件](https://github.com/JonGates/DesktopLife/releases)


## 下载与使用（无需开发环境）

支持 **Windows 10/11 x64**。便携包自带运行环境，无需安装 .NET、SDK 或 Visual Studio。

**[下载 v0.4.1 便携包 ZIP](https://github.com/JonGates/DesktopLife/releases/download/v0.4.1/DesktopLife-Portable-win-x64-v0.4.1.zip)** · [查看发布页](https://github.com/JonGates/DesktopLife/releases/tag/v0.4.1) · [所有版本](https://github.com/JonGates/DesktopLife/releases)

桌面宠物版 v0.4.1 为预览版（Pre-release）。

### 屏保版 v0.4.1

**[下载屏保便携包](https://github.com/JonGates/DesktopLife/releases/download/v0.4.1/DesktopLife-ScreenSaver-win-x64-v0.4.1.zip)** · [屏保发布页](https://github.com/JonGates/DesktopLife/releases/tag/v0.4.1)

完整解压后，运行 `Configure-ScreenSaver.cmd` 选择深色／浅色和昆虫数量；`Preview-FullScreen.cmd` 体验全屏效果；`Install-ScreenSaver.cmd` 打开 Windows 屏保设置，选择等待时间并应用。移动鼠标或按键退出屏保。屏保包也自带运行环境，安装后需保留解压文件夹。[详细步骤](docs/SCREENSAVER.md)。

### 桌面宠物版：1. 在 GitHub 下载运行包

如果你打开的是 **Tags** 页面，请切换到旁边的 **Releases**，找到对应版本，展开 **Assets**，下载 `DesktopLife-Portable-win-x64-v0.4.1.zip`。

| 下载项 | 用途 |
| --- | --- |
| `DesktopLife-Portable-win-x64-v0.4.1.zip` | 桌面宠物模式，包含程序和运行环境。 |
| `DesktopLife-ScreenSaver-win-x64-v0.4.1.zip` | Windows 屏保模式，包含 `.scr`、配置与安装入口和运行环境。 |
| [`SHA256SUMS.txt`](https://github.com/JonGates/DesktopLife/releases/download/v0.4.1/SHA256SUMS.txt) | 两个 ZIP 共用的 SHA256 清单，每个文件各有一行校验值，无需运行。 |
| Tags 页面里的 `zip` / `tar.gz`，或 Releases 的 `Source code` | 项目源码，供开发者使用，不能直接当作便携程序运行。 |

### 桌面宠物版：2. 解压并启动

1. 将下载的 ZIP **完整解压**到一个文件夹，再打开解压后的目录。
2. 双击 **`Start-DesktopLife.cmd`**，启动程序并打开设置窗口。
3. 也可以直接双击 **`DesktopLife.exe`** 启动；双击右下角系统托盘中的 DesktopLife 图标打开设置（图标可能收在 `^` 隐藏图标区域）。

### 桌面宠物版：3. 开始使用

- **调整数量**：修改昆虫数量（统一在“昆虫配置”中），点击“保存数量与尺寸”；设为 0 可关闭该物种。所有屏幕共享这些数量，苍蝇固定 1 只。
- **与苍蝇互动**：移动鼠标让它跟随；单击左键指定落点，实际停落 3 秒后继续飞行。
- **暂停／恢复**：默认 `Ctrl+Alt+P` 暂停，`Ctrl+Alt+S` 恢复；也可在设置中操作或修改快捷键。快捷键在程序运行时有效。
- **语言**：设置窗口右上角切换简体中文或 English。
- **多屏**：自动识别 Windows 显示排列；相接的屏幕边缘可连续通行，无需按屏幕分别设置数量。
- **退出**：关闭设置窗口后，昆虫仍会运行。右键托盘图标，选择“退出”才能结束程序。

## 常见问题：Windows 屏保与桌面宠物

### 怎样在 Windows 11 或 Windows 10 设置自动屏保？

下载屏保版并完整解压，运行 `Install-ScreenSaver.cmd` 打开系统“屏幕保护程序设置”，选择 DesktopLife、设置等待分钟数并应用。`Configure-ScreenSaver.cmd` 可切换深色／浅色背景及昆虫数量；安装后请保留文件夹位置。

### 双屏、竖屏或上下排列可以使用吗？

两种模式都读取 Windows 显示排列，并共享全桌面昆虫总数。相接的屏幕边缘可以通行；屏保模式遇到显示排列变化时退出本次运行，下次启动读取新排列。

### 屏保和桌面宠物的鼠标行为有什么区别？

桌面模式中，苍蝇跟随鼠标，点击后停落 3 秒。屏保模式中，苍蝇自动游走和停落，移动鼠标或按键会退出屏保。等待时间和恢复时登录选项由 Windows 管理。

## 主屏实录（桌面宠物模式）

顶部为主屏全景，下面为主屏局部特写。均录自正在运行的 DesktopLife；使用静态背景突出昆虫，保留实际运动速度。录制时使用的昆虫数量高于默认值。

### 苍蝇跟随鼠标，点击后停落 3 秒

![主屏局部实录：苍蝇点击停落后恢复飞行](docs/images/live-fly.gif)

### 设置窗口与中英文切换

保存全桌面昆虫数量，实时切换语言，向下滚动可设置启动／恢复与暂停快捷键。

![主屏实录：昆虫在设置窗口上活动，中英文界面切换](docs/images/live-settings.gif)

[录制与 GIF 导出方法](docs/images/README.md)。

## 苍蝇互动

- 苍蝇持续跟随鼠标，在附近短促窜飞、改变方向和短暂停悬，鼠标静止时也不会离开。
- **单击鼠标左键**指定落点。苍蝇先飞过去，实际落下后静止 **3 秒**，再恢复围绕当前鼠标飞行。
- 停落期间移动鼠标不会带走苍蝇；再次点击可指定新落点，同一点再次点击会重新计时。
- 使用写实透明素材，尺寸比上一版缩小 40%。飞行时翅膀呈半透明振翅模糊，停落时前足交替搓动；身体锚点保持固定。下图为程序实际尺寸下的两种姿态（左停落、右飞行）。
- 点击通过只读系统钩子观察，不消费或阻止原来的点击；本程序设置窗口内的操作不会指定落点。暂停期间的点击不会在恢复后补执行。

![写实苍蝇姿态](docs/images/fly-poses.png)

素材由内置 imagegen 生成；完整提示词与透明度说明见 [素材说明](src/DesktopLife.Rendering/Assets/README.md)。

## 数量控制与跨屏

- 双击 `Run-DesktopLife.cmd` 打开控制窗口，或双击托盘图标 / 右键选择“数量设置”。程序已运行时再次执行启动脚本，会打开已有实例的设置。
- 苍蝇固定 **1 只**，无需数量设置。蟑螂和蚂蚁 **0–500**、毛毛虫 **0–100**，输入或拖动滑块，点击 **保存数量**。爬行昆虫设为 0 可关闭。旧配置中的蟑螂数量保留，旧苍蝇数量忽略。
- 所有屏幕共享同一批生物。增加数量只补充差额，减少数量保留其余个体；屏幕插拔不改变总数。
- 自动读取 Windows 显示设置中的左右、上下、负坐标与错位排列。蟑螂、蚂蚁和毛毛虫均可从相接的边缘连续爬到邻屏；苍蝇跟随全桌面鼠标。
- 错位屏幕只在实际相接的边缘段通行；没有屏幕的空隙和仅角点接触处不作为爬行通道。若两屏不相接，可在 Windows 显示设置中调整排列。
- 屏幕移除或移动后，仍在有效区域的生物保留状态；失去屏幕的生物移回最近的有效位置，保留身份与总数。
- 关闭设置窗口后继续在托盘运行；“暂停全部 / 恢复全部”控制所有屏幕，暂停中修改数量和接入屏幕仍保持暂停。

## 语言与快捷键

- 右上角切换 **简体中文 / English**，窗口、提示、托盘立即更新，选择自动保存。
- 向下滚动到“全局快捷键”，点击输入框并按组合键，点击 **保存快捷键**。Backspace 清空后保存可禁用对应操作。
- 支持 Ctrl、Alt、Shift 搭配字母、数字或 F1–F11；重复组合和被占用的组合会提示错误，并保留原设置。
- 启动／恢复只恢复运行，暂停只暂停；重复按同一操作不会反向切换。关闭设置窗口后快捷键仍有效。
- 快捷键只在 DesktopLife 进程运行期间有效；退出程序后请用启动脚本或 exe 启动。
- 语言与快捷键保存在 `%AppData%\DesktopLife\settings.preferences.json`，与原数量配置分开保存。
- 使用 Windows [RegisterHotKey](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey) 注册组合键，禁用或退出时释放。

完整步骤见 [开发与打包指南](docs/DEVELOPMENT_AND_PACKAGING.md)，包含单文件 EXE、便携 ZIP、环境准备与发布验证。

## 图标与录屏显示

EXE、设置窗口、任务栏及托盘使用统一昆虫图标。资源位于 `src/DesktopLife.App/Assets`，可通过 `scripts/Generate-Icon.ps1` 重建 16–256 像素 ICO。

设置底部可启用“从录屏中隐藏昆虫和设置窗口”，默认关闭，切换后立即保存。程序在自己的屏幕上仍然可见，新增屏幕的昆虫窗口也继承该选项。

此功能调用 Windows `WDA_EXCLUDEFROMCAPTURE`，要求 Windows 10 2004 或更新版本。只对支持该机制的截图、录屏和共享工具有效，不能保证所有监控软件无法录制，也不会隐藏任务栏/托盘图标或进程。Windows 拒绝应用时会提示失败。具体限制见 [Microsoft 文档](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity)。

## 打包分享（开发者）

直接分享已发布的程序，可将 [Releases 下载页](https://github.com/JonGates/DesktopLife/releases) 发给朋友。以下步骤用于从源码重新打包。

运行下面的命令生成 Windows x64 便携 ZIP：

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Package-Portable.ps1 -Version 0.4.1
```

输出为 `artifacts/DesktopLife-Portable-win-x64-v0.4.1.zip`，附带 SHA256 校验文件。将 ZIP 发给朋友，完整解压后双击 `Start-DesktopLife.cmd` 即可打开设置，无需安装 .NET 或开发工具。运行环境的版本由脚本参数 `-RuntimeVersion` 指定。

便携版支持 Windows 10/11 x64，包含中文/英文、全局快捷键和全部现有昆虫。用户配置仍保存在 `%AppData%\DesktopLife`，不会把开发者的个人配置打包分享。程序退出时释放快捷键；首次启动可能需要片刻解包运行环境。

## 从本地构建运行（开发者）

双击根目录 **`Run-DesktopLife.cmd`**，或者 **`artifacts/publish/DesktopLife.exe`**。

- `artifacts/publish/` 是本地生成的依赖运行环境版本，需要本机安装 **.NET 10 Desktop Runtime x64**。Releases 中的便携 ZIP 自带运行环境。
- 请保留 `artifacts/publish/` 内所有文件；不要只复制 exe。
- 启动后稍等，苍蝇会从桌面的外露边缘进入并围绕鼠标。
- 启动后几秒内，蟑螂从屏幕四周陆续爬出。鼠标靠近时会四散，逃到边缘后可藏起来，安全时再出现。
- 右下角系统托盘（可能在折叠区域中）找到 **DesktopLife**，菜单显示屏幕数和总生物数；**暂停 / 恢复 / 退出**同时作用于所有屏幕。
- 只允许运行一个实例；重复打开不会再生成一只苍蝇。
- Release 没有 HUD。Debug 右上角显示输入状态、更新频率、逻辑更新时间、Fly 状态与生物数量。
- 全局快捷键默认：`Ctrl+Alt+S` 启动／恢复，`Ctrl+Alt+P` 暂停，可在设置窗口修改。

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

# 实际设置控件、配置保存/错误、全局数量、零数量、暂停和关闭/重开
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --controls artifacts/controls-probe

# 同一生物跨两个视口，在 100% / 125% / 150% 下拼合一致
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --seams artifacts/seam-probe

# 实际主循环的点击落点、3 秒停留、暂停恢复与跨屏降落；原生鼠标钩子注册/释放
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --fly-landing artifacts/fly-landing-probe

# 写实素材飞行/停落两种姿态、透明背景和物理坐标
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --fly-art artifacts/fly-art-probe

# 语言持久化、实际快捷键注册/冲突/回滚、消息分发与中英文窗口
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --preferences artifacts/preferences-probe

# Release 发布：默认使用已安装的 Desktop Runtime
powershell -ExecutionPolicy Bypass -File scripts/Publish.ps1

# 可选：自带运行时单文件，需下载 Microsoft 官方运行时 NuGet 包
powershell -ExecutionPolicy Bypass -File scripts/Publish.ps1 -SelfContained
```

脚本的 ExecutionPolicy 参数只影响本次 PowerShell 进程，不修改系统策略。

## 架构

| 项目 | 职责 |
| --- | --- |
| `DesktopLife.Engine` | DesktopLayout 的屏幕并集与外露边界、MouseTracker、GameLoop、CreatureManager、SimulationWorld；不引用 WPF / Win32 |
| `DesktopLife.Creatures` | Fly 与 Cockroach 状态机、蚂蚁爬行与毛毛虫缓行、全局群体差额调整、共享世界与布局同步、邻近避让；仅依赖 Engine |
| `DesktopLife.Windows` | Win32 鼠标采样、只读全局左键观察、显示器枚举与物理矩形、窗口扩展样式 |
| `DesktopLife.Rendering` | 按物种与姿态批量绘制；苍蝇使用嵌入的透明写实图集，爬行昆虫使用独立身体纹理和程序驱动的足部、触角或尾部；素材缓存并 Freeze |
| `DesktopLife.App` | DesktopHost 统一管理每屏视口、单一主循环及鼠标采样、数量控制窗口、JSON 配置、托盘、Debug HUD、异常日志 |

模拟坐标为屏幕物理像素，Renderer 显式减去显示器原点，再除以当前 WPF DPI 比例。Manifest 使用 PerMonitorV2。

Overlay 在显示前配置 `WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW`，去除 `WS_EX_APPWINDOW`；同时使用 `ShowActivated=false`、`SWP_NOACTIVATE` 与消息处理防止激活。穿透依赖原生 layered window 样式，不仅是 WPF 命中开关。技术依据：[Microsoft Window Features](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features)。

苍蝇默认 `Offscreen → Approach → Orbit`，快速甩动鼠标会短暂 `Panic`；点击进入 `Landing → Landed → Approach`。停落 3 秒从到达落点后开始，使用真实帧间隔；仅在鼠标不属于实际桌面区域时离开。运动步长限制为 50ms。暂停取消更新并清空待处理点击，恢复时重新建立鼠标采样基线。

蟑螂使用 `Hidden → Emerge → Crawl → Panic → Flee` 状态机；到边缘后 Hidden，安全后可恢复 Crawl。个体速度、游走转向、反应时长和逃跑偏角使用可固定种子的随机源。CreatureManager 将只读邻居集合交给行为层，24px 范围内的可见蟑螂互相避让。没有每只生物的定时器或控件。

## 范围与限制

- 支持 Windows 10/11 x64 多显示器。写实模式使用生成的身体纹理，配合程序驱动的足部、触角与翅膀；素材是插画资源，并非实拍照片。
- 已通过 Debug / Release 构建、自动化测试，以及真实控制窗口、双屏窗口、跨屏渲染、点击降落与暂停/恢复检查。当前结果见 [v0.4.1 发布说明](https://github.com/JonGates/DesktopLife/releases/tag/v0.4.1)；旧版本验证记录作为历史保留。
- 尚未完成稳定 60 FPS 和 500 只群体的性能验收；数量上限是输入约束，不代表任何设备均能流畅运行上限数量。
- 125% / 150% 已通过离屏渲染检查，插拔与拓扑变化已通过模拟布局的真实窗口检查；实际混合系统缩放、物理拔插、浏览器点击体验和不同 GPU 尚未完整验收。
- 暂未提供开机自启和传统安装器；桌面宠物与屏保均以便携 ZIP 提供。
- 数量配置保存在 `%AppData%\DesktopLife\settings.json`；每次保存立即应用，下次启动自动读取。文件损坏或数量无效时使用默认值，并在设置窗口提示；保存失败时保留原数量。
- 异常日志：`%LocalAppData%\DesktopLife\logs\yyyy-MM-dd.log`。
- v0.4.1 的桌面宠物与屏保便携包均自带 .NET 运行环境，无需另外安装 .NET、SDK 或 Visual Studio。

完整产品规格：`docs/DesktopLife_Codex_Development_Spec.md`；当前计划：`docs/superpowers/plans/2026-09-15-shared-desktop-controls.md`。本次用户要求已取代此前“每屏固定 1/20”的规则。
