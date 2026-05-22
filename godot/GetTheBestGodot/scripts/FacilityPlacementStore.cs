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
    private OfficeNavigationStore? _officeNavigationStore;

    public FacilityPlacementStore()
    {
        SeedPresetFacilities();
    }

    public override void _Ready()
    {
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
        _officeNavigationStore = GetNodeOrNull<OfficeNavigationStore>("../OfficeNavigationStore");
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
        if (_officeNavigationStore?.IsFacilityOccupied(cell) == true)
        {
            issue = FacilityPlacementIssue.Occupied;
            return false;
        }

        if (!(_officeNavigationStore?.CanStandAt(cell) == true))
        {
            issue = FacilityPlacementIssue.MissingRequiredRoom;
            return false;
        }

        if (_officeNavigationStore?.IsDoorCell(cell) == true)
        {
            issue = FacilityPlacementIssue.DoorPassage;
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
        if (!(_officeNavigationStore?.CanStandAt(targetCell, facility.Id) == true))
        {
            return false;
        }

        if (_officeNavigationStore?.IsDoorCell(targetCell) == true)
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
        return TryMoveFacility(facilityId, targetCell, facing: null, out movedFacility);
    }

    public bool TryMoveFacility(
        int facilityId,
        Vector2I targetCell,
        FacilityFacing facing,
        out FacilityPlacement? movedFacility
    )
    {
        return TryMoveFacility(facilityId, targetCell, (FacilityFacing?)facing, out movedFacility);
    }

    private bool TryMoveFacility(
        int facilityId,
        Vector2I targetCell,
        FacilityFacing? facing,
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

            movedFacility = facility with
            {
                Cell = targetCell,
                Facing = facing ?? facility.Facing,
            };
            _facilities[index] = movedFacility;
            return true;
        }

        return false;
    }

    public FacilityPlacement? FindById(int facilityId)
    {
        foreach (var facility in _facilities)
        {
            if (facility.Id == facilityId)
            {
                return facility;
            }
        }

        return null;
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
}

public enum FacilityPlacementIssue
{
    None,
    Occupied,
    MissingRequiredRoom,
    WrongRoomType,
    DoorPassage,
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
