using Godot;

namespace GetTheBestGodot;

public partial class FacilityRenderer : Node2D
{
    private static readonly Color DeskFill = new(0.72f, 0.50f, 0.28f, 0.92f);
    private static readonly Color WhiteboardFill = new(0.86f, 0.92f, 0.90f, 0.92f);
    private static readonly Color ServerRackFill = new(0.34f, 0.46f, 0.70f, 0.92f);
    private static readonly Color FacilityStroke = new(0.08f, 0.10f, 0.10f, 0.95f);
    private static readonly Color HighlightStroke = new(1.0f, 0.95f, 0.42f, 1.0f);

    private FacilityPlacementStore? _facilityPlacementStore;
    private FacilityPlacement? _highlightedFacility;

    public override void _Ready()
    {
        _facilityPlacementStore = GetNodeOrNull<FacilityPlacementStore>("../FacilityPlacementStore");
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
            var cellRect = OfficeWorldConfig.CellToWorldRect(facility.Cell);
            var facilityRect = cellRect.Grow(-18.0f);
            DrawRect(facilityRect, GetFacilityFillColor(facility.FacilityType), filled: true);
            DrawRect(facilityRect, FacilityStroke, filled: false, width: 3.0f);

            if (_highlightedFacility?.Id == facility.Id)
            {
                DrawRect(facilityRect.Grow(5.0f), HighlightStroke, filled: false, width: 4.0f);
            }
        }
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
