using Godot;

namespace GetTheBestGodot;

public partial class OfficeGridRenderer : Node2D
{
    private static readonly Color GridColor = new(0.62f, 0.72f, 0.66f, 0.62f);
    private static readonly Color BorderColor = new(0.68f, 0.78f, 0.72f, 0.88f);

    public override void _Ready()
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        var bounds = OfficeWorldConfig.OfficeBounds;
        var gridSize = OfficeWorldConfig.GridSize;

        for (var x = bounds.Position.X; x <= bounds.End.X; x += gridSize)
        {
            DrawLine(new Vector2(x, bounds.Position.Y), new Vector2(x, bounds.End.Y), GridColor, 2.0f);
        }

        for (var y = bounds.Position.Y; y <= bounds.End.Y; y += gridSize)
        {
            DrawLine(new Vector2(bounds.Position.X, y), new Vector2(bounds.End.X, y), GridColor, 2.0f);
        }

        DrawRect(bounds, BorderColor, filled: false, width: 4.0f);
    }
}
