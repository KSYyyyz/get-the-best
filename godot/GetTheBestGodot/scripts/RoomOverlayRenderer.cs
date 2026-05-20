using Godot;

namespace GetTheBestGodot;

public partial class RoomOverlayRenderer : Node2D
{
    private static readonly Color RoomStroke = new(0.86f, 0.90f, 0.86f, 0.90f);
    private static readonly Color HighlightedRoomStroke = new(1.0f, 0.95f, 0.42f, 1.0f);

    private RoomFootprintStore? _roomFootprintStore;
    private RoomFootprint? _highlightedRoom;

    public override void _Ready()
    {
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
    }

    public void RefreshRooms()
    {
        QueueRedraw();
    }

    public void HighlightRoom(RoomFootprint? room)
    {
        _highlightedRoom = room;
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
            foreach (var cell in room.Cells)
            {
                var rect = OfficeWorldConfig.CellToWorldRect(cell);
                DrawRect(rect, GetRoomFillColor(room.RoomType), filled: true);
                DrawRect(rect, RoomStroke, filled: false, width: 2.0f);

                if (_highlightedRoom?.Id == room.Id)
                {
                    DrawRect(rect.Grow(2.0f), HighlightedRoomStroke, filled: false, width: 4.0f);
                }
            }
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
