using Godot;

namespace GetTheBestGodot;

public partial class TimeScaleHudController : PanelContainer
{
    private static readonly Color PanelColor = new(0.05f, 0.07f, 0.08f, 0.58f);
    private static readonly Color TextColor = new(0.92f, 0.96f, 0.94f, 0.96f);
    private static readonly Color ActiveTextColor = new(0.52f, 0.95f, 0.64f, 0.96f);

    private EmployeeAutonomyController? _employeeAutonomyController;
    private Label? _speedStatusLabel;
    private Button? _pauseButton;
    private Button? _normalSpeedButton;
    private Button? _doubleSpeedButton;
    private Button? _tripleSpeedButton;
    private float _currentTimeScale = 1.0f;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        _employeeAutonomyController = GetNodeOrNull<EmployeeAutonomyController>(
            "../../InteractionRoot/EmployeeAutonomyController"
        );
        _speedStatusLabel = GetNodeOrNull<Label>("TimeScaleRows/SpeedStatusLabel");
        _pauseButton = GetNodeOrNull<Button>("TimeScaleRows/TimeScaleButtons/PauseButton");
        _normalSpeedButton = GetNodeOrNull<Button>("TimeScaleRows/TimeScaleButtons/NormalSpeedButton");
        _doubleSpeedButton = GetNodeOrNull<Button>("TimeScaleRows/TimeScaleButtons/DoubleSpeedButton");
        _tripleSpeedButton = GetNodeOrNull<Button>("TimeScaleRows/TimeScaleButtons/TripleSpeedButton");

        ConfigurePanel();
        ConfigureButton(_pauseButton);
        ConfigureButton(_normalSpeedButton);
        ConfigureButton(_doubleSpeedButton);
        ConfigureButton(_tripleSpeedButton);
        ConfigureLabel(_speedStatusLabel);

        if (_pauseButton != null)
        {
            _pauseButton.Pressed += () => SetSimulationTimeScale(0.0f);
        }
        if (_normalSpeedButton != null)
        {
            _normalSpeedButton.Pressed += () => SetSimulationTimeScale(1.0f);
        }
        if (_doubleSpeedButton != null)
        {
            _doubleSpeedButton.Pressed += () => SetSimulationTimeScale(2.0f);
        }
        if (_tripleSpeedButton != null)
        {
            _tripleSpeedButton.Pressed += () => SetSimulationTimeScale(3.0f);
        }

        SetSimulationTimeScale(1.0f);
    }

    private void SetSimulationTimeScale(float scale)
    {
        _currentTimeScale = Mathf.Clamp(scale, 0.0f, 3.0f);
        _employeeAutonomyController?.SetSimulationTimeScale(scale);
        UpdateStatusLabel();
        ApplyButtonState();
    }

    private void UpdateStatusLabel()
    {
        if (_speedStatusLabel == null)
        {
            return;
        }

        _speedStatusLabel.Text = _currentTimeScale <= 0.0f
            ? "\u65f6\u95f4 \u6682\u505c"
            : $"\u65f6\u95f4 {_currentTimeScale:0}x";
    }

    private void ApplyButtonState()
    {
        ApplyButtonState(_pauseButton, _currentTimeScale <= 0.0f);
        ApplyButtonState(_normalSpeedButton, Mathf.IsEqualApprox(_currentTimeScale, 1.0f));
        ApplyButtonState(_doubleSpeedButton, Mathf.IsEqualApprox(_currentTimeScale, 2.0f));
        ApplyButtonState(_tripleSpeedButton, Mathf.IsEqualApprox(_currentTimeScale, 3.0f));
    }

    private static void ApplyButtonState(Button? button, bool isActive)
    {
        if (button == null)
        {
            return;
        }

        button.AddThemeColorOverride("font_color", isActive ? ActiveTextColor : TextColor);
    }

    private void ConfigurePanel()
    {
        var panelStyle = new StyleBoxFlat
        {
            BgColor = PanelColor,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4,
            ContentMarginLeft = 10.0f,
            ContentMarginTop = 6.0f,
            ContentMarginRight = 10.0f,
            ContentMarginBottom = 6.0f,
        };
        panelStyle.SetBorderWidthAll(0);
        AddThemeStyleboxOverride("panel", panelStyle);
    }

    private static void ConfigureButton(Button? button)
    {
        if (button == null)
        {
            return;
        }

        button.Flat = true;
        button.FocusMode = FocusModeEnum.None;
        button.CustomMinimumSize = new Vector2(42.0f, 26.0f);
    }

    private static void ConfigureLabel(Label? label)
    {
        if (label == null)
        {
            return;
        }

        label.AddThemeColorOverride("font_color", TextColor);
        label.CustomMinimumSize = new Vector2(82.0f, 26.0f);
        label.VerticalAlignment = VerticalAlignment.Center;
    }
}
