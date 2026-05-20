using Godot;

namespace GetTheBestGodot;

public partial class MainController : Node2D
{
    private Label? _statusLabel;
    private Control? _topStatusBar;
    private Control? _buildModePanel;
    private Control? _floatingTooltip;
    private Vector2 _lastViewportSize;

    public override void _Ready()
    {
        _topStatusBar = GetNodeOrNull<Control>("HudRoot/TopStatusBar");
        _buildModePanel = GetNodeOrNull<Control>("HudRoot/BuildModePanel");
        _floatingTooltip = GetNodeOrNull<Control>("HudRoot/FloatingTooltip");
        _statusLabel = GetNodeOrNull<Label>("HudRoot/TopStatusBar/StatusLabel");
        var bridge = GetNodeOrNull<V2CoreBridge>("V2CoreBridge");

        if (_statusLabel != null)
        {
            _statusLabel.Text = bridge?.GetInitialStatusText() ?? "Get The Best V2-0：办公室骨架已启动";
        }

        if (GetNodeOrNull<Control>("HudRoot") is { } hudRoot)
        {
            RemoveTextShadow(hudRoot);
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

        if (_buildModePanel != null)
        {
            var width = Mathf.Clamp(viewportSize.X * 0.42f, 460.0f, 680.0f);
            _buildModePanel.Position = new Vector2(
                Mathf.Max(16.0f, viewportSize.X - width - 16.0f),
                12.0f
            );
            _buildModePanel.Size = new Vector2(width, 112.0f);
        }

        if (_floatingTooltip != null)
        {
            _floatingTooltip.Size = new Vector2(168.0f, 34.0f);
        }
    }

    private static void RemoveTextShadow(Control control)
    {
        control.AddThemeColorOverride("font_shadow_color", new Color(0.0f, 0.0f, 0.0f, 0.0f));
        control.AddThemeConstantOverride("shadow_offset_x", 0);
        control.AddThemeConstantOverride("shadow_offset_y", 0);
        control.AddThemeConstantOverride("outline_size", 0);

        foreach (var child in control.GetChildren())
        {
            if (child is Control childControl)
            {
                RemoveTextShadow(childControl);
            }
        }
    }
}
