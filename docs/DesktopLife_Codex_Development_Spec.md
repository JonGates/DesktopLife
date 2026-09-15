# DesktopLife — Codex 开发规格书

> 项目代号：DesktopLife  
> 目标平台：Windows 10 / Windows 11  
> 语言：C#  
> Runtime：.NET 10  
> UI：WPF  
> 系统能力：Win32 API  
> 首版生物：苍蝇、蟑螂、蚂蚁  
> 文档目的：让 Codex 按阶段直接实现一个可运行、可测试、可打包的 Windows 桌面动态生物软件。

---

# 0. 给 Codex 的最高优先级指令

你正在开发一个新的 Windows 桌面应用 **DesktopLife**。

请先完整阅读本文件，然后严格按“阶段 0 → 阶段 1 → 阶段 2 ……”依次实现。

## 必须遵守

1. 使用 **C# + .NET 10 + WPF + Win32 API**。
2. 第一版只支持 Windows，不要为了跨平台引入 Avalonia、Electron、Tauri、Unity、Cocos。
3. 第一版不要引入数据库。
4. 第一版不要引入 ECS 框架、DI 框架、游戏引擎或大型第三方框架。
5. 第一版渲染不要“一只虫一个 WPF 控件”。
6. 所有生物必须由一个统一渲染面批量绘制。
7. 生物行为逻辑与 WPF/Win32/UI 必须解耦。
8. 鼠标必须可以正常点击 Overlay 下方的软件。
9. Overlay 不允许抢夺当前程序焦点。
10. 每完成一个阶段都必须：
    - `dotnet build`
    - `dotnet test`
    - 启动程序做最小冒烟测试
11. 不要一次完成所有功能后再测试。
12. 任何优化都不得牺牲代码结构。
13. 若美术素材尚未提供，使用程序绘制的占位图形或内置占位 PNG，不能因此阻塞功能开发。
14. 不要擅自修改本文确定的核心技术路线。
15. 热路径中禁止频繁 LINQ、每帧大规模对象创建、每帧加载图片。
16. 所有随机行为必须允许通过注入随机数源或固定 Seed 进行测试。
17. 第一版目标是“好玩、稳定、低占用”，不是追求复杂架构。

---

# 1. 产品目标

DesktopLife 是一个运行在 Windows 桌面最上层的透明动态生物系统。

它看起来像：

```text
┌─────────────────────────────────────────────┐
│  Windows 桌面 / 浏览器 / 微信 / IDE / 文件夹  │
│                                             │
│      🪳                 🪰                   │
│           🐜 🐜 🐜                          │
│                         🖱️                  │
│                 🪳                          │
└─────────────────────────────────────────────┘
             ↑
      透明 DesktopLife Overlay
```

Overlay 本身：

- 完全透明；
- 无标题栏；
- 无边框；
- 默认全屏；
- 永远置顶；
- 不在 Alt+Tab 中作为普通工作窗口出现；
- 不激活、不抢焦点；
- 鼠标点击穿透；
- 可以正常读取全局鼠标位置和运动状态；
- 用户仍能正常操作下面的浏览器、IDE、游戏等软件。

第一版重点不是“桌宠窗口”，而是让生物像真正生活在桌面上。

---

# 2. V0.1 核心体验

V0.1 只实现三种生物：

## 2.1 苍蝇 Fly

核心感觉：

> 鼠标一动，苍蝇飞回来追着鼠标；鼠标停一段时间，苍蝇自行飞走。

行为要求：

- 默认 1 只。
- 鼠标开始移动时：
  - 如果苍蝇在屏幕外，立即进入屏幕；
  - 朝鼠标附近飞；
  - 不允许直接锁死在鼠标中心。
- 跟随时：
  - 存在随机扰动；
  - 围绕鼠标 40~120 px 范围盘旋；
  - 偶尔快速掠过鼠标。
- 鼠标快速甩动时：
  - 苍蝇短暂进入 Panic；
  - 与鼠标拉开距离；
  - 随后重新靠近。
- 鼠标静止超过约 1.5 秒：
  - 苍蝇进入 Depart；
  - 朝随机屏幕边缘飞走；
  - 最终允许停留在屏幕外。
- 鼠标再次移动：
  - 苍蝇重新飞回来。

状态建议：

```text
Offscreen
   ↓ 鼠标活动
Approach
   ↓ 接近鼠标
Orbit / Follow
   ↓ 鼠标快速移动
Panic
   ↓ 恢复
Orbit / Follow
   ↓ 鼠标静止 > 1.5s
Depart
   ↓
Offscreen
```

---

## 2.2 蟑螂 Cockroach

核心感觉：

> 鼠标不动时蟑螂慢慢从屏幕边缘爬出来；鼠标靠近时瞬间四散逃跑。

默认数量：

- 20 只。

行为：

