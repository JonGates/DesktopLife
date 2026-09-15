using Forms = System.Windows.Forms;
namespace DesktopLife.App.Tray;
public sealed class TrayService : IDisposable
{
    private readonly Forms.NotifyIcon _icon;
    private readonly Forms.ContextMenuStrip _menu = new();
    private readonly Forms.ToolStripMenuItem _pause;
    private readonly Forms.ToolStripItem _summary;

    public TrayService(Action showSettings, Action togglePause, Action exit)
    {
        _summary = _menu.Items.Add("DesktopLife · 桌面生物");
        _summary.Enabled = false;
        _menu.Items.Add(new Forms.ToolStripSeparator());
        _menu.Items.Add("数量设置…", null, (_, _) => showSettings());
        _pause = new Forms.ToolStripMenuItem("暂停");
        _pause.Click += (_, _) => togglePause();
        _menu.Items.Add(_pause);
        _menu.Items.Add("退出", null, (_, _) => exit());
        _icon = new Forms.NotifyIcon
        {
            Text = "DesktopLife — 苍蝇与蟑螂",
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenuStrip = _menu,
            Visible = true
        };
        _icon.DoubleClick += (_, _) => showSettings();
    }

    public void SetPopulationSummary(int screens, int flies, int cockroaches, int ants, int caterpillars)
    {
        _summary.Text = $"{screens} 块屏幕 · {flies} 只苍蝇 · {cockroaches} 只蟑螂 · {ants} 只蚂蚁 · {caterpillars} 只毛毛虫";
        _icon.Text = $"DesktopLife — {screens} 屏 / {flies} 苍蝇 / {cockroaches} 蟑螂 / {ants} 蚂蚁 / {caterpillars} 毛毛虫";
    }

    public void SetPaused(bool paused) => _pause.Text = paused ? "恢复" : "暂停";

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
        _menu.Dispose();
    }
}
