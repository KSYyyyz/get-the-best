using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class FacilityPlacementStore : Node
{
    private static readonly Dictionary<FacilityBuildType, RoomBuildType> RequiredRooms = new()
    {
        [FacilityBuildType.OfficeDesk] = RoomBuildType.ResearchRoom,
        [FacilityBuildType.ProductWhiteboard] = RoomBuildType.MarketRoom,
        [FacilityBuildType.ServerRack] = RoomBuildType.ServerRoom,
    };

    private readonly List<FacilityPlacement> _facilities = [];
    private int _nextFacilityId = 1;
    private RoomFootprintStore? _roomFootprintStore;

    public override void _Ready()
    {
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
    }

    public IReadOnlyList<FacilityPlacement> GetFacilities()
    {
        return _facilities;
    }

    public bool CanPlace(FacilityBuildType facilityType, Vector2I cell)
    {
        return CanPlace(facilityType, cell, out _);
    }

    public bool CanPlace(FacilityBuildType facilityType, Vector2I cell, out FacilityPlacementIssue issue)
    {
        if (FindAtCell(cell) != null)
        {
            issue = FacilityPlacementIssue.Occupied;
            return false;
        }

        var room = _roomFootprintStore?.FindAtCell(cell);
        if (room == null)
        {
            issue = FacilityPlacementIssue.MissingRequiredRoom;
            return false;
        }

        if (room.RoomType != RequiredRooms[facilityType])
        {
            issue = FacilityPlacementIssue.WrongRoomType;
            return false;
        }

        issue = FacilityPlacementIssue.None;
        return true;
    }

    public bool TryPlace(FacilityBuildType facilityType, Vector2I cell, out FacilityPlacement? facility)
    {
        if (!CanPlace(facilityType, cell))
        {
            facility = null;
            return false;
        }

        facility = new FacilityPlacement(_nextFacilityId, facilityType, cell);
        _nextFacilityId++;
        _facilities.Add(facility);
        return true;
    }

    public FacilityPlacement? FindAtCell(Vector2I cell)
    {
        for (var index = _facilities.Count - 1; index >= 0; index--)
        {
            var facility = _facilities[index];
            if (facility.Cell == cell)
            {
                return facility;
            }
        }

        return null;
    }

    public int RemoveInSelection(Vector2I startCell, Vector2I endCell)
    {
        var minX = Mathf.Min(startCell.X, endCell.X);
        var maxX = Mathf.Max(startCell.X, endCell.X);
        var minY = Mathf.Min(startCell.Y, endCell.Y);
        var maxY = Mathf.Max(startCell.Y, endCell.Y);
        var removed = 0;

        for (var index = _facilities.Count - 1; index >= 0; index--)
        {
            var cell = _facilities[index].Cell;
            if (cell.X < minX || cell.X > maxX || cell.Y < minY || cell.Y > maxY)
            {
                continue;
            }

            _facilities.RemoveAt(index);
            removed++;
        }

        return removed;
    }
}

public enum FacilityPlacementIssue
{
    None,
    Occupied,
    MissingRequiredRoom,
    WrongRoomType,
}

public sealed record FacilityPlacement(int Id, FacilityBuildType FacilityType, Vector2I Cell);
