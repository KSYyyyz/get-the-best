using Godot;

namespace GetTheBestGodot;

public partial class OfficeSelectionController : Node2D
{
    private const float TooltipOffset = 6.0f;

    private PanelContainer? _floatingTooltip;
    private Label? _tooltipLabel;
    private PlacementPreviewController? _placementPreviewController;
    private BuildModeController? _buildModeController;
    private RoomOverlayRenderer? _roomOverlayRenderer;
    private FacilityRenderer? _facilityRenderer;
    private bool _isDraggingSelection;
    private Vector2I _dragStartCell;
    private Vector2I _dragCurrentCell;

    public override void _Ready()
    {
        _floatingTooltip = GetNodeOrNull<PanelContainer>("../../HudRoot/FloatingTooltip");
        _tooltipLabel = GetNodeOrNull<Label>("../../HudRoot/FloatingTooltip/TooltipLabel");
        _placementPreviewController = GetNodeOrNull<PlacementPreviewController>(
            "../PlacementPreviewController"
        );
        _buildModeController = GetNodeOrNull<BuildModeController>("../BuildModeController");
        _roomOverlayRenderer = GetNodeOrNull<RoomOverlayRenderer>("../RoomOverlayRenderer");
        _facilityRenderer = GetNodeOrNull<FacilityRenderer>("../FacilityRenderer");
        HidePointerTooltip();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            CancelInteraction();
            return;
        }

        if (@event is InputEventMouseMotion motionEvent)
        {
            UpdateHoverOrDragPreview(motionEvent.Position);
            return;
        }

        if (@event is not InputEventMouseButton mouseEvent)
        {
            return;
        }

        if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
        {
            if (_isDraggingSelection)
            {
                CancelDragSelection();
                return;
            }

            CancelInteraction();
            return;
        }

        if (mouseEvent.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        if (mouseEvent.Pressed)
        {
            if (_buildModeController?.IsPointerMode() == true)
            {
                SelectObjectAtPointer(mouseEvent.Position);
                return;
            }

            if (_buildModeController?.IsPlaceFacilityMode() == true)
            {
                FinishFacilityPlacement(mouseEvent.Position);
                return;
            }

            BeginSelection(mouseEvent.Position);
            return;
        }

        if (_buildModeController?.IsDeleteRoomMode() == true)
        {
            FinishDeleteSelection(mouseEvent.Position);
            return;
        }

        if (_buildModeController?.IsBuildRoomMode() == true)
        {
            FinishSelection(mouseEvent.Position);
        }
    }

    private void BeginSelection(Vector2 screenPosition)
    {
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            CancelDragSelection();
            return;
        }

