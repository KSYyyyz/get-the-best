using Godot;

namespace GetTheBestGodot;

public partial class BuildModeHudController : PanelContainer
{
    private BuildModeController? _buildModeController;
    private Label? _buildModeLabel;
    private Button? _buildMenuButton;
    private Button? _deleteRoomButton;
    private Button? _cancelActionButton;
    private HBoxContainer? _roomTypeButtons;
    private Button? _researchRoomButton;
    private Button? _marketRoomButton;
    private Button? _serverRoomButton;
    private bool _isBuildMenuOpen;

    public override void _Ready()
    {
        _buildModeController = GetNodeOrNull<BuildModeController>("../../InteractionRoot/BuildModeController");
        _buildModeLabel = GetNodeOrNull<Label>("BuildModeRows/BuildModeLabel");
        _buildMenuButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/BuildMenuButton");
        _deleteRoomButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/DeleteRoomButton");
        _cancelActionButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/CancelActionButton");
        _roomTypeButtons = GetNodeOrNull<HBoxContainer>("BuildModeRows/RoomTypeButtons");
        _researchRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/ResearchRoomButton");
        _marketRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/MarketRoomButton");
        _serverRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/ServerRoomButton");

        if (_buildMenuButton != null)
        {
            _buildMenuButton.Text = "建造";
            _buildMenuButton.Pressed += ToggleBuildMenu;
        }

        if (_deleteRoomButton != null)
        {
            _deleteRoomButton.Text = "删除房间";
            _deleteRoomButton.Pressed += StartDeleteRoomMode;
        }

        if (_cancelActionButton != null)
        {
            _cancelActionButton.Text = "取消";
            _cancelActionButton.Pressed += CancelAction;
        }

        if (_researchRoomButton != null)
        {
            _researchRoomButton.Text = BuildModeController.GetRoomTypeLabel(RoomBuildType.ResearchRoom);
            _researchRoomButton.Pressed += () => SetRoomType(RoomBuildType.ResearchRoom);
        }

        if (_marketRoomButton != null)
        {
            _marketRoomButton.Text = BuildModeController.GetRoomTypeLabel(RoomBuildType.MarketRoom);
            _marketRoomButton.Pressed += () => SetRoomType(RoomBuildType.MarketRoom);
        }

        if (_serverRoomButton != null)
        {
            _serverRoomButton.Text = BuildModeController.GetRoomTypeLabel(RoomBuildType.ServerRoom);
            _serverRoomButton.Pressed += () => SetRoomType(RoomBuildType.ServerRoom);
        }

        RefreshRoomTypeVisibility();
        RefreshBuildModeLabel();
    }

    private void ToggleBuildMenu()
    {
        _isBuildMenuOpen = !_isBuildMenuOpen;
        RefreshRoomTypeVisibility();
        RefreshBuildModeLabel();
    }

    private void SetRoomType(RoomBuildType roomType)
    {
        _buildModeController?.SetActiveRoomType(roomType);
        _isBuildMenuOpen = true;
        RefreshRoomTypeVisibility();
        RefreshBuildModeLabel();
    }

    private void StartDeleteRoomMode()
    {
        _buildModeController?.StartDeleteRoomMode();
        _isBuildMenuOpen = false;
        RefreshRoomTypeVisibility();
        RefreshBuildModeLabel();
    }

    private void CancelAction()
    {
        _buildModeController?.CancelActiveTool();
        _isBuildMenuOpen = false;
        RefreshRoomTypeVisibility();
        RefreshBuildModeLabel();
    }

    private void RefreshRoomTypeVisibility()
    {
        if (_roomTypeButtons != null)
        {
            _roomTypeButtons.Visible = _isBuildMenuOpen;
        }
    }

    private void RefreshBuildModeLabel()
    {
        if (_buildModeLabel == null)
        {
            return;
        }

        var label = _buildModeController?.GetActiveRoomTypeLabel() ?? "研发室";
        if (_buildModeController?.IsDeleteRoomMode() == true)
        {
            _buildModeLabel.Text = "删除房间：点击已有房间";
            return;
        }

        _buildModeLabel.Text = _isBuildMenuOpen ? $"建造：{label}" : "选择功能入口";
    }
}
