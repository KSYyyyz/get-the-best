using Godot;

namespace GetTheBestGodot;

public partial class OfficeSelection3DController : Node
{
    private const float TooltipOffset = 6.0f;
    private const float EmployeeHitRadiusPixels = 28.0f;
    private const float FacilityHitRadiusPixels = 34.0f;

    private Camera3D? _camera;
    private PanelContainer? _floatingTooltip;
    private Label? _tooltipLabel;
    private PlacementPreview3DController? _placementPreviewController;
    private BuildModeController? _buildModeController;
    private RoomOverlay3DRenderer? _roomOverlayRenderer;
    private Facility3DRenderer? _facilityRenderer;
    private OfficeNavigationStore? _officeNavigationStore;
    private EmployeeStore? _employeeStore;
    private Employee3DRenderer? _employeeRenderer;
    private bool _isDraggingSelection;
    private bool _isDraggingEmployee;
    private bool _isDraggingFacility;
    private Vector2I _dragStartCell;
    private Vector2I _dragCurrentCell;
    private EmployeeVisual? _draggedEmployee;
    private Vector2I _dragEmployeeOriginCell;
    private Vector2I _dragEmployeeCurrentCell;
    private bool _dragEmployeeTargetLegal;
    private FacilityPlacement? _draggedFacility;
    private Vector2I _dragFacilityOriginCell;
    private Vector2I _dragFacilityCurrentCell;
    private bool _dragFacilityTargetLegal;
    private Vector2I? _lastHoveredCell;
    private Vector2 _lastPointerScreenPosition;

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
        _officeNavigationStore = GetNodeOrNull<OfficeNavigationStore>("../OfficeNavigationStore");
        _employeeStore = GetNodeOrNull<EmployeeStore>("../EmployeeStore");
        _employeeRenderer = GetNodeOrNull<Employee3DRenderer>("../Employee3DRenderer");
        HidePointerTooltip();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                CancelInteraction();
                return;
            }

            if (keyEvent.Keycode == Key.R && _buildModeController?.IsPlaceFacilityMode() == true)
            {
                _buildModeController.RotateActiveFacilityFacing();
                if (_lastHoveredCell != null)
                {
                    ShowFacilityPlacementPreview(_lastHoveredCell.Value, _lastPointerScreenPosition);
                }
                return;
            }

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
            if (_isDraggingEmployee)
            {
                FinishEmployeeDrag(mouseEvent.Position);
                return;
            }

            if (_isDraggingFacility)
            {
                FinishFacilityDrag(mouseEvent.Position);
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

            if (
                _buildModeController?.IsPointerMode() == true
                && TryBeginEmployeeDrag(mouseEvent.Position)
            )
            {
                return;
            }

            if (
                _buildModeController?.IsPointerMode() == true
                && TryBeginFacilityDrag(mouseEvent.Position)
            )
            {
                return;
            }

            if (ShouldBeginAreaSelection())
            {
                BeginSelection(mouseEvent.Position);
            }
            return;
        }

        if (_isDraggingEmployee || _isDraggingFacility)
        {
            return;
        }

        if (_buildModeController?.IsPointerMode() == true)
        {
            FinishPointerSelection(mouseEvent.Position);
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
            RefreshPendingRoomPreview();
            ShowPointerTooltip("请选择门的位置", screenPosition);
            return;
        }

        ShowSelectionPreview(screenPosition);
    }

    private void FinishPointerSelection(Vector2 screenPosition)
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
        if (!IsPointerSelectionDrag())
        {
            _placementPreviewController?.ClearPreview();
            SelectObjectAtPointer(screenPosition);
            return;
        }

        ClearSelectedObjects();
        if (SelectEmployeesInSelection(screenPosition))
        {
            _placementPreviewController?.ClearPreview();
            return;
        }

        ShowPointerSelectionRect();
        HidePointerTooltip();
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
            RefreshPendingRoomPreview();
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
            _facilityRenderer?.RefreshFacilities();
            ClearSelectedRoom();
            _facilityRenderer?.HighlightFacility(facility);
            ShowFacilityTooltip(facility, screenPosition);
            return;
        }

        ShowPointerTooltip(
            _buildModeController?.GetFacilityPlacementFailureMessage(cell) ?? string.Empty,
            screenPosition
        );
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
        if (TryScreenPositionToEmployee(screenPosition, out var employee) && employee != null)
        {
            SelectEmployeeAtPointer(employee, screenPosition);
            return;
        }

        if (TryScreenPositionToFacility(screenPosition, out var facility) && facility != null)
        {
            SelectFacilityAtPointer(facility, screenPosition);
            return;
        }

        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            ClearSelectedObjects();
            HidePointerTooltip();
            return;
        }

        SelectRoomAtPointer(cell, screenPosition);
    }

    private void SelectEmployeeAtPointer(EmployeeVisual employee, Vector2 screenPosition)
    {
        ClearSelectedObjects();
        _employeeRenderer?.HighlightEmployee(employee);
        ShowEmployeeTooltip(employee, screenPosition);
    }

    private bool TryBeginEmployeeDrag(Vector2 screenPosition)
    {
        if (!TryScreenPositionToEmployee(screenPosition, out var employee) || employee == null)
        {
            return false;
        }

        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            return false;
        }

        _isDraggingEmployee = true;
        _draggedEmployee = employee;
        _dragEmployeeOriginCell = employee.Cell;
        _dragEmployeeCurrentCell = employee.Cell;
        _dragEmployeeTargetLegal = true;
        ClearSelectedObjects();
        ClearObjectHoverState();
        _employeeRenderer?.HighlightEmployee(employee);
        UpdateEmployeeDragPreview(cell, screenPosition);
        return true;
    }

    private void UpdateEmployeeDragPreview(Vector2I cell, Vector2 screenPosition)
    {
        if (_draggedEmployee == null)
        {
            return;
        }

        _dragEmployeeCurrentCell = cell;
        var path = _officeNavigationStore?.FindPath(_dragEmployeeOriginCell, cell);
        _dragEmployeeTargetLegal =
            _employeeStore?.CanMoveEmployee(_draggedEmployee, cell) == true
            && path is { Count: > 0 };
        _employeeRenderer?.ShowEmployeeDragPreview(_draggedEmployee, cell, _dragEmployeeTargetLegal);
        ShowPointerTooltip(
            _dragEmployeeTargetLegal ? "\u70b9\u51fb\u653e\u4e0b" : "\u4e0d\u80fd\u653e\u7f6e",
            screenPosition
        );
    }

    private void FinishEmployeeDrag(Vector2 screenPosition)
    {
        if (_draggedEmployee == null)
        {
            CancelEmployeeDrag();
            return;
        }

        if (TryScreenPositionToCell(screenPosition, out var cell))
        {
            UpdateEmployeeDragPreview(cell, screenPosition);
        }
        else
        {
            _dragEmployeeTargetLegal = false;
        }

        if (_dragEmployeeCurrentCell == _dragEmployeeOriginCell)
        {
            _employeeRenderer?.ClearEmployeeDragPreview();
            _placementPreviewController?.ClearPreview();
            _employeeRenderer?.HighlightEmployee(null);
            ClearObjectHoverState();
            HidePointerTooltip();
            ResetEmployeeDragState();
            return;
        }

        if (
            _dragEmployeeTargetLegal
            && _employeeStore?.TryMoveEmployee(
                _draggedEmployee.Id,
                _dragEmployeeCurrentCell,
                out var movedEmployee
            ) == true
            && movedEmployee != null
        )
        {
            _employeeRenderer?.ClearEmployeeDragPreview();
            _placementPreviewController?.ClearPreview();
            _employeeRenderer?.RefreshEmployees();
            _employeeRenderer?.HighlightEmployee(null);
            ClearObjectHoverState();
            HidePointerTooltip();
            ResetEmployeeDragState();
            return;
        }

        _employeeRenderer?.ClearEmployeeDragPreview();
        _placementPreviewController?.ClearPreview();
        _employeeRenderer?.RefreshEmployees();
        _employeeRenderer?.HighlightEmployee(null);
        ClearObjectHoverState();
        ShowPointerTooltip("\u8fd4\u56de\u539f\u4f4d", screenPosition);
        ResetEmployeeDragState();
    }

    private bool TryBeginFacilityDrag(Vector2 screenPosition)
    {
        if (!TryScreenPositionToFacility(screenPosition, out var facility) || facility == null)
        {
            return false;
        }

        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            return false;
        }

        _isDraggingFacility = true;
        _draggedFacility = facility;
        _dragFacilityOriginCell = facility.Cell;
        _dragFacilityCurrentCell = facility.Cell;
        _dragFacilityTargetLegal = true;
        ClearSelectedObjects();
        ClearObjectHoverState();
        _facilityRenderer?.HighlightFacility(facility);
        UpdateFacilityDragPreview(cell, screenPosition);
        return true;
    }

    private void UpdateFacilityDragPreview(Vector2I cell, Vector2 screenPosition)
    {
        if (_draggedFacility == null)
        {
            return;
        }

        _dragFacilityCurrentCell = cell;
        _dragFacilityTargetLegal =
            _buildModeController?.CanMoveFacility(_draggedFacility, cell) == true;
        _facilityRenderer?.ShowFacilityDragPreview(
            _draggedFacility,
            cell,
            _dragFacilityTargetLegal
        );
        ShowPointerTooltip(
            _dragFacilityTargetLegal
                ? "\u70b9\u51fb\u653e\u4e0b"
                : "\u4e0d\u80fd\u653e\u7f6e",
            screenPosition
        );
    }

    private void FinishFacilityDrag(Vector2 screenPosition)
    {
        if (_draggedFacility == null)
        {
            CancelFacilityDrag();
            return;
        }

        if (TryScreenPositionToCell(screenPosition, out var cell))
        {
            UpdateFacilityDragPreview(cell, screenPosition);
        }
        else
        {
            _dragFacilityTargetLegal = false;
        }

        if (_dragFacilityCurrentCell == _dragFacilityOriginCell)
        {
            _facilityRenderer?.ClearFacilityDragPreview();
            _placementPreviewController?.ClearPreview();
            _facilityRenderer?.HighlightFacility(null);
            ClearObjectHoverState();
            HidePointerTooltip();
            ResetFacilityDragState();
            return;
        }

        if (
            _dragFacilityTargetLegal
            && _buildModeController?.TryMoveFacility(
                _draggedFacility.Id,
                _dragFacilityCurrentCell,
                out var movedFacility
            ) == true
            && movedFacility != null
        )
        {
            _facilityRenderer?.ClearFacilityDragPreview();
            _placementPreviewController?.ClearPreview();
            _facilityRenderer?.RefreshFacilities();
            _facilityRenderer?.HighlightFacility(null);
            ClearObjectHoverState();
            HidePointerTooltip();
            ResetFacilityDragState();
            return;
        }

        _facilityRenderer?.ClearFacilityDragPreview();
        _placementPreviewController?.ClearPreview();
        _facilityRenderer?.RefreshFacilities();
        _facilityRenderer?.HighlightFacility(null);
        ClearObjectHoverState();
        ShowPointerTooltip("\u8fd4\u56de\u539f\u4f4d", screenPosition);
        ResetFacilityDragState();
    }

    private void CancelEmployeeDrag()
    {
        if (_draggedEmployee != null)
        {
            _employeeRenderer?.ClearEmployeeDragPreview();
            _employeeRenderer?.RefreshEmployees();
            _employeeRenderer?.HighlightEmployee(null);
        }

        ResetEmployeeDragState();
        _placementPreviewController?.ClearPreview();
        HidePointerTooltip();
    }

    private void CancelFacilityDrag()
    {
        if (_draggedFacility != null)
        {
            _facilityRenderer?.ClearFacilityDragPreview();
            _facilityRenderer?.RefreshFacilities();
            _facilityRenderer?.HighlightFacility(null);
        }

        ResetFacilityDragState();
        _placementPreviewController?.ClearPreview();
        HidePointerTooltip();
    }

    private void ResetEmployeeDragState()
    {
        _isDraggingEmployee = false;
        _draggedEmployee = null;
        _dragEmployeeOriginCell = Vector2I.Zero;
        _dragEmployeeCurrentCell = Vector2I.Zero;
        _dragEmployeeTargetLegal = false;
    }

    private void ResetFacilityDragState()
    {
        _isDraggingFacility = false;
        _draggedFacility = null;
        _dragFacilityOriginCell = Vector2I.Zero;
        _dragFacilityCurrentCell = Vector2I.Zero;
        _dragFacilityTargetLegal = false;
    }

    private bool SelectEmployeesInSelection(Vector2 screenPosition)
    {
        var employees = _employeeStore?.FindInSelection(_dragStartCell, _dragCurrentCell);
        if (employees == null || employees.Count == 0)
        {
            return false;
        }

        ClearSelectedObjects();
        _employeeRenderer?.HighlightEmployees(employees);
        ShowPointerTooltip($"\u5df2\u9009\u4e2d {employees.Count} \u540d\u5458\u5de5", screenPosition);
        return true;
    }

    private void SelectFacilityAtPointer(FacilityPlacement facility, Vector2 screenPosition)
    {
        ClearSelectedObjects();
        _facilityRenderer?.HighlightFacility(facility);
        ShowFacilityTooltip(facility, screenPosition);
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
        _lastPointerScreenPosition = screenPosition;
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            _lastHoveredCell = null;
            if (_isDraggingEmployee)
            {
                _dragEmployeeTargetLegal = false;
                _placementPreviewController?.ClearPreview();
                ShowPointerTooltip("\u4e0d\u80fd\u653e\u7f6e", screenPosition);
                return;
            }

            if (_isDraggingFacility)
            {
                _dragFacilityTargetLegal = false;
                _placementPreviewController?.ClearPreview();
                ShowPointerTooltip("\u4e0d\u80fd\u653e\u7f6e", screenPosition);
                return;
            }

            if (_buildModeController?.IsPlaceRoomDoorMode() == true)
            {
                RefreshPendingRoomPreview();
                ShowPointerTooltip(
                    _buildModeController.HasPendingDoor()
                        ? "\u70b9\u51fb\u786e\u8ba4\u5b8c\u6210\u5efa\u9020"
                        : "\u9009\u62e9\u95e8\u7684\u4f4d\u7f6e",
                    screenPosition
                );
                return;
            }

            if (!_isDraggingSelection)
            {
                _placementPreviewController?.ClearPreview();
                ClearObjectHoverState();
                HidePointerTooltip();
            }
            return;
        }

        _lastHoveredCell = cell;
        if (_isDraggingEmployee)
        {
            UpdateEmployeeDragPreview(cell, screenPosition);
            return;
        }

        if (_isDraggingFacility)
        {
            UpdateFacilityDragPreview(cell, screenPosition);
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
            ShowFacilityPlacementPreview(cell, screenPosition);
            return;
        }

        if (_buildModeController?.IsPlaceRoomDoorMode() == true)
        {
            RefreshPendingRoomPreview();
            ShowPointerTooltip(
                _buildModeController.HasPendingDoor() ? "点击确认完成建造" : "选择门的位置",
                screenPosition
            );
            return;
        }

        _placementPreviewController?.ClearPreview();
        if (TryScreenPositionToEmployee(screenPosition, out var hoveredEmployee) && hoveredEmployee != null)
        {
            _employeeRenderer?.HoverEmployee(hoveredEmployee);
            _facilityRenderer?.HoverFacility(null);
            ShowEmployeeTooltip(hoveredEmployee, screenPosition);
            return;
        }

        if (TryScreenPositionToFacility(screenPosition, out var hoveredFacility) && hoveredFacility != null)
        {
            _employeeRenderer?.HoverEmployee(null);
            _facilityRenderer?.HoverFacility(hoveredFacility);
            ShowFacilityTooltip(hoveredFacility, screenPosition);
            return;
        }

        var hoveredRoom = _buildModeController?.FindRoomAtCell(cell);
        if (hoveredRoom != null)
        {
            ClearObjectHoverState();
            ShowOccupiedRoom(hoveredRoom, screenPosition);
            return;
        }

        ClearObjectHoverState();
        HidePointerTooltip();
    }

    private void ShowSelectionPreview(Vector2 screenPosition)
    {
        var size = BuildModeController.FormatSelectionSize(_dragStartCell, _dragCurrentCell);
        if (_buildModeController?.IsPointerMode() == true)
        {
            ShowPointerSelectionRect();
            HidePointerTooltip();
            return;
        }

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

    private void RefreshPendingRoomPreview()
    {
        var pendingSelection = _buildModeController?.GetPendingRoomSelection();
        if (pendingSelection == null)
        {
            return;
        }

        _placementPreviewController?.ShowSelectionRect(
            pendingSelection.StartCell,
            pendingSelection.EndCell,
            isLegal: true
        );
        if (pendingSelection.DoorPlacement != null)
        {
            _placementPreviewController?.ShowRoomDoorPreview(pendingSelection.DoorPlacement);
        }
    }

    private void CancelInteraction()
    {
        if (_isDraggingEmployee)
        {
            CancelEmployeeDrag();
            return;
        }

        if (_isDraggingFacility)
        {
            CancelFacilityDrag();
            return;
        }

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
        return _buildModeController?.IsPointerMode() == true
            || _buildModeController?.IsBuildRoomMode() == true
            || _buildModeController?.IsDeleteRoomMode() == true;
    }

    private bool IsPointerSelectionDrag()
    {
        return _dragStartCell != _dragCurrentCell;
    }

    private void ShowPointerSelectionRect()
    {
        _placementPreviewController?.ShowSelectionRect(_dragStartCell, _dragCurrentCell, isLegal: true);
    }

    private void ShowFacilityPlacementPreview(Vector2I cell, Vector2 screenPosition)
    {
        if (_buildModeController == null)
        {
            return;
        }

        var canPlace = _buildModeController.CanPlaceFacility(cell);
        _placementPreviewController?.ShowFacilityCell(
            cell,
            canPlace,
            FacilityDefinitionCatalog.GetDefinition(_buildModeController.GetActiveFacilityType()),
            _buildModeController.GetActiveFacilityFacing()
        );
        ShowPointerTooltip(
            canPlace
                ? _buildModeController.GetActiveFacilityTypeLabel()
                : _buildModeController.GetFacilityPlacementFailureMessage(cell),
            screenPosition
        );
    }

    private void ClearSelectedObjects()
    {
        ClearSelectedRoom();
        _facilityRenderer?.HighlightFacility(null);
        _employeeRenderer?.HighlightEmployee(null);
    }

    private void ClearObjectHoverState()
    {
        _employeeRenderer?.HoverEmployee(null);
        _facilityRenderer?.HoverFacility(null);
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

    private void ShowEmployeeTooltip(EmployeeVisual employee, Vector2 screenPosition)
    {
        ShowPointerTooltip($"{employee.DisplayName} / {employee.RoleLabel}", screenPosition);
    }

    private bool TryScreenPositionToEmployee(Vector2 screenPosition, out EmployeeVisual? employee)
    {
        employee = null;
        if (_camera == null || _employeeStore == null)
        {
            return false;
        }

        var bestDistance = float.MaxValue;
        foreach (var candidate in _employeeStore.GetEmployees())
        {
            var cellCenter = OfficeWorld3DConfig.CellToWorldPosition(candidate.Cell);
            var distance = GetNearestProjectedObjectDistance(
                screenPosition,
                cellCenter,
                [
                    OfficeWorld3DConfig.GridSize * 0.25f,
                    OfficeWorld3DConfig.GridSize * 0.75f,
                    OfficeWorld3DConfig.GridSize * 1.15f,
                ]
            );
            if (distance >= bestDistance || distance > EmployeeHitRadiusPixels)
            {
                continue;
            }

            bestDistance = distance;
            employee = candidate;
        }

        return employee != null;
    }

    private bool TryScreenPositionToFacility(Vector2 screenPosition, out FacilityPlacement? facility)
    {
        facility = null;
        if (_camera == null || _buildModeController == null)
        {
            return false;
        }

        var bestDistance = float.MaxValue;
        for (var y = 0; y < OfficeWorld3DConfig.Rows; y++)
        {
            for (var x = 0; x < OfficeWorld3DConfig.Columns; x++)
            {
                var candidate = _buildModeController.FindFacilityAtCell(new Vector2I(x, y));
                if (candidate == null)
                {
                    continue;
                }

                var cellCenter = OfficeWorld3DConfig.CellToWorldPosition(candidate.Cell);
                var distance = GetNearestProjectedObjectDistance(
                    screenPosition,
                    cellCenter,
                    [
                        OfficeWorld3DConfig.GridSize * 0.25f,
                        OfficeWorld3DConfig.GridSize * 0.55f,
                        OfficeWorld3DConfig.GridSize * 0.85f,
                    ]
                );
                if (distance >= bestDistance || distance > FacilityHitRadiusPixels)
                {
                    continue;
                }

                bestDistance = distance;
                facility = candidate;
            }
        }

        return facility != null;
    }

    private float GetNearestProjectedObjectDistance(
        Vector2 screenPosition,
        Vector3 cellCenter,
        float[] sampleHeights
    )
    {
        if (_camera == null)
        {
            return float.MaxValue;
        }

        var bestDistance = float.MaxValue;
        foreach (var height in sampleHeights)
        {
            var samplePoint = cellCenter + new Vector3(0.0f, height, 0.0f);
            if (_camera.IsPositionBehind(samplePoint))
            {
                continue;
            }

            bestDistance = Mathf.Min(
                bestDistance,
                screenPosition.DistanceTo(_camera.UnprojectPosition(samplePoint))
            );
        }

        return bestDistance;
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