        _dragStartCell = cell;
        _dragCurrentCell = cell;
        _isDraggingSelection = true;
        ShowSelectionPreview(screenPosition);
    }

    private void FinishSelection(Vector2 screenPosition)
    {
        if (!_isDraggingSelection)
        {
            return;
        }

        if (TryScreenPositionToCell(screenPosition, out var cell))
        {
            _dragCurrentCell = cell;
        }

        _isDraggingSelection = false;
        if (
            _buildModeController?.TryCreateRoom(_dragStartCell, _dragCurrentCell, out var room)
                == true
            && room != null
        )
        {
            _placementPreviewController?.ClearPreview();
            ClearSelectedObjects();
            _roomOverlayRenderer?.RefreshRooms();
            ShowOccupiedRoom(room, screenPosition);
            return;
        }

        ShowSelectionPreview(screenPosition);
    }

    private void FinishFacilityPlacement(Vector2 screenPosition)
    {
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            HidePointerTooltip();
            return;
        }

        if (_buildModeController?.TryPlaceFacility(cell, out var facility) == true && facility != null)
        {
            _placementPreviewController?.ClearPreview();
            ClearSelectedObjects();
            _facilityRenderer?.RefreshFacilities();
            ShowFacilityTooltip(facility, screenPosition);
            return;
        }

        ShowPointerTooltip("不能放置在这里", screenPosition);
    }

    private void FinishDeleteSelection(Vector2 screenPosition)
    {
        if (!_isDraggingSelection)
        {
            return;
        }

        if (TryScreenPositionToCell(screenPosition, out var cell))
        {
            _dragCurrentCell = cell;
        }

        _isDraggingSelection = false;
        var deletedFacilities =
            _buildModeController?.DeleteFacilitiesInSelection(_dragStartCell, _dragCurrentCell) ?? 0;
        if (
            _buildModeController?.TryDeleteRoomsInSelection(
                _dragStartCell,
                _dragCurrentCell,
                out var deletedCount
            ) == true
        )
        {
            _placementPreviewController?.ClearPreview();
            ClearSelectedObjects();
            _roomOverlayRenderer?.RefreshRooms();
            _facilityRenderer?.RefreshFacilities();
            ShowPointerTooltip($"已删除 {deletedCount} 格，出售 {deletedFacilities} 个设施", screenPosition);
            return;
        }

        if (deletedFacilities > 0)
        {
            _placementPreviewController?.ClearPreview();
            ClearSelectedObjects();
            _facilityRenderer?.RefreshFacilities();
            ShowPointerTooltip($"已出售 {deletedFacilities} 个设施", screenPosition);
            return;
        }

        _placementPreviewController?.ClearPreview();
        ClearSelectedObjects();
        ShowPointerTooltip("没有可删除地块", screenPosition);
    }

    private void SelectObjectAtPointer(Vector2 screenPosition)
    {
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            ClearSelectedObjects();
            HidePointerTooltip();
            return;
        }

        if (SelectFacilityAtPointer(cell, screenPosition))
        {
            return;
        }

        SelectRoomAtPointer(cell, screenPosition);
    }

    private bool SelectFacilityAtPointer(Vector2I cell, Vector2 screenPosition)
    {
        var facility = _buildModeController?.FindFacilityAtCell(cell);
        if (facility == null)
        {
            return false;
        }

        ClearSelectedObjects();
        _facilityRenderer?.HighlightFacility(facility);
        ShowFacilityTooltip(facility, screenPosition);
        return true;
    }

    private void SelectRoomAtPointer(Vector2I cell, Vector2 screenPosition)
    {
        var room = _buildModeController?.FindRoomAtCell(cell);
        if (room == null)
        {
            ClearSelectedObjects();
            HidePointerTooltip();
            return;
        }

        ClearSelectedObjects();
        _roomOverlayRenderer?.HighlightRoom(room);
        _roomOverlayRenderer?.RefreshRooms();
        ShowOccupiedRoom(room, screenPosition);
    }

    private void UpdateHoverOrDragPreview(Vector2 screenPosition)
    {
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            if (!_isDraggingSelection)
            {
                _placementPreviewController?.ClearPreview();
                HidePointerTooltip();
            }
            return;
        }

        if (_isDraggingSelection)
        {
            _dragCurrentCell = cell;
            ShowSelectionPreview(screenPosition);
            return;
        }

        if (_buildModeController?.IsPlaceFacilityMode() == true)
        {
            var canPlace = _buildModeController.CanPlaceFacility(cell);
            _placementPreviewController?.ShowFacilityCell(cell, canPlace);
            ShowPointerTooltip(_buildModeController.GetActiveFacilityTypeLabel(), screenPosition);
            return;
        }

        _placementPreviewController?.ClearPreview();
        var hoveredFacility = _buildModeController?.FindFacilityAtCell(cell);
        if (hoveredFacility != null)
        {
            ShowFacilityTooltip(hoveredFacility, screenPosition);
            return;
        }

        var hoveredRoom = _buildModeController?.FindRoomAtCell(cell);
        if (hoveredRoom != null)
        {
            ShowOccupiedRoom(hoveredRoom, screenPosition);
            return;
        }

        HidePointerTooltip();
    }

    private void ShowSelectionPreview(Vector2 screenPosition)
    {
        var size = BuildModeController.FormatSelectionSize(_dragStartCell, _dragCurrentCell);
        if (_buildModeController?.IsDeleteRoomMode() == true)
        {
            var isLegal = _buildModeController.CanDeleteSelection(_dragStartCell, _dragCurrentCell);
            _placementPreviewController?.ShowSelectionRect(_dragStartCell, _dragCurrentCell, isLegal);
            ShowPointerTooltip($"删除 {size}", screenPosition);
            return;
        }

        var isBuildLegal = _buildModeController?.IsSelectionLegal(_dragStartCell, _dragCurrentCell) ?? true;
        _placementPreviewController?.ShowSelectionRect(_dragStartCell, _dragCurrentCell, isBuildLegal);
        ShowPointerTooltip(size, screenPosition);
    }

    private void CancelInteraction()
    {
        CancelDragSelection();
        ClearSelectedObjects();
        _buildModeController?.CancelActiveTool();
    }

    private void CancelDragSelection()
    {
        _isDraggingSelection = false;
        _placementPreviewController?.ClearPreview();
        HidePointerTooltip();
    }

    private void ClearSelectedObjects()
    {
        ClearSelectedRoom();
        _facilityRenderer?.HighlightFacility(null);
    }

    private void ClearSelectedRoom()
    {
        _roomOverlayRenderer?.HighlightRoom(null);
    }

    private void ShowOccupiedRoom(RoomFootprint room, Vector2 screenPosition)
    {
        var prefix = _buildModeController?.IsDeleteRoomMode() == true ? "删除：" : string.Empty;
        ShowPointerTooltip($"{prefix}{BuildModeController.GetRoomTypeLabel(room.RoomType)}", screenPosition);
    }

    private void ShowFacilityTooltip(FacilityPlacement facility, Vector2 screenPosition)
    {
        var prefix = _buildModeController?.IsDeleteRoomMode() == true ? "出售：" : string.Empty;
        ShowPointerTooltip(
            $"{prefix}{BuildModeController.GetFacilityTypeLabel(facility.FacilityType)}",
            screenPosition
        );
    }

    private bool TryScreenPositionToCell(Vector2 screenPosition, out Vector2I cell)
    {
        var worldPosition = GetViewport().GetCanvasTransform().AffineInverse() * screenPosition;
        return OfficeWorldConfig.TryWorldToCell(worldPosition, out cell);
    }

    private void ShowPointerTooltip(string text, Vector2 screenPosition)
    {
        if (_floatingTooltip == null || _tooltipLabel == null)
        {
            return;
        }

        _tooltipLabel.HorizontalAlignment = HorizontalAlignment.Left;
        _tooltipLabel.VerticalAlignment = VerticalAlignment.Top;
        _tooltipLabel.Text = text;
        _floatingTooltip.Visible = true;
        PositionPointerTooltip(screenPosition);
    }

    private void PositionPointerTooltip(Vector2 screenPosition)
    {
        if (_floatingTooltip == null || _tooltipLabel == null)
        {
            return;
        }

        var viewportSize = GetViewportRect().Size;
        var tooltipSize = _tooltipLabel.GetMinimumSize();
        if (tooltipSize.X <= 0.0f || tooltipSize.Y <= 0.0f)
        {
            tooltipSize = new Vector2(64.0f, 24.0f);
        }

        _floatingTooltip.Size = tooltipSize;
        _tooltipLabel.Position = Vector2.Zero;
        _tooltipLabel.Size = tooltipSize;
        _floatingTooltip.Position = new Vector2(
            Mathf.Clamp(screenPosition.X + TooltipOffset, 8.0f, viewportSize.X - tooltipSize.X - 8.0f),
            Mathf.Clamp(screenPosition.Y + TooltipOffset, 8.0f, viewportSize.Y - tooltipSize.Y - 8.0f)
        );
    }

    private void HidePointerTooltip()
    {
        if (_floatingTooltip != null)
        {
            _floatingTooltip.Visible = false;
        }
    }
}
