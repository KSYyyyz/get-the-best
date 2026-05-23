using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GetTheBestGodot;

public partial class BuildModeHudController : PanelContainer
{
    private enum CommandMenu
    {
        None,
        Build,
        Facility,
        EmployeeManagement,
        Administration,
        Publishing,
        Statistics,
    }

    private static readonly Color BarColor = new(0.70f, 0.71f, 0.69f, 0.96f);
    private static readonly Color MenuColor = new(0.96f, 0.96f, 0.94f, 0.98f);
    private static readonly Color BorderColor = new(0.34f, 0.36f, 0.35f, 1.0f);
    private static readonly Color NormalButtonColor = new(0.12f, 0.14f, 0.14f, 1.0f);
    private static readonly Color HoverButtonColor = new(0.04f, 0.28f, 0.50f, 1.0f);
    private static readonly Color ActiveButtonColor = new(0.02f, 0.45f, 0.18f, 1.0f);
    private static readonly Color DisabledTextColor = new(0.55f, 0.57f, 0.56f, 1.0f);
    private const float ToolbarClosedHeight = 50.0f;
    private const float ToolbarCompactWidth = 542.0f;
    private const float MenuPopupWidth = 186.0f;
    private const string BuildMenuText = "建造";
    private const string FacilityMenuText = "设施";
    private const string EmployeeManagementMenuText = "员工";
    private const string AdministrationMenuText = "管理部";
    private const string PublishingMenuText = "发行部";
    private const string StatisticsMenuText = "统计";
    private const string DeleteRoomText = "出售";

    private BuildModeController? _buildModeController;
    private Button? _buildMenuButton;
    private Button? _facilityMenuButton;
    private Button? _employeeManagementButton;
    private Button? _administrationButton;
    private Button? _publishingButton;
    private Button? _statisticsButton;
    private Button? _deleteRoomButton;
    private HBoxContainer? _entryButtons;
    private Label? _entrySeparator;
    private Label? _facilityEntrySeparator;
    private VBoxContainer? _roomTypeButtons;
    private VBoxContainer? _facilityTypeButtons;
    private VBoxContainer? _employeeManagementButtons;
    private VBoxContainer? _administrationButtons;
    private VBoxContainer? _publishingButtons;
    private VBoxContainer? _statisticsButtons;
    private Button? _researchRoomButton;
    private Button? _marketRoomButton;
    private Button? _serverRoomButton;
    private Button? _deskFacilityButton;
    private Button? _whiteboardFacilityButton;
    private Button? _serverRackFacilityButton;
    private Button? _hoveredButton;
    private PanelContainer? _menuPopup;
    private VBoxContainer? _menuPopupRows;
    private CommandMenu _openMenu = CommandMenu.None;

