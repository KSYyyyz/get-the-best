using System;
using System.Collections.Generic;
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
    PlaceRoomDoor,
    DeleteRoom,
    PlaceFacility,
}

public sealed record PendingRoomSelection(
    RoomBuildType RoomType,
    Vector2I StartCell,
    Vector2I EndCell,
    RoomDoorPlacement? DoorPlacement
);

public partial class BuildModeController : Node
{
    private const int MinimumRoomWidth = 2;
    private const int MinimumRoomHeight = 3;
    private RoomFootprintStore? _roomFootprintStore;
    private FacilityPlacementStore? _facilityPlacementStore;
    private RoomBuildType _activeRoomType = RoomBuildType.ResearchRoom;
    private FacilityBuildType _activeFacilityType = FacilityBuildType.OfficeDesk;
    private FacilityFacing _activeFacilityFacing = FacilityFacing.South;
    private BuildToolMode _activeToolMode = BuildToolMode.Pointer;
    private PendingRoomSelection? _pendingRoomSelection;
    private string _buildStatusMessage = string.Empty;

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

        return MeetsMinimumRoomSize(startCell, endCell)
            && (_roomFootprintStore?.CanReserve(startCell, endCell) ?? true);
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

    public bool TryStartPendingRoomSelection(Vector2I startCell, Vector2I endCell)
    {
        if (_activeToolMode != BuildToolMode.BuildRoom)
        {
            return false;
        }

        if (!IsSelectionLegal(startCell, endCell))
        {
            ShowBuildStatus(GetRoomBuildFailureMessage(startCell, endCell));
            return false;
        }

        ClearBuildStatus();
        _pendingRoomSelection = new PendingRoomSelection(
            _activeRoomType,
            startCell,
            endCell,
            DoorPlacement: null
        );
        SetActiveToolMode(BuildToolMode.PlaceRoomDoor);
        ToolModeChanged?.Invoke();
        return true;
    }

    public bool TrySetPendingDoor(Vector2I cell, RoomDoorSide side)
    {
        if (_pendingRoomSelection == null || !IsDoorOnPendingBoundary(cell, side))
        {
            return false;
        }

        _pendingRoomSelection = _pendingRoomSelection with
        {
            DoorPlacement = new RoomDoorPlacement(cell, side),
        };
        ClearBuildStatus();
        ToolModeChanged?.Invoke();
        return true;
    }

    public bool TrySetPendingDoorFromWorldPosition(Vector2I cell, Vector3 worldPosition)
    {
        if (_pendingRoomSelection == null)
        {
            return false;
        }

        var candidateSides = new List<(RoomDoorSide Side, float Distance)>();
        var roomBounds = GetPendingRoomBounds();
        var cellCenter = OfficeWorld3DConfig.CellToWorldPosition(cell);
        var local = worldPosition - cellCenter;
        var halfCell = OfficeWorld3DConfig.GridSize / 2.0f;

        if (cell.Y == roomBounds.MinCell.Y)
        {
            candidateSides.Add((RoomDoorSide.North, Mathf.Abs(local.Z + halfCell)));
        }
        if (cell.Y == roomBounds.MaxCell.Y)
        {
            candidateSides.Add((RoomDoorSide.South, Mathf.Abs(local.Z - halfCell)));
        }
        if (cell.X == roomBounds.MinCell.X)
        {
            candidateSides.Add((RoomDoorSide.West, Mathf.Abs(local.X + halfCell)));
        }
        if (cell.X == roomBounds.MaxCell.X)
        {
            candidateSides.Add((RoomDoorSide.East, Mathf.Abs(local.X - halfCell)));
        }

        if (candidateSides.Count == 0)
        {
            return false;
        }

        candidateSides.Sort((left, right) => left.Distance.CompareTo(right.Distance));
        return TrySetPendingDoor(cell, candidateSides[0].Side);
    }