- 大部分从屏幕边缘生成。
- 平常沿随机方向爬动。
- 偏好屏幕边缘、角落。
- 鼠标长时间静止时，可以逐渐有更多蟑螂从边缘进入。
- 鼠标靠近时立即逃跑。
- 鼠标速度越快，受惊距离可以略微增加。
- 多只蟑螂受到惊吓时方向不能完全一致。
- 逃跑一段距离后：
  - 可躲入最近屏幕边缘；
  - 或恢复普通爬行。
- 不允许所有蟑螂一直堆积在一个像素点。
- 初版不做真实碰撞，可使用轻量 Separation 避免重叠。

状态建议：

```text
Hidden
  ↓
Emerge
  ↓
Crawl
  ↓ 鼠标进入恐惧范围
Panic
  ↓
Flee
  ↓ 到达安全区域/边缘
Hide 或 Crawl
```

---

## 2.3 蚂蚁 Ant

核心感觉：

> 鼠标移动会留下看不见的“信息素轨迹”，蚂蚁会逐渐发现并沿鼠标经过的路线移动。

默认数量：

- 50 只。

要求：

- 不使用 NavMesh。
- 不做每像素路径搜索。
- 使用低分辨率信息素场。
- 鼠标移动时在信息素场增加浓度。
- 信息素随时间衰减。
- 蚂蚁前进时采样：
  - 前方；
  - 左前；
  - 右前。
- 优先向浓度高的方向前进。
- 当附近没有明显信息素时随机游走。
- 加入少量随机噪声，避免所有蚂蚁完全重叠。
- 加入轻量 Separation。
- 蚂蚁移动轨迹应该自然弯曲，而不是机械直线。

---

# 3. 非目标

V0.1 明确不做：

- macOS；
- Linux；
- 手机；
- 联网；
- 登录；
- 云同步；
- Steam；
- 创意工坊；
- 生物商城；
- 3D；
- Unity；
- Cocos；
- 复杂物理引擎；
- 音效系统；
- 自动更新；
- 窗口碰撞；
- OCR；
- AI 模型；
- 识别屏幕内容；
- 生物爬到真实窗口后面；
- 多层 Z-Order 伪装；
- 编辑器；
- MOD 系统。

这些可以后续再增加。

---

# 4. 总体架构

解决方案名称：

```text
DesktopLife.sln
```

推荐结构：

```text
DesktopLife/
│
├─ DesktopLife.sln
├─ README.md
├─ Directory.Build.props
├─ .gitignore
│
├─ src/
│  ├─ DesktopLife.App/
│  │  ├─ App.xaml
│  │  ├─ App.xaml.cs
│  │  ├─ Overlay/
│  │  │  ├─ OverlayWindow.xaml
│  │  │  ├─ OverlayWindow.xaml.cs
│  │  │  └─ RenderSurface.cs
│  │  ├─ Tray/
│  │  │  └─ TrayService.cs
│  │  ├─ Settings/
│  │  │  ├─ SettingsWindow.xaml
│  │  │  └─ SettingsWindow.xaml.cs
│  │  └─ Bootstrap/
│  │     └─ AppBootstrapper.cs
│  │
│  ├─ DesktopLife.Engine/
│  │  ├─ Time/
│  │  │  ├─ GameLoop.cs
│  │  │  └─ GameTime.cs
│  │  ├─ Input/
│  │  │  ├─ MouseState.cs
│  │  │  └─ MouseTracker.cs
│  │  ├─ Math/
│  │  │  ├─ VectorMath.cs
│  │  │  └─ RandomSource.cs
│  │  ├─ Creatures/
│  │  │  ├─ Creature.cs
│  │  │  ├─ CreatureContext.cs
│  │  │  ├─ CreatureManager.cs
│  │  │  └─ ICreature.cs
│  │  └─ World/
│  │     ├─ WorldBounds.cs
│  │     └─ SimulationWorld.cs
│  │
│  ├─ DesktopLife.Creatures/
│  │  ├─ Fly/
│  │  │  ├─ FlyCreature.cs
│  │  │  ├─ FlyBrain.cs
│  │  │  └─ FlyState.cs
│  │  ├─ Cockroach/
│  │  │  ├─ CockroachCreature.cs
│  │  │  ├─ CockroachBrain.cs
│  │  │  └─ CockroachState.cs
│  │  └─ Ant/
│  │     ├─ AntCreature.cs
│  │     ├─ AntBrain.cs
│  │     ├─ AntState.cs
│  │     └─ PheromoneField.cs
│  │
│  ├─ DesktopLife.Rendering/
│  │  ├─ IRenderer.cs
│  │  ├─ WpfCreatureRenderer.cs
│  │  ├─ Sprite.cs
│  │  ├─ SpriteAnimation.cs
│  │  └─ AssetCache.cs
│  │
│  └─ DesktopLife.Windows/
│     ├─ NativeMethods.cs
│     ├─ WindowStyles.cs
│     ├─ OverlayWindowHelper.cs
│     ├─ GlobalHotkeyService.cs
│     └─ MonitorService.cs
│
├─ tests/
│  ├─ DesktopLife.Engine.Tests/
│  └─ DesktopLife.Creatures.Tests/
│
└─ assets/
   ├─ fly/
   ├─ cockroach/
   └─ ant/
```

