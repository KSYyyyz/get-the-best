using Godot;

namespace GetTheBestGodot;

public partial class OfficeSelection3DController : Node
{
    private const float TooltipOffset = 6.0f;

    private Camera3D? _camera;
    private PanelContainer? _floatingTooltip;
    private Label? _tooltipLabel;
    private PlacementPreview3DController? _placementPreviewController;
    private BuildModeController? _buildModeController;
    private RoomOverlay3DRenderer? _roomOverlayRenderer;
    private Facility3DRenderer? _facilityRenderer;
    private bool _isDraggingSelection;
    private Vector2I _dragStartCell;
    private Vector2I _dragCurrentCell;

    public override void _Ready()
    {
        _camera = GetNodeOrNull<Camera3D>("../../OfficeWorld/OfficeCamera");
        _floatingTooltip = GetNodeOrNull<PanelContainer>("../../HudRoot/FloatingTooltip");
        _tooltipLabel = GetNodeOrNull<Label>("../../HudRoot/FloatingTooltip/TooltipLabel");
        _placementPreviewController = GetNodeOrNull<PlacementPreview3DController>(
            "../PlacementPreview3DController"
        );
        _buildModeController = GetNodeOrNull<BuildModeController>("../BuildModeController");
        _roomOverlayRenderer = GetNodeOrNull<RoomOverlay3DRenderer>("../RoomOverlay3DRenderer");
        _facilityRenderer = GetNodeOrNull<Facility3DRenderer>("../Facility3DRenderer");
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

            if (_buildModeController?.IsPlaceRoomDoorMode() == true)
            {
                FinishDoorPlacement(mouseEvent.Position);
                return;
            }

            if (ShouldBeginAreaSelection())
            {
                BeginSelection(mouseEvent.Position);
            }
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
        if (_buildModeController?.TryStartPendingRoomSelection(_dragStartCell, _dragCurrentCell) == true)
        {
            ClearSelectedObjects();
            ShowPointerTooltip("请选择门的位置", screenPosition);
            return;
        }

        ShowSelectionPreview(screenPosition);
    }

    private void FinishDoorPlacement(Vector2 screenPosition)
    {
        if (
            !TryScreenPositionToCell(screenPosition, out var cell)
            || !TryScreenPositionToWorldPosition(screenPosition, out var worldPosition)
        )
        {
            ShowPointerTooltip("请选择房间边缘", screenPosition);
            return;
        }

        if (_buildModeController?.TrySetPendingDoorFromWorldPosition(cell, worldPosition) == true)
        {
            ShowPointerTooltip("门已设置，点击确认完成建造", screenPosition);
            return;
        }

        ShowPointerTooltip("门必须放在房间边缘", screenPosition);
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
            DeleteSingleCellAtPointer(screenPosition);
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

        if (
            _dragStartCell == _dragCurrentCell
            && _buildModeController?.TryDeleteRoomAtCell(_dragCurrentCell, out var singleDeletedRoom)
                == true
            && singleDeletedRoom != null
        )
        {
            RefreshAfterRoomDeletion(singleDeletedRoom, screenPosition);
            return;
        }

        _placementPreviewController?.ClearPreview();
        ClearSelectedObjects();
        ShowPointerTooltip("没有可删除地块", screenPosition);
    }

    private void DeleteSingleCellAtPointer(Vector2 screenPosition)
    {
        var hasWorldPosition = TryScreenPositionToWorldPosition(screenPosition, out var worldPosition);
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            if (
                hasWorldPosition
                && _buildModeController?.TryDeleteRoomDoorAtWorldPosition(worldPosition, out var doorRoom)
                    == true
                && doorRoom != null
            )
            {
                RefreshAfterRoomDeletion(doorRoom, screenPosition);
                return;
            }

            _placementPreviewController?.ClearPreview();
            ClearSelectedObjects();
            HidePointerTooltip();
            return;
        }

        var deletedFacilities = _buildModeController?.DeleteFacilitiesInSelection(cell, cell) ?? 0;
        if (_buildModeController?.TryDeleteRoomAtCell(cell, out var room) == true && room != null)
        {
            RefreshAfterRoomDeletion(room, screenPosition);
            return;
        }

        if (
            hasWorldPosition
            && _buildModeController?.TryDeleteRoomDoorAtWorldPosition(worldPosition, out room) == true
            && room != null
        )
        {
            RefreshAfterRoomDeletion(room, screenPosition);
            return;
        }

        if (deletedFacilities > 0)
        {
            _placementPreviewController?.ClearPreview();
            ClearSelectedObjects();
            _facilityRenderer?.RefreshFacilities();
            ShowPointerTooltip(
                $"\u5df2\u51fa\u552e {deletedFacilities} \u4e2a\u8bbe\u65bd",
                screenPosition
            );
            return;
        }

        _placementPreviewController?.ClearPreview();
        ClearSelectedObjects();
        ShowPointerTooltip("\u6ca1\u6709\u53ef\u5220\u9664\u5730\u5757", screenPosition);
    }

    private void RefreshAfterRoomDeletion(RoomFootprint room, Vector2 screenPosition)
    {
        _placementPreviewController?.ClearPreview();
        ClearSelectedObjects();
        _roomOverlayRenderer?.RefreshRooms();
        _facilityRenderer?.RefreshFacilities();
        ShowPointerTooltip(
            $"\u5df2\u5220\u9664 1 \u683c: {BuildModeController.GetRoomTypeLabel(room.RoomType)}",
            screenPosition
        );
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

        if (_buildModeController?.IsPlaceRoomDoorMode() == true)
        {
            ShowPointerTooltip(
                _buildModeController.HasPendingDoor() ? "点击确认完成建造" : "选择门的位置",
                screenPosition
            );
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

    private bool ShouldBeginAreaSelection()
    {
        return _buildModeController?.IsBuildRoomMode() == true
            || _buildModeController?.IsDeleteRoomMode() == true;
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
        cell = Vector2I.Zero;
        if (!TryScreenPositionToWorldPosition(screenPosition, out var worldPosition))
        {
            return false;
        }

        return OfficeWorld3DConfig.TryWorldToCell(worldPosition, out cell);
    }

    private bool TryScreenPositionToWorldPosition(Vector2 screenPosition, out Vector3 worldPosition)
    {
        worldPosition = Vector3.Zero;
        if (_camera == null)
        {
            return false;
        }

        var rayOrigin = _camera.ProjectRayOrigin(screenPosition);
        var rayDirection = _camera.ProjectRayNormal(screenPosition);
        var groundPlane = new Plane(Vector3.Up, 0.0f);
        if (Mathf.IsZeroApprox(rayDirection.Y))
        {
            return false;
        }

        var distance = -rayOrigin.Y / rayDirection.Y;
        if (distance < 0.0f)
        {
            return false;
        }

        worldPosition = rayOrigin + rayDirection * distance;
        _ = groundPlane;
        return true;
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

        var viewportSize = GetViewport().GetVisibleRect().Size;
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
