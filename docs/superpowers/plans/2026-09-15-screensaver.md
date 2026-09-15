# Windows 屏保实现计划

## 目标与设计

在现有 DesktopLife 之外提供独立 Windows x64 `.scr` 屏保，复用 Engine、Creatures、Rendering，保留现有桌面模式。用户选择了可切换深色／浅色背景。

- 独立 WPF ScreenSaver 项目：`/s` 全屏、`/c[:HWND]` 配置、`/p HWND` 嵌入预览，无参数进入配置；非法参数退出，不能误开全屏。
- 全屏为每个实际显示器创建不透明窗口，共享一个世界与全桌面数量。使用自动移动的虚拟目标驱动苍蝇，定期停落，不依赖静止鼠标。
- 屏保启动短暂缓冲后，检测真实输入退出；监视显示排列与会话变化并安全退出。预览不退出于普通输入，父窗口销毁时退出。
- 配置独立保存到 `%AppData%/DesktopLife/screensaver.json`，支持两种背景、蟑螂/蚂蚁/毛毛虫总数量；苍蝇固定一只。中英文标签，保存错误可见。
- 生成自带运行环境的屏保 ZIP，包含 `.scr`、安装/设置入口、全屏体验入口和说明。由 Windows 屏保设置决定启用、等待时间和恢复登录；不自动修改系统锁屏选项。

## 实施与验证顺序

1. 为参数解析和启动输入缓冲编写失败测试；在 Engine 的 ScreenSaving 中实现可单测逻辑。
2. 新建 ScreenSaver 项目：配置模型与窗口、Native 互操作、运行与预览生命周期、渲染面板；加入 solution。
3. 增加专用 Diagnostics 验证：真实 HWND 预览、父窗口销毁、深浅色渲染、独立配置、启动与退出。运行已有测试检查回归。
4. 新增打包脚本与中英文使用说明。生成屏保便携包，验证 `/c`、`/p`、`/s`；不改写 v0.1.0 资产，不自动启用系统屏保。
5. 独立代码审查，修复发现的问题，提交并推送代码，交付可运行包。

参考：[Microsoft 屏保参数](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ms686421(v=vs.85))、[屏保安装入口](https://learn.microsoft.com/en-us/windows/win32/devnotes/scrnsave-exe)。
