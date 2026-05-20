using Godot;

namespace GetTheBestGodot;

public partial class BuildModeHudController : PanelContainer
{
    private BuildModeController? _buildModeController;
    private Label? _buildModeLabel;
    private Button? _researchRoomButton;
    private Button? _marketRoomButton;
    private Button? _serverRoomButton;

    public override void _Ready()
    {
        _buildModeController = GetNodeOrNull<BuildModeController>("../../InteractionRoot/BuildModeController");
        _buildModeLabel = GetNodeOrNull<Label>("BuildModeRows/BuildModeLabel");
        _researchRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/ResearchRoomButton");
        _marketRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/MarketRoomButton");
        _serverRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/ServerRoomButton");

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

        RefreshBuildModeLabel();
    }

    private void SetRoomType(RoomBuildType roomType)
    {
        _buildModeController?.SetActiveRoomType(roomType);
        RefreshBuildModeLabel();
    }

    private void RefreshBuildModeLabel()
    {
        if (_buildModeLabel == null)
        {
            return;
        }

        var label = _buildModeController?.GetActiveRoomTypeLabel() ?? "研发室";
        _buildModeLabel.Text = $"当前建造：{label}";
    }
}
