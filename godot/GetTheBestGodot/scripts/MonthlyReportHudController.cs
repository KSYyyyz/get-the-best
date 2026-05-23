using System.Globalization;
using System.Linq;
using Godot;
using StartupSim.Core;

namespace GetTheBestGodot;

public partial class MonthlyReportHudController : PanelContainer
{
    private static readonly Color ReportWindowColor = new(0.94f, 0.94f, 0.92f, 0.98f);
    private static readonly Color TitleBarColor = new(0.09f, 0.47f, 0.74f, 1.0f);
    private static readonly Color BorderColor = new(0.18f, 0.20f, 0.20f, 1.0f);
    private static readonly Color TitleTextColor = new(1.0f, 1.0f, 0.96f, 1.0f);
    private static readonly Color PrimaryTextColor = new(0.14f, 0.15f, 0.15f, 1.0f);
    private static readonly Color MutedTextColor = new(0.36f, 0.38f, 0.38f, 1.0f);
    private static readonly Color ButtonTextColor = new(0.08f, 0.22f, 0.10f, 1.0f);
    private static readonly Vector2 MonthlyReportSize = new Vector2(460.0f, 430.0f);

    private Label? _titleLabel;
    private Label? _metricsLabel;
    private Label? _deltaLabel;
    private Label? _reasonLabel;
    private Label? _nextStepLabel;
    private Button? _continueButton;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        SetProcessInput(true);
        _titleLabel = GetNodeOrNull<Label>("MonthlyReportRows/MonthlyReportTitleLabel");
        _metricsLabel = GetNodeOrNull<Label>("MonthlyReportRows/MonthlyReportMetricsLabel");
        _deltaLabel = GetNodeOrNull<Label>("MonthlyReportRows/MonthlyReportDeltaLabel");
        _reasonLabel = GetNodeOrNull<Label>("MonthlyReportRows/MonthlyReportReasonLabel");
        _nextStepLabel = GetNodeOrNull<Label>("MonthlyReportRows/MonthlyReportNextStepLabel");
        _continueButton = GetNodeOrNull<Button>(
            "MonthlyReportRows/MonthlyReportButtonRow/ContinueMonthlyReportButton"
        );

        ConfigurePanel();
        ConfigureTitleBar();
        CustomMinimumSize = MonthlyReportSize;
        Size = MonthlyReportSize;
        ConfigureTitleLabel(_titleLabel);
        ConfigureBodyLabel(_metricsLabel, fontSize: 14, minHeight: 54);
        ConfigureBodyLabel(_deltaLabel, fontSize: 14, minHeight: 126);
        ConfigureBodyLabel(_reasonLabel, fontSize: 13, minHeight: 58, isMuted: true);
        ConfigureBodyLabel(_nextStepLabel, fontSize: 13, minHeight: 52, isMuted: true);
        ConfigureButton(_continueButton);

        if (_continueButton != null)
        {
            _continueButton.Pressed += HideMonthlyReport;
        }

        Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (
            !Visible
            || @event is not InputEventMouseButton mouseButton
            || !mouseButton.Pressed
            || mouseButton.ButtonIndex != MouseButton.Left
        )
        {
            return;
        }

        if (_continueButton?.GetGlobalRect().HasPoint(mouseButton.Position) == true)
        {
            return;
        }

