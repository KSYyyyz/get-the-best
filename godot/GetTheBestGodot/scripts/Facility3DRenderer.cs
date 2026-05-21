using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class Facility3DRenderer : Node3D
{
    private const float CellInnerSize = OfficeWorld3DConfig.GridSize * 0.72f;
    private const float HighlightStrokeSize = OfficeWorld3DConfig.GridSize * 0.86f;
    private static readonly Color DeskFill = new(0.72f, 0.50f, 0.28f, 0.92f);
    private static readonly Color WhiteboardFill = new(0.86f, 0.92f, 0.90f, 0.92f);
    private static readonly Color ServerRackFill = new(0.34f, 0.46f, 0.70f, 0.92f);
    private static readonly Color HighlightStroke = new(1.0f, 0.95f, 0.42f, 1.0f);
    private readonly Dictionary<FacilityBuildType, Texture2D?> _facilityTextures = [];
    private readonly List<Node> _renderedFacilities = [];
    private FacilityPlacementStore? _facilityPlacementStore;
    private FacilityPlacement? _highlightedFacility;

    public override void _Ready()
    {
        _facilityPlacementStore = GetNodeOrNull<FacilityPlacementStore>("../FacilityPlacementStore");
        LoadFacilityTextures();
    }

    public void RefreshFacilities()
    {
        foreach (var renderedFacility in _renderedFacilities)
        {
            renderedFacility.QueueFree();
        }
        _renderedFacilities.Clear();

        if (_facilityPlacementStore == null)
        {
            return;
        }

        foreach (var facility in _facilityPlacementStore.GetFacilities())
        {
            AddFacilitySprite(facility);
        }
    }

    public void HighlightFacility(FacilityPlacement? facility)
    {
        _highlightedFacility = facility;
        RefreshFacilities();
    }

    private void LoadFacilityTextures()
    {
        _facilityTextures[FacilityBuildType.OfficeDesk] = ResourceLoader.Load<Texture2D>(
            FacilityDefinitionCatalog.GetDefinition(FacilityBuildType.OfficeDesk).TexturePath
        );
        _facilityTextures[FacilityBuildType.ProductWhiteboard] = ResourceLoader.Load<Texture2D>(
            FacilityDefinitionCatalog.GetDefinition(FacilityBuildType.ProductWhiteboard).TexturePath
        );
        _facilityTextures[FacilityBuildType.ServerRack] = ResourceLoader.Load<Texture2D>(
            FacilityDefinitionCatalog.GetDefinition(FacilityBuildType.ServerRack).TexturePath
        );
    }

    private void AddFacilitySprite(FacilityPlacement facility)
    {
        var position = OfficeWorld3DConfig.CellToWorldPosition(facility.Cell);
        var texture = GetFacilityTexture(facility.FacilityType);
        AddFacilityVolume(facility, position);
        if (texture == null)
        {
            return;
        }

        AddFacilitySpriteBillboard(facility, texture, position);
    }

    private void AddFacilitySpriteBillboard(
        FacilityPlacement facility,
        Texture2D texture,
        Vector3 position
    )
    {
        var sprite = new Sprite3D
        {
            Texture = texture,
            PixelSize = 0.07f,
            Position = position + Vector3.Up * GetFacilitySpriteHeight(facility.FacilityType),
            RotationDegrees = new Vector3(-60.0f, 0.0f, 0.0f),
        };
        AddChild(sprite);
        _renderedFacilities.Add(sprite);

        if (_highlightedFacility?.Id == facility.Id)
        {
            AddHighlight(position);
        }
    }

    private void AddFacilityVolume(FacilityPlacement facility, Vector3 position)
    {
        var size = GetFacilityVolumeSize(facility.FacilityType);
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = CreateMaterial(GetFacilityFillColor(facility.FacilityType)),
            Position = position + Vector3.Up * (size.Y / 2.0f + 0.10f),
        };
        AddChild(mesh);
        _renderedFacilities.Add(mesh);

        if (facility.FacilityType == FacilityBuildType.ProductWhiteboard)
        {
            AddVerticalPanel(
                position,
                new Vector3(CellInnerSize * 0.86f, OfficeWorld3DConfig.GridSize * 0.55f, CellInnerSize * 0.08f),
                WhiteboardFill
            );
        }
        else if (facility.FacilityType == FacilityBuildType.ServerRack)
        {
            AddVerticalPanel(
                position,
                new Vector3(CellInnerSize * 0.70f, OfficeWorld3DConfig.GridSize * 0.60f, CellInnerSize * 0.34f),
                ServerRackFill
            );
        }
    }

    private void AddVerticalPanel(Vector3 position, Vector3 size, Color color)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = CreateMaterial(color),
            Position =
                position
                + new Vector3(
                    0.0f,
                    size.Y / 2.0f + OfficeWorld3DConfig.GridSize * 0.08f,
                    -OfficeWorld3DConfig.GridSize * 0.22f
                ),
        };
        AddChild(mesh);
        _renderedFacilities.Add(mesh);
    }

    private void AddHighlight(Vector3 position)
    {
        var mesh = new MeshInstance3D
        {
            Mesh =
                new BoxMesh
                {
                    Size = new Vector3(
                        HighlightStrokeSize,
                        OfficeWorld3DConfig.GridSize * 0.035f,
                        HighlightStrokeSize
                    ),
                },
            MaterialOverride = CreateMaterial(HighlightStroke),
            Position = position - Vector3.Up * (OfficeWorld3DConfig.GridSize * 0.035f),
        };
        AddChild(mesh);
        _renderedFacilities.Add(mesh);
    }

    private Texture2D? GetFacilityTexture(FacilityBuildType facilityType)
    {
        return _facilityTextures.TryGetValue(facilityType, out var texture) ? texture : null;
    }

    private static Vector3 GetFacilityVolumeSize(FacilityBuildType facilityType)
    {
        return facilityType switch
        {
            FacilityBuildType.ProductWhiteboard =>
                new Vector3(
                    CellInnerSize * 0.88f,
                    OfficeWorld3DConfig.GridSize * 0.08f,
                    CellInnerSize * 0.32f
                ),
            FacilityBuildType.ServerRack =>
                new Vector3(
                    CellInnerSize * 0.72f,
                    OfficeWorld3DConfig.GridSize * 0.10f,
                    CellInnerSize * 0.60f
                ),
            _ =>
                new Vector3(
                    CellInnerSize,
                    OfficeWorld3DConfig.GridSize * 0.18f,
                    CellInnerSize * 0.74f
                ),
        };
    }

    private static float GetFacilitySpriteHeight(FacilityBuildType facilityType)
    {
        return facilityType switch
        {
            FacilityBuildType.ProductWhiteboard => OfficeWorld3DConfig.GridSize * 0.72f,
            FacilityBuildType.ServerRack => OfficeWorld3DConfig.GridSize * 0.76f,
            _ => OfficeWorld3DConfig.GridSize * 0.34f,
        };
    }

    private static Color GetFacilityFillColor(FacilityBuildType facilityType)
    {
        return facilityType switch
        {
            FacilityBuildType.OfficeDesk => DeskFill,
            FacilityBuildType.ProductWhiteboard => WhiteboardFill,
            FacilityBuildType.ServerRack => ServerRackFill,
            _ => new Color(0.60f, 0.60f, 0.60f, 0.90f),
        };
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
