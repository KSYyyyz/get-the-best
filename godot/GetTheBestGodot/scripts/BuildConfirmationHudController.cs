using Godot;

namespace GetTheBestGodot;

public partial class BuildConfirmationHudController : PanelContainer
{
    private static readonly Color PanelColor = new(0.12f, 0.15f, 0.14f, 0.88f);
    private static readonly Color ReadyColor = new(0.54f, 1.0f, 0.68f, 1.0f);
    private static readonly Color WaitingColor = new(1.0f, 0.86f, 0.46f, 1.0f);
    private BuildModeController? _buildModeController;
    private PlacementPreview3DController? _placementPreviewController;
    private RoomOverlay3DRenderer? _roomOverlayRenderer;
    private Label? _confirmStatusLabel;
    private Button? _cancelBuildButton;
    private Button? _confirmBuildButton;

    public override void _Ready()
    {
        _buildModeController = GetNodeOrNull<BuildModeController>(
            "../../InteractionRoot/BuildModeController"
        );
        _placementPreviewController = GetNodeOrNull<PlacementPreview3DController>(
            "../../InteractionRoot/PlacementPreview3DController"
        );
        _roomOverlayRenderer = GetNodeOrNull<RoomOverlay3DRenderer>(
            "../../InteractionRoot/RoomOverlay3DRenderer"
        );
        _confirmStatusLabel = GetNodeOrNull<Label>("ConfirmRows/ConfirmStatusLabel");
        _cancelBuildButton = GetNodeOrNull<Button>("ConfirmRows/CancelBuildButton");
        _confirmBuildButton = GetNodeOrNull<Button>("ConfirmRows/ConfirmBuildButton");

        AddThemeStyleboxOverride("panel", CreatePanelStyle());
        ConfigureButton(_cancelBuildButton, "X", CancelPendingRoomSelection);
        ConfigureButton(_confirmBuildButton, "√", ConfirmPendingRoom);

        if (_buildModeController != null)
        {
            _buildModeController.ToolModeChanged += Refresh;
        }

        Refresh();
    }

    public override void _ExitTree()
    {
        if (_buildModeController != null)
        {
            _buildModeController.ToolModeChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        var hasPendingSelection = _buildModeController?.HasPendingRoomSelection() == true;
        Visible = hasPendingSelection;
        if (!hasPendingSelection)
        {
            return;
        }

        var canConfirm = _buildModeController?.CanConfirmPendingRoom() == true;
        if (_confirmStatusLabel != null)
        {
            _confirmStatusLabel.Text = canConfirm ? "区域已选门，可确认建造" : "区域待确认：请选择门";
            _confirmStatusLabel.AddThemeColorOverride("font_color", canConfirm ? ReadyColor : WaitingColor);
        }

        if (_confirmBuildButton != null)
        {
            _confirmBuildButton.Disabled = !canConfirm;
            _confirmBuildButton.AddThemeColorOverride("font_color", canConfirm ? ReadyColor : WaitingColor);
        }
    }

    private void CancelPendingRoomSelection()
    {
        _buildModeController?.CancelPendingRoomSelection();
        _placementPreviewController?.ClearPreview();
        Refresh();
    }

    private void ConfirmPendingRoom()
    {
        if (_buildModeController?.ConfirmPendingRoom(out var room) != true || room == null)
        {
            Refresh();
            return;
        }

        _placementPreviewController?.ClearPreview();
        _roomOverlayRenderer?.RefreshRooms();
        Refresh();
    }

    private static void ConfigureButton(Button? button, string text, System.Action action)
    {
        if (button == null)
        {
            return;
        }

        button.Text = text;
        button.Flat = true;
        button.CustomMinimumSize = new Vector2(34.0f, 30.0f);
        button.Pressed += action;
    }

    private static StyleBoxFlat CreatePanelStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = PanelColor,
            ContentMarginLeft = 10.0f,
            ContentMarginRight = 10.0f,
            ContentMarginTop = 6.0f,
            ContentMarginBottom = 6.0f,
        };
    }
}
