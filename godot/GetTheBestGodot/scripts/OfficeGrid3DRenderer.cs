using Godot;

namespace GetTheBestGodot;

public partial class OfficeGrid3DRenderer : Node3D
{
    private const float GridLineThickness = OfficeWorld3DConfig.GridSize * 0.006f;
    private static readonly Color GridColor = new(0.56f, 0.66f, 0.60f, 0.48f);

    public override void _Ready()
    {
        BuildGrid();
    }

    private void BuildGrid()
    {
        var material = new StandardMaterial3D
        {
            AlbedoColor = GridColor,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        };

        for (var x = 0; x <= OfficeWorld3DConfig.Columns; x++)
        {
            var worldX = OfficeWorld3DConfig.OfficeBounds.Position.X + x * OfficeWorld3DConfig.GridSize;
            AddLine(
                new Vector3(worldX, 0.025f, 0.0f),
                new Vector3(GridLineThickness, GridLineThickness, OfficeWorld3DConfig.OfficeBounds.Size.Y),
                material
            );
        }

        for (var y = 0; y <= OfficeWorld3DConfig.Rows; y++)
        {
            var worldZ = OfficeWorld3DConfig.OfficeBounds.Position.Y + y * OfficeWorld3DConfig.GridSize;
            AddLine(
                new Vector3(0.0f, 0.025f, worldZ),
                new Vector3(OfficeWorld3DConfig.OfficeBounds.Size.X, GridLineThickness, GridLineThickness),
                material
            );
        }
    }

    private void AddLine(Vector3 position, Vector3 size, Material material)
    {
        var line = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = material,
            Position = position,
        };
        AddChild(line);
    }

}
