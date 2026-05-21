using Godot;

namespace GetTheBestGodot;

public partial class OfficeBoundary3DRenderer : Node3D
{
    private const float WallHeight = OfficeWorld3DConfig.GridSize * 0.22f;
    private const float WallThickness = OfficeWorld3DConfig.GridSize * 0.06f;
    private static readonly Color WallColor = new(0.48f, 0.50f, 0.45f, 1.0f);
    private static readonly Color WallTrimColor = new(0.62f, 0.64f, 0.58f, 1.0f);
    private static readonly Color CornerPostColor = new(0.70f, 0.72f, 0.66f, 1.0f);

    public override void _Ready()
    {
        BuildBoundary();
    }

    private void BuildBoundary()
    {
        var bounds = OfficeWorld3DConfig.OfficeBounds;
        var minX = bounds.Position.X;
        var maxX = bounds.Position.X + bounds.Size.X;
        var minZ = bounds.Position.Y;
        var maxZ = bounds.Position.Y + bounds.Size.Y;
        var centerX = minX + bounds.Size.X / 2.0f;
        var centerZ = minZ + bounds.Size.Y / 2.0f;

        AddWall(
            new Vector3(centerX, WallHeight / 2.0f, minZ - WallThickness / 2.0f),
            new Vector3(bounds.Size.X + WallThickness * 2.0f, WallHeight, WallThickness)
        );
        AddWall(
            new Vector3(centerX, WallHeight / 2.0f, maxZ + WallThickness / 2.0f),
            new Vector3(bounds.Size.X + WallThickness * 2.0f, WallHeight, WallThickness)
        );
        AddWall(
            new Vector3(minX - WallThickness / 2.0f, WallHeight / 2.0f, centerZ),
            new Vector3(WallThickness, WallHeight, bounds.Size.Y)
        );
        AddWall(
            new Vector3(maxX + WallThickness / 2.0f, WallHeight / 2.0f, centerZ),
            new Vector3(WallThickness, WallHeight, bounds.Size.Y)
        );

        AddCornerPost(new Vector3(minX - WallThickness / 2.0f, WallHeight / 2.0f, minZ - WallThickness / 2.0f));
        AddCornerPost(new Vector3(maxX + WallThickness / 2.0f, WallHeight / 2.0f, minZ - WallThickness / 2.0f));
        AddCornerPost(new Vector3(minX - WallThickness / 2.0f, WallHeight / 2.0f, maxZ + WallThickness / 2.0f));
        AddCornerPost(new Vector3(maxX + WallThickness / 2.0f, WallHeight / 2.0f, maxZ + WallThickness / 2.0f));
    }

    private void AddWall(Vector3 position, Vector3 size)
    {
        var wall = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = CreateMaterial(WallColor),
            Position = position,
        };
        AddChild(wall);

        var trim = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(size.X, 0.10f, size.Z) },
            MaterialOverride = CreateMaterial(WallTrimColor),
            Position = position + Vector3.Up * (size.Y / 2.0f + 0.06f),
        };
        AddChild(trim);
    }

    private void AddCornerPost(Vector3 position)
    {
        var post = new MeshInstance3D
        {
            Mesh = new BoxMesh
            {
                Size = new Vector3(WallThickness * 1.35f, WallHeight * 1.08f, WallThickness * 1.35f),
            },
            MaterialOverride = CreateMaterial(CornerPostColor),
            Position = position,
        };
        AddChild(post);
    }

    private static StandardMaterial3D CreateMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 0.95f,
        };
    }
}
