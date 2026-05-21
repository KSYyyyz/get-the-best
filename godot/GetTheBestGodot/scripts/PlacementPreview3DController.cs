using Godot;

namespace GetTheBestGodot;

public partial class PlacementPreview3DController : Node3D
{
    private static readonly Color LegalFill = new(0.28f, 0.95f, 0.55f, 0.34f);
    private static readonly Color IllegalFill = new(1.0f, 0.20f, 0.20f, 0.34f);
    private static readonly Color DoorPreviewFill = new(1.0f, 0.92f, 0.48f, 0.95f);
    private static readonly Color FacilityFacingFill = new(1.0f, 0.92f, 0.32f, 0.95f);
    private MeshInstance3D? _previewMesh;
    private MeshInstance3D? _doorPreviewMesh;
    private MeshInstance3D? _facilityFacingPreviewMesh;

    public void ShowHoverCell(Vector2I cell)
    {
        ShowSelectionRect(cell, cell, isLegal: true);
    }

    public void ShowFacilityCell(Vector2I cell, bool isLegal)
    {
        ShowSelectionRect(cell, cell, isLegal);
    }

    public void ShowFacilityCell(Vector2I cell, bool isLegal, FacilityDefinition definition)
    {
        ShowFacilityCell(cell, isLegal, definition, FacilityFacing.South);
    }

    public void ShowFacilityCell(
        Vector2I cell,
        bool isLegal,
        FacilityDefinition definition,
        FacilityFacing facing
    )
    {
        var endCell = cell + definition.Footprint - Vector2I.One;
        ShowSelectionRect(cell, endCell, isLegal);
        ShowFacilityFacingMarker(cell, facing);
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
        _doorPreviewMesh.Position = RoomDoorGeometry.GetPosition(doorPlacement);
        _doorPreviewMesh.Mesh = new BoxMesh { Size = RoomDoorGeometry.GetSize(doorPlacement.Side) };
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

        if (_facilityFacingPreviewMesh != null)
        {
            _facilityFacingPreviewMesh.Visible = false;
        }
    }

    private void ShowFacilityFacingMarker(Vector2I cell, FacilityFacing facing)
    {
        _facilityFacingPreviewMesh ??= CreatePreviewMesh();
        _facilityFacingPreviewMesh.Visible = true;
        _facilityFacingPreviewMesh.Position =
            OfficeWorld3DConfig.CellToWorldPosition(cell)
            + GetFacingOffset(facing)
            + Vector3.Up * 0.18f;
        _facilityFacingPreviewMesh.RotationDegrees = new Vector3(
            0.0f,
            GetFacingYawDegrees(facing),
            0.0f
        );
        _facilityFacingPreviewMesh.Mesh = new BoxMesh
        {
            Size = new Vector3(
                OfficeWorld3DConfig.GridSize * 0.46f,
                OfficeWorld3DConfig.GridSize * 0.06f,
                OfficeWorld3DConfig.GridSize * 0.12f
            ),
        };
        _facilityFacingPreviewMesh.MaterialOverride = CreateMaterial(FacilityFacingFill);
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

    private static Vector3 GetFacingOffset(FacilityFacing facing)
    {
        var offset = OfficeWorld3DConfig.GridSize * 0.32f;
        return facing switch
        {
            FacilityFacing.North => new Vector3(0.0f, 0.0f, -offset),
            FacilityFacing.East => new Vector3(offset, 0.0f, 0.0f),
            FacilityFacing.South => new Vector3(0.0f, 0.0f, offset),
            _ => new Vector3(-offset, 0.0f, 0.0f),
        };
    }

    private static float GetFacingYawDegrees(FacilityFacing facing)
    {
        return facing switch
        {
            FacilityFacing.North => 0.0f,
            FacilityFacing.East => 90.0f,
            FacilityFacing.South => 180.0f,
            _ => 270.0f,
        };
    }
}