---

# 5. 项目依赖方向

必须保持依赖单向：

```text
DesktopLife.App
   ↓
DesktopLife.Rendering
DesktopLife.Windows
DesktopLife.Creatures
   ↓
DesktopLife.Engine
```

其中：

```text
DesktopLife.Engine
```

不得引用：

- WPF；
- Win32；
- DesktopLife.App；
- DesktopLife.Rendering。

`DesktopLife.Creatures` 只依赖 `DesktopLife.Engine`。

这样以后可以替换 WPF Renderer，而不需要重写动物 AI。

---

# 6. 核心数据模型

推荐使用 `System.Numerics.Vector2`。

## 6.1 ICreature

最低接口：

```csharp
public interface ICreature
{
    Guid Id { get; }

    Vector2 Position { get; }
    Vector2 Velocity { get; }

    float Rotation { get; }
    float Scale { get; }

    bool IsVisible { get; }

    void Update(float deltaTime, in CreatureContext context);
}
```

不要把 WPF `Image`、`BitmapImage`、`Canvas` 放进 Creature。

Creature 只保存逻辑状态。

---

## 6.2 Creature 基类

可提供：

```csharp
public abstract class Creature : ICreature
{
    public Guid Id { get; } = Guid.NewGuid();

    public Vector2 Position { get; protected set; }
    public Vector2 Velocity { get; protected set; }

    public float Rotation { get; protected set; }
    public float Scale { get; protected set; } = 1f;

    public bool IsVisible { get; protected set; } = true;

    public abstract void Update(
        float deltaTime,
        in CreatureContext context);
}
```

---

## 6.3 CreatureContext

每帧传给所有 Creature：

```csharp
public readonly record struct CreatureContext(
    MouseState Mouse,
    WorldBounds Bounds,
    float TotalTime,
    IRandomSource Random,
    PheromoneField? PheromoneField);
```

如果希望彻底避免 Engine 依赖 Ant 类型，可以将 PheromoneField 移到通用 World Service，或把 Context 中的环境能力抽象为接口。

优先保持依赖方向正确。

---

# 7. MouseTracker

不需要一开始安装全局 Mouse Hook。

第一版每帧直接调用 Win32：

```text
GetCursorPos
```

获取屏幕坐标即可。

维护：

```csharp
public readonly record struct MouseState(
    Vector2 Position,
    Vector2 Delta,
    float Speed,
    bool IsMoving,
    TimeSpan IdleTime);
```

算法：

```text
delta = currentPosition - previousPosition
speed = delta.Length / deltaTime
```

需要：

- 对异常大的 `deltaTime` 做 clamp；
- 避免除 0；
- 可做简单指数平滑；
- 小于 1~2 px 的抖动视为静止。

建议：

```text
IsMoving = Speed > 15 px/s
```

空闲计时从“最后一次有效移动”开始。

---

# 8. GameLoop

禁止使用多个 DispatcherTimer 分别驱动每种动物。

整个应用只需要一个主循环。

推荐：

```text
CompositionTarget.Rendering
      ↓
Stopwatch
      ↓
deltaTime
      ↓
MouseTracker.Update()
      ↓
SimulationWorld.Update()
      ↓
RenderSurface.InvalidateVisual()
```

要求：

- `deltaTime` 最大 clamp 到 0.05 秒；
- 窗口卡顿后不能让生物瞬间飞出屏幕；
- 不允许每只 Creature 自己创建 Timer；
- 逻辑更新集中在主循环；
- Renderer 只负责绘制，不负责动物 AI。

---

# 9. OverlayWindow

WPF Overlay 必须：

```text
WindowStyle=None
AllowsTransparency=True
Background=Transparent
ShowInTaskbar=False
Topmost=True
ResizeMode=NoResize
```

同时通过 Win32 扩展样式实现：

```text
WS_EX_LAYERED
WS_EX_TRANSPARENT
WS_EX_NOACTIVATE
WS_EX_TOOLWINDOW
```

目标：

- 鼠标点击穿透；
- 不抢焦点；
- 不出现在普通 Alt+Tab 列表；
- 保持置顶。

重要：

> 如果仅靠 WPF `IsHitTestVisible=false` 仍无法让顶层窗口完整穿透，必须在 Win32 层修复，不要把问题规避成“用户手动暂停后才能点击”。

第一版默认覆盖主显示器。

