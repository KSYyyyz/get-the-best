using Godot;

namespace GetTheBestGodot;

public partial class RoomOverlayRenderer : Node2D
{
    private static readonly Color RoomFill = new(0.23f, 0.56f, 1.0f, 0.20f);
    private static readonly Color RoomStroke = new(0.42f, 0.72f, 1.0f, 0.90f);

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
            DrawRect(rect, RoomFill, filled: true);
            DrawRect(rect, RoomStroke, filled: false, width: 4.0f);
        }
    }
}
