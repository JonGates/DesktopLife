# 国际化 / Localization

v0.6.0 支持 `zh-CN`、`en-US`、`zh-TW`、`ja-JP`、`ko-KR`。主程序和屏保均可切换语言；两者分别保存。旧配置继续兼容。

## 资源

- `src/DesktopLife.Rendering/Localization/UiLanguage.cs`：共享语言列表、翻译查询与格式化。
- `translations.tsv`：UTF-8 文本，每行三列，以 `|` 分隔：英文源文案、日文、韩文。键需唯一，不要在文案中使用分隔符。
- `zh-TW.json`：简体源文案到繁体文案的静态映射；可直接审核和修改，不依赖运行机器的繁简转换接口。
- `src/DesktopLife.App/Settings/LanguageService.cs`：主程序资源键与原有中英文文案，切换时更新 WPF 动态资源。
- 屏保窗口通过共享 `UiLanguage.Text` 查找文案，保持自身语言状态。

新增或改写文案时，同步修改三种新语言的资源。包含数量的文案请使用 `{0}`、`{1}` 等参数，再格式化；不要把数字拼进翻译键。保留所有参数及其含义。未收录的日/韩文文案会回退到英文，缺少的繁体文案回退到源文案；正式发布前必须检查资源覆盖。

繁体文案初稿使用 OpenCC s2twp 转换，已作为静态文本纳入仓库；OpenCC 不是应用运行或构建依赖。日语、韩语文案与生物名称可通过 PR 继续校订。

## 验证

```powershell
dotnet test DesktopLife.sln -c Release
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --localization artifacts/localization
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --control-center artifacts/localization/main
dotnet run --project tools/DesktopLife.Diagnostics -c Release -- --saver-settings-ui artifacts/localization/saver
```

检查主程序资源和生物名称完整性、格式参数、五种语言的保存/读取、旧配置与无效语言回退。界面检查覆盖 470/620 像素宽度、五种语言、全部设置页及屏保场景，还验证切换语言保留未保存的输入及错误提示翻译。