架构必须为未来“一显示器一个 OverlayWindow”保留扩展空间。

---

# 10. RenderSurface

禁止：

```text
Canvas.Children.Add(new Image(...))
```

每只虫创建一个 WPF `Image` 是错误设计。

正确方式：

```text
一个 RenderSurface
     ↓
读取 CreatureManager 中全部可见 Creature
     ↓
一次 OnRender
     ↓
DrawingContext.DrawImage / DrawGeometry
```

推荐：

```csharp
public sealed class RenderSurface : FrameworkElement
{
    protected override void OnRender(DrawingContext dc)
    {
        // renderer.Render(...)
    }
}
```

如果没有正式贴图：

- Fly：小黑色椭圆 + 半透明翅膀；
- Cockroach：棕色椭圆；
- Ant：小黑点/椭圆。

这样先完成行为。

不要因为没有美术素材拖延功能。

---

# 11. Sprite 与 AssetCache

图片只允许加载一次。

`AssetCache`：

```text
key -> frozen BitmapSource
```

要求：

- 应用启动时或首次使用时缓存；
- WPF `BitmapImage` 初始化后 `Freeze()`；
- 每帧不得读取磁盘；
- 每帧不得 new `BitmapImage`。

Sprite 渲染支持：

- Position；
- Rotation；
- Scale；
- Opacity；
- Sprite Frame。

---

# 12. Fly 详细行为参数

默认配置：

```json
{
  "count": 1,
  "followSpeed": 420,
  "panicSpeed": 720,
  "orbitMinRadius": 40,
  "orbitMaxRadius": 120,
  "idleDepartMs": 1500,
  "panicMouseSpeed": 900
}
```

单位：

- speed：px/s；
- radius：px；
- 时间：ms。

## Follow

不要：

```text
Position = Mouse.Position
```

使用 steering：

```text
desiredVelocity = Normalize(target - position) * maxSpeed
velocity = Lerp(velocity, desiredVelocity, steeringFactor)
```

目标点不是鼠标本身。

目标点：

```text
mouse position
+
orbit offset
+
noise offset
```

Orbit angle 随时间变化。

## Depart

鼠标静止后：

1. 选择一个离开方向；
2. 优先向距离当前位置较近的屏幕边缘；
3. 加入随机角度；
4. 移动到屏幕外 50~150 px；
5. 状态变为 Offscreen。

## Approach

鼠标重新移动后：

- 从最近/合理的屏幕边缘出现；
- 目标为鼠标周围；
- 不瞬移。

---

# 13. Cockroach 详细行为参数

建议：

```json
{
  "count": 20,
  "crawlSpeedMin": 45,
  "crawlSpeedMax": 95,
  "fleeSpeedMin": 220,
  "fleeSpeedMax": 420,
  "fearRadius": 160,
  "separationRadius": 24
}
```

## Flee

基础逃跑方向：

```text
normalize(roach.position - mouse.position)
```

加入：

```text
随机偏转 ±20°
```

最终：

```text
escapeDirection =
    mouseAway * 0.75
  + randomDirection * 0.15
  + edgePreference * 0.10
```

具体权重可调，但不允许所有蟑螂同方向同步移动。

## Separation

对于附近蟑螂，只考虑很小邻域。

第一版 20~100 只时可以 O(n²)。

如果数量以后 >300，再实现 Spatial Hash/Grid。

不要在 V0.1 过早优化。

---

# 14. PheromoneField

这是蚂蚁系统的重点。

不要创建与屏幕像素相同大小的数组。

推荐固定低分辨率：

```text
128 × 72
```

或根据屏幕比例动态计算近似网格。

例如：

```csharp
float[,] strengths;
```

功能：

```csharp
void Add(Vector2 screenPosition, float amount);
float Sample(Vector2 screenPosition);
void Decay(float deltaTime);
void Clear();
```

屏幕坐标映射：

```text
screen coordinate
      ↓
normalized 0~1
      ↓
grid coordinate
```

信息素每帧：

```text
strength *= exp(-decayRate * dt)
```

也可以使用近似乘法。

要求：

- 最大值 clamp；
- 可配置 decayRate；
- 网格不进行每帧内存重建；
- `Decay()` 原地更新。

---

# 15. 鼠标留下信息素

当鼠标有效移动时：

- 鼠标每移动约 12~20 px；
- 在当前位置附近加入信息素；
- 可以给附近几个 cell 加不同权重；
- 形成连续轨迹，而不是稀疏点。

伪代码：

```text
if mouseMovedEnough:
    pheromone.Add(mouse.Position, 1.0)
```

为了避免快速甩鼠标时出现断线：

需要在上一点和当前点之间做简单线性插值采样。

---

# 16. Ant Brain

每只蚂蚁具有：

```text
Position
Heading
Speed
WanderNoise
```

