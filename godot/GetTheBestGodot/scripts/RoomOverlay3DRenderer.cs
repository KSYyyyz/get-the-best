using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class RoomOverlay3DRenderer : Node3D
{
    private const float RoomCarpetHeight = 0.08f;
    private const float RoomBoundaryHeight = 0.30f;
    private const float RoomBoundaryThickness = 0.10f;
    private static readonly Color ResearchRoomFill = new(0.20f, 0.48f, 0.74f, 0.42f);
    private static readonly Color MarketRoomFill = new(0.56f, 0.42f, 0.82f, 0.42f);
    private static readonly Color ServerRoomFill = new(0.30f, 0.64f, 0.56f, 0.42f);
    private static readonly Color HighlightedRoomStroke = new(1.0f, 0.94f, 0.34f, 0.78f);
    private static readonly Color RoomSignPlateFill = new(0.86f, 0.84f, 0.70f, 0.88f);
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
        AddRoomCarpet(room);
        AddRoomBoundary(room);
        AddRoomSignPlate(room);
    }

    private void AddRoomCarpet(RoomFootprint room)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh
            {
                Size = OfficeWorld3DConfig.SelectionSize(room.MinCell, room.MaxCell, RoomCarpetHeight),
            },
            MaterialOverride = CreateMaterial(GetRoomFillColor(room.RoomType)),
            Position =
                OfficeWorld3DConfig.SelectionCenter(room.MinCell, room.MaxCell)
                + Vector3.Up * (RoomCarpetHeight / 2.0f + 0.04f),
        };
        AddChild(mesh);
        _renderedRooms.Add(mesh);
    }

    private void AddRoomBoundary(RoomFootprint room)
    {
        var size = OfficeWorld3DConfig.SelectionSize(room.MinCell, room.MaxCell, RoomBoundaryHeight);
        var center = OfficeWorld3DConfig.SelectionCenter(room.MinCell, room.MaxCell);
        var color = _highlightedRoom == room ? HighlightedRoomStroke : GetRoomBoundaryColor(room.RoomType);
        var material = CreateMaterial(color);
        var halfX = size.X / 2.0f;
        var halfZ = size.Z / 2.0f;
        var y = RoomBoundaryHeight / 2.0f + 0.08f;

        AddBoundaryStrip(
            center + new Vector3(0.0f, y, -halfZ),
            new Vector3(size.X, RoomBoundaryHeight, RoomBoundaryThickness),
            material
        );
        AddBoundaryStrip(
            center + new Vector3(0.0f, y, halfZ),
            new Vector3(size.X, RoomBoundaryHeight, RoomBoundaryThickness),
            material
        );
        AddBoundaryStrip(
            center + new Vector3(-halfX, y, 0.0f),
            new Vector3(RoomBoundaryThickness, RoomBoundaryHeight, size.Z),
            material
        );
        AddBoundaryStrip(
            center + new Vector3(halfX, y, 0.0f),
            new Vector3(RoomBoundaryThickness, RoomBoundaryHeight, size.Z),
            material
        );
    }

    private void AddBoundaryStrip(Vector3 position, Vector3 size, Material material)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = material,
            Position = position,
        };
        AddChild(mesh);
        _renderedRooms.Add(mesh);
    }

    private void AddRoomSignPlate(RoomFootprint room)
    {
        var signPosition =
            OfficeWorld3DConfig.CellToWorldPosition(room.MinCell)
            + new Vector3(0.0f, RoomBoundaryHeight + 0.18f, -0.72f);
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(1.2f, 0.12f, 0.18f) },
            MaterialOverride = CreateMaterial(RoomSignPlateFill),
            Position = signPosition,
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

    private static Color GetRoomBoundaryColor(RoomBuildType roomType)
    {
        var fill = GetRoomFillColor(roomType);
        return new Color(fill.R + 0.16f, fill.G + 0.16f, fill.B + 0.16f, 0.82f);
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
