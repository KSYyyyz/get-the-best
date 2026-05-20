using Godot;

namespace GetTheBestGodot;

public partial class OfficeSelectionController : Node2D
{
    private const float TooltipOffset = 18.0f;

    private PanelContainer? _floatingTooltip;
    private Label? _tooltipLabel;
    private PlacementPreviewController? _placementPreviewController;
    private BuildModeController? _buildModeController;
    private RoomOverlayRenderer? _roomOverlayRenderer;
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
            CancelInteraction();
            return;
        }

        if (mouseEvent.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        if (mouseEvent.Pressed)
        {
            BeginSelection(mouseEvent.Position);
            return;
        }

        if (_buildModeController?.IsDeleteRoomMode() == true)
        {
            FinishDeleteSelection(mouseEvent.Position);
            return;
        }

        FinishSelection(mouseEvent.Position);
    }

    private void BeginSelection(Vector2 screenPosition)
    {
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            CancelInteraction();
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
        if (_buildModeController?.TryCreateRoom(_dragStartCell, _dragCurrentCell, out var room) == true && room != null)
        {
            _placementPreviewController?.ClearPreview();
            _roomOverlayRenderer?.RefreshRooms();
            ShowOccupiedRoom(room, screenPosition);
            return;
        }

        ShowSelectionPreview(screenPosition);
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
        if (_buildModeController?.CanDeleteSelection(_dragStartCell, _dragCurrentCell) != true)
        {
            _placementPreviewController?.ShowSelectionRect(
                _dragStartCell,
                _dragCurrentCell,
                isLegal: false
            );
            ShowPointerTooltip("区域内有设施，不能删除", screenPosition);
            return;
        }

        if (_buildModeController?.TryDeleteRoomsInSelection(_dragStartCell, _dragCurrentCell, out var deletedCount) == true)
        {
            _placementPreviewController?.ClearPreview();
            _roomOverlayRenderer?.HighlightRoom(null);
            _roomOverlayRenderer?.RefreshRooms();
            ShowPointerTooltip($"已删除 {deletedCount} 个房间", screenPosition);
            return;
        }

        _placementPreviewController?.ClearPreview();
        _roomOverlayRenderer?.HighlightRoom(null);
        ShowPointerTooltip("这里没有可删除房间", screenPosition);
    }

    private void UpdateHoverOrDragPreview(Vector2 screenPosition)
    {
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            if (!_isDraggingSelection)
            {
                _placementPreviewController?.ClearPreview();
                _roomOverlayRenderer?.HighlightRoom(null);
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

        var hoveredRoom = _buildModeController?.FindRoomAtCell(cell);
        if (hoveredRoom != null)
        {
            _roomOverlayRenderer?.HighlightRoom(hoveredRoom);
            _placementPreviewController?.ShowHoverCell(cell);
            ShowOccupiedRoom(hoveredRoom, screenPosition);
            return;
        }

        _roomOverlayRenderer?.HighlightRoom(null);
        _placementPreviewController?.ShowHoverCell(cell);
        ShowPointerTooltip(
            _buildModeController?.IsDeleteRoomMode() == true ? "没有房间" : "空地",
            screenPosition
        );
    }

    private void ShowSelectionPreview(Vector2 screenPosition)
    {
        if (_buildModeController?.IsDeleteRoomMode() == true)
        {
            var isLegal = _buildModeController.CanDeleteSelection(_dragStartCell, _dragCurrentCell);
            _placementPreviewController?.ShowSelectionRect(_dragStartCell, _dragCurrentCell, isLegal);
            var cellCount = OfficeWorldConfig.CountCells(_dragStartCell, _dragCurrentCell);
            var size = BuildModeController.FormatSelectionSize(_dragStartCell, _dragCurrentCell);
            ShowPointerTooltip($"删除区域 {size} / {cellCount}格", screenPosition);
            return;
        }

        var isBuildLegal = _buildModeController?.IsSelectionLegal(_dragStartCell, _dragCurrentCell) ?? true;
        _placementPreviewController?.ShowSelectionRect(_dragStartCell, _dragCurrentCell, isBuildLegal);
        var summary = _buildModeController?.GetSelectionSummary(_dragStartCell, _dragCurrentCell)
            ?? "预览区域：当前可建造";
        ShowPointerTooltip(summary, screenPosition);
    }

    private void CancelInteraction()
    {
        _isDraggingSelection = false;
        _placementPreviewController?.ClearPreview();
        _roomOverlayRenderer?.HighlightRoom(null);
        HidePointerTooltip();
    }

    private void ShowOccupiedRoom(RoomFootprint room, Vector2 screenPosition)
    {
        var prefix = _buildModeController?.IsDeleteRoomMode() == true ? "删除：" : string.Empty;
        ShowPointerTooltip($"{prefix}{BuildModeController.GetRoomTypeLabel(room.RoomType)}", screenPosition);
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

        _tooltipLabel.Text = text;
        _floatingTooltip.Visible = true;
        PositionPointerTooltip(screenPosition);
    }

    private void PositionPointerTooltip(Vector2 screenPosition)
    {
        if (_floatingTooltip == null)
        {
            return;
        }

        var viewportSize = GetViewportRect().Size;
        var tooltipSize = _floatingTooltip.Size;
        if (tooltipSize.X <= 0.0f || tooltipSize.Y <= 0.0f)
        {
            tooltipSize = new Vector2(168.0f, 34.0f);
        }

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