每次更新采样：

```text
前方   30px
左前   30px / -35°
右前   30px / +35°
```

计算：

```text
leftStrength
forwardStrength
rightStrength
```

选择：

- forward 最强 → 小幅继续直行；
- left 最强 → 左转；
- right 最强 → 右转；
- 都很弱 → Wander。

加入少量随机噪声：

```text
heading += random(-wander, +wander)
```

再加入 Separation。

---

# 17. 边界处理

不同动物不同处理。

## Fly

允许：

```text
飞到屏幕外
```

因此不能强制 Clamp。

## Cockroach

普通 Crawl 时：

- 接近边缘可以沿边缘；
- 或转向；
- Flee 时可以进入 Hidden。

## Ant

默认不离开屏幕：

- 碰到边缘反射；
- 或小角度转向屏幕内部。

不能简单把所有实体统一 Clamp，否则 Fly 无法真正飞走。

---

# 18. CreatureManager

职责：

- 创建 Creature；
- 删除 Creature；
- Update 全部；
- 提供只读渲染快照/遍历；
- 根据 Settings 调整数量。

接口示意：

```csharp
public sealed class CreatureManager
{
    public IReadOnlyList<ICreature> Creatures { get; }

    public void Update(
        float deltaTime,
        in CreatureContext context);

    public void SetFlyCount(int count);
    public void SetCockroachCount(int count);
    public void SetAntCount(int count);
}
```

不要让 UI 直接操作 List。

---

# 19. SimulationWorld

由一个对象管理：

```text
MouseTracker
PheromoneField
CreatureManager
WorldBounds
RandomSource
```

每帧：

```text
MouseTracker.Update
      ↓
PheromoneField.Add / Decay
      ↓
CreatureManager.Update
      ↓
Renderer
```

---

# 20. 设置系统

配置存放：

```text
%AppData%\DesktopLife\settings.json
```

建议模型：

```csharp
public sealed class DesktopLifeSettings
{
    public bool FlyEnabled { get; set; } = true;
    public bool CockroachEnabled { get; set; } = true;
    public bool AntEnabled { get; set; } = true;

    public int FlyCount { get; set; } = 1;
    public int CockroachCount { get; set; } = 20;
    public int AntCount { get; set; } = 50;

    public float GlobalSpeedMultiplier { get; set; } = 1f;

    public bool StartWithWindows { get; set; }
}
```

要求：

- 文件不存在 → 自动使用默认配置；
- JSON 损坏 → 回退默认配置；
- 不因为配置异常导致程序无法启动。

---

# 21. 托盘菜单

程序启动后必须有系统托盘入口。

菜单：

```text
DesktopLife
────────────────
✓ 苍蝇
✓ 蟑螂
✓ 蚂蚁

密度
  少
  中
  多
  地狱

──────────────
暂停 / 恢复
设置
退出
```

密度预设：

```text
少：
Fly        1
Cockroach  5
Ant        20

中：
Fly        1
Cockroach  20
Ant        50

多：
Fly        3
Cockroach  60
Ant        150

地狱：
Fly        10
Cockroach  150
Ant        300
```

注意：

“地狱”只是压力模式。

如果当前 WPF Renderer 达不到性能目标，先保证程序稳定，并记录为后续 Skia Renderer 优化项。

---

# 22. 全局快捷键

使用 Win32：

```text
RegisterHotKey
```

建议：

```text
Ctrl + Alt + D
```

切换：

```text
Pause / Resume
```

暂停后：

- Overlay 可以隐藏；
- 或停止绘制；
- CPU 使用应明显降低。

---

# 23. 性能目标

测试环境：

普通 Windows 10/11 桌面电脑。

正常模式：

```text
1 Fly
20 Cockroaches
50 Ants
```

目标：

- 动画主观流畅；
- 目标 60 FPS；
- 无明显 UI 卡顿；
- 鼠标移动不受影响；
- 用户操作下层软件无异常；
- 空闲状态 CPU 应尽可能低。

多模式：

```text
3 Fly
60 Cockroaches
150 Ants
```

目标：

- 至少保持可用；
- 不出现持续 GC 抖动；
- 不出现内存持续增长。

---

# 24. 热路径优化规则

每帧禁止：

```text
new BitmapImage
File.ReadAllBytes
Directory.GetFiles
大量 ToList()
大量 LINQ
大量 Guid 创建
字符串拼接日志
创建新 Random
创建新 Timer
```

建议：

- List 复用；
- Vector2 使用 struct；
- Sprite 缓存；
- Renderer 不创建 Creature；
- Brain 不访问 UI；
- 信息素网格复用；
- 仅 Debug 模式输出诊断日志。

---

# 25. WPF 渲染性能升级路径

V0.1：

```text
WPF DrawingContext
```

如果正常模式性能达标：

不要提前迁移。

