using Godot;

namespace GetTheBestGodot;

public enum RoomBuildType
{
    ResearchRoom,
    MarketRoom,
    ServerRoom,
}

public partial class BuildModeController : Node
{
    private RoomFootprintStore? _roomFootprintStore;
    private RoomBuildType _activeRoomType = RoomBuildType.ResearchRoom;

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
        var cellCount = OfficeWorldConfig.CountCells(startCell, endCell);
        var status = IsSelectionLegal(startCell, endCell) ? "当前可建造" : "与已有房间重叠";
        return $"预览{GetActiveRoomTypeLabel()}：{cellCount} 格，{status}";
    }

    public bool TryCreateRoom(Vector2I startCell, Vector2I endCell, out RoomFootprint? room)
    {
        if (_roomFootprintStore == null)
        {
            room = null;
            return false;
        }

        return _roomFootprintStore.TryReserve(_activeRoomType, startCell, endCell, out room);
    }

    public RoomFootprint? FindRoomAtCell(Vector2I cell)
    {
        return _roomFootprintStore?.FindAtCell(cell);
    }

    public void SetActiveRoomType(RoomBuildType roomType)
    {
        _activeRoomType = roomType;
    }

    public RoomBuildType GetActiveRoomType()
    {
        return _activeRoomType;
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
}
