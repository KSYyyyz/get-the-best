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
        var position = OfficeWorld3DConfig.CellToWorldPosition(facility.Cell) + Vector3.Up * 0.45f;
        var texture = GetFacilityTexture(facility.FacilityType);
        if (texture == null)
        {
            AddFallbackBox(facility, position);
            return;
        }

        var sprite = new Sprite3D
        {
            Texture = texture,
            PixelSize = 0.018f,
            Position = position,
            RotationDegrees = new Vector3(-60.0f, 0.0f, 0.0f),
        };
        AddChild(sprite);
        _renderedFacilities.Add(sprite);

        if (_highlightedFacility?.Id == facility.Id)
        {
            AddHighlight(position);
        }
    }

    private void AddFallbackBox(FacilityPlacement facility, Vector3 position)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(1.2f, 0.45f, 1.2f) },
            MaterialOverride = CreateMaterial(GetFacilityFillColor(facility.FacilityType)),
            Position = position,
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