        GetViewport().SetInputAsHandled();
    }

    public void ShowMonthlyReport(CoreOfficeSimulationResult result)
    {
        SetLabel(_titleLabel, FormatReportTitle(result));
        SetLabel(_metricsLabel, FormatOverview(result));
        SetLabel(_deltaLabel, FormatReportRows(result));
        SetLabel(_reasonLabel, FormatReason(result));
        SetLabel(_nextStepLabel, FormatNextStep(result));
        Visible = true;
    }

    public void HideMonthlyReport()
    {
        Visible = false;
    }

    private void ConfigurePanel()
    {
        var panelStyle = new StyleBoxFlat
        {
            BgColor = ReportWindowColor,
            BorderColor = BorderColor,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusBottomLeft = 8,
            ContentMarginLeft = 24.0f,
            ContentMarginTop = 18.0f,
            ContentMarginRight = 24.0f,
            ContentMarginBottom = 18.0f,
        };
        panelStyle.SetBorderWidthAll(2);
        AddThemeStyleboxOverride("panel", panelStyle);
    }

    private void ConfigureTitleBar()
    {
        if (_titleLabel == null || _titleLabel.GetParent() is not VBoxContainer rows)
        {
            return;
        }

        var titleIndex = _titleLabel.GetIndex();
        var titleBar = new PanelContainer
        {
            Name = "MonthlyReportTitleBar",
            CustomMinimumSize = new Vector2(0.0f, 46.0f),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        var titleBarStyle = new StyleBoxFlat
        {
            BgColor = TitleBarColor,
            BorderColor = BorderColor,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            ContentMarginLeft = 12.0f,
            ContentMarginTop = 2.0f,
            ContentMarginRight = 12.0f,
            ContentMarginBottom = 2.0f,
        };
        titleBarStyle.SetBorderWidthAll(1);
        titleBar.AddThemeStyleboxOverride("panel", titleBarStyle);

        rows.RemoveChild(_titleLabel);
        titleBar.AddChild(_titleLabel);
        rows.AddChild(titleBar);
        rows.MoveChild(titleBar, titleIndex);
    }

    private static void ConfigureTitleLabel(Label? label)
    {
        if (label == null)
        {
            return;
        }

        label.CustomMinimumSize = new Vector2(0.0f, 44.0f);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.AddThemeColorOverride("font_color", TitleTextColor);
        label.AddThemeColorOverride("font_shadow_color", new Color(0.0f, 0.0f, 0.0f, 0.35f));
        label.AddThemeConstantOverride("shadow_outline_size", 1);
        label.AddThemeFontSizeOverride("font_size", 22);
    }

    private static void ConfigureBodyLabel(
        Label? label,
        int fontSize,
        int minHeight,
        bool isMuted = false
    )
    {
        if (label == null)
        {
            return;
        }

        label.AddThemeColorOverride("font_color", isMuted ? MutedTextColor : PrimaryTextColor);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.CustomMinimumSize = new Vector2(0.0f, minHeight);
        label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        label.ClipText = false;
    }

    private static void ConfigureButton(Button? button)
    {
        if (button == null)
        {
            return;
        }

        button.Text = "继续经营";
        button.CustomMinimumSize = new Vector2(132.0f, 38.0f);
        button.FocusMode = FocusModeEnum.None;
        button.AddThemeColorOverride("font_color", ButtonTextColor);
    }

    private static void SetLabel(Label? label, string text)
    {
        if (label != null)
        {
            label.Text = text;
        }
    }

    private static string FormatReportTitle(CoreOfficeSimulationResult result)
    {
        var reportEvent = FindMonthlyReportEvent(result);
        return reportEvent == null ? "月报待查看" : $"经营月报：{reportEvent.SubjectId}";
    }

    private static string FormatOverview(CoreOfficeSimulationResult result)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "公司 76号\n现金 ¥{0:0}    用户 {1}    MRR ¥{2:0}",
            result.CompanyTotals.CurrentCash,
            result.CompanyTotals.CurrentActiveUsers,
            result.CompanyTotals.CurrentMonthlyRecurringRevenue
        );
    }

    private static string FormatReportRows(CoreOfficeSimulationResult result)
    {
        return string.Join(
            "\n",
            "本月变化",
            FormatReportLine("现金结余", result.CompanyTotals.CurrentCash, result.CashDelta, "¥"),
            FormatReportLine(
                "开发进度",
                result.CompanyTotals.CurrentProjectProgress,
                result.ProjectProgressDelta,
                string.Empty
            ),
            FormatReportLine("用户数量", result.CompanyTotals.CurrentActiveUsers, 0.0, string.Empty),
            FormatReportLine("本月收入", result.CompanyTotals.CurrentMonthlyRecurringRevenue, result.RevenueDelta, "¥")
        );
    }

    private static string FormatReportLine(string label, double value, double delta, string prefix)
    {
        var deltaText = string.Format(CultureInfo.InvariantCulture, "{0:+0.#;-0.#;0}", delta);
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0,-6} {1}{2:0.#}    变化 {3}",
            label,
            prefix,
            value,
            deltaText
        );
    }

    private static string FormatReason(CoreOfficeSimulationResult result)
    {
        var reportEvent = FindMonthlyReportEvent(result);
        if (reportEvent != null)
        {
            return $"经营说明\nCore 已生成月报，本月现金变化 {FormatDelta(result.CashDelta)}，收入变化 {FormatDelta(result.RevenueDelta)}。";
        }

        var recentEvent = result.PresentationEvents.LastOrDefault();
        return recentEvent == null ? "经营说明\n本月暂无额外事件。" : $"经营说明\n{recentEvent.Message}";
    }

    private static string FormatNextStep(CoreOfficeSimulationResult result)
    {
        return $"下一步\n{FormatOutcome(result.OutcomeKind)}，继续观察用户、现金和软件表现。";
    }

    private static string FormatDelta(double delta)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:+0.#;-0.#;0}", delta);
    }

    private static CoreSimulationPresentationEvent? FindMonthlyReportEvent(
        CoreOfficeSimulationResult result
    )
    {
        return result.PresentationEvents.LastOrDefault(
            eventSummary => eventSummary.Kind == SimulationEventKind.MonthlyReportReady
        );
    }

    private static string FormatOutcome(PhaseOutcomeKind outcomeKind)
    {
        return outcomeKind switch
        {
            PhaseOutcomeKind.MvpCompleted => "MVP 完成",
            PhaseOutcomeKind.FirstUsersAcquired => "获得首批用户",
            PhaseOutcomeKind.RevenuePositive => "收入转正",
            PhaseOutcomeKind.FailedCashDepleted => "现金耗尽",
            _ => "进行中",
        };
    }
}