只有在真实测试表明 DrawingContext 成为瓶颈时，再新增：

```text
SkiaSharp Renderer
```

Renderer 应通过：

```csharp
public interface IRenderer
{
    void Render(
        DrawingContext drawingContext,
        IReadOnlyList<ICreature> creatures);
}
```

或等价抽象。

未来可以：

```text
WpfCreatureRenderer
        ↓
SkiaCreatureRenderer
```

但 Creature/Brain 完全不改。

---

# 26. DPI

V0.1 必须至少在：

```text
100%
125%
150%
```

Windows 缩放下检查：

- 鼠标位置；
- Overlay 尺寸；
- 生物绘制位置。

不能出现：

```text
鼠标实际在 A
苍蝇追的是 B
```

如果 WPF DIP 与 Win32 Physical Pixel 不一致：

必须显式处理坐标转换。

不要通过硬编码 DPI 比例解决。

---

# 27. 崩溃与异常处理

App 顶层注册：

```text
DispatcherUnhandledException
AppDomain.CurrentDomain.UnhandledException
```

至少：

- 写入本地日志；
- 尝试安全退出；
- 不允许无限弹 MessageBox。

日志：

```text
%LocalAppData%\DesktopLife\logs\
```

第一版简单文本即可。

---

# 28. 测试项目

使用：

```text
xUnit
```

至少测试：

## Engine

- MouseState 速度计算；
- IdleTime；
- WorldBounds；
- 向量辅助方法；
- 固定 Seed Random。

## Fly

- 鼠标活动时 Offscreen → Approach；
- 到达目标附近 → Orbit；
- 鼠标静止超过阈值 → Depart；
- 鼠标恢复 → Approach。

## Cockroach

- 鼠标进入 fearRadius → Flee；
- Flee 方向总体远离鼠标；
- 没有鼠标威胁时 Crawl。

## Pheromone

- Add 后 Sample > 0；
- Decay 后降低；
- Clear 后为 0；
- 屏幕坐标映射正确。

## Ant

- 左侧信息素更强时总体趋向左；
- 右侧更强时总体趋向右；
- 没有信息素时仍能移动。

---

# 29. 调试 HUD

仅 Debug 构建显示。

右上角：

```text
DesktopLife DEBUG

FPS: 60
Frame: 2.1ms

Mouse:
x: 1240
y: 582
speed: 422 px/s
idle: 0.05s

Creatures:
Fly: 1
Cockroach: 20
Ant: 50
```

Release 默认关闭。

快捷键可选：

```text
F12
```

切换 HUD。

---

# 30. 开发阶段

以下阶段严格按顺序执行。

---

## 阶段 0 — 初始化项目

完成：

- 创建 Solution；
- 创建 5 个 src project；
- 创建 2 个 test project；
- 设置 ProjectReference；
- 配置 `.gitignore`；
- 配置 nullable；
- 配置 implicit usings；
- 创建 README；
- 能 build；
- tests 能运行。

验收：

```powershell
dotnet build DesktopLife.sln
dotnet test DesktopLife.sln
```

必须全部成功。

---

## 阶段 1 — 透明 Overlay

只做：

- 无边框透明窗口；
- 主显示器覆盖；
- Topmost；
- click-through；
- no activate；
- Win32 扩展 style；
- Esc 不退出；
- 程序可以通过托盘/Debug 临时退出。

在 Overlay 中画：

```text
一个测试圆点
```

验收：

1. 圆点能显示；
2. 背景完全透明；
3. 鼠标能点击圆点下面的浏览器；
4. 点击下层浏览器时浏览器得到焦点；
5. DesktopLife 不抢焦点；
6. Alt+Tab 不出现普通工作窗口。

如果点击穿透失败：

此阶段不得继续。

---

## 阶段 2 — MouseTracker

完成：

- `GetCursorPos`；
- Position；
- Delta；
- Speed；
- IsMoving；
- IdleTime；
- Debug HUD。

验收：

- HUD 坐标与鼠标一致；
- 鼠标静止后 IdleTime 连续增加；
- 移动后立即归零；
- 不同 DPI 下基本正确。

---

## 阶段 3 — GameLoop + Creature Engine

完成：

- GameLoop；
- Creature；
- ICreature；
- CreatureManager；
- SimulationWorld；
- WorldBounds；
- RandomSource；
- RenderSurface；
- WpfCreatureRenderer。

创建一个：

```text
DebugCreature
```

让其在屏幕移动。

验收：

- 只有一个主循环；
- 没有 Creature Timer；
- 逻辑与 Render 分离；
- 60 FPS 左右运行；
- build/test 成功。

---

## 阶段 4 — Fly

只实现苍蝇。

完成：

- FlyState；
- FlyBrain；
- Approach；
- Orbit/Follow；
- Panic；
- Depart；
- Offscreen。

先使用占位图。

