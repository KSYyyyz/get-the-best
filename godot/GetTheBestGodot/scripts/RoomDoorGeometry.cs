using Godot;

namespace GetTheBestGodot;

public static class RoomDoorGeometry
{
    private const float DoorLength = OfficeWorld3DConfig.GridSize * 0.72f;
    private const float DoorThickness = OfficeWorld3DConfig.GridSize * 0.11f;
    private const float DoorHeight = OfficeWorld3DConfig.GridSize * 0.92f;
    private const float DoorY = DoorHeight / 2.0f;

    public static Vector3 GetPosition(RoomDoorPlacement doorPlacement)
    {
        var center = OfficeWorld3DConfig.CellToWorldPosition(doorPlacement.Cell);
        var halfCell = OfficeWorld3DConfig.GridSize / 2.0f;
        return doorPlacement.Side switch
        {
            RoomDoorSide.North => center + new Vector3(0.0f, DoorY, -halfCell),
            RoomDoorSide.South => center + new Vector3(0.0f, DoorY, halfCell),
            RoomDoorSide.West => center + new Vector3(-halfCell, DoorY, 0.0f),
            RoomDoorSide.East => center + new Vector3(halfCell, DoorY, 0.0f),
            _ => center + Vector3.Up * DoorY,
        };
    }

    public static Vector3 GetSize(RoomDoorSide side)
    {
        return side switch
        {
            RoomDoorSide.North or RoomDoorSide.South =>
                new Vector3(DoorLength, DoorHeight, DoorThickness),
            _ => new Vector3(DoorThickness, DoorHeight, DoorLength),
        };
    }

    public static Vector2 GetHitHalfExtents(RoomDoorSide side)
    {
        var size = GetSize(side);
        return side switch
        {
            RoomDoorSide.North or RoomDoorSide.South => new Vector2(size.X / 2.0f, size.Z / 2.0f),
            _ => new Vector2(size.X / 2.0f, size.Z / 2.0f),
        };
    }
}
