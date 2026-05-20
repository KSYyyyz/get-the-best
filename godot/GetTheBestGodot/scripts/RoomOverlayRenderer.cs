using Godot;

namespace GetTheBestGodot;

public partial class RoomOverlayRenderer : Node2D
{
    private static readonly Color RoomStroke = new(0.86f, 0.90f, 0.86f, 0.90f);

    private RoomFootprintStore? _roomFootprintStore;

    public override void _Ready()
    {
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
    }

    public void RefreshRooms()
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_roomFootprintStore == null)
        {
            return;
        }

        foreach (var room in _roomFootprintStore.GetRooms())
        {
            var rect = room.ToWorldRect();
            DrawRect(rect, GetRoomFillColor(room.RoomType), filled: true);
            DrawRect(rect, RoomStroke, filled: false, width: 4.0f);
        }
    }

    private static Color GetRoomFillColor(RoomBuildType roomType)
    {
        return roomType switch
        {
            RoomBuildType.ResearchRoom => new Color(0.23f, 0.56f, 1.0f, 0.22f),
            RoomBuildType.MarketRoom => new Color(1.0f, 0.66f, 0.22f, 0.22f),
            RoomBuildType.ServerRoom => new Color(0.56f, 0.82f, 0.36f, 0.22f),
            _ => new Color(0.60f, 0.60f, 0.60f, 0.20f),
        };
    }
}