    public bool ConfirmPendingRoom(out RoomFootprint? room)
    {
        room = null;
        if (_pendingRoomSelection?.DoorPlacement == null || _roomFootprintStore == null)
        {
            return false;
        }

        var selection = _pendingRoomSelection;
        var created = _roomFootprintStore.TryReserve(
            selection.RoomType,
            selection.StartCell,
            selection.EndCell,
            selection.DoorPlacement,
            out room
        );
        if (!created)
        {
            return false;
        }

        _pendingRoomSelection = null;
        ClearBuildStatus();
        SetActiveToolMode(BuildToolMode.BuildRoom);
        ToolModeChanged?.Invoke();
        return true;
    }

    public void CancelPendingRoomSelection()
    {
        if (_pendingRoomSelection == null)
        {
            return;
        }

        _pendingRoomSelection = null;
        ClearBuildStatus();
        SetActiveToolMode(BuildToolMode.BuildRoom);
        ToolModeChanged?.Invoke();
    }

    public PendingRoomSelection? GetPendingRoomSelection()
    {
        return _pendingRoomSelection;
    }

    public bool HasPendingRoomSelection()
    {
        return _pendingRoomSelection != null;
    }

    public bool HasPendingDoor()
    {
        return _pendingRoomSelection?.DoorPlacement != null;
    }

    public bool CanConfirmPendingRoom()
    {
        return _pendingRoomSelection?.DoorPlacement != null;
    }

    public string GetBuildStatusMessage()
    {
        return _buildStatusMessage;
    }

    public void ShowBuildStatus(string message)
    {
        _buildStatusMessage = message;
        ToolModeChanged?.Invoke();
    }

    public void ClearBuildStatus()
    {
        if (string.IsNullOrEmpty(_buildStatusMessage))
        {
            return;
        }

        _buildStatusMessage = string.Empty;
        ToolModeChanged?.Invoke();
    }

    public bool CanPlaceFacility(Vector2I cell)
    {
        return _facilityPlacementStore?.CanPlace(_activeFacilityType, cell) ?? false;
    }

    public FacilityPlacementIssue GetFacilityPlacementIssue(Vector2I cell)
    {
        if (_facilityPlacementStore == null)
        {
            return FacilityPlacementIssue.MissingRequiredRoom;
        }

        _facilityPlacementStore.CanPlace(_activeFacilityType, cell, out var issue);
        return issue;
    }

    public string GetFacilityPlacementFailureMessage(Vector2I cell)
    {
        return GetFacilityPlacementIssue(cell) switch
        {
            FacilityPlacementIssue.Occupied => "\u683c\u5b50\u5df2\u5360\u7528",
            FacilityPlacementIssue.WrongRoomType =>
                $"\u9700\u8981{GetRoomTypeLabel(GetRequiredRoomType(_activeFacilityType))}",
            FacilityPlacementIssue.MissingRequiredRoom =>
                $"\u9700\u8981{GetRoomTypeLabel(GetRequiredRoomType(_activeFacilityType))}",
            _ => string.Empty,
        };
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

        return _facilityPlacementStore.TryPlace(_activeFacilityType, cell, _activeFacilityFacing, out facility);
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
        if (_roomFootprintStore.RemoveAtCell(cell, out room))
        {
            return true;
        }

        if (_roomFootprintStore.RemoveDoorOwnerAtAdjacentCell(cell, out room))
        {
            return true;
        }

        return false;
    }

    public bool TryDeleteRoomDoorAtWorldPosition(Vector3 worldPosition, out RoomFootprint? room)
    {
        if (_roomFootprintStore == null)
        {
            room = null;
            return false;
        }

        return _roomFootprintStore.RemoveDoorOwnerAtWorldPosition(worldPosition, out room);
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
        _pendingRoomSelection = null;
        ClearBuildStatus();
        _activeRoomType = roomType;
        SetActiveToolMode(BuildToolMode.BuildRoom);
    }

    public RoomBuildType GetActiveRoomType()
    {
        return _activeRoomType;
    }

