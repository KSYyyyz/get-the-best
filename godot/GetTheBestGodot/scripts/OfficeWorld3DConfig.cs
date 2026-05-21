using Godot;

namespace GetTheBestGodot;

public static class OfficeWorld3DConfig
{
    public const int SourcePixelWidth = 6400;
    public const int SourcePixelHeight = 4000;
    public const int Columns = 32;
    public const int Rows = 20;
    public const float GridSize = 10.0f;
    public const float FloorY = 0.0f;

    public static readonly Rect2 OfficeBounds = new(
        new Vector2(-Columns * GridSize / 2.0f, -Rows * GridSize / 2.0f),
        new Vector2(Columns * GridSize, Rows * GridSize)
    );

    public static bool TryWorldToCell(Vector3 worldPosition, out Vector2I cell)
    {
        var worldXZ = new Vector2(worldPosition.X, worldPosition.Z);
        if (!OfficeBounds.HasPoint(worldXZ))
        {
            cell = Vector2I.Zero;
            return false;
        }

        var local = worldXZ - OfficeBounds.Position;
        cell = new Vector2I(
            Mathf.FloorToInt(local.X / GridSize),
            Mathf.FloorToInt(local.Y / GridSize)
        );
        return true;
    }

    public static Vector3 CellToWorldPosition(Vector2I cell)
    {
        return new Vector3(
            OfficeBounds.Position.X + (cell.X + 0.5f) * GridSize,
            FloorY,
            OfficeBounds.Position.Y + (cell.Y + 0.5f) * GridSize
        );
    }

    public static Vector3 SelectionCenter(Vector2I startCell, Vector2I endCell)
    {
        var minX = Mathf.Min(startCell.X, endCell.X);
        var maxX = Mathf.Max(startCell.X, endCell.X);
        var minY = Mathf.Min(startCell.Y, endCell.Y);
        var maxY = Mathf.Max(startCell.Y, endCell.Y);
        return new Vector3(
            OfficeBounds.Position.X + (minX + (maxX - minX + 1) / 2.0f) * GridSize,
            FloorY,
            OfficeBounds.Position.Y + (minY + (maxY - minY + 1) / 2.0f) * GridSize
        );
    }

    public static Vector3 SelectionSize(Vector2I startCell, Vector2I endCell, float height)
    {
        return new Vector3(
            (Mathf.Abs(endCell.X - startCell.X) + 1) * GridSize,
            height,
            (Mathf.Abs(endCell.Y - startCell.Y) + 1) * GridSize
        );
    }
}
