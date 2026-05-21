using Godot;

namespace GetTheBestGodot;

public partial class OfficeGrid3DRenderer : Node3D
{
    private const string FloorTileTexturePath =
        "res://assets/third_party_placeholder_assets/kenney_prototype_textures/floor_light_texture_02.png";
    private const float TileInset = OfficeWorld3DConfig.GridSize * 0.035f;
    private const float TileHeight = OfficeWorld3DConfig.GridSize * 0.006f;
    private static readonly Color FloorTileA = new(0.72f, 0.78f, 0.74f, 1.0f);
    private static readonly Color FloorTileB = new(0.64f, 0.72f, 0.68f, 1.0f);
    private static Texture2D? _floorTileTexture;

    public override void _Ready()
    {
        BuildFloorTiles();
    }

    private void BuildFloorTiles()
    {
        var materialA = CreateTileMaterial(FloorTileA);
        var materialB = CreateTileMaterial(FloorTileB);
        var tileSize = Mathf.Max(OfficeWorld3DConfig.GridSize - TileInset * 2.0f, 0.1f);

        for (var x = 0; x < OfficeWorld3DConfig.Columns; x++)
        {
            for (var y = 0; y < OfficeWorld3DConfig.Rows; y++)
            {
                var tile = new MeshInstance3D
                {
                    Mesh = new BoxMesh { Size = new Vector3(tileSize, TileHeight, tileSize) },
                    MaterialOverride = (x + y) % 2 == 0 ? materialA : materialB,
                    Position =
                        OfficeWorld3DConfig.CellToWorldPosition(new Vector2I(x, y))
                        + Vector3.Up * (0.055f + TileHeight / 2.0f),
                };
                AddChild(tile);
            }
        }
    }

    private static StandardMaterial3D CreateTileMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            AlbedoTexture = LoadFloorTileTexture(),
            Roughness = 1.0f,
        };
    }

    private static Texture2D? LoadFloorTileTexture()
    {
        _floorTileTexture ??= GD.Load<Texture2D>(FloorTileTexturePath);
        return _floorTileTexture;
    }
}
