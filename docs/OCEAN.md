# 海洋模式 / Ocean mode

海洋模式目前位于开发预览版，GitHub v0.4.2 下载包尚不包含此功能。

## 使用

- 设置窗口切换「森林 / 海洋」标签，桌面立即切换场景并保存。两套生物数量、最小与最大尺寸独立保存。
- 海洋固定一只小绿龟跟随鼠标。点击后立即在当前位置缩壳，停留三秒，再伸展鳍肢恢复跟随；停留期间再次点击会重新计时。
- 12 种鱼各自配置数量（0–100）及尺寸百分比（10–300%）；默认每种 2 条、80–120%。设为 0 隐藏该鱼种。
- 写实与可爱风格均支持海洋。鱼类摆尾摆鳍，鱼头朝向实际移动方向，小绿龟摆动鳍肢。
- 共用桌面的实际显示器排列，相接的屏幕边缘允许游动穿越。数量是整个桌面的总量。
- 屏保配置同样提供两个标签，选好场景后保存。屏保配置与桌面配置分别保存。

| 中文 | English |
| --- | --- |
| 小丑鱼 | Clownfish |
| 蓝倒吊 | Blue tang |
| 黄倒吊 | Yellow tang |
| 蝴蝶鱼 | Butterflyfish |
| 神仙鱼 | Emperor angelfish |
| 狮子鱼 | Lionfish |
| 河豚 | Pufferfish |
| 海马 | Seahorse |
| 麒麟鱼 | Mandarin fish |
| 皇家草莓鱼 | Royal gramma |
| 镰鱼 | Moorish idol |
| 六线隆头鱼 | Six-line wrasse |

尺寸基于屏幕可读性与物种轮廓设计，并非真实厘米比例。写实鱼类采用透明纹理与程序化鳍尾动作，可爱版采用原生矢量绘制。

## 开发验证

```powershell
dotnet test DesktopLife.sln -c Release
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --ocean-settings-probe artifacts/ocean-settings
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --ocean-render artifacts/ocean-render
```

测试覆盖旧配置兼容、场景切换与配置保留、小绿龟停留、多屏边界，以及双风格渲染和不同 DPI。打包继续使用项目现有自包含发布流程，桌面与屏保采用同一版本号。
