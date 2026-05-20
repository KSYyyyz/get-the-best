using Godot;
using System.Collections.Generic;

namespace GetTheBestGodot;

public partial class FacilityRenderer : Node2D
{
    private static readonly Color DeskFill = new(0.72f, 0.50f, 0.28f, 0.92f);
    private static readonly Color WhiteboardFill = new(0.86f, 0.92f, 0.90f, 0.92f);
    private static readonly Color ServerRackFill = new(0.34f, 0.46f, 0.70f, 0.92f);
    private static readonly Color FacilityStroke = new(0.08f, 0.10f, 0.10f, 0.95f);
    private static readonly Color HighlightStroke = new(1.0f, 0.95f, 0.42f, 1.0f);

    private readonly Dictionary<FacilityBuildType, Texture2D?> _facilityTextures = [];
    private FacilityPlacementStore? _facilityPlacementStore;
    private FacilityPlacement? _highlightedFacility;

    public override void _Ready()
    {
        _facilityPlacementStore = GetNodeOrNull<FacilityPlacementStore>("../FacilityPlacementStore");
        LoadFacilityTextures();
    }

    public void RefreshFacilities()
    {
        QueueRedraw();
    }

    public void HighlightFacility(FacilityPlacement? facility)
    {
        _highlightedFacility = facility;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_facilityPlacementStore == null)
        {
            return;
        }

        foreach (var facility in _facilityPlacementStore.GetFacilities())
        {
            var definition = FacilityDefinitionCatalog.GetDefinition(facility.FacilityType);
            var cellRect = OfficeWorldConfig.CellToWorldRect(facility.Cell);
            var facilityRect = GetFacilityRenderRect(cellRect, definition);
            DrawFacility(facility, facilityRect);
            DrawRect(facilityRect, FacilityStroke, filled: false, width: 3.0f);

            if (_highlightedFacility?.Id == facility.Id)
            {
                DrawRect(facilityRect.Grow(5.0f), HighlightStroke, filled: false, width: 4.0f);
            }
        }
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

    private void DrawFacility(FacilityPlacement facility, Rect2 facilityRect)
    {
        var texture = GetFacilityTexture(facility.FacilityType);
        if (texture == null)
        {
            DrawRect(facilityRect, GetFacilityFillColor(facility.FacilityType), filled: true);
            return;
        }

        DrawTextureRect(texture, FitTextureRect(texture, facilityRect), tile: false);
    }

    private Texture2D? GetFacilityTexture(FacilityBuildType facilityType)
    {
        return _facilityTextures.TryGetValue(facilityType, out var texture) ? texture : null;
    }

    private static Rect2 GetFacilityRenderRect(Rect2 cellRect, FacilityDefinition definition)
    {
        var footprintSize = new Vector2(
            OfficeWorldConfig.GridSize * definition.Footprint.X,
            OfficeWorldConfig.GridSize * definition.Footprint.Y
        );
        var baseRect = new Rect2(cellRect.Position, footprintSize);
        return baseRect.Grow(-8.0f);
    }

    private static Rect2 FitTextureRect(Texture2D texture, Rect2 bounds)
    {
        var textureSize = texture.GetSize();
        if (textureSize.X <= 0.0f || textureSize.Y <= 0.0f)
        {
            return bounds;
        }

        var scale = Mathf.Min(bounds.Size.X / textureSize.X, bounds.Size.Y / textureSize.Y);
        var size = textureSize * scale;
        return new Rect2(bounds.Position + (bounds.Size - size) / 2.0f, size);
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
}
