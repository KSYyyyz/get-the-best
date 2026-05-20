using Godot;

namespace GetTheBestGodot;

public static class OfficeWorldConfig
{
    public const float GridSize = 80.0f;
    public static readonly Rect2 OfficeBounds = new(new Vector2(-1600, -1000), new Vector2(3200, 2000));

    public static bool TryWorldToCell(Vector2 worldPosition, out Vector2I cell)
    {
        if (!OfficeBounds.HasPoint(worldPosition))
        {
            cell = Vector2I.Zero;
            return false;
        }

        var local = worldPosition - OfficeBounds.Position;
        cell = new Vector2I(
            Mathf.FloorToInt(local.X / GridSize),
            Mathf.FloorToInt(local.Y / GridSize)
        );
        return true;
    }

    public static Rect2 CellToWorldRect(Vector2I cell)
    {
        return new Rect2(
            OfficeBounds.Position + new Vector2(cell.X * GridSize, cell.Y * GridSize),
            new Vector2(GridSize, GridSize)
        );
    }

    public static Rect2 CellsToWorldRect(Vector2I startCell, Vector2I endCell)
    {
        var minX = Mathf.Min(startCell.X, endCell.X);
        var minY = Mathf.Min(startCell.Y, endCell.Y);
        var maxX = Mathf.Max(startCell.X, endCell.X);
        var maxY = Mathf.Max(startCell.Y, endCell.Y);

        return new Rect2(
            OfficeBounds.Position + new Vector2(minX * GridSize, minY * GridSize),
            new Vector2((maxX - minX + 1) * GridSize, (maxY - minY + 1) * GridSize)
        );
    }

    public static int CountCells(Vector2I startCell, Vector2I endCell)
    {
        return (Mathf.Abs(endCell.X - startCell.X) + 1) * (Mathf.Abs(endCell.Y - startCell.Y) + 1);
    }
}
