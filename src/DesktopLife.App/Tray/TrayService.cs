using Forms = System.Windows.Forms;
namespace DesktopLife.App.Tray;
public sealed class TrayService : IDisposable
{
    private readonly Forms.NotifyIcon _icon;
    private readonly Forms.ContextMenuStrip _menu = new();
    private bool _paused;
    private readonly Forms.ToolStripItem _summary;

    public TrayService(Action togglePause, Action exit)
    {
        _summary = _menu.Items.Add("DesktopLife · 桌面生物");
        _summary.Enabled = false;
        _menu.Items.Add(new Forms.ToolStripSeparator());
        var pause = new Forms.ToolStripMenuItem("暂停");
        pause.Click += (_, _) =>
        {
            togglePause();
            _paused = !_paused;
            pause.Text = _paused ? "恢复" : "暂停";
        };
        _menu.Items.Add(pause);
        _menu.Items.Add("退出", null, (_, _) => exit());
        _icon = new Forms.NotifyIcon
        {
            Text = "DesktopLife — 苍蝇与蟑螂",
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenuStrip = _menu,
            Visible = true
        };
    }

    public void SetPopulationSummary(int screens, int flies, int cockroaches)
    {
        _summary.Text = $"{screens} 块屏幕 · {flies} 只苍蝇 · {cockroaches} 只蟑螂";
        _icon.Text = $"DesktopLife — {screens} 屏 / {flies} 苍蝇 / {cockroaches} 蟑螂";
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
        _menu.Dispose();
    }
}
