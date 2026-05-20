using Godot;

namespace GetTheBestGodot;

public partial class MainController : Node2D
{
    private Label? _statusLabel;
    private Control? _topStatusBar;
    private Control? _contextPanel;
    private Vector2 _lastViewportSize;

    public override void _Ready()
    {
        _topStatusBar = GetNodeOrNull<Control>("HudRoot/TopStatusBar");
        _contextPanel = GetNodeOrNull<Control>("HudRoot/ContextPanel");
        _statusLabel = GetNodeOrNull<Label>("HudRoot/TopStatusBar/StatusLabel");
        var bridge = GetNodeOrNull<V2CoreBridge>("V2CoreBridge");

        if (_statusLabel != null)
        {
            _statusLabel.Text = bridge?.GetInitialStatusText() ?? "Get The Best V2-0：办公室骨架已启动";
        }

        LayoutHud();
    }

    public override void _Process(double delta)
    {
        var viewportSize = GetViewportRect().Size;
        if (viewportSize != _lastViewportSize)
        {
            LayoutHud();
        }
    }

    private void LayoutHud()
    {
        var viewportSize = GetViewportRect().Size;
        _lastViewportSize = viewportSize;

        if (_topStatusBar != null)
        {
            _topStatusBar.Position = new Vector2(16.0f, 12.0f);
            _topStatusBar.Size = new Vector2(Mathf.Clamp(viewportSize.X * 0.58f, 520.0f, 920.0f), 40.0f);
        }

        if (_contextPanel != null)
        {
            var width = Mathf.Clamp(viewportSize.X * 0.38f, 420.0f, 600.0f);
            _contextPanel.Position = new Vector2(16.0f, Mathf.Max(72.0f, viewportSize.Y - 140.0f));
            _contextPanel.Size = new Vector2(width, 116.0f);
        }
    }
}
