using Godot;

namespace GetTheBestGodot;

public partial class OfficeSelectionController : Node2D
{
    private Label? _contextLabel;
    private PlacementPreviewController? _placementPreviewController;
    private BuildModeController? _buildModeController;
    private bool _isDraggingSelection;
    private Vector2I _dragStartCell;
    private Vector2I _dragCurrentCell;

    public override void _Ready()
    {
        _contextLabel = GetNodeOrNull<Label>("../../HudRoot/ContextPanel/ContextLabel");
        _placementPreviewController = GetNodeOrNull<PlacementPreviewController>("../PlacementPreviewController");
        _buildModeController = GetNodeOrNull<BuildModeController>("../BuildModeController");
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
        ShowSelectionPreview();
    }

    private void UpdateHoverOrDragPreview(Vector2 screenPosition)
    {
        if (!TryScreenPositionToCell(screenPosition, out var cell))
        {
            if (!_isDraggingSelection)
            {
                _placementPreviewController?.ClearPreview();
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

        _placementPreviewController?.ShowHoverCell(cell);
        SetContextText($"空地：格子 {FormatCell(cell)}，可建造");
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
        SetContextText("未选中对象");
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
