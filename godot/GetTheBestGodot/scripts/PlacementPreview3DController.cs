using Godot;

namespace GetTheBestGodot;

public partial class PlacementPreview3DController : Node3D
{
    private static readonly Color LegalFill = new(0.28f, 0.95f, 0.55f, 0.34f);
    private static readonly Color IllegalFill = new(1.0f, 0.20f, 0.20f, 0.34f);
    private MeshInstance3D? _previewMesh;

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

    public void ClearPreview()
    {
        if (_previewMesh != null)
        {
            _previewMesh.Visible = false;
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
}
