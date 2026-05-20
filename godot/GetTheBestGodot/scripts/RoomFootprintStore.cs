using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class RoomFootprintStore : Node
{
    private readonly List<RoomFootprint> _rooms = [];
    private int _nextRoomId = 1;

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

    public bool TryReserve(RoomBuildType roomType, Vector2I startCell, Vector2I endCell, out RoomFootprint? room)
    {
        if (!CanReserve(startCell, endCell))
        {
            room = null;
            return false;
        }

        room = RoomFootprint.FromCells(_nextRoomId, roomType, startCell, endCell);
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
            if (!_rooms[index].Contains(cell))
            {
                continue;
            }

            room = _rooms[index];
            _rooms.RemoveAt(index);
            return true;
        }

        room = null;
        return false;
    }

    public bool RemoveOverlapping(Vector2I startCell, Vector2I endCell, out int deletedCount)
    {
        deletedCount = 0;
        var selection = RoomFootprint.FromCells(0, RoomBuildType.ResearchRoom, startCell, endCell);
        if (selection.CellCount <= 0)
        {
            return false;
        }

        for (var index = _rooms.Count - 1; index >= 0; index--)
        {
            if (!_rooms[index].Overlaps(selection))
            {
                continue;
            }

            _rooms.RemoveAt(index);
            deletedCount++;
        }

        return deletedCount > 0;
    }
}

public sealed class RoomFootprint
{
    private RoomFootprint(int id, RoomBuildType roomType, Vector2I minCell, Vector2I maxCell)
    {
        Id = id;
        RoomType = roomType;
        MinCell = minCell;
        MaxCell = maxCell;
    }

    public int Id { get; }
    public RoomBuildType RoomType { get; }
    public Vector2I MinCell { get; }
    public Vector2I MaxCell { get; }
    public int Width => MaxCell.X - MinCell.X + 1;
    public int Height => MaxCell.Y - MinCell.Y + 1;
    public int CellCount => Width * Height;

    public static RoomFootprint FromCells(
        int id,
        RoomBuildType roomType,
        Vector2I startCell,
        Vector2I endCell
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
        return new RoomFootprint(id, roomType, minCell, maxCell);
    }

    public bool Contains(Vector2I cell)
    {
        return cell.X >= MinCell.X
            && cell.X <= MaxCell.X
            && cell.Y >= MinCell.Y
            && cell.Y <= MaxCell.Y;
    }

    public bool Overlaps(RoomFootprint other)
    {
        return MinCell.X <= other.MaxCell.X
            && MaxCell.X >= other.MinCell.X
            && MinCell.Y <= other.MaxCell.Y
            && MaxCell.Y >= other.MinCell.Y;
    }

    public Rect2 ToWorldRect()
    {
        return OfficeWorldConfig.CellsToWorldRect(MinCell, MaxCell);
    }
}
