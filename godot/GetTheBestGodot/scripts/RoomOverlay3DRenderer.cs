using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class RoomOverlay3DRenderer : Node3D
{
    private const float RoomCarpetHeight = OfficeWorld3DConfig.GridSize * 0.008f;
    private const float RoomBoundaryHeight = OfficeWorld3DConfig.GridSize * 0.03f;
    private const float RoomBoundaryThickness = OfficeWorld3DConfig.GridSize * 0.08f;
    private static readonly Color ResearchRoomFill = new(0.20f, 0.48f, 0.74f, 0.42f);
    private static readonly Color MarketRoomFill = new(0.56f, 0.42f, 0.82f, 0.42f);
    private static readonly Color ServerRoomFill = new(0.30f, 0.64f, 0.56f, 0.42f);
    private static readonly Color HighlightedRoomStroke = new(1.0f, 0.94f, 0.34f, 0.78f);
    private static readonly Color RoomDoorFill = new(0.86f, 0.84f, 0.70f, 0.92f);
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
        foreach (var cell in room.Cells)
        {
            AddRoomCellCarpet(room, cell);
            AddRoomCellBoundary(room, cell);
        }

        AddRoomDoor(room);
    }

    private void AddRoomCellCarpet(RoomFootprint room, Vector2I cell)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh
            {
                Size = new Vector3(
                    OfficeWorld3DConfig.GridSize,
                    RoomCarpetHeight,
                    OfficeWorld3DConfig.GridSize
                ),
            },
            MaterialOverride = CreateMaterial(GetRoomFillColor(room.RoomType)),
            Position =
                OfficeWorld3DConfig.CellToWorldPosition(cell)
                + Vector3.Up * (RoomCarpetHeight / 2.0f + 0.04f),
        };
        AddChild(mesh);
        _renderedRooms.Add(mesh);
    }

    private void AddRoomCellBoundary(RoomFootprint room, Vector2I cell)
    {
        var center = OfficeWorld3DConfig.CellToWorldPosition(cell);
        var color = _highlightedRoom == room ? HighlightedRoomStroke : GetRoomBoundaryColor(room.RoomType);
        var material = CreateMaterial(color);
        var halfCell = OfficeWorld3DConfig.GridSize / 2.0f;
        var y = RoomBoundaryHeight / 2.0f + RoomCarpetHeight;

        if (!HasNeighbor(room, cell + Vector2I.Up) && !IsDoorEdge(room, cell, RoomDoorSide.North))
        {
            AddBoundaryStrip(
                center + new Vector3(0.0f, y, -halfCell),
                new Vector3(OfficeWorld3DConfig.GridSize, RoomBoundaryHeight, RoomBoundaryThickness),
                material
            );
        }

        if (!HasNeighbor(room, cell + Vector2I.Down) && !IsDoorEdge(room, cell, RoomDoorSide.South))
        {
            AddBoundaryStrip(
                center + new Vector3(0.0f, y, halfCell),
                new Vector3(OfficeWorld3DConfig.GridSize, RoomBoundaryHeight, RoomBoundaryThickness),
                material
            );
        }

        if (!HasNeighbor(room, cell + Vector2I.Left) && !IsDoorEdge(room, cell, RoomDoorSide.West))
        {
            AddBoundaryStrip(
                center + new Vector3(-halfCell, y, 0.0f),
                new Vector3(RoomBoundaryThickness, RoomBoundaryHeight, OfficeWorld3DConfig.GridSize),
                material
            );
        }

        if (!HasNeighbor(room, cell + Vector2I.Right) && !IsDoorEdge(room, cell, RoomDoorSide.East))
        {
            AddBoundaryStrip(
                center + new Vector3(halfCell, y, 0.0f),
                new Vector3(RoomBoundaryThickness, RoomBoundaryHeight, OfficeWorld3DConfig.GridSize),
                material
            );
        }
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

    private static bool HasNeighbor(RoomFootprint room, Vector2I cell)
    {
        return room.Contains(cell);
    }

    private static bool IsDoorEdge(RoomFootprint room, Vector2I cell, RoomDoorSide side)
    {
        return room.DoorPlacement is { } doorPlacement
            && doorPlacement.Cell == cell
            && doorPlacement.Side == side;
    }

    private void AddRoomDoor(RoomFootprint room)
    {
        if (room.DoorPlacement == null)
        {
            return;
        }

        var doorPlacement = room.DoorPlacement;
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = RoomDoorGeometry.GetSize(doorPlacement.Side) },
            MaterialOverride = CreateMaterial(RoomDoorFill),
            Position = RoomDoorGeometry.GetPosition(doorPlacement),
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
