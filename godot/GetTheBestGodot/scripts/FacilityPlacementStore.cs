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

    public FacilityPlacementStore()
    {
        SeedPresetFacilities();
    }

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
        if (!IsCellInsideOffice(cell))
        {
            issue = FacilityPlacementIssue.MissingRequiredRoom;
            return false;
        }

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

    public bool CanMoveFacility(FacilityPlacement facility, Vector2I targetCell)
    {
        if (!IsCellInsideOffice(targetCell))
        {
            return false;
        }

        if (FindAtCellExcluding(facility.Id, targetCell) != null)
        {
            return false;
        }

        var room = _roomFootprintStore?.FindAtCell(targetCell);
        return room != null && room.RoomType == RequiredRooms[facility.FacilityType];
    }

    public bool TryMoveFacility(
        int facilityId,
        Vector2I targetCell,
        out FacilityPlacement? movedFacility
    )
    {
        movedFacility = null;
        for (var index = 0; index < _facilities.Count; index++)
        {
            var facility = _facilities[index];
            if (facility.Id != facilityId)
            {
                continue;
            }

            if (!CanMoveFacility(facility, targetCell))
            {
                return false;
            }

            movedFacility = facility with { Cell = targetCell };
            _facilities[index] = movedFacility;
            return true;
        }

        return false;
    }

    public bool TryPlace(
        FacilityBuildType facilityType,
        Vector2I cell,
        FacilityFacing facing,
        out FacilityPlacement? facility
    )
    {
        if (!CanPlace(facilityType, cell))
        {
            facility = null;
            return false;
        }

        facility = new FacilityPlacement(_nextFacilityId, facilityType, cell, facing);
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

    public FacilityPlacement? FindAtCellExcluding(int facilityId, Vector2I cell)
    {
        for (var index = _facilities.Count - 1; index >= 0; index--)
        {
            var facility = _facilities[index];
            if (facility.Id != facilityId && facility.Cell == cell)
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

    private void SeedPresetFacilities()
    {
        AddPresetFacility(FacilityBuildType.OfficeDesk, new Vector2I(9, 5), FacilityFacing.South);
        AddPresetFacility(FacilityBuildType.OfficeDesk, new Vector2I(11, 5), FacilityFacing.South);
        AddPresetFacility(
            FacilityBuildType.ProductWhiteboard,
            new Vector2I(17, 5),
            FacilityFacing.South
        );
        AddPresetFacility(FacilityBuildType.ServerRack, new Vector2I(22, 5), FacilityFacing.South);
    }

    private void AddPresetFacility(
        FacilityBuildType facilityType,
        Vector2I cell,
        FacilityFacing facing
    )
    {
        _facilities.Add(new FacilityPlacement(_nextFacilityId, facilityType, cell, facing));
        _nextFacilityId++;
    }

    private static bool IsCellInsideOffice(Vector2I cell)
    {
        return cell.X >= 0
            && cell.Y >= 0
            && cell.X < OfficeWorld3DConfig.Columns
            && cell.Y < OfficeWorld3DConfig.Rows;
    }
}

public enum FacilityPlacementIssue
{
    None,
    Occupied,
    MissingRequiredRoom,
    WrongRoomType,
}

public enum FacilityFacing
{
    North,
    East,
    South,
    West,
}

public sealed record FacilityPlacement(
    int Id,
    FacilityBuildType FacilityType,
    Vector2I Cell,
    FacilityFacing Facing
);