    public override void _Ready()
    {
        SetProcessInput(true);
        _buildModeController = GetNodeOrNull<BuildModeController>(
            "../../InteractionRoot/BuildModeController"
        );
        if (_buildModeController != null)
        {
            _buildModeController.ToolModeChanged += OnToolModeChanged;
        }

        _buildMenuButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/BuildMenuButton");
        _facilityMenuButton = GetNodeOrNull<Button>(
            "BuildModeRows/BuildEntryButtons/FacilityMenuButton"
        );
        _employeeManagementButton = GetNodeOrNull<Button>(
            "BuildModeRows/BuildEntryButtons/EmployeeManagementButton"
        );
        _administrationButton = GetNodeOrNull<Button>(
            "BuildModeRows/BuildEntryButtons/AdministrationButton"
        );
        _publishingButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/PublishingButton");
        _statisticsButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/StatisticsButton");
        _deleteRoomButton = GetNodeOrNull<Button>("BuildModeRows/BuildEntryButtons/DeleteRoomButton");
        _entryButtons = GetNodeOrNull<HBoxContainer>("BuildModeRows/BuildEntryButtons");
        _entrySeparator = GetNodeOrNull<Label>("BuildModeRows/BuildEntryButtons/EntrySeparator");
        _facilityEntrySeparator = GetNodeOrNull<Label>(
            "BuildModeRows/BuildEntryButtons/FacilityEntrySeparator"
        );
        _roomTypeButtons = GetNodeOrNull<VBoxContainer>("BuildModeRows/RoomTypeButtons");
        _facilityTypeButtons = GetNodeOrNull<VBoxContainer>("BuildModeRows/FacilityTypeButtons");
        _employeeManagementButtons = GetNodeOrNull<VBoxContainer>(
            "BuildModeRows/EmployeeManagementButtons"
        );
        _administrationButtons = GetNodeOrNull<VBoxContainer>("BuildModeRows/AdministrationButtons");
        _publishingButtons = GetNodeOrNull<VBoxContainer>("BuildModeRows/PublishingButtons");
        _statisticsButtons = GetNodeOrNull<VBoxContainer>("BuildModeRows/StatisticsButtons");
        _researchRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/ResearchRoomButton");
        _marketRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/MarketRoomButton");
        _serverRoomButton = GetNodeOrNull<Button>("BuildModeRows/RoomTypeButtons/ServerRoomButton");
        _deskFacilityButton = GetNodeOrNull<Button>(
            "BuildModeRows/FacilityTypeButtons/DeskFacilityButton"
        );
        _whiteboardFacilityButton = GetNodeOrNull<Button>(
            "BuildModeRows/FacilityTypeButtons/WhiteboardFacilityButton"
        );
        _serverRackFacilityButton = GetNodeOrNull<Button>(
            "BuildModeRows/FacilityTypeButtons/ServerRackFacilityButton"
        );

        ConfigurePanel();
        CreateMenuPopup();
        MoveMenuToPopup(_roomTypeButtons);
        MoveMenuToPopup(_facilityTypeButtons);
        MoveMenuToPopup(_employeeManagementButtons);
        MoveMenuToPopup(_administrationButtons);
        MoveMenuToPopup(_publishingButtons);
        MoveMenuToPopup(_statisticsButtons);
        ConfigureEntryButtons();
        ConfigureSeparator(_entrySeparator);
        ConfigureSeparator(_facilityEntrySeparator);
        ConfigureButton(_buildMenuButton, BuildMenuText, () => ToggleMenu(CommandMenu.Build));
        ConfigureButton(
            _facilityMenuButton,
            FacilityMenuText,
            () => ToggleMenu(CommandMenu.Facility)
        );
        ConfigureButton(
            _employeeManagementButton,
            EmployeeManagementMenuText,
            () => ToggleMenu(CommandMenu.EmployeeManagement)
        );
        ConfigureButton(
            _administrationButton,
            AdministrationMenuText,
            () => ToggleMenu(CommandMenu.Administration),
            minWidth: 64.0f
        );
        ConfigureButton(
            _publishingButton,
            PublishingMenuText,
            () => ToggleMenu(CommandMenu.Publishing),
            minWidth: 64.0f
        );
        ConfigureButton(_statisticsButton, StatisticsMenuText, () => ToggleMenu(CommandMenu.Statistics));
        ConfigureButton(_deleteRoomButton, DeleteRoomText, StartDeleteRoomMode, minWidth: 50.0f);
        ConfigureBuildMenuButtons();
        ConfigureFacilityMenuButtons();
        ConfigurePassiveMenu(_employeeManagementButtons);
        ConfigurePassiveMenu(_administrationButtons);
        ConfigurePassiveMenu(_publishingButtons);
        ConfigurePassiveMenu(_statisticsButtons);

        RefreshToolMenuVisibility();
        ApplyToolButtonState();
    }

    public override void _Input(InputEvent @event)
    {
        if (
            @event is not InputEventMouseButton mouseButton
            || !mouseButton.Pressed
            || mouseButton.ButtonIndex != MouseButton.Left
        )
        {
            return;
        }

        if (TryHandleToolbarClick(mouseButton.Position))
        {
            GetViewport().SetInputAsHandled();
        }
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
            _openMenu = CommandMenu.None;
            RefreshToolMenuVisibility();
        }

