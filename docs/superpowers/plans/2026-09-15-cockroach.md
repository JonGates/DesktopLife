# Cockroach Implementation Plan

**Goal:** MILESTONE 2：在现有桌面苍蝇原型上增加 20 只蟑螂。
**Spec:** `docs/DesktopLife_Codex_Development_Spec.md` 的 2.2、13、阶段 5。
**Architecture:** 沿用单一 SimulationWorld 和 DrawingContext。CreatureKind 让渲染器选择缓存素材；只读邻居集合由 CreatureManager 注入 CreatureContext。蟑螂逻辑只依赖 Engine。

## 行为设计

- 初始从主屏四周生成，Hidden 使用个体随机延迟后 Emerge，向屏内移动至 Crawl。
- Crawl 速度 45–95 px/s，分段改变游走方向；靠近边缘时转向并沿边行进。
- 鼠标进入约 160 px 恐惧范围（快速移动略扩大）立即 Panic；每只虫的短暂反应时长、逃跑速度 220–420 px/s、偏转角 ±20°均由注入随机源确定。
- Flee 持续远离鼠标，到达边缘进入 Hidden，安全后可恢复 Crawl。Hidden 后稍候重新出现。
- 使用 24 px 邻域 Separation，只考虑可见同类；完全重叠时使用个体随机方向解开，避免除零。
- 不新增每只虫定时器、WPF 控件或运行时资源读取。

## 执行清单

- [x] 写失败测试：正常 Crawl、近鼠 Panic→Flee、位移远离鼠标、快速鼠标扩大恐惧半径、边缘隐藏并重新出现、重叠分离、固定种子群体不同步。
- [x] 增加 `Creatures/Cockroach/{CockroachState,CockroachOptions,CockroachBrain,CockroachCreature}.cs` 与 `DesktopPopulation.cs`；由 `CreatureManager` 提供只读邻居列表。
- [x] 增加 CreatureKind 与棕色六足蟑螂的两帧缓存矢量素材；Fly 渲染回归验证。
- [x] App 接入 1 Fly + 20 Cockroaches；更新 HUD 数量、可见状态判定及托盘文案，保留暂停和退出。
- [x] 测试群体五分钟模拟、100 只压力模拟、非法 dt 和边界情况；运行 Debug / Release build + test、原生窗口 probe、三个比例的 WPF 离屏渲染检查。
- [x] 独立代码复查并解决重要问题；实际运行采样；更新 README 与里程碑验证记录，发布新包并启动。

完整系统 DPI 切换、用户浏览器交互和长期稳定性沿用第一里程碑的未验证限制。图形使用程序绘制占位素材。设置、密度菜单与蚂蚁留到后续里程碑。


## 验收状态
实现与自动检查已完成；稳定 60 FPS、完整手工桌面体验与长期运行尚未全部验收，见 docs/MILESTONE2_VERIFICATION.md。
