using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class Facility3DRenderer : Node3D
{
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
            PixelSize = 0.018f,
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
            AddVerticalPanel(position, new Vector3(1.45f, 1.10f, 0.10f), WhiteboardFill);
        }
        else if (facility.FacilityType == FacilityBuildType.ServerRack)
        {
            AddVerticalPanel(position, new Vector3(1.30f, 1.35f, 0.55f), ServerRackFill);
        }
    }

    private void AddVerticalPanel(Vector3 position, Vector3 size, Color color)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = CreateMaterial(color),
            Position = position + new Vector3(0.0f, size.Y / 2.0f + 0.18f, -0.34f),
        };
        AddChild(mesh);
        _renderedFacilities.Add(mesh);
    }

    private void AddHighlight(Vector3 position)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(1.7f, 0.08f, 1.7f) },
            MaterialOverride = CreateMaterial(HighlightStroke),
            Position = position - Vector3.Up * 0.35f,
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
            FacilityBuildType.ProductWhiteboard => new Vector3(1.45f, 0.18f, 0.55f),
            FacilityBuildType.ServerRack => new Vector3(1.30f, 0.22f, 1.10f),
            _ => new Vector3(1.45f, 0.42f, 1.15f),
        };
    }

    private static float GetFacilitySpriteHeight(FacilityBuildType facilityType)
    {
        return facilityType switch
        {
            FacilityBuildType.ProductWhiteboard => 1.15f,
            FacilityBuildType.ServerRack => 1.25f,
            _ => 0.62f,
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