验收场景：

### Case A

鼠标连续移动：

```text
苍蝇进入屏幕
→ 飞向鼠标附近
→ 持续盘旋跟随
```

### Case B

鼠标停住 > 1.5 秒：

```text
苍蝇离开
→ 飞出屏幕
```

### Case C

再次移动：

```text
苍蝇从边缘飞回来
```

### Case D

快速甩鼠标：

```text
苍蝇先受到惊吓
→ 拉开距离
→ 再回来
```

达到这些效果后才能继续。

---

## 阶段 5 — Cockroach

完成：

- CockroachState；
- Crawl；
- Panic；
- Flee；
- Hidden/Emerge；
- Separation；
- 20 只默认生成。

验收：

1. 正常时随机爬；
2. 不全部重叠；
3. 鼠标靠近，一批蟑螂迅速四散；
4. 逃跑方向总体远离鼠标；
5. 不出现所有蟑螂完全同步；
6. 能从屏幕边缘出现/消失。

---

## 阶段 6 — PheromoneField

先不要实现蚂蚁 AI。

完成：

- 128×72 左右信息素网格；
- Add；
- Sample；
- Decay；
- Clear；
- 鼠标移动时写入；
- Debug 模式可选择显示信息素 Heatmap。

Heatmap 只用于 Debug。

验收：

```text
鼠标移动
→ 屏幕留下热度轨迹
→ 静止后逐渐消失
```

---

## 阶段 7 — Ant

完成：

- 50 只；
- 前/左前/右前 Sensor；
- Follow Pheromone；
- Wander；
- Separation；
- 边缘转向。

验收：

```text
鼠标画一个弧线
→ 若干秒内蚂蚁逐渐发现轨迹
→ 沿轨迹形成明显移动趋势
```

不能要求 100% 每只蚂蚁都完全沿线。

要保留自然随机性。

---

## 阶段 8 — Tray + Settings

完成：

- NotifyIcon；
- Fly 开关；
- Cockroach 开关；
- Ant 开关；
- 密度；
- 暂停；
- 设置；
- 退出；
- settings.json 保存/加载。

验收：

关闭再启动程序：

```text
上次设置仍然存在
```

---

## 阶段 9 — Hotkey

完成：

```text
Ctrl + Alt + D
```

Pause / Resume。

验收：

- 无论当前使用 Chrome/IDE/微信，都能触发；
- 不抢焦点；
- Pause 后 CPU 使用明显下降。

---

## 阶段 10 — 性能整理

用：

```text
1 / 20 / 50
3 / 60 / 150
10 / 150 / 300
```

分别测试。

检查：

- FPS；
- CPU；
- 内存；
- GC；
- 鼠标操作体验。

只优化真实瓶颈。

不要为了“可能”出现的问题重构。

---

## 阶段 11 — Release 打包

目标：

生成可直接运行的：

```text
DesktopLife.exe
```

发布：

```powershell
dotnet publish
```

优先：

- `win-x64`；
- Release；
- self-contained 或 framework-dependent 根据最终体积测试决定；
- 第一版不做安装器也可以。

README 中记录：

- 构建命令；
- 发布命令；
- 快捷键；
- 配置文件路径。

---

# 31. Definition of Done

V0.1 完成必须同时满足：

- [ ] Windows 10/11 可运行
- [ ] Overlay 透明
- [ ] Overlay 不抢焦点
- [ ] 鼠标点击可穿透
- [ ] Fly 会追鼠标
- [ ] Fly 在鼠标静止后飞走
- [ ] Fly 在鼠标再次移动后回来
- [ ] Cockroach 会随机爬
- [ ] Cockroach 会躲鼠标
- [ ] Cockroach 群体逃跑看起来不是完全同步
- [ ] Mouse 可留下信息素
- [ ] Ant 能对信息素轨迹产生明显跟随行为
- [ ] 托盘可打开/关闭每种生物
- [ ] 可调密度
- [ ] 可 Pause
- [ ] `Ctrl + Alt + D` 可暂停/恢复
- [ ] 设置能保存
- [ ] Debug HUD 可查看核心状态
- [ ] Release 默认无 HUD
- [ ] `dotnet build` 成功
- [ ] `dotnet test` 成功
- [ ] 正常密度没有明显卡顿
- [ ] 运行一段时间内存不持续增长

---

# 32. 第二阶段候选功能

V0.1 完成后再讨论。

优先级建议：

## P1 — 蜘蛛

- 从屏幕顶部垂下；
- 鼠标靠近后快速爬回去；
- 简单蛛丝。

## P1 — 更多 Fly 行为

- 落在鼠标附近；
- 落在屏幕边缘；
- 多苍蝇互相避让。

## P1 — 虫灾模式

热键触发：

```text
20 → 50 → 100 → 300
```

逐渐增加蟑螂。

