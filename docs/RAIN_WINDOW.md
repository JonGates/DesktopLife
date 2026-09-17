# 雨窗 / Rain window（开发预览）

主程序「生物」页和屏保内容设置中新增第三个标签「雨窗」。森林／海洋数量保持独立，雨窗中不生成生物。雨量可选毛毛雨、小雨、中雨、大雨、暴雨五档，默认中雨。桌面与屏保分别保存；点击保存应用。雨量随实际显示面积生成，各档总雨滴上限依次为 100、220、360、480、600。

- 雨滴落到玻璃表面时略微铺展，然后附着并积大；达到阈值后受重力连续下滑，不再中途停顿。
- 桌面模式中鼠标碰触雨滴会触发下滑，移动路径也参与碰触检测。
- 滑落雨滴会吞并沿途雨滴，体积相加后重新计算大小，留下逐渐消失的水痕。
- 桌面左键点击随机产生不规则分叉裂纹、交错网状裂纹或中心密集碎纹，约 3 秒后消失；最多保留 5 处效果。点击仍会传给下面的应用，设置窗口内的点击不触发特效。
- 暂停时隐藏画面并冻结模拟，恢复后继续；场景切换会清除雨滴和裂纹。
- 水滴采用不规则轮廓、深色边缘、局部高光和底部聚光，细小水珠与大滴混合。细水痕逐渐淡出。
- 桌面使用透明覆盖层，不抓取或扭曲桌面内容；屏保选择背景图片时，水滴会采样相应位置的图片，呈现带径向弯曲的近似倒像折射；背景预先柔焦，水滴从清晰原图采样。真实键鼠输入仍会退出屏保，雨滴和碎裂由自动交互驱动。
- 跨屏按物理坐标绘制；雨滴可落入相接的下方显示器，掉入屏幕外的空隙时消失。

## 验证

性能优化：桌面水滴高光预绘制为 12 张小纹理并复用；只绘制当前屏幕范围内的效果。水痕按距离与时间采样，总量限制为 1,000 段。桌面雨窗目标刷新上限为 30 次／秒，森林和海洋不受此限制。

`--rain-performance` 使用双 1080p 布局模拟，并离屏绘制其中一屏；本机约 580 颗总雨滴、1,000 段水痕时，单屏软件绘制中位数约 15.5 ms。此结果不代表实际桌面 FPS；多屏透明窗口合成、GPU 和显示分辨率仍会影响流畅度。

```powershell
dotnet test DesktopLife.sln -c Release
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --rain artifacts/rain-check
```

Rain window is a third scene available in desktop and screen saver settings. Drops stick, grow, slide under gravity, and gather smaller drops along their path. The pointer releases drops; clicks generate one of three temporary fracture effects. Screen saver input still exits normally. Desktop glass uses highlights without screen capture; screen saver images support a cached curved, inverted local lens effect over a softly blurred backdrop.

## 屏保折射预览

在主程序的屏保设置中选择背景图片，并将屏保场景设置为雨窗后预览。纯色背景没有可折射的图像细节；桌面透明模式仍不抓取桌面。

背景采样图最长边限制为 1024 像素；每颗较大水滴使用 32×32 像素透镜纹理，移动超过 4 像素或尺寸明显改变后更新。细小水滴继续复用高光纹理。这是性能受控的光学近似，不是完整光线追踪。
