using Godot;

namespace GetTheBestGodot;

public partial class PlacementPreview3DController : Node3D
{
    private static readonly Color LegalFill = new(0.28f, 0.95f, 0.55f, 0.34f);
    private static readonly Color IllegalFill = new(1.0f, 0.20f, 0.20f, 0.34f);
    private static readonly Color DoorPreviewFill = new(1.0f, 0.92f, 0.48f, 0.95f);
    private MeshInstance3D? _previewMesh;
    private MeshInstance3D? _doorPreviewMesh;

    public void ShowHoverCell(Vector2I cell)
    {
        ShowSelectionRect(cell, cell, isLegal: true);
    }

    public void ShowFacilityCell(Vector2I cell, bool isLegal)
    {
        ShowSelectionRect(cell, cell, isLegal);
    }

    public void ShowSelectionRect(Vector2I startCell, Vector2I endCell, bool isLegal)
    {
        _previewMesh ??= CreatePreviewMesh();
        _previewMesh.Visible = true;
        _previewMesh.Position = OfficeWorld3DConfig.SelectionCenter(startCell, endCell) + Vector3.Up * 0.08f;
        _previewMesh.Mesh = new BoxMesh
        {
            Size = OfficeWorld3DConfig.SelectionSize(startCell, endCell, 0.08f),
        };
        _previewMesh.MaterialOverride = CreateMaterial(isLegal ? LegalFill : IllegalFill);
    }

    public void ShowRoomDoorPreview(RoomDoorPlacement doorPlacement)
    {
        _doorPreviewMesh ??= CreatePreviewMesh();
        _doorPreviewMesh.Visible = true;
        _doorPreviewMesh.Position = GetDoorPreviewPosition(doorPlacement);
        _doorPreviewMesh.Mesh = new BoxMesh { Size = GetDoorPreviewSize(doorPlacement.Side) };
        _doorPreviewMesh.MaterialOverride = CreateMaterial(DoorPreviewFill);
    }

    public void ClearPreview()
    {
        if (_previewMesh != null)
        {
            _previewMesh.Visible = false;
        }

        if (_doorPreviewMesh != null)
        {
            _doorPreviewMesh.Visible = false;
        }
    }

    private MeshInstance3D CreatePreviewMesh()
    {
        var mesh = new MeshInstance3D { Visible = false };
        AddChild(mesh);
        return mesh;
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

    private static Vector3 GetDoorPreviewPosition(RoomDoorPlacement doorPlacement)
    {
        var center = OfficeWorld3DConfig.CellToWorldPosition(doorPlacement.Cell);
        var halfCell = OfficeWorld3DConfig.GridSize / 2.0f;
        var y = 0.56f;
        return doorPlacement.Side switch
        {
            RoomDoorSide.North => center + new Vector3(0.0f, y, -halfCell),
            RoomDoorSide.South => center + new Vector3(0.0f, y, halfCell),
            RoomDoorSide.West => center + new Vector3(-halfCell, y, 0.0f),
            RoomDoorSide.East => center + new Vector3(halfCell, y, 0.0f),
            _ => center + Vector3.Up * y,
        };
    }

    private static Vector3 GetDoorPreviewSize(RoomDoorSide side)
    {
        return side switch
        {
            RoomDoorSide.North or RoomDoorSide.South => new Vector3(1.20f, 0.16f, 0.26f),
            _ => new Vector3(0.26f, 0.16f, 1.20f),
        };
    }
}
