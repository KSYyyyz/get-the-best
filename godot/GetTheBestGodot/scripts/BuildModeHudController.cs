using Godot;

namespace GetTheBestGodot;

public partial class BuildModeHudController : PanelContainer
{
    private static readonly Color NormalButtonColor = new(0.90f, 0.94f, 0.92f, 0.82f);
    private static readonly Color HoverButtonColor = new(1.0f, 0.95f, 0.58f, 1.0f);
    private static readonly Color ActiveButtonColor = new(0.54f, 1.0f, 0.68f, 1.0f);
    private static readonly Color SeparatorColor = new(0.72f, 0.76f, 0.74f, 0.82f);
    private const string BuildMenuText = "建造";
    private const string DeleteRoomText = "删除";

    private BuildModeController? _buildModeController;
    private Button? _buildMenuButton;
    private Button? _deleteRoomButton;
    private Label? _entrySeparator;
    private VBoxContainer? _roomTypeButtons;
    private Button? _researchRoomButton;
    private Button? _marketRoomButton;
    private Button? _serverRoomButton;
    private Button? _hoveredButton;
    private bool _isBuildMenuOpen;

    public override void _Ready()
    {
        _buildModeController = GetNodeOrNull<BuildModeController>(
            "../../InteractionRoot/BuildModeController"
        );
        if (_buildModeController != null)
        {
            _buildModeController.ToolModeChanged += OnToolModeChanged;
        }

        _buildMenuButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/BuildMenuButton");
        _deleteRoomButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/DeleteRoomButton");
        _entrySeparator = GetNodeOrNull<Label>("BuildModeRows/BuildEntryButtons/EntrySeparator");
        _roomTypeButtons = GetNodeOrNull<VBoxContainer>("BuildModeRows/RoomTypeButtons");
        _researchRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/ResearchRoomButton");
        _marketRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/MarketRoomButton");
        _serverRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/ServerRoomButton");

        ConfigureButton(_deleteRoomButton, DeleteRoomText, StartDeleteRoomMode, minWidth: 62.0f);
        ConfigureSeparator();
        ConfigureButton(_buildMenuButton, BuildMenuText, ToggleBuildMenu, minWidth: 62.0f);
        ConfigureButton(
            _researchRoomButton,
            BuildModeController.GetRoomTypeLabel(RoomBuildType.ResearchRoom),
            () => SetRoomType(RoomBuildType.ResearchRoom)
        );
        ConfigureButton(
            _marketRoomButton,
            BuildModeController.GetRoomTypeLabel(RoomBuildType.MarketRoom),
            () => SetRoomType(RoomBuildType.MarketRoom)
        );
        ConfigureButton(
            _serverRoomButton,
            BuildModeController.GetRoomTypeLabel(RoomBuildType.ServerRoom),
            () => SetRoomType(RoomBuildType.ServerRoom)
        );

        RefreshRoomTypeVisibility();
        ApplyToolButtonState();
    }

    public override void _ExitTree()
    {
        if (_buildModeController != null)
        {
            _buildModeController.ToolModeChanged -= OnToolModeChanged;
        }
    }

    private void OnToolModeChanged()
    {
        if (_buildModeController?.IsPointerMode() == true)
        {
            _isBuildMenuOpen = false;
            RefreshRoomTypeVisibility();
        }

        ApplyToolButtonState();
    }

    private void ToggleBuildMenu()
    {
        var nextOpenState = !_isBuildMenuOpen;
        if (_buildModeController?.IsDeleteRoomMode() == true)
        {
            _buildModeController.CancelActiveTool();
        }

        _isBuildMenuOpen = nextOpenState;
        RefreshRoomTypeVisibility();
        ApplyToolButtonState();
    }

    private void SetRoomType(RoomBuildType roomType)
    {
        _buildModeController?.SetActiveRoomType(roomType);
        _isBuildMenuOpen = true;
        RefreshRoomTypeVisibility();
        ApplyToolButtonState();
    }

    private void StartDeleteRoomMode()
    {
        _buildModeController?.ToggleDeleteRoomMode();
        _isBuildMenuOpen = false;
        RefreshRoomTypeVisibility();
        ApplyToolButtonState();
    }

    private void RefreshRoomTypeVisibility()
    {
        if (_roomTypeButtons != null)
        {
            _roomTypeButtons.Visible = _isBuildMenuOpen;
        }
    }

    private void ConfigureButton(
        Button? button,
        string text,
        System.Action action,
        float minWidth = 120.0f
    )
    {
        if (button == null)
        {
            return;
        }

        button.Text = text;
        button.CustomMinimumSize = new Vector2(minWidth, 30.0f);
        button.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        button.Pressed += action;
        button.MouseEntered += () => ApplyHoverState(button, isHovered: true);
        button.MouseExited += () => ApplyHoverState(button, isHovered: false);
    }

    private void ConfigureSeparator()
    {
        if (_entrySeparator == null)
        {
            return;
        }

        _entrySeparator.Text = "|";
        _entrySeparator.CustomMinimumSize = new Vector2(14.0f, 30.0f);
        _entrySeparator.HorizontalAlignment = HorizontalAlignment.Center;
        _entrySeparator.VerticalAlignment = VerticalAlignment.Center;
        _entrySeparator.AddThemeColorOverride("font_color", SeparatorColor);
    }

    private void ApplyHoverState(Button button, bool isHovered)
    {
        _hoveredButton = isHovered ? button : _hoveredButton == button ? null : _hoveredButton;
        ApplyToolButtonState();
    }

    private void ApplyToolButtonState()
    {
        SetButtonState(_buildMenuButton, BuildMenuText, _isBuildMenuOpen);
        SetButtonState(
            _deleteRoomButton,
            DeleteRoomText,
            _buildModeController?.IsDeleteRoomMode() == true
        );
        SetButtonState(
            _researchRoomButton,
            BuildModeController.GetRoomTypeLabel(RoomBuildType.ResearchRoom),
            IsActiveRoomButton(RoomBuildType.ResearchRoom)
        );
        SetButtonState(
            _marketRoomButton,
            BuildModeController.GetRoomTypeLabel(RoomBuildType.MarketRoom),
            IsActiveRoomButton(RoomBuildType.MarketRoom)
        );
        SetButtonState(
            _serverRoomButton,
            BuildModeController.GetRoomTypeLabel(RoomBuildType.ServerRoom),
            IsActiveRoomButton(RoomBuildType.ServerRoom)
        );
    }

    private bool IsActiveRoomButton(RoomBuildType roomType)
    {
        return _buildModeController?.IsBuildRoomMode() == true
            && _buildModeController.GetActiveRoomType() == roomType;
    }

    private void SetButtonState(Button? button, string label, bool isActive)
    {
        if (button == null)
        {
            return;
        }

        var isHovered = _hoveredButton == button;
        button.Text = $"{GetButtonPrefix(isActive, isHovered)}{label}";
        SetButtonColor(button, isActive ? ActiveButtonColor : isHovered ? HoverButtonColor : NormalButtonColor);
    }

    private static string GetButtonPrefix(bool isActive, bool isHovered)
    {
        if (isActive)
        {
            return "> ";
        }

        return isHovered ? "- " : "  ";
    }

    private static void SetButtonColor(Button? button, Color color)
    {
        button?.AddThemeColorOverride("font_color", color);
        button?.AddThemeColorOverride("font_hover_color", HoverButtonColor);
        button?.AddThemeColorOverride("font_pressed_color", ActiveButtonColor);
    }
}
