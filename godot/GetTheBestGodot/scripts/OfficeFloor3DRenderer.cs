using Godot;

namespace GetTheBestGodot;

public partial class OfficeFloor3DRenderer : MeshInstance3D
{
    private const string FloorBaseTexturePath =
        "res://assets/third_party_placeholder_assets/kenney_prototype_textures/floor_light_texture_02.png";

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
            AlbedoColor = new Color(0.58f, 0.64f, 0.60f, 1.0f),
            AlbedoTexture = GD.Load<Texture2D>(FloorBaseTexturePath),
            Roughness = 1.0f,
        };
    }
}
