using Godot;

namespace GetTheBestGodot;

public enum RoomBuildType
{
    ResearchRoom,
    MarketRoom,
    ServerRoom,
}

public enum BuildToolMode
{
    BuildRoom,
    DeleteRoom,
}

public partial class BuildModeController : Node
{
    private RoomFootprintStore? _roomFootprintStore;
    private RoomBuildType _activeRoomType = RoomBuildType.ResearchRoom;
    private BuildToolMode _activeToolMode = BuildToolMode.BuildRoom;

    public override void _Ready()
    {
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
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
        // V2-0.2 还没有设施实体；后续桌子、椅子等接入后，删除地块会先默认出售设施。
        return 0;
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
        _activeToolMode = BuildToolMode.BuildRoom;
    }

    public RoomBuildType GetActiveRoomType()
    {
        return _activeRoomType;
    }

    public void StartDeleteRoomMode()
    {
        _activeToolMode = BuildToolMode.DeleteRoom;
    }

    public void CancelActiveTool()
    {
        _activeToolMode = BuildToolMode.BuildRoom;
    }

    public bool IsDeleteRoomMode()
    {
        return _activeToolMode == BuildToolMode.DeleteRoom;
    }

    public string GetActiveRoomTypeLabel()
    {
        return GetRoomTypeLabel(_activeRoomType);
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

    public static string FormatSelectionSize(Vector2I startCell, Vector2I endCell)
    {
        var minX = Mathf.Min(startCell.X, endCell.X);
        var maxX = Mathf.Max(startCell.X, endCell.X);
        var minY = Mathf.Min(startCell.Y, endCell.Y);
        var maxY = Mathf.Max(startCell.Y, endCell.Y);
        return $"{maxX - minX + 1}x{maxY - minY + 1}";
    }
}
