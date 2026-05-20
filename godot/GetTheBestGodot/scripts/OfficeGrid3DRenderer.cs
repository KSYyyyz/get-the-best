using Godot;

namespace GetTheBestGodot;

public partial class OfficeGrid3DRenderer : Node3D
{
    private static readonly Color GridColor = new(0.56f, 0.66f, 0.60f, 0.34f);
    private static readonly Color MajorGridColor = new(0.62f, 0.72f, 0.66f, 0.56f);

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
            AddGridLine(
                x,
                new Vector3(worldX, 0.025f, 0.0f),
                new Vector3(0.025f, 0.025f, OfficeWorld3DConfig.OfficeBounds.Size.Y),
                material
            );
        }

        for (var y = 0; y <= OfficeWorld3DConfig.Rows; y++)
        {
            var worldZ = OfficeWorld3DConfig.OfficeBounds.Position.Y + y * OfficeWorld3DConfig.GridSize;
            AddGridLine(
                y,
                new Vector3(0.0f, 0.025f, worldZ),
                new Vector3(OfficeWorld3DConfig.OfficeBounds.Size.X, 0.025f, 0.025f),
                material
            );
        }
    }

    private void AddGridLine(int lineIndex, Vector3 position, Vector3 size, Material baseMaterial)
    {
        if (lineIndex % 5 == 0)
        {
            AddLine(
                position + Vector3.Up * 0.006f,
                new Vector3(Mathf.Max(size.X, 0.045f), 0.035f, Mathf.Max(size.Z, 0.045f)),
                CreateMaterial(MajorGridColor)
            );
            return;
        }

        AddLine(position, size, baseMaterial);
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

    private static StandardMaterial3D CreateMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        };
    }
}
