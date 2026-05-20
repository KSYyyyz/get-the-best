using Godot;

namespace GetTheBestGodot;

public partial class OfficeSelectionController : Node2D
{
    private Label? _contextLabel;
    private PlacementPreviewController? _placementPreviewController;
    private BuildModeController? _buildModeController;
    private RoomOverlayRenderer? _roomOverlayRenderer;
    private bool _isDraggingSelection;
    private Vector2I _dragStartCell;
    private Vector2I _dragCurrentCell;

    public override void _Ready()
    {
        _contextLabel = GetNodeOrNull<Label>("../../HudRoot/ContextPanel/ContextLabel");
        _placementPreviewController = GetNodeOrNull<PlacementPreviewController>("../PlacementPreviewController");
        _buildModeController = GetNodeOrNull<BuildModeController>("../BuildModeController");
        _roomOverlayRenderer = GetNodeOrNull<RoomOverlayRenderer>("../RoomOverlayRenderer");
        SetContextText("未选中对象");
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

        if (_buildModeController?.IsDeleteRoomMode() == true)
        {
            if (mouseEvent.Pressed)
            {
                TryDeleteRoomAtScreenPosition(mouseEvent.Position);
            }
            return;
        }

        if (mouseEvent.Pressed)
        {
            BeginSelection(mouseEvent.Position);
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
        ShowSelectionPreview();
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
            ShowOccupiedRoom(room);
            return;
        }

        ShowSelectionPreview();
    }

    private void UpdateHoverOrDragPreview(Vector2 screenPosition)
    {
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            if (!_isDraggingSelection)
            {
                _placementPreviewController?.ClearPreview();
                _roomOverlayRenderer?.HighlightRoom(null);
                SetContextText("未选中对象");
            }
            return;
        }

        if (_isDraggingSelection)
        {
            _dragCurrentCell = cell;
            ShowSelectionPreview();
            return;
        }

        var hoveredRoom = _buildModeController?.FindRoomAtCell(cell);
        if (hoveredRoom != null)
        {
            _roomOverlayRenderer?.HighlightRoom(hoveredRoom);
            _placementPreviewController?.ShowHoverCell(cell);
            ShowOccupiedRoom(hoveredRoom);
            return;
        }

        _roomOverlayRenderer?.HighlightRoom(null);
        _placementPreviewController?.ShowHoverCell(cell);
        SetContextText(
            _buildModeController?.IsDeleteRoomMode() == true
                ? $"删除房间：格子 {FormatCell(cell)} 没有房间"
                : $"空地：格子 {FormatCell(cell)}，可建造"
        );
    }

    private void ShowSelectionPreview()
    {
        var isLegal = _buildModeController?.IsSelectionLegal(_dragStartCell, _dragCurrentCell) ?? true;
        _placementPreviewController?.ShowSelectionRect(_dragStartCell, _dragCurrentCell, isLegal);
        var summary = _buildModeController?.GetSelectionSummary(_dragStartCell, _dragCurrentCell) ?? "预览区域：当前可建造";
        SetContextText($"{summary}\n起点 {FormatCell(_dragStartCell)}，终点 {FormatCell(_dragCurrentCell)}");
    }

    private void CancelInteraction()
    {
        _isDraggingSelection = false;
        _placementPreviewController?.ClearPreview();
        _roomOverlayRenderer?.HighlightRoom(null);
        SetContextText("未选中对象");
    }

    private void TryDeleteRoomAtScreenPosition(Vector2 screenPosition)
    {
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            SetContextText("删除房间：请选择已有房间");
            return;
        }

        if (_buildModeController?.TryDeleteRoomAtCell(cell, out var room) == true && room != null)
        {
            _placementPreviewController?.ClearPreview();
            _roomOverlayRenderer?.HighlightRoom(null);
            _roomOverlayRenderer?.RefreshRooms();
            SetContextText($"已删除 {BuildModeController.GetRoomTypeLabel(room.RoomType)} #{room.Id}");
            return;
        }

        _roomOverlayRenderer?.HighlightRoom(null);
        SetContextText("删除房间：这里没有房间");
    }

    private void ShowOccupiedRoom(RoomFootprint room)
    {
        var actionHint = _buildModeController?.IsDeleteRoomMode() == true ? "\n点击删除该房间" : string.Empty;
        SetContextText(
            $"{BuildModeController.GetRoomTypeLabel(room.RoomType)} #{room.Id}：{room.CellCount} 格\n"
                + $"范围 x={room.MinCell.X}-{room.MaxCell.X}，y={room.MinCell.Y}-{room.MaxCell.Y}"
                + actionHint
        );
    }

    private bool TryScreenPositionToCell(Vector2 screenPosition, out Vector2I cell)
    {
        var worldPosition = GetViewport().GetCanvasTransform().AffineInverse() * screenPosition;
        return OfficeWorldConfig.TryWorldToCell(worldPosition, out cell);
    }

    private void SetContextText(string text)
    {
        if (_contextLabel != null)
        {
            _contextLabel.Text = text;
        }
    }

    private static string FormatCell(Vector2I cell)
    {
        return $"x={cell.X}, y={cell.Y}";
    }
}
