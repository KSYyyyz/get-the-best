using Godot;

namespace GetTheBestGodot;

public partial class BuildModeHudController : PanelContainer
{
    private BuildModeController? _buildModeController;
    private Button? _buildMenuButton;
    private Button? _deleteRoomButton;
    private HBoxContainer? _roomTypeButtons;
    private Button? _researchRoomButton;
    private Button? _marketRoomButton;
    private Button? _serverRoomButton;
    private bool _isBuildMenuOpen;

    public override void _Ready()
    {
        _buildModeController = GetNodeOrNull<BuildModeController>(
            "../../InteractionRoot/BuildModeController"
        );
        _buildMenuButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/BuildMenuButton");
        _deleteRoomButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/DeleteRoomButton");
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
            _deleteRoomButton.Text = "删除";
            _deleteRoomButton.Pressed += StartDeleteRoomMode;
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
    }

    private void ToggleBuildMenu()
    {
        _isBuildMenuOpen = !_isBuildMenuOpen;
        RefreshRoomTypeVisibility();
    }

    private void SetRoomType(RoomBuildType roomType)
    {
        _buildModeController?.SetActiveRoomType(roomType);
        _isBuildMenuOpen = true;
        RefreshRoomTypeVisibility();
    }

    private void StartDeleteRoomMode()
    {
        _buildModeController?.StartDeleteRoomMode();
        _isBuildMenuOpen = false;
        RefreshRoomTypeVisibility();
    }

    private void RefreshRoomTypeVisibility()
    {
        if (_roomTypeButtons != null)
        {
            _roomTypeButtons.Visible = _isBuildMenuOpen;
        }
    }
}