    public void SetActiveFacilityType(FacilityBuildType facilityType)
    {
        _pendingRoomSelection = null;
        ClearBuildStatus();
        _activeFacilityType = facilityType;
        SetActiveToolMode(BuildToolMode.PlaceFacility);
    }

    public FacilityBuildType GetActiveFacilityType()
    {
        return _activeFacilityType;
    }

    public FacilityFacing GetActiveFacilityFacing()
    {
        return _activeFacilityFacing;
    }

    public void RotateActiveFacilityFacing()
    {
        _activeFacilityFacing = _activeFacilityFacing switch
        {
            FacilityFacing.North => FacilityFacing.East,
            FacilityFacing.East => FacilityFacing.South,
            FacilityFacing.South => FacilityFacing.West,
            _ => FacilityFacing.North,
        };
        ToolModeChanged?.Invoke();
    }

    public BuildToolMode GetActiveToolMode()
    {
        return _activeToolMode;
    }

    public void StartDeleteRoomMode()
    {
        _pendingRoomSelection = null;
        ClearBuildStatus();
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
        _pendingRoomSelection = null;
        ClearBuildStatus();
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

    public bool IsPlaceRoomDoorMode()
    {
        return _activeToolMode == BuildToolMode.PlaceRoomDoor;
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

    public bool IsRoomSelectionTooSmall(Vector2I startCell, Vector2I endCell)
    {
        return !MeetsMinimumRoomSize(startCell, endCell);
    }

    public string GetRoomBuildFailureMessage(Vector2I startCell, Vector2I endCell)
    {
        if (IsRoomSelectionTooSmall(startCell, endCell))
        {
            return "\u533a\u57df\u81f3\u5c11\u9700\u8981 2x3 \u624d\u80fd\u5efa\u9020";
        }

        return "\u8fd9\u7247\u533a\u57df\u4e0d\u80fd\u5efa\u9020";
    }

    private static bool MeetsMinimumRoomSize(Vector2I startCell, Vector2I endCell)
    {
        var minX = Mathf.Min(startCell.X, endCell.X);
        var maxX = Mathf.Max(startCell.X, endCell.X);
        var minY = Mathf.Min(startCell.Y, endCell.Y);
        var maxY = Mathf.Max(startCell.Y, endCell.Y);
        var width = maxX - minX + 1;
        var height = maxY - minY + 1;
        return (width >= MinimumRoomWidth && height >= MinimumRoomHeight)
            || (width >= MinimumRoomHeight && height >= MinimumRoomWidth);
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

    private bool IsDoorOnPendingBoundary(Vector2I cell, RoomDoorSide side)
    {
        if (_pendingRoomSelection == null)
        {
            return false;
        }

        var bounds = GetPendingRoomBounds();
        if (
            cell.X < bounds.MinCell.X
            || cell.X > bounds.MaxCell.X
            || cell.Y < bounds.MinCell.Y
            || cell.Y > bounds.MaxCell.Y
        )
        {
            return false;
        }

        return side switch
        {
            RoomDoorSide.North => cell.Y == bounds.MinCell.Y,
            RoomDoorSide.South => cell.Y == bounds.MaxCell.Y,
            RoomDoorSide.West => cell.X == bounds.MinCell.X,
            RoomDoorSide.East => cell.X == bounds.MaxCell.X,
            _ => false,
        };
    }

    private PendingRoomBounds GetPendingRoomBounds()
    {
        if (_pendingRoomSelection == null)
        {
            return new PendingRoomBounds(Vector2I.Zero, Vector2I.Zero);
        }

        var startCell = _pendingRoomSelection.StartCell;
        var endCell = _pendingRoomSelection.EndCell;
        return new PendingRoomBounds(
            new Vector2I(Mathf.Min(startCell.X, endCell.X), Mathf.Min(startCell.Y, endCell.Y)),
            new Vector2I(Mathf.Max(startCell.X, endCell.X), Mathf.Max(startCell.Y, endCell.Y))
        );
    }

    private readonly record struct PendingRoomBounds(Vector2I MinCell, Vector2I MaxCell);
}
