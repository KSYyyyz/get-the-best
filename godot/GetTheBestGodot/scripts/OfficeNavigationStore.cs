using System;
using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class OfficeNavigationStore : Node
{
    private static readonly Vector2I[] CardinalDirections =
    [
        Vector2I.Up,
        Vector2I.Right,
        Vector2I.Down,
        Vector2I.Left,
    ];

    private RoomFootprintStore? _roomFootprintStore;
    private FacilityPlacementStore? _facilityPlacementStore;

    public override void _Ready()
    {
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
        _facilityPlacementStore = GetNodeOrNull<FacilityPlacementStore>(
            "../FacilityPlacementStore"
        );
    }

    public bool IsInsideOffice(Vector2I cell)
    {
        return cell.X >= 0
            && cell.Y >= 0
            && cell.X < OfficeWorld3DConfig.Columns
            && cell.Y < OfficeWorld3DConfig.Rows;
    }

    public bool IsWalkable(Vector2I cell)
    {
        return IsInsideOffice(cell);
    }

    public bool IsBlocked(Vector2I cell)
    {
        return !IsWalkable(cell) || IsFacilityOccupied(cell);
    }

    public bool CanStandAt(Vector2I cell)
    {
        return CanStandAt(cell, ignoredFacilityId: null);
    }

    public bool CanStandAt(Vector2I cell, int? ignoredFacilityId)
    {
        return IsWalkable(cell) && !IsFacilityOccupied(cell, ignoredFacilityId);
    }

    public bool IsFacilityOccupied(Vector2I cell)
    {
        return IsFacilityOccupied(cell, ignoredFacilityId: null);
    }

    public bool IsFacilityOccupied(Vector2I cell, int? ignoredFacilityId)
    {
        var facility = _facilityPlacementStore?.FindAtCell(cell);
        return facility != null && facility.Id != ignoredFacilityId;
    }

    public bool IsDoorCell(Vector2I cell)
    {
        if (_roomFootprintStore == null)
        {
            return false;
        }

        foreach (var room in _roomFootprintStore.GetRooms())
        {
            if (room.DoorPlacement == null)
            {
                continue;
            }

            if (room.DoorPlacement.Cell == cell || GetDoorOutsideCell(room.DoorPlacement) == cell)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsDoorPassage(Vector2I fromCell, Vector2I toCell)
    {
        if (!AreAdjacent(fromCell, toCell) || _roomFootprintStore == null)
        {
            return false;
        }

        foreach (var room in _roomFootprintStore.GetRooms())
        {
            if (room.DoorPlacement == null)
            {
                continue;
            }

            var insideCell = room.DoorPlacement.Cell;
            var outsideCell = GetDoorOutsideCell(room.DoorPlacement);
            if (
                (fromCell == insideCell && toCell == outsideCell)
                || (fromCell == outsideCell && toCell == insideCell)
            )
            {
                return true;
            }
        }

        return false;
    }

    public bool CanMoveBetween(Vector2I fromCell, Vector2I toCell)
    {
        if (!AreAdjacent(fromCell, toCell) || !IsWalkable(fromCell) || !CanStandAt(toCell))
        {
            return false;
        }

        var fromRoom = _roomFootprintStore?.FindAtCell(fromCell);
        var toRoom = _roomFootprintStore?.FindAtCell(toCell);
        if (fromRoom == null && toRoom == null)
        {
            return true;
        }

        if (fromRoom != null && toRoom != null && fromRoom.Id == toRoom.Id)
        {
            return true;
        }

        return IsDoorPassage(fromCell, toCell);
    }

    public IReadOnlyList<Vector2I> FindPath(Vector2I startCell, Vector2I targetCell)
    {
        if (!CanStandAt(startCell) || !CanStandAt(targetCell))
        {
            return System.Array.Empty<Vector2I>();
        }

        var frontier = new Queue<Vector2I>();
        var visited = new HashSet<Vector2I> { startCell };
        var cameFrom = new Dictionary<Vector2I, Vector2I>();
        frontier.Enqueue(startCell);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current == targetCell)
            {
                return ReconstructPath(startCell, targetCell, cameFrom);
            }

            foreach (var direction in CardinalDirections)
            {
                var next = current + direction;
                if (visited.Contains(next) || !CanMoveBetween(current, next))
                {
                    continue;
                }

                visited.Add(next);
                cameFrom[next] = current;
                frontier.Enqueue(next);
            }
        }

        return System.Array.Empty<Vector2I>();
    }

    private static IReadOnlyList<Vector2I> ReconstructPath(
        Vector2I startCell,
        Vector2I targetCell,
        Dictionary<Vector2I, Vector2I> cameFrom
    )
    {
        var path = new List<Vector2I> { targetCell };
        var current = targetCell;
        while (current != startCell)
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
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

    private static bool AreAdjacent(Vector2I left, Vector2I right)
    {
        var delta = right - left;
        return Mathf.Abs(delta.X) + Mathf.Abs(delta.Y) == 1;
    }
}
