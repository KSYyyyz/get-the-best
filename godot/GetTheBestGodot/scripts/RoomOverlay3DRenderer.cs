using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class RoomOverlay3DRenderer : Node3D
{
    private static readonly Color ResearchRoomFill = new(0.20f, 0.48f, 0.74f, 0.42f);
    private static readonly Color MarketRoomFill = new(0.56f, 0.42f, 0.82f, 0.42f);
    private static readonly Color ServerRoomFill = new(0.30f, 0.64f, 0.56f, 0.42f);
    private static readonly Color HighlightedRoomStroke = new(1.0f, 0.94f, 0.34f, 0.78f);
    private readonly List<Node> _renderedRooms = [];
    private RoomFootprintStore? _roomFootprintStore;
    private RoomFootprint? _highlightedRoom;

    public override void _Ready()
    {
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
    }

    public void RefreshRooms()
    {
        foreach (var renderedRoom in _renderedRooms)
        {
            renderedRoom.QueueFree();
        }
        _renderedRooms.Clear();

        if (_roomFootprintStore == null)
        {
            return;
        }

        foreach (var room in _roomFootprintStore.GetRooms())
        {
            AddRoomMesh(room);
        }
    }

    public void HighlightRoom(RoomFootprint? room)
    {
        _highlightedRoom = room;
        RefreshRooms();
    }

    private void AddRoomMesh(RoomFootprint room)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh
            {
                Size = OfficeWorld3DConfig.SelectionSize(room.MinCell, room.MaxCell, 0.12f),
            },
            MaterialOverride = CreateMaterial(
                _highlightedRoom == room ? HighlightedRoomStroke : GetRoomFillColor(room.RoomType)
            ),
            Position = OfficeWorld3DConfig.SelectionCenter(room.MinCell, room.MaxCell) + Vector3.Up * 0.10f,
        };
        AddChild(mesh);
        _renderedRooms.Add(mesh);
    }

    private static Color GetRoomFillColor(RoomBuildType roomType)
    {
        return roomType switch
        {
            RoomBuildType.ResearchRoom => ResearchRoomFill,
            RoomBuildType.MarketRoom => MarketRoomFill,
            RoomBuildType.ServerRoom => ServerRoomFill,
            _ => ResearchRoomFill,
        };
    }

    private static StandardMaterial3D CreateMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        };
    }
}
