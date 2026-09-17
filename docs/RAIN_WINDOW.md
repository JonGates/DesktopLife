# 雨窗 / Rain window（开发预览）

主程序「生物」页和屏保内容设置中新增第三个标签「雨窗」。森林／海洋数量保持独立，雨窗中不生成生物。无需设置数量，雨量随实际显示面积生成，总雨滴数有上限。

- 雨滴落到玻璃表面时出现短暂冲击光圈，然后附着并积大；达到阈值后受重力下滑。
- 桌面模式中鼠标碰触雨滴会触发下滑，移动路径也参与碰触检测。
- 滑落雨滴会吞并沿途雨滴，体积相加后重新计算大小，留下逐渐消失的水痕。
- 桌面左键点击随机产生放射裂纹、蛛网裂纹或下落碎片，约 4 秒后消失；最多保留 5 处效果。点击仍会传给下面的应用，设置窗口内的点击不触发特效。
- 暂停时隐藏画面并冻结模拟，恢复后继续；场景切换会清除雨滴和裂纹。
- 桌面使用透明覆盖层和程序绘制的水滴高光，不抓取或扭曲桌面内容。屏保可配合背景图片使用，真实键鼠输入仍会退出屏保，雨滴和碎裂由自动交互驱动。
- 跨屏按物理坐标绘制；雨滴可落入相接的下方显示器，掉入屏幕外的空隙时消失。

## 验证

```powershell
dotnet test DesktopLife.sln -c Release
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --rain artifacts/rain-check
```

Rain window is a third scene available in desktop and screen saver settings. Drops stick, grow, slide under gravity, and gather smaller drops along their path. The pointer releases drops; clicks generate one of three temporary fracture effects. Screen saver input still exits normally. Glass highlights are drawn over the desktop without screen capture or true optical refraction.
