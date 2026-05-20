using Godot;

namespace GetTheBestGodot;

public partial class OfficeSelectionController : Node2D
{
    private static readonly Rect2 OfficeBounds = new(new Vector2(160, 120), new Vector2(960, 540));
    private Label? _contextLabel;

    public override void _Ready()
    {
        _contextLabel = GetNodeOrNull<Label>("../HudRoot/ContextPanel/ContextLabel");
        if (_contextLabel != null)
        {
            _contextLabel.Text = "未选中对象";
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent)
        {
            return;
        }
        if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        var camera = GetViewport().GetCamera2D();
        var worldPosition = camera?.GetGlobalMousePosition() ?? mouseEvent.Position;

        if (_contextLabel == null)
        {
            return;
        }

        _contextLabel.Text = OfficeBounds.HasPoint(worldPosition)
            ? $"已选中办公室区域：{FormatPosition(worldPosition)}"
            : "未选中对象";
    }

    private static string FormatPosition(Vector2 position)
    {
        return $"x={Mathf.RoundToInt(position.X)}, y={Mathf.RoundToInt(position.Y)}";
    }
}