        ApplyToolButtonState();
    }

    private void ToggleMenu(CommandMenu menu)
    {
        if (_buildModeController?.IsDeleteRoomMode() == true)
        {
            _buildModeController.CancelActiveTool();
        }

        _openMenu = _openMenu == menu ? CommandMenu.None : menu;
        RefreshToolMenuVisibility();
        ApplyToolButtonState();
    }

    private bool TryHandleToolbarClick(Vector2 screenPosition)
    {
        if (HandleButtonClick(_buildMenuButton, screenPosition, () => ToggleMenu(CommandMenu.Build)))
        {
            return true;
        }

        if (
            HandleButtonClick(
                _facilityMenuButton,
                screenPosition,
                () => ToggleMenu(CommandMenu.Facility)
            )
        )
        {
            return true;
        }

        if (
            HandleButtonClick(
                _employeeManagementButton,
                screenPosition,
                () => ToggleMenu(CommandMenu.EmployeeManagement)
            )
        )
        {
            return true;
        }

        if (
            HandleButtonClick(
                _administrationButton,
                screenPosition,
                () => ToggleMenu(CommandMenu.Administration)
            )
        )
        {
            return true;
        }

        if (
            HandleButtonClick(
                _publishingButton,
                screenPosition,
                () => ToggleMenu(CommandMenu.Publishing)
            )
        )
        {
            return true;
        }

        if (
            HandleButtonClick(
                _statisticsButton,
                screenPosition,
                () => ToggleMenu(CommandMenu.Statistics)
            )
        )
        {
            return true;
        }

        return HandleButtonClick(_deleteRoomButton, screenPosition, StartDeleteRoomMode);
    }

    private static bool HandleButtonClick(Button? button, Vector2 screenPosition, System.Action action)
    {
        if (button?.Visible != true || !button.GetGlobalRect().HasPoint(screenPosition))
        {
            return false;
        }

        action();
        return true;
    }

    private void SetRoomType(RoomBuildType roomType)
    {
        _buildModeController?.SetActiveRoomType(roomType);
        _openMenu = CommandMenu.Build;
        RefreshToolMenuVisibility();
        ApplyToolButtonState();
    }

    private void SetFacilityType(FacilityBuildType facilityType)
    {
        _buildModeController?.SetActiveFacilityType(facilityType);
        _openMenu = CommandMenu.Facility;
        RefreshToolMenuVisibility();
        ApplyToolButtonState();
    }

    private void StartDeleteRoomMode()
    {
        _buildModeController?.ToggleDeleteRoomMode();
        _openMenu = CommandMenu.None;
        RefreshToolMenuVisibility();
        ApplyToolButtonState();
    }

    private void RefreshToolMenuVisibility()
    {
        SetMenuVisible(_roomTypeButtons, _openMenu == CommandMenu.Build);
        SetMenuVisible(_facilityTypeButtons, _openMenu == CommandMenu.Facility);
        SetMenuVisible(_employeeManagementButtons, _openMenu == CommandMenu.EmployeeManagement);
        SetMenuVisible(_administrationButtons, _openMenu == CommandMenu.Administration);
        SetMenuVisible(_publishingButtons, _openMenu == CommandMenu.Publishing);
        SetMenuVisible(_statisticsButtons, _openMenu == CommandMenu.Statistics);
        if (_menuPopup != null)
        {
            _menuPopup.Visible = _openMenu != CommandMenu.None;
            PositionPopupMenu();
        }
        RefreshToolbarLayout();
    }

    private void RefreshToolbarLayout()
    {
        var viewportSize = GetViewport().GetVisibleRect().Size;
        Position = new Vector2(368.0f, viewportSize.Y - 4.0f - ToolbarClosedHeight);
        CustomMinimumSize = new Vector2(ToolbarCompactWidth, ToolbarClosedHeight);
        Size = CustomMinimumSize;
        PositionPopupMenu();
    }

    private void CreateMenuPopup()
    {
        if (_menuPopup != null)
        {
            return;
        }

        _menuPopup = GetNodeOrNull<PanelContainer>("../BuildModeMenuPopup");
        _menuPopupRows = GetNodeOrNull<VBoxContainer>("../BuildModeMenuPopup/BuildModeMenuPopupRows");
        if (_menuPopupRows == null)
        {
            _menuPopupRows = new VBoxContainer
            {
                Name = "BuildModeMenuPopupRows",
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
        }
        _menuPopupRows.AddThemeConstantOverride("separation", 0);

        _menuPopup ??= new PanelContainer
        {
            Name = "BuildModeMenuPopup",
            Visible = false,
            CustomMinimumSize = new Vector2(MenuPopupWidth, 36.0f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        _menuPopup.Visible = false;
        _menuPopup.CustomMinimumSize = new Vector2(MenuPopupWidth, 36.0f);
        _menuPopup.MouseFilter = MouseFilterEnum.Stop;
        var popupStyle = new StyleBoxFlat
        {
            BgColor = MenuColor,
            BorderColor = BorderColor,
            ContentMarginLeft = 3.0f,
            ContentMarginTop = 3.0f,
            ContentMarginRight = 3.0f,
            ContentMarginBottom = 3.0f,
        };
        popupStyle.SetBorderWidthAll(1);
        _menuPopup.AddThemeStyleboxOverride("panel", popupStyle);
        if (_menuPopupRows.GetParent() == null)
        {
            _menuPopup.AddChild(_menuPopupRows);
        }
        if (_menuPopup.GetParent() == null)
        {
            GetParent()?.AddChild(_menuPopup);
        }
    }

    private void MoveMenuToPopup(VBoxContainer? menu)
    {
        if (menu == null || _menuPopupRows == null)
        {
            return;
        }

        menu.GetParent()?.RemoveChild(menu);
        menu.Visible = false;
        menu.CustomMinimumSize = new Vector2(MenuPopupWidth - 6.0f, 0.0f);
        _menuPopupRows.AddChild(menu);
    }

    private void PositionPopupMenu()
    {
        if (_menuPopup?.Visible != true)
        {
            return;
        }

        var activeButton = GetActiveMenuButton();
        if (activeButton == null)
        {
            _menuPopup.Visible = false;
            return;
        }

        var itemCount = GetActiveMenuItemCount();
        var popupHeight = Mathf.Max(36.0f, itemCount * 30.0f + 8.0f);
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var buttonRect = activeButton.GetGlobalRect();
        _menuPopup.Size = new Vector2(MenuPopupWidth, popupHeight);
        _menuPopup.Position = new Vector2(
            Mathf.Clamp(
                buttonRect.Position.X + buttonRect.Size.X / 2.0f - MenuPopupWidth / 2.0f,
                8.0f,
                Mathf.Max(8.0f, viewportSize.X - MenuPopupWidth - 8.0f)
            ),
            Mathf.Max(8.0f, Position.Y - popupHeight - 4.0f)
        );
    }

    private Button? GetActiveMenuButton()
    {
        return _openMenu switch
        {
            CommandMenu.Build => _buildMenuButton,
            CommandMenu.Facility => _facilityMenuButton,
            CommandMenu.EmployeeManagement => _employeeManagementButton,
            CommandMenu.Administration => _administrationButton,
            CommandMenu.Publishing => _publishingButton,
            CommandMenu.Statistics => _statisticsButton,
            _ => null,
        };
    }

    private int GetActiveMenuItemCount()
    {
        return _openMenu switch
        {
            CommandMenu.Build => _roomTypeButtons?.GetChildCount() ?? 0,
            CommandMenu.Facility => _facilityTypeButtons?.GetChildCount() ?? 0,
            CommandMenu.EmployeeManagement => _employeeManagementButtons?.GetChildCount() ?? 0,
            CommandMenu.Administration => _administrationButtons?.GetChildCount() ?? 0,
            CommandMenu.Publishing => _publishingButtons?.GetChildCount() ?? 0,
            CommandMenu.Statistics => _statisticsButtons?.GetChildCount() ?? 0,
            _ => 0,
        };
    }

    private void ConfigureBuildMenuButtons()
    {
        ConfigureMenuButton(
            _researchRoomButton,
            BuildModeController.GetRoomTypeLabel(RoomBuildType.ResearchRoom),
            () => SetRoomType(RoomBuildType.ResearchRoom)
        );
        ConfigureMenuButton(
            _marketRoomButton,
            BuildModeController.GetRoomTypeLabel(RoomBuildType.MarketRoom),
            () => SetRoomType(RoomBuildType.MarketRoom)
        );
        ConfigureMenuButton(
            _serverRoomButton,
            BuildModeController.GetRoomTypeLabel(RoomBuildType.ServerRoom),
            () => SetRoomType(RoomBuildType.ServerRoom)
        );
    }

    private void ConfigureFacilityMenuButtons()
    {
        ConfigureMenuButton(
            _deskFacilityButton,
            BuildModeController.GetFacilityTypeLabel(FacilityBuildType.OfficeDesk),
            () => SetFacilityType(FacilityBuildType.OfficeDesk)
        );
        ConfigureMenuButton(
            _whiteboardFacilityButton,
            BuildModeController.GetFacilityTypeLabel(FacilityBuildType.ProductWhiteboard),
            () => SetFacilityType(FacilityBuildType.ProductWhiteboard)
        );
        ConfigureMenuButton(
            _serverRackFacilityButton,
            BuildModeController.GetFacilityTypeLabel(FacilityBuildType.ServerRack),
            () => SetFacilityType(FacilityBuildType.ServerRack)
        );
    }

    private void ConfigurePassiveMenu(VBoxContainer? menu)
    {
        if (menu == null)
        {
            return;
        }

        menu.AddThemeConstantOverride("separation", 0);
        foreach (var button in menu.GetChildren().OfType<Button>())
        {
            ConfigureMenuButton(button, button.Text, () => { }, isPlaceholder: true);
        }
    }

    private void ConfigurePanel()
    {
        var panelStyle = new StyleBoxFlat
        {
            BgColor = BarColor,
            BorderColor = BorderColor,
            ContentMarginLeft = 8.0f,
            ContentMarginTop = 6.0f,
            ContentMarginRight = 8.0f,
            ContentMarginBottom = 6.0f,
        };
        panelStyle.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", panelStyle);
    }

    private void ConfigureEntryButtons()
    {
        _entryButtons?.AddThemeConstantOverride("separation", 3);
    }

    private void ConfigureSeparator(Label? separator)
    {
        if (separator == null)
        {
            return;
        }

        separator.Visible = false;
    }

    private void ConfigureButton(
        Button? button,
        string text,
        System.Action action,
        float minWidth = 58.0f
    )
    {
        if (button == null)
        {
            return;
        }

        button.Text = text;
        button.Flat = true;
        button.FocusMode = FocusModeEnum.None;
        button.CustomMinimumSize = new Vector2(minWidth, 36.0f);
        button.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        button.Pressed += action;
        button.MouseEntered += () => ApplyHoverState(button, isHovered: true);
        button.MouseExited += () => ApplyHoverState(button, isHovered: false);
    }

    private void ConfigureMenuButton(
        Button? button,
        string text,
        System.Action action,
        bool isPlaceholder = false
    )
    {
        if (button == null)
        {
            return;
        }

        button.Text = text;
        button.Flat = true;
        button.FocusMode = FocusModeEnum.None;
        button.CustomMinimumSize = new Vector2(180.0f, 30.0f);
        button.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        button.Pressed += action;
        button.AddThemeColorOverride("font_color", isPlaceholder ? DisabledTextColor : NormalButtonColor);
        button.AddThemeColorOverride("font_hover_color", HoverButtonColor);
        button.AddThemeColorOverride("font_pressed_color", ActiveButtonColor);
    }

    private static void SetMenuVisible(Control? control, bool visible)
    {
        if (control != null)
        {
            control.Visible = visible;
        }
    }

    private void ApplyHoverState(Button button, bool isHovered)
    {
        _hoveredButton = isHovered ? button : _hoveredButton == button ? null : _hoveredButton;
        ApplyToolButtonState();
    }

    private void ApplyToolButtonState()
    {
        var buttons = new List<(Button? Button, string Label, bool Active)>
        {
            (_buildMenuButton, BuildMenuText, _openMenu == CommandMenu.Build),
            (_facilityMenuButton, FacilityMenuText, _openMenu == CommandMenu.Facility),
            (
                _employeeManagementButton,
                EmployeeManagementMenuText,
                _openMenu == CommandMenu.EmployeeManagement
            ),
            (_administrationButton, AdministrationMenuText, _openMenu == CommandMenu.Administration),
            (_publishingButton, PublishingMenuText, _openMenu == CommandMenu.Publishing),
            (_statisticsButton, StatisticsMenuText, _openMenu == CommandMenu.Statistics),
            (_deleteRoomButton, DeleteRoomText, _buildModeController?.IsDeleteRoomMode() == true),
        };

        foreach (var buttonState in buttons)
        {
            SetButtonState(buttonState.Button, buttonState.Label, buttonState.Active);
        }

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
        SetButtonState(
            _deskFacilityButton,
            BuildModeController.GetFacilityTypeLabel(FacilityBuildType.OfficeDesk),
            IsActiveFacilityButton(FacilityBuildType.OfficeDesk)
        );
        SetButtonState(
            _whiteboardFacilityButton,
            BuildModeController.GetFacilityTypeLabel(FacilityBuildType.ProductWhiteboard),
            IsActiveFacilityButton(FacilityBuildType.ProductWhiteboard)
        );
        SetButtonState(
            _serverRackFacilityButton,
            BuildModeController.GetFacilityTypeLabel(FacilityBuildType.ServerRack),
            IsActiveFacilityButton(FacilityBuildType.ServerRack)
        );
    }

    private bool IsActiveRoomButton(RoomBuildType roomType)
    {
        return _buildModeController?.IsBuildRoomMode() == true
            && _buildModeController.GetActiveRoomType() == roomType;
    }

    private bool IsActiveFacilityButton(FacilityBuildType facilityType)
    {
        return _buildModeController?.IsPlaceFacilityMode() == true
            && _buildModeController.GetActiveFacilityType() == facilityType;
    }

    private void SetButtonState(Button? button, string label, bool isActive)
    {
        if (button == null)
        {
            return;
        }

        var isHovered = _hoveredButton == button;
        button.Text = label;
        SetButtonColor(button, isActive ? ActiveButtonColor : isHovered ? HoverButtonColor : NormalButtonColor);
    }

    private static void SetButtonColor(Button? button, Color color)
    {
        button?.AddThemeColorOverride("font_color", color);
        button?.AddThemeColorOverride("font_hover_color", HoverButtonColor);
        button?.AddThemeColorOverride("font_pressed_color", ActiveButtonColor);
    }
}
