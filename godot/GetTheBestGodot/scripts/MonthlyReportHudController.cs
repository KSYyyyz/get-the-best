using System.Globalization;
using System.Linq;
using Godot;
using StartupSim.Core;

namespace GetTheBestGodot;

public partial class MonthlyReportHudController : PanelContainer
{
    private static readonly Color PanelColor = new(0.04f, 0.06f, 0.07f, 0.94f);
    private static readonly Color BorderColor = new(0.74f, 0.86f, 0.78f, 0.42f);
    private static readonly Color TitleTextColor = new(0.98f, 0.96f, 0.82f, 1.0f);
    private static readonly Color PrimaryTextColor = new(0.92f, 0.96f, 0.94f, 0.98f);
    private static readonly Color MutedTextColor = new(0.70f, 0.80f, 0.75f, 0.94f);
    private static readonly Color ButtonTextColor = new(0.10f, 0.16f, 0.13f, 1.0f);

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
        CustomMinimumSize = new Vector2(620.0f, 224.0f);
        ConfigureLabel(_titleLabel, TitleTextColor, fontSize: 21);
        ConfigureLabel(_metricsLabel, PrimaryTextColor, fontSize: 17);
        ConfigureLabel(_deltaLabel, PrimaryTextColor, fontSize: 17);
        ConfigureLabel(_reasonLabel, MutedTextColor, fontSize: 17);
        ConfigureLabel(_nextStepLabel, MutedTextColor, fontSize: 17);
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

        HideMonthlyReport();
        GetViewport().SetInputAsHandled();
    }

    public void ShowMonthlyReport(CoreOfficeSimulationResult result)
    {
        SetLabel(_titleLabel, FormatReportTitle(result));
        SetLabel(_metricsLabel, FormatMetrics(result));
        SetLabel(_deltaLabel, FormatDelta(result));
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
            BgColor = PanelColor,
            BorderColor = BorderColor,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomRight = 6,
            CornerRadiusBottomLeft = 6,
            ContentMarginLeft = 22.0f,
            ContentMarginTop = 18.0f,
            ContentMarginRight = 22.0f,
            ContentMarginBottom = 18.0f,
        };
        panelStyle.SetBorderWidthAll(2);
        AddThemeStyleboxOverride("panel", panelStyle);
    }

    private static void ConfigureLabel(Label? label, Color color, int fontSize = 22)
    {
        if (label == null)
        {
            return;
        }

        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.CustomMinimumSize = new Vector2(0.0f, 26.0f);
        label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        label.ClipText = true;
    }

    private static void ConfigureButton(Button? button)
    {
        if (button == null)
        {
            return;
        }

        button.Text = "继续经营";
        button.CustomMinimumSize = new Vector2(132.0f, 34.0f);
        button.FocusMode = FocusModeEnum.None;
        button.AddThemeColorOverride("font_color", ButtonTextColor);
    }

    private static void SetLabel(Label? label, string text)
    {
        if (label == null)
        {
            return;
        }

        label.Text = text;
    }

    private static string FormatReportTitle(CoreOfficeSimulationResult result)
    {
        var reportEvent = FindMonthlyReportEvent(result);
        return reportEvent == null ? "月报待查看" : $"月报：{reportEvent.SubjectId}";
    }

    private static string FormatMetrics(CoreOfficeSimulationResult result)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "现金 ¥{0:0} | MVP {1:0.#}/{2:0.#} | 用户 {3} | MRR ¥{4:0}",
            result.CompanyTotals.CurrentCash,
            result.CompanyTotals.CurrentProjectProgress,
            result.CompanyTotals.ProjectRequiredProgress,
            result.CompanyTotals.CurrentActiveUsers,
            result.CompanyTotals.CurrentMonthlyRecurringRevenue
        );
    }

    private static string FormatDelta(CoreOfficeSimulationResult result)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "本月变化：现金 {0:+0;-0;0} | MVP {1:+0.#;-0.#;0} | 收入 {2:+0;-0;0}",
            result.CashDelta,
            result.ProjectProgressDelta,
            result.RevenueDelta,
            FormatOutcome(result.OutcomeKind)
        );
    }

    private static string FormatReason(CoreOfficeSimulationResult result)
    {
        var reportEvent = FindMonthlyReportEvent(result);
        if (reportEvent != null)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "原因：Core 已生成月报，现金 {0:+0;-0;0}，收入 {1:+0;-0;0}。",
                result.CashDelta,
                result.RevenueDelta
            );
        }

        var recentEvent = result.PresentationEvents.LastOrDefault();
        return recentEvent == null
            ? "原因：本月 Core tick 已完成，暂无额外事件。"
            : $"原因：{recentEvent.Message}";
    }

    private static string FormatNextStep(CoreOfficeSimulationResult result)
    {
        return $"下一步：{FormatOutcome(result.OutcomeKind)}，观察用户、MRR 与现金。";
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
