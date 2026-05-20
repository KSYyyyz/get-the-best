using Godot;

namespace GetTheBestGodot;

public partial class OfficeFloor3DRenderer : MeshInstance3D
{
    public override void _Ready()
    {
        Mesh = new BoxMesh
        {
            Size = new Vector3(
                OfficeWorld3DConfig.OfficeBounds.Size.X,
                0.08f,
                OfficeWorld3DConfig.OfficeBounds.Size.Y
            ),
        };
        MaterialOverride = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.22f, 0.25f, 0.23f, 1.0f),
            Roughness = 1.0f,
        };
    }
}
