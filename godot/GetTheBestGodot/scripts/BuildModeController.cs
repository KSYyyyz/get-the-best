using System;
using Godot;

namespace GetTheBestGodot;

public enum RoomBuildType
{
    ResearchRoom,
    MarketRoom,
    ServerRoom,
}

public enum FacilityBuildType
{
    OfficeDesk,
    ProductWhiteboard,
    ServerRack,
}

public enum BuildToolMode
{
    Pointer,
    BuildRoom,
    DeleteRoom,
    PlaceFacility,
}

public partial class BuildModeController : Node
{
    private RoomFootprintStore? _roomFootprintStore;
    private FacilityPlacementStore? _facilityPlacementStore;
    private RoomBuildType _activeRoomType = RoomBuildType.ResearchRoom;
    private FacilityBuildType _activeFacilityType = FacilityBuildType.OfficeDesk;
    private BuildToolMode _activeToolMode = BuildToolMode.Pointer;

    public event Action? ToolModeChanged;

    public override void _Ready()
    {
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
        _facilityPlacementStore = GetNodeOrNull<FacilityPlacementStore>(
            "../FacilityPlacementStore"
        );
    }

    public bool IsSelectionLegal(Vector2I startCell, Vector2I endCell)
    {
        if (OfficeWorldConfig.CountCells(startCell, endCell) <= 0)
        {
            return false;
        }

        return _roomFootprintStore?.CanReserve(startCell, endCell) ?? true;
    }

    public string GetSelectionSummary(Vector2I startCell, Vector2I endCell)
    {
        return FormatSelectionSize(startCell, endCell);
    }

    public bool TryCreateRoom(Vector2I startCell, Vector2I endCell, out RoomFootprint? room)
    {
        if (_activeToolMode != BuildToolMode.BuildRoom)
        {
            room = null;
            return false;
        }

        if (_roomFootprintStore == null)
        {
            room = null;
            return false;
        }

        return _roomFootprintStore.TryReserve(_activeRoomType, startCell, endCell, out room);
    }

    public bool CanPlaceFacility(Vector2I cell)
    {
        return _facilityPlacementStore?.CanPlace(_activeFacilityType, cell) ?? false;
    }

    public bool TryPlaceFacility(Vector2I cell, out FacilityPlacement? facility)
    {
        if (_activeToolMode != BuildToolMode.PlaceFacility)
        {
            facility = null;
            return false;
        }

        if (_facilityPlacementStore == null)
        {
            facility = null;
            return false;
        }

        return _facilityPlacementStore.TryPlace(_activeFacilityType, cell, out facility);
    }

    public FacilityPlacement? FindFacilityAtCell(Vector2I cell)
    {
        return _facilityPlacementStore?.FindAtCell(cell);
    }

    public int DeleteFacilitiesInSelection(Vector2I startCell, Vector2I endCell)
    {
        return _facilityPlacementStore?.RemoveInSelection(startCell, endCell) ?? 0;
    }

    public bool TryDeleteRoomAtCell(Vector2I cell, out RoomFootprint? room)
    {
        if (_roomFootprintStore == null)
        {
            room = null;
            return false;
        }

        SellFixturesInSelection(cell, cell);
        return _roomFootprintStore.RemoveAtCell(cell, out room);
    }

    public int SellFixturesInSelection(Vector2I startCell, Vector2I endCell)
    {
        return DeleteFacilitiesInSelection(startCell, endCell);
    }

    public bool CanDeleteSelection(Vector2I startCell, Vector2I endCell)
    {
        return OfficeWorldConfig.CountCells(startCell, endCell) > 0;
    }

    public bool TryDeleteRoomsInSelection(
        Vector2I startCell,
        Vector2I endCell,
        out int deletedCount
    )
    {
        deletedCount = 0;
        if (_roomFootprintStore == null || !CanDeleteSelection(startCell, endCell))
        {
            return false;
        }

        SellFixturesInSelection(startCell, endCell);
        return _roomFootprintStore.RemoveCells(startCell, endCell, out deletedCount);
    }

    public RoomFootprint? FindRoomAtCell(Vector2I cell)
    {
        return _roomFootprintStore?.FindAtCell(cell);
    }

    public void SetActiveRoomType(RoomBuildType roomType)
    {
        _activeRoomType = roomType;
        SetActiveToolMode(BuildToolMode.BuildRoom);
    }

    public RoomBuildType GetActiveRoomType()
    {
        return _activeRoomType;
    }

    public void SetActiveFacilityType(FacilityBuildType facilityType)
    {
        _activeFacilityType = facilityType;
        SetActiveToolMode(BuildToolMode.PlaceFacility);
    }

    public FacilityBuildType GetActiveFacilityType()
    {
        return _activeFacilityType;
    }

    public BuildToolMode GetActiveToolMode()
    {
        return _activeToolMode;
    }

    public void StartDeleteRoomMode()
    {
        SetActiveToolMode(BuildToolMode.DeleteRoom);
    }

    public void ToggleDeleteRoomMode()
    {
        if (IsDeleteRoomMode())
        {
            CancelActiveTool();
            return;
        }

        StartDeleteRoomMode();
    }

    public void CancelActiveTool()
    {
        SetActiveToolMode(BuildToolMode.Pointer);
    }

    public bool IsPointerMode()
    {
        return _activeToolMode == BuildToolMode.Pointer;
    }

    public bool IsBuildRoomMode()
    {
        return _activeToolMode == BuildToolMode.BuildRoom;
    }

    public bool IsDeleteRoomMode()
    {
        return _activeToolMode == BuildToolMode.DeleteRoom;
    }

    public bool IsPlaceFacilityMode()
    {
        return _activeToolMode == BuildToolMode.PlaceFacility;
    }

    public string GetActiveRoomTypeLabel()
    {
        return GetRoomTypeLabel(_activeRoomType);
    }

    public string GetActiveFacilityTypeLabel()
    {
        return GetFacilityTypeLabel(_activeFacilityType);
    }

    public static string GetRoomTypeLabel(RoomBuildType roomType)
    {
        return roomType switch
        {
            RoomBuildType.ResearchRoom => "研发室",
            RoomBuildType.MarketRoom => "市场室",
            RoomBuildType.ServerRoom => "服务器室",
            _ => "未知房间",
        };
    }

    public static string GetFacilityTypeLabel(FacilityBuildType facilityType)
    {
        return facilityType switch
        {
            FacilityBuildType.OfficeDesk => "办公桌",
            FacilityBuildType.ProductWhiteboard => "产品白板",
            FacilityBuildType.ServerRack => "服务器机柜",
            _ => "未知设施",
        };
    }

    public static RoomBuildType GetRequiredRoomType(FacilityBuildType facilityType)
    {
        return facilityType switch
        {
            FacilityBuildType.ProductWhiteboard => RoomBuildType.MarketRoom,
            FacilityBuildType.ServerRack => RoomBuildType.ServerRoom,
            _ => RoomBuildType.ResearchRoom,
        };
    }

    public static string FormatSelectionSize(Vector2I startCell, Vector2I endCell)
    {
        var minX = Mathf.Min(startCell.X, endCell.X);
        var maxX = Mathf.Max(startCell.X, endCell.X);
        var minY = Mathf.Min(startCell.Y, endCell.Y);
        var maxY = Mathf.Max(startCell.Y, endCell.Y);
        return $"{maxX - minX + 1}x{maxY - minY + 1}";
    }

    private void SetActiveToolMode(BuildToolMode toolMode)
    {
        if (_activeToolMode == toolMode)
        {
            return;
        }

        _activeToolMode = toolMode;
        ToolModeChanged?.Invoke();
    }
}
