# README 动图

## 主屏实际录制（当前 README）

`live-desktop.gif`、`live-fly.gif`、`live-settings.gif` 均录自 Windows 主屏（1920×1080，坐标 0,0）。录制对象是单独运行的 `artifacts/exe/DesktopLife.exe`，使用当前真实数量：118 只蟑螂、82 只蚂蚁、9 只毛毛虫及固定 1 只苍蝇。

背景窗口只绘制静态背景和标题，不生成昆虫；所有昆虫和设置均来自实际应用。全景缩小到 960×540，苍蝇特写裁剪主屏区域后放大 1.5 倍，设置特写裁剪主屏中央区域。设置片段剪去两段录制之间的操作等待，其余按实际采集时间播放。

录制步骤：

1. 启动 DesktopLife，临时关闭“从录屏中隐藏昆虫和设置窗口”，记录原设置以便恢复。
2. 可运行 `dotnet run --project tools/DesktopLife.Diagnostics -- --recording-backdrop`，在主屏显示干净背景（Esc 关闭），避免录入其他工作窗口。
3. 使用录屏工具选择**主屏**，分别录制桌面活动、苍蝇点击停落与恢复飞行、设置窗口中英文切换。本次使用 Computer Use 的连续屏幕采集，逐帧检查范围为 `(0,0,1920,1080)`；没有采集副屏。
4. 完成后关闭录制背景，恢复原来的录屏排除设置。
5. 检查画面，裁剪、压缩并导出循环 GIF。

本次原始 PNG 位于 `artifacts/primary-recordings/{desktop,fly,settings}`，每段附有 `timing*.json`，记录 `{ "file": "0000.png", "timeMs": 123 }` 格式的帧文件名与采集时间。原始录制不提交 Git。已有这些文件时，执行：

```powershell
python scripts/Encode-Primary-Recordings.py
```

编码脚本使用 Pillow，验证主屏尺寸、时间戳、GIF 循环和播放总时长。它只编码现有录制，不会自行录屏。

## 旧版引擎演示（保留供开发参考）

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
