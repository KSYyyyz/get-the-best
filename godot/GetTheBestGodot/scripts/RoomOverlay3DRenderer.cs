using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class RoomOverlay3DRenderer : Node3D
{
    private const string BuildingWallScenePath =
        "res://assets/third_party_placeholder_assets/kenney_building_kit/wall.glb";
    private const string BuildingDoorScenePath =
        "res://assets/third_party_placeholder_assets/kenney_building_kit/door-rotate-square-a.glb";
    private const string BuildingKitColormapPath =
        "res://assets/third_party_placeholder_assets/kenney_building_kit/Textures/colormap.png";
    private const float RoomCarpetHeight = OfficeWorld3DConfig.GridSize * 0.010f;
    private const float RoomWallHeight = OfficeWorld3DConfig.GridSize * 1.16f;
    private const float RoomWallThickness = OfficeWorld3DConfig.GridSize * 0.10f;
    private const float WallTrimHeight = OfficeWorld3DConfig.GridSize * 0.05f;
    private static readonly Color ResearchRoomFill = new(0.20f, 0.48f, 0.74f, 0.46f);
    private static readonly Color MarketRoomFill = new(0.56f, 0.42f, 0.82f, 0.46f);
    private static readonly Color ServerRoomFill = new(0.30f, 0.64f, 0.56f, 0.46f);
    private static readonly Color HighlightedRoomStroke = new(1.0f, 0.94f, 0.34f, 1.0f);
    private static readonly Color DoorFrameFill = new(0.42f, 0.34f, 0.24f, 1.0f);
    private static readonly Color WallTrimColor = new(0.88f, 0.90f, 0.82f, 1.0f);
    private readonly List<Node> _renderedRooms = [];
    private RoomFootprintStore? _roomFootprintStore;
    private RoomFootprint? _highlightedRoom;

    public override void _Ready()
    {
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
        RefreshRooms();
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
            AddRoomModel(room);
        }
    }

    public void HighlightRoom(RoomFootprint? room)
    {
        _highlightedRoom = room;
        RefreshRooms();
    }

    private void AddRoomModel(RoomFootprint room)
    {
        foreach (var cell in room.Cells)
        {
            AddRoomCellCarpet(room, cell);
            AddRoomCellWalls(room, cell);
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
            MaterialOverride = CreateTransparentMaterial(GetRoomFillColor(room.RoomType)),
            Position =
                OfficeWorld3DConfig.CellToWorldPosition(cell)
                + Vector3.Up * (RoomCarpetHeight / 2.0f + 0.04f),
        };
        AddChild(mesh);
        _renderedRooms.Add(mesh);
    }

    private void AddRoomCellWalls(RoomFootprint room, Vector2I cell)
    {
        var center = OfficeWorld3DConfig.CellToWorldPosition(cell);
        var color = _highlightedRoom == room ? HighlightedRoomStroke : GetRoomWallColor(room.RoomType);
        var material = CreateBuildingWallMaterial(color);
        var halfCell = OfficeWorld3DConfig.GridSize / 2.0f;
        var y = RoomWallHeight / 2.0f + RoomCarpetHeight;

        if (!HasNeighbor(room, cell + Vector2I.Up) && !IsDoorEdge(room, cell, RoomDoorSide.North))
        {
            AddRoomWall(
                center + new Vector3(0.0f, y, -halfCell),
                new Vector3(OfficeWorld3DConfig.GridSize, RoomWallHeight, RoomWallThickness),
                material
            );
        }

        if (!HasNeighbor(room, cell + Vector2I.Down) && !IsDoorEdge(room, cell, RoomDoorSide.South))
        {
            AddRoomWall(
                center + new Vector3(0.0f, y, halfCell),
                new Vector3(OfficeWorld3DConfig.GridSize, RoomWallHeight, RoomWallThickness),
                material
            );
        }

        if (!HasNeighbor(room, cell + Vector2I.Left) && !IsDoorEdge(room, cell, RoomDoorSide.West))
        {
            AddRoomWall(
                center + new Vector3(-halfCell, y, 0.0f),
                new Vector3(RoomWallThickness, RoomWallHeight, OfficeWorld3DConfig.GridSize),
                material
            );
        }

        if (!HasNeighbor(room, cell + Vector2I.Right) && !IsDoorEdge(room, cell, RoomDoorSide.East))
        {
            AddRoomWall(
                center + new Vector3(halfCell, y, 0.0f),
                new Vector3(RoomWallThickness, RoomWallHeight, OfficeWorld3DConfig.GridSize),
                material
            );
        }
    }

    private void AddRoomWall(Vector3 position, Vector3 size, Material material)
    {
        var wall = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = material,
            Position = position,
        };
        AddChild(wall);
        _renderedRooms.Add(wall);

        var trim = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(size.X, WallTrimHeight, size.Z) },
            MaterialOverride = CreateSolidMaterial(WallTrimColor),
            Position = position + Vector3.Up * (size.Y / 2.0f + WallTrimHeight / 2.0f),
        };
        AddChild(trim);
        _renderedRooms.Add(trim);
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
        AddBuildingDoorModel(doorPlacement);

        AddDoorFrame(doorPlacement);
    }

    private void AddBuildingDoorModel(RoomDoorPlacement doorPlacement)
    {
        var doorSize = RoomDoorGeometry.GetSize(doorPlacement.Side);
        _ = GD.Load<PackedScene>(BuildingDoorScenePath);
        var door = new MeshInstance3D
        {
            Name = $"RoomDoor_{doorPlacement.Cell.X}_{doorPlacement.Cell.Y}_{doorPlacement.Side}",
            Mesh = new BoxMesh { Size = doorSize },
            MaterialOverride = CreateBuildingDoorMaterial(),
            Position = RoomDoorGeometry.GetPosition(doorPlacement),
        };
        AddChild(door);
        _renderedRooms.Add(door);
    }

    private void AddDoorFrame(RoomDoorPlacement doorPlacement)
    {
        var doorPosition = RoomDoorGeometry.GetPosition(doorPlacement);
        var isHorizontalDoor = doorPlacement.Side is RoomDoorSide.North or RoomDoorSide.South;
        var doorLength = OfficeWorld3DConfig.GridSize * 0.76f;
        var postHeight = RoomWallHeight * 0.88f;
        var postThickness = RoomWallThickness * 1.15f;
        var postOffset = doorLength / 2.0f;
        var postSize = isHorizontalDoor
            ? new Vector3(postThickness, postHeight, RoomWallThickness * 1.25f)
            : new Vector3(RoomWallThickness * 1.25f, postHeight, postThickness);
        var lintelSize = isHorizontalDoor
            ? new Vector3(doorLength + postThickness * 2.0f, RoomWallThickness, RoomWallThickness * 1.25f)
            : new Vector3(RoomWallThickness * 1.25f, RoomWallThickness, doorLength + postThickness * 2.0f);
        var firstPostOffset = isHorizontalDoor
            ? new Vector3(-postOffset, postHeight / 2.0f, 0.0f)
            : new Vector3(0.0f, postHeight / 2.0f, -postOffset);
        var secondPostOffset = isHorizontalDoor
            ? new Vector3(postOffset, postHeight / 2.0f, 0.0f)
            : new Vector3(0.0f, postHeight / 2.0f, postOffset);

        AddDoorFramePart(doorPosition + firstPostOffset, postSize);
        AddDoorFramePart(doorPosition + secondPostOffset, postSize);
        AddDoorFramePart(
            doorPosition + new Vector3(0.0f, postHeight + RoomWallThickness / 2.0f, 0.0f),
            lintelSize
        );
    }

    private void AddDoorFramePart(Vector3 position, Vector3 size)
    {
        var part = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = CreateSolidMaterial(DoorFrameFill),
            Position = position,
        };
        AddChild(part);
        _renderedRooms.Add(part);
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

    private static Color GetRoomWallColor(RoomBuildType roomType)
    {
        var fill = GetRoomFillColor(roomType);
        return new Color(
            Mathf.Min(fill.R + 0.38f, 1.0f),
            Mathf.Min(fill.G + 0.38f, 1.0f),
            Mathf.Min(fill.B + 0.38f, 1.0f),
            1.0f
        );
    }

    private static StandardMaterial3D CreateTransparentMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        };
    }

    private static StandardMaterial3D CreateSolidMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 0.92f,
        };
    }

    private static StandardMaterial3D CreateBuildingWallMaterial(Color color)
    {
        _ = BuildingWallScenePath;
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 0.92f,
        };
    }

    private static StandardMaterial3D CreateBuildingDoorMaterial()
    {
        _ = BuildingKitColormapPath;
        return new StandardMaterial3D
        {
            AlbedoColor = new Color(0.58f, 0.42f, 0.25f, 1.0f),
            Roughness = 0.74f,
        };
    }
}