## P2 — 蝴蝶

- 鼠标快速靠近 → 飞散；
- 鼠标缓慢 → 附近盘旋。

## P2 — 萤火虫

- 夜间/暗色桌面视觉；
- 发光粒子。

## P2 — Desktop Cat

- 猫追鼠标；
- 扑鼠标；
- 发呆；
- 睡觉。

## P3 — 多显示器

改成：

```text
Monitor 1 → OverlayWindow A
Monitor 2 → OverlayWindow B
Monitor 3 → OverlayWindow C
```

Simulation 可以共享。

---

# 33. 未来插件化方向

暂时不要实现，只保留架构空间。

未来每种生物可以拥有：

```text
CreatureDefinition
BehaviorConfig
SpriteSet
AnimationSet
```

例如：

```json
{
  "id": "cockroach.default",
  "displayName": "Cockroach",
  "scale": 1.0,
  "speed": 90,
  "fearRadius": 160
}
```

最终目标可以扩展成：

```text
DesktopLife
├─ 生物引擎
├─ 桌面行为引擎
├─ 主题
├─ 生物包
└─ 社区 MOD
```

V0.1 不做插件加载器。

---

# 34. Codex 工作原则

在整个开发过程中：

## 每个阶段开始前

先检查：

```text
当前阶段依赖是否完成？
```

## 每个阶段完成后

必须执行：

```powershell
dotnet build DesktopLife.sln
dotnet test DesktopLife.sln
```

然后实际启动程序检查。

## 如果发现 Bug

不要绕过需求。

例如：

```text
Overlay 无法点击穿透
```

错误处理：

```text
“先把 Overlay 暂时关闭，让用户点击”
```

正确处理：

```text
定位 Win32 Window Style / WPF Hit Test / Window Message 原因
真正修复点击穿透
```

## 如果性能不够

先 Profile。

不要未经验证直接换引擎。

---

# 35. Codex 首次执行任务

现在开始开发时，只执行：

```text
阶段 0
阶段 1
阶段 2
阶段 3
阶段 4
```

也就是首先交付：

> 一只真正能在 Windows 桌面上追随鼠标、鼠标静止后飞走、鼠标再次移动后重新飞回来的苍蝇。

不要在第一次迭代同时做：

- 蟑螂；
- 蚂蚁；
- Settings；
- Installer；
- 蜘蛛；
- 其他生物。

第一里程碑目录：

```text
MILESTONE 1 — Fly Prototype
```

必须满足：

```text
透明桌面
+
点击穿透
+
MouseTracker
+
Creature Engine
+
Fly AI
+
Debug HUD
```

完成后再进入：

```text
MILESTONE 2 — Cockroach
MILESTONE 3 — Ant/Pheromone
MILESTONE 4 — Productization
```

---

# 36. 第一里程碑验收脚本

Codex 完成 MILESTONE 1 后，应手工确认：

## 测试 1

打开 Chrome。

运行 DesktopLife。

预期：

- Chrome 仍能正常点击；
- DesktopLife 不抢焦点。

## 测试 2

快速移动鼠标。

预期：

- 苍蝇从屏幕边缘飞来；
- 飞向鼠标附近；
- 不直接黏在鼠标点上。

## 测试 3

缓慢移动鼠标。

预期：

- 苍蝇围绕鼠标附近活动；
- 有自然扰动。

## 测试 4

快速甩鼠标。

预期：

- 苍蝇明显受到惊吓；
- 短暂远离；
- 随后重新接近。

## 测试 5

停止鼠标 1.5 秒以上。

预期：

- 苍蝇主动离开；
- 飞出屏幕。

## 测试 6

再次移动鼠标。

预期：

- 苍蝇重新从屏幕边缘返回。

## 测试 7

程序连续运行至少一段时间。

预期：

- 不崩溃；
- 不持续增加内存；
- 不影响鼠标操作；
- 不出现明显卡顿。

---

# 37. 第一里程碑交付要求

Codex 最终回复用户时应列出：

1. 创建/修改了哪些主要文件；
2. 架构说明；
3. Overlay 如何实现穿透；
4. Fly 状态机；
5. build 结果；
6. test 结果；
7. 如何运行；
8. 已知限制；
9. 下一阶段建议。

在没有实际运行命令验证之前，不允许声称：

```text
“已经完成”
“测试全部通过”
“可以正常运行”
```

必须以实际命令结果为依据。

---

# 38. 最终产品方向

DesktopLife 不是单纯“鼠标特效”。

长期目标是：

> 一个拥有自主行为、会对鼠标和桌面环境产生反应的 Windows 桌面生命系统。

第一版成功标准不是生物种类多，而是：

```text
用户移动鼠标的一瞬间，
会感觉屏幕里的东西“真的看到我了”。
```

这就是 DesktopLife 最核心的产品体验。
