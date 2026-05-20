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
        ConfigureButton(_confirmBuildButton, "\u221a", ConfirmPendingRoom);

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
        var statusMessage = _buildModeController?.GetBuildStatusMessage() ?? string.Empty;
        var hasStatusMessage = !string.IsNullOrEmpty(statusMessage);
        Visible = hasPendingSelection || hasStatusMessage;
        if (!Visible)
        {
            return;
        }

        if (hasStatusMessage && !hasPendingSelection)
        {
            ShowBuildStatus(statusMessage);
            return;
        }

        ClearBuildStatus();
        var canConfirm = _buildModeController?.CanConfirmPendingRoom() == true;
        if (_confirmStatusLabel != null)
        {
            _confirmStatusLabel.Text = canConfirm
                ? "\u533a\u57df\u5df2\u9009\u95e8\uff0c\u53ef\u786e\u8ba4\u5efa\u9020"
                : "\u533a\u57df\u5f85\u786e\u8ba4\uff1a\u8bf7\u9009\u62e9\u95e8";
            _confirmStatusLabel.AddThemeColorOverride("font_color", canConfirm ? ReadyColor : WaitingColor);
        }

        if (_confirmBuildButton != null)
        {
            _confirmBuildButton.Disabled = !canConfirm;
            _confirmBuildButton.Visible = true;
            _confirmBuildButton.AddThemeColorOverride("font_color", canConfirm ? ReadyColor : WaitingColor);
        }

        if (_cancelBuildButton != null)
        {
            _cancelBuildButton.Visible = true;
        }
    }

    private void ShowBuildStatus(string message)
    {
        if (_confirmStatusLabel != null)
        {
            _confirmStatusLabel.Text = message;
            _confirmStatusLabel.AddThemeColorOverride("font_color", WaitingColor);
        }

        if (_confirmBuildButton != null)
        {
            _confirmBuildButton.Visible = false;
        }

        if (_cancelBuildButton != null)
        {
            _cancelBuildButton.Visible = false;
        }
    }

    private void ClearBuildStatus()
    {
        if (_confirmBuildButton != null)
        {
            _confirmBuildButton.Visible = true;
        }

        if (_cancelBuildButton != null)
        {
            _cancelBuildButton.Visible = true;
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
        button.CustomMinimumSize = new Vector2(28.0f, 28.0f);
        button.Pressed += action;
    }

    private static StyleBoxFlat CreatePanelStyle()
    {
        return new StyleBoxFlat
        {
            BgColor = PanelColor,
            ContentMarginLeft = 8.0f,
            ContentMarginRight = 8.0f,
            ContentMarginTop = 5.0f,
            ContentMarginBottom = 5.0f,
        };
    }
}
