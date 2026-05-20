using Godot;

namespace GetTheBestGodot;

public partial class PlacementPreviewController : Node2D
{
    private static readonly Color HoverFill = new(1.0f, 0.86f, 0.28f, 0.20f);
    private static readonly Color HoverStroke = new(1.0f, 0.92f, 0.40f, 0.95f);
    private static readonly Color LegalFill = new(0.28f, 0.95f, 0.55f, 0.24f);
    private static readonly Color LegalStroke = new(0.44f, 1.0f, 0.68f, 0.95f);
    private static readonly Color IllegalFill = new(1.0f, 0.20f, 0.20f, 0.24f);
    private static readonly Color IllegalStroke = new(1.0f, 0.32f, 0.32f, 0.95f);

    private Rect2? _hoverRect;
    private Rect2? _facilityRect;
    private Rect2? _selectionRect;
    private bool _isFacilityLegal = true;
    private bool _isSelectionLegal = true;

    public void ShowHoverCell(Vector2I cell)
    {
        _hoverRect = OfficeWorldConfig.CellToWorldRect(cell);
        QueueRedraw();
    }

    public void ShowSelectionRect(Vector2I startCell, Vector2I endCell, bool isLegal)
    {
        _selectionRect = OfficeWorldConfig.CellsToWorldRect(startCell, endCell);
        _isSelectionLegal = isLegal;
        QueueRedraw();
    }

    public void ShowFacilityCell(Vector2I cell, bool isLegal)
    {
        _facilityRect = OfficeWorldConfig.CellToWorldRect(cell);
        _isFacilityLegal = isLegal;
        QueueRedraw();
    }

    public void ClearPreview()
    {
        _hoverRect = null;
        _facilityRect = null;
        _selectionRect = null;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_hoverRect is { } hoverRect)
        {
            DrawRect(hoverRect, HoverFill, filled: true);
            DrawRect(hoverRect, HoverStroke, filled: false, width: 3.0f);
        }

        if (_selectionRect is not { } selectionRect)
        {
            if (_facilityRect is { } facilityRect)
            {
                DrawRect(facilityRect, _isFacilityLegal ? LegalFill : IllegalFill, filled: true);
                DrawRect(
                    facilityRect,
                    _isFacilityLegal ? LegalStroke : IllegalStroke,
                    filled: false,
                    width: 4.0f
                );
            }

            return;
        }

        DrawRect(selectionRect, _isSelectionLegal ? LegalFill : IllegalFill, filled: true);
        DrawRect(selectionRect, _isSelectionLegal ? LegalStroke : IllegalStroke, filled: false, width: 4.0f);
    }
}
