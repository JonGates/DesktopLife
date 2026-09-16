# 昆虫与比例 · 当前开发版

桌面与屏保共用 12 种昆虫的模拟与绘制。全桌面固定一只苍蝇，其余物种按全桌面总数配置；相接的屏幕可连续通行。

## 新增物种

| 物种 | 100% 身体长度（物理像素，不含足、触角及尾部附肢） | 外观与动作 |
| --- | ---: | --- |
| 瓢虫 Ladybug | 10 | 红黑圆拱鞘翅，短触角，小步爬行 |
| 步甲 Ground beetle | 22 | 深色纵纹鞘翅，较快爬行，接近鼠标时躲避 |
| 蠼螋 Earwig | 24 | 分节腹部与尾钳，爬行和短暂停步 |
| 衣鱼 Silverfish | 18 | 银灰渐尖身体，三根尾丝，快速转向 |
| 蟋蟀 Cricket | 26 | 深褐身体、长触角、粗后腿，走走停停 |
| 蚱蜢 Grasshopper | 38 | 绿色翅鞘、短触角、粗后腿，慢走与停步 |
| 螳螂 Mantis | 60 | 三角头、细长前胸、折叠捕捉足，缓慢转向 |
| 竹节虫 Stick insect | 80 | 细长枝条形身体与长足，低速爬行 |

新增种类默认数量 0，每种可设 0–100；尺寸默认 80%–120%，允许 10%–300%。尺寸按身体连同附肢一起缩放。旧配置保留原数量与尺寸，更新不会自动增加昆虫。

这些是参考常见昆虫体型关系的视觉设计值。同一类别包含许多大小不同的物种，显示器的像素密度也不同，因此不是毫米校准或精确物种鉴定模型。既有苍蝇、蟑螂、蚂蚁和毛毛虫保留原始基准，避免已有配置突然改变。

结构参考：[瓢虫形态与体型（马里兰大学）](https://www.extension.umd.edu/resource/ladybugs-or-ladybird-beetles)、[衣鱼的三尾丝与体型（马里兰大学）](https://extension.umd.edu/resource/silverfish-and-firebrats)、[蠼螋（明尼苏达大学）](https://extension.umn.edu/garden-and-home/yard-and-garden/yard-and-garden-insects/earwigs)、[螳螂（密苏里州保护部门）](https://mdc.mo.gov/discover-nature/field-guide/mantids-mantises)、[竹节虫（密苏里州保护部门）](https://mdc.mo.gov/discover-nature/field-guide/walkingsticks-stick-insects)。未使用这些网站的照片。

## 外观与当前动作范围

新增 8 种使用 AI 生成的独立身体素材，程序绘制并驱动足、触角、尾钳或尾丝；不是实拍照片。当前开发版已为全部 12 种补齐可爱外观：圆润矢量身体、柔和配色和小眼睛，保留各类附肢特征与相同比例、步态。桌面与屏保均可切换。公开的 v0.4.0 下载包仍为旧版风格支持范围。

爬行步态由实际移动距离驱动，停步时保持足部姿势。各类拥有不同速度、转向速度与停步概率。当前没有模拟跳跃、捕食、蜕皮或新增物种的起飞行为。

## 扩展与验证

- `InsectCatalog` 定义名称、身体比例、速度和步长；枚举只追加，保持旧配置兼容。
- `PopulationSettings.Additional` 保存新增物种数量和尺寸；桌面与屏保独立持久化。
- `AdditionalInsectSprite` 缓存每种 8 帧身体与附肢。纹理按透明空行分割，保留原始 alpha。
- `dotnet test DesktopLife.sln -c Release` 检查数量、边界、尺寸、暂停与旧配置。
- `dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --insect-catalog artifacts/insect-catalog` 验证透明度、8 帧动作与暂停姿势并输出渲染示意。
- `dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --additional-settings artifacts/additional-settings` 验证桌面与屏保的新增配置、无效输入与双语切换。
