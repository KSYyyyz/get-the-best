using Godot;

namespace GetTheBestGodot;

public partial class MainController : Node2D
{
    private Label? _statusLabel;

    public override void _Ready()
    {
        _statusLabel = GetNodeOrNull<Label>("HudRoot/TopStatusBar/StatusLabel");
        var bridge = GetNodeOrNull<V2CoreBridge>("V2CoreBridge");

        if (_statusLabel != null)
        {
            _statusLabel.Text = bridge?.GetInitialStatusText() ?? "Get The Best V2-0：办公室骨架已启动";
        }
    }
}

