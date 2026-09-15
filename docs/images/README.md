# README 动图

三张 `demo-*.gif` 来自项目内的实际运动引擎、渲染器和 WPF 设置控件，由固定输入生成，属于功能演示，并非用户桌面的录屏。

- 苍蝇：20 FPS，10 秒；点击后实际停落 3 秒，再恢复跟随。
- 跨屏：20 FPS，5 秒；固定向右的随机输入，展示蟑螂、蚂蚁、毛毛虫穿过相接屏幕的边缘。
- 设置：6 个操作状态，每个停留 1.8 秒，展示数量保存、中英文切换及快捷键区域。
- 昆虫演示按 2 倍放大，时间保持原速。设置使用独立配置文件，不更改用户配置，不注册全局快捷键。

## 重新生成

在 Windows 仓库根目录执行，需要 .NET 10 SDK、Python 和 Pillow：

```powershell
python -m pip install Pillow
dotnet run --project tools/DesktopLife.Diagnostics -- --readme-demo
python scripts/Encode-Readme-Gifs.py
```

中间 PNG 位于已被 Git 忽略的 `artifacts/readme-demo`。生成器会检查落地时长、三种昆虫跨屏、数量保存，以及 GIF 循环和总时长。提交前还需查看各状态画面，确认文字与动作清晰。
