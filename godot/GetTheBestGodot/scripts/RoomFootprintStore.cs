using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class RoomFootprintStore : Node
{
    private readonly List<RoomFootprint> _rooms = [];
    private int _nextRoomId = 1;

    public RoomFootprintStore()
    {
        SeedPresetOfficeRooms();
    }

    public IReadOnlyList<RoomFootprint> GetRooms()
    {
        return _rooms;
    }

    public bool CanReserve(Vector2I startCell, Vector2I endCell)
    {
        var candidate = RoomFootprint.FromCells(0, RoomBuildType.ResearchRoom, startCell, endCell);
        if (candidate.CellCount <= 0)
        {
            return false;
        }

        foreach (var room in _rooms)
        {
            if (room.Overlaps(candidate))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryReserve(
        RoomBuildType roomType,
        Vector2I startCell,
        Vector2I endCell,
        out RoomFootprint? room
    )
    {
        return TryReserve(roomType, startCell, endCell, doorPlacement: null, out room);
    }

    public bool TryReserve(
        RoomBuildType roomType,
        Vector2I startCell,
        Vector2I endCell,
        RoomDoorPlacement? doorPlacement,
        out RoomFootprint? room
    )
    {
        if (!CanReserve(startCell, endCell))
        {
            room = null;
            return false;
        }

        room = RoomFootprint.FromCells(_nextRoomId, roomType, startCell, endCell, doorPlacement);
        _nextRoomId++;
        _rooms.Add(room);
        return true;
    }

    public RoomFootprint? FindAtCell(Vector2I cell)
    {
        for (var index = _rooms.Count - 1; index >= 0; index--)
        {
            var room = _rooms[index];
            if (room.Contains(cell))
            {
                return room;
            }
        }

        return null;
    }

    public bool RemoveAtCell(Vector2I cell, out RoomFootprint? room)
    {
        for (var index = _rooms.Count - 1; index >= 0; index--)
        {
            room = _rooms[index];
            if (!room.RemoveCell(cell))
            {
                continue;
            }

            if (room.IsEmpty)
            {
                _rooms.RemoveAt(index);
            }

            return true;
        }

        room = null;
        return false;
    }

    public bool RemoveDoorOwnerAtAdjacentCell(Vector2I cell, out RoomFootprint? room)
    {
        for (var index = _rooms.Count - 1; index >= 0; index--)
        {
            room = _rooms[index];
            if (room.DoorPlacement == null || GetDoorOutsideCell(room.DoorPlacement) != cell)
            {
                continue;
            }

            room.RemoveCell(room.DoorPlacement.Cell);
            if (room.IsEmpty)
            {
                _rooms.RemoveAt(index);
            }

            return true;
        }

        room = null;
        return false;
    }

    public bool RemoveDoorOwnerAtWorldPosition(Vector3 worldPosition, out RoomFootprint? room)
    {
        for (var index = _rooms.Count - 1; index >= 0; index--)
        {
            room = _rooms[index];
            if (room.DoorPlacement == null || !IsWorldPositionOnDoor(worldPosition, room.DoorPlacement))
            {
                continue;
            }

            room.RemoveCell(room.DoorPlacement.Cell);
            if (room.IsEmpty)
            {
                _rooms.RemoveAt(index);
            }

            return true;
        }

        room = null;
        return false;
    }

    public bool RemoveCells(Vector2I startCell, Vector2I endCell, out int deletedCellCount)
    {
        deletedCellCount = 0;
        var selection = RoomFootprint.FromCells(0, RoomBuildType.ResearchRoom, startCell, endCell);
        if (selection.CellCount <= 0)
        {
            return false;
        }

        for (var index = _rooms.Count - 1; index >= 0; index--)
        {
            deletedCellCount += _rooms[index].RemoveCells(selection);
            if (_rooms[index].IsEmpty)
            {
                _rooms.RemoveAt(index);
            }
        }

        return deletedCellCount > 0;
    }

    private static Vector2I GetDoorOutsideCell(RoomDoorPlacement doorPlacement)
    {
        return doorPlacement.Side switch
        {
            RoomDoorSide.North => doorPlacement.Cell + Vector2I.Up,
            RoomDoorSide.South => doorPlacement.Cell + Vector2I.Down,
            RoomDoorSide.West => doorPlacement.Cell + Vector2I.Left,
            RoomDoorSide.East => doorPlacement.Cell + Vector2I.Right,
            _ => doorPlacement.Cell,
        };
    }

    private static bool IsWorldPositionOnDoor(Vector3 worldPosition, RoomDoorPlacement doorPlacement)
    {
        var delta = worldPosition - RoomDoorGeometry.GetPosition(doorPlacement);
        var halfExtents = RoomDoorGeometry.GetHitHalfExtents(doorPlacement.Side);
        return doorPlacement.Side switch
        {
            RoomDoorSide.North or RoomDoorSide.South =>
                Mathf.Abs(delta.X) <= halfExtents.X && Mathf.Abs(delta.Z) <= halfExtents.Y,
            RoomDoorSide.West or RoomDoorSide.East =>
                Mathf.Abs(delta.X) <= halfExtents.X && Mathf.Abs(delta.Z) <= halfExtents.Y,
            _ => false,
        };
    }

    private void SeedPresetOfficeRooms()
    {
        AddPresetRoom(
            RoomBuildType.ResearchRoom,
            new Vector2I(7, 4),
            new Vector2I(13, 9),
            new RoomDoorPlacement(new Vector2I(10, 9), RoomDoorSide.South)
        );
        AddPresetRoom(
            RoomBuildType.MarketRoom,
            new Vector2I(15, 4),
            new Vector2I(19, 8),
            new RoomDoorPlacement(new Vector2I(17, 8), RoomDoorSide.South)
        );
        AddPresetRoom(
            RoomBuildType.ServerRoom,
            new Vector2I(21, 4),
            new Vector2I(24, 8),
            new RoomDoorPlacement(new Vector2I(22, 8), RoomDoorSide.South)
        );
    }

    private void AddPresetRoom(
        RoomBuildType roomType,
        Vector2I startCell,
        Vector2I endCell,
        RoomDoorPlacement doorPlacement
    )
    {
        _rooms.Add(RoomFootprint.FromCells(_nextRoomId, roomType, startCell, endCell, doorPlacement));
        _nextRoomId++;
    }
}

public enum RoomDoorSide
{
    North,
    South,
    West,
    East,
}

public sealed record RoomDoorPlacement(Vector2I Cell, RoomDoorSide Side);

public sealed class RoomFootprint
{
    private readonly HashSet<Vector2I> _cells;

    private RoomFootprint(
        int id,
        RoomBuildType roomType,
        HashSet<Vector2I> cells,
        RoomDoorPlacement? doorPlacement
    )
    {
        Id = id;
        RoomType = roomType;
        _cells = cells;
        DoorPlacement = doorPlacement;
    }

    public int Id { get; }
    public RoomBuildType RoomType { get; }
    public RoomDoorPlacement? DoorPlacement { get; private set; }
    public IReadOnlyCollection<Vector2I> Cells => _cells;
    public Vector2I MinCell => GetBounds().MinCell;
    public Vector2I MaxCell => GetBounds().MaxCell;
    public int Width => MaxCell.X - MinCell.X + 1;
    public int Height => MaxCell.Y - MinCell.Y + 1;
    public int CellCount => _cells.Count;
    public bool IsEmpty => _cells.Count == 0;

    public static RoomFootprint FromCells(
        int id,
        RoomBuildType roomType,
        Vector2I startCell,
        Vector2I endCell,
        RoomDoorPlacement? doorPlacement = null
    )
    {
        var minCell = new Vector2I(
            Mathf.Min(startCell.X, endCell.X),
            Mathf.Min(startCell.Y, endCell.Y)
        );
        var maxCell = new Vector2I(
            Mathf.Max(startCell.X, endCell.X),
            Mathf.Max(startCell.Y, endCell.Y)
        );
        var cells = new HashSet<Vector2I>();
        for (var y = minCell.Y; y <= maxCell.Y; y++)
        {
            for (var x = minCell.X; x <= maxCell.X; x++)
            {
                cells.Add(new Vector2I(x, y));
            }
        }

        return new RoomFootprint(id, roomType, cells, doorPlacement);
    }

    public bool Contains(Vector2I cell)
    {
        return _cells.Contains(cell);
    }

    public bool Overlaps(RoomFootprint other)
    {
        foreach (var cell in other.Cells)
        {
            if (_cells.Contains(cell))
            {
                return true;
            }
        }

        return false;
    }

    public bool RemoveCell(Vector2I cell)
    {
        var removed = _cells.Remove(cell);
        if (removed)
        {
            ClearDoorIfRemoved();
        }

        return removed;
    }

    public int RemoveCells(RoomFootprint selection)
    {
        var removed = 0;
        foreach (var cell in selection.Cells)
        {
            if (_cells.Remove(cell))
            {
                removed++;
            }
        }

        if (removed > 0)
        {
            ClearDoorIfRemoved();
        }

        return removed;
    }

    public Rect2 ToWorldRect()
    {
        return OfficeWorldConfig.CellsToWorldRect(MinCell, MaxCell);
    }

    private RoomBounds GetBounds()
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        foreach (var cell in _cells)
        {
            minX = Mathf.Min(minX, cell.X);
            minY = Mathf.Min(minY, cell.Y);
            maxX = Mathf.Max(maxX, cell.X);
            maxY = Mathf.Max(maxY, cell.Y);
        }

        return new RoomBounds(new Vector2I(minX, minY), new Vector2I(maxX, maxY));
    }

    private void ClearDoorIfRemoved()
    {
        if (DoorPlacement != null && !_cells.Contains(DoorPlacement.Cell))
        {
            DoorPlacement = null;
        }
    }

    private readonly record struct RoomBounds(Vector2I MinCell, Vector2I MaxCell);
}
