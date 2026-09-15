using DesktopLife.App.Settings;
using Forms = System.Windows.Forms;
namespace DesktopLife.App.Tray;
public sealed class TrayService : IDisposable
{
    private readonly Forms.NotifyIcon _icon;
    private readonly Forms.ContextMenuStrip _menu = new();
    private readonly Forms.ToolStripMenuItem _pause;
    private readonly Forms.ToolStripItem _summary;
    private readonly Forms.ToolStripItem _settings, _exit;
    private bool _paused;
    private int _screens, _flies, _roaches, _ants, _caterpillars;

    public TrayService(Action showSettings, Action togglePause, Action exit)
    {
        _summary = _menu.Items.Add("DesktopLife · 桌面生物");
        _summary.Enabled = false;
        _menu.Items.Add(new Forms.ToolStripSeparator());
        _settings = _menu.Items.Add("数量设置…", null, (_, _) => showSettings());
        _pause = new Forms.ToolStripMenuItem("暂停");
        _pause.Click += (_, _) => togglePause();
        _menu.Items.Add(_pause);
        _exit = _menu.Items.Add("退出", null, (_, _) => exit());
        _icon = new Forms.NotifyIcon
        {
            Text = "DesktopLife — 苍蝇与蟑螂",
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenuStrip = _menu,
            Visible = true
        };
        _icon.DoubleClick += (_, _) => showSettings();
        LanguageService.Changed += Translate;
        Translate();
    }

    public void SetPopulationSummary(int screens, int flies, int cockroaches, int ants, int caterpillars)
    {
        _screens = screens; _flies = flies; _roaches = cockroaches; _ants = ants; _caterpillars = caterpillars;
        Translate();
    }
    private void Translate()
    {
        _settings.Text = LanguageService.Get("Settings");
        _exit.Text = LanguageService.Get("Exit");
        _pause.Text = LanguageService.Get(_paused ? "Resume" : "Stop");
        _summary.Text = LanguageService.Choose($"{_screens} 屏 · {_flies} 苍蝇 · {_roaches} 蟑螂 · {_ants} 蚂蚁 · {_caterpillars} 毛毛虫",
            $"{_screens} displays · {_flies} fly · {_roaches} roaches · {_ants} ants · {_caterpillars} caterpillars");
        _icon.Text = "DesktopLife — " + _summary.Text;
    }
    public void SetPaused(bool paused) { _paused = paused; Translate(); }
    public void Dispose()
    {
        LanguageService.Changed -= Translate;
        _icon.Visible = false;
        _icon.Dispose();
        _menu.Dispose();
    }
}
