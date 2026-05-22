using System.Globalization;
using System.Linq;
using Godot;
using StartupSim.Core;

namespace GetTheBestGodot;

public partial class BusinessFeedbackHudController : PanelContainer
{
    private static readonly Color PanelColor = new(0.05f, 0.07f, 0.08f, 0.64f);
    private static readonly Color BorderColor = new(0.65f, 0.78f, 0.70f, 0.28f);
    private static readonly Color PrimaryTextColor = new(0.92f, 0.96f, 0.94f, 0.96f);
    private static readonly Color MutedTextColor = new(0.68f, 0.78f, 0.73f, 0.88f);
    private static readonly Color PositiveDeltaColor = new(0.52f, 0.95f, 0.64f, 0.96f);
    private static readonly Color NegativeDeltaColor = new(1.0f, 0.48f, 0.44f, 0.96f);

    private Label? _cashValueLabel;
    private Label? _projectProgressValueLabel;
    private Label? _usersValueLabel;
    private Label? _revenueValueLabel;
    private Label? _outcomeValueLabel;
    private Label? _lastEventValueLabel;
    private Label? _objectiveValueLabel;
    private Label? _nextObjectiveValueLabel;
    private Label? _recentCoreEventValueLabel;
    private Label? _phaseRecapTitleLabel;
    private Label? _phaseRecapSummaryLabel;
    private Label? _phaseRecapReasonLabel;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _cashValueLabel = GetNodeOrNull<Label>("BusinessRows/BusinessMetricsRow/CashValueLabel");
        _projectProgressValueLabel = GetNodeOrNull<Label>(
            "BusinessRows/BusinessMetricsRow/ProjectProgressValueLabel"
        );
        _usersValueLabel = GetNodeOrNull<Label>("BusinessRows/BusinessMetricsRow/UsersValueLabel");
        _revenueValueLabel = GetNodeOrNull<Label>(
            "BusinessRows/BusinessMetricsRow/RevenueValueLabel"
        );
        _outcomeValueLabel = GetNodeOrNull<Label>(
            "BusinessRows/BusinessMetricsRow/OutcomeValueLabel"
        );
        _lastEventValueLabel = GetNodeOrNull<Label>("BusinessRows/LastEventValueLabel");
        _objectiveValueLabel = GetNodeOrNull<Label>("BusinessRows/ObjectiveValueLabel");
        _nextObjectiveValueLabel = GetNodeOrNull<Label>("BusinessRows/NextObjectiveValueLabel");
        _recentCoreEventValueLabel = GetNodeOrNull<Label>(
            "BusinessRows/RecentCoreEventValueLabel"
        );
        _phaseRecapTitleLabel = GetNodeOrNull<Label>("BusinessRows/PhaseRecapTitleLabel");
        _phaseRecapSummaryLabel = GetNodeOrNull<Label>("BusinessRows/PhaseRecapSummaryLabel");
        _phaseRecapReasonLabel = GetNodeOrNull<Label>("BusinessRows/PhaseRecapReasonLabel");

        ConfigurePanel();
        ConfigureLabel(_cashValueLabel, PrimaryTextColor);
        ConfigureLabel(_projectProgressValueLabel, PrimaryTextColor);
        ConfigureLabel(_usersValueLabel, PrimaryTextColor);
        ConfigureLabel(_revenueValueLabel, PrimaryTextColor);
        ConfigureLabel(_outcomeValueLabel, PrimaryTextColor);
        ConfigureLongLineLabel(_lastEventValueLabel, MutedTextColor);
        ConfigureLongLineLabel(_objectiveValueLabel, MutedTextColor);
        ConfigureLongLineLabel(_nextObjectiveValueLabel, MutedTextColor);
        ConfigureLongLineLabel(_recentCoreEventValueLabel, MutedTextColor);
        ConfigureLongLineLabel(_phaseRecapTitleLabel, PrimaryTextColor);
        ConfigureLongLineLabel(_phaseRecapSummaryLabel, MutedTextColor);
        ConfigureLongLineLabel(_phaseRecapReasonLabel, MutedTextColor);
        ResetDisplay();
    }

    public void ApplySimulationResult(CoreOfficeSimulationResult result)
    {
        SetLabel(
            _cashValueLabel,
            FormatMoneyMetric(
                "\u73b0\u91d1",
                result.CompanyTotals.CurrentCash,
                result.CashDelta
            ),
            result.CashDelta
        );
        SetLabel(
            _projectProgressValueLabel,
            string.Format(
                CultureInfo.InvariantCulture,
                "MVP {0:0.#}/{1:0.#} ({2:+0.#;-0.#;0})",
                result.CompanyTotals.CurrentProjectProgress,
                result.CompanyTotals.ProjectRequiredProgress,
                result.ProjectProgressDelta
            )
        );
        SetLabel(
            _usersValueLabel,
            string.Format(
                CultureInfo.InvariantCulture,
                "\u7528\u6237 {0}",
                result.CompanyTotals.CurrentActiveUsers
            )
        );
        SetLabel(
            _revenueValueLabel,
            FormatMoneyMetric(
                "MRR",
                result.CompanyTotals.CurrentMonthlyRecurringRevenue,
                result.RevenueDelta
            ),
            result.RevenueDelta
        );
        SetLabel(_outcomeValueLabel, FormatOutcome(result.OutcomeKind, result.CompanyTotals.ProductStage));
        SetLabel(_lastEventValueLabel, FormatLastEvent(result));
        SetLabel(
            _objectiveValueLabel,
            FormatFirstLoopObjective(result.CompanyTotals, result.OutcomeKind)
        );
        SetLabel(
            _nextObjectiveValueLabel,
            FormatNextObjective(result.CompanyTotals, result.OutcomeKind)
        );
        SetLabel(_recentCoreEventValueLabel, FormatRecentCoreEvent(result));
        SetLabel(_phaseRecapTitleLabel, FormatPhaseRecapTitle(result));
        SetLabel(_phaseRecapSummaryLabel, FormatPhaseRecapSummary(result));
        SetLabel(_phaseRecapReasonLabel, FormatPhaseRecapReason(result));
    }

    private void ResetDisplay()
    {
        SetLabel(_cashValueLabel, "\u73b0\u91d1 --");
        SetLabel(_projectProgressValueLabel, "MVP --");
        SetLabel(_usersValueLabel, "\u7528\u6237 --");
        SetLabel(_revenueValueLabel, "MRR --");
        SetLabel(_outcomeValueLabel, "\u9636\u6bb5 --");
        SetLabel(_lastEventValueLabel, "\u7b49\u5f85 Core tick");
        SetLabel(_objectiveValueLabel, "\u76ee\u6807 \u7b49\u5f85 Core tick");
        SetLabel(_nextObjectiveValueLabel, "\u4e0b\u4e00\u6b65 \u89c2\u5bdf\u5458\u5de5\u884c\u52a8");
        SetLabel(_recentCoreEventValueLabel, "\u6700\u8fd1\u4e8b\u4ef6 --");
        SetLabel(_phaseRecapTitleLabel, "\u9636\u6bb5\u590d\u76d8 \u7b49\u5f85 Core \u7ed3\u679c");
        SetLabel(_phaseRecapSummaryLabel, "\u590d\u76d8\u6458\u8981 --");
        SetLabel(_phaseRecapReasonLabel, "\u590d\u76d8\u539f\u56e0 --");
    }

    private void ConfigurePanel()
    {
        var panelStyle = new StyleBoxFlat
        {
            BgColor = PanelColor,
            BorderColor = BorderColor,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4,
            ContentMarginLeft = 12.0f,
            ContentMarginTop = 8.0f,
            ContentMarginRight = 12.0f,
            ContentMarginBottom = 8.0f,
        };
        panelStyle.SetBorderWidthAll(1);
        AddThemeStyleboxOverride("panel", panelStyle);
    }

    private static void ConfigureLabel(Label? label, Color color)
    {
        if (label == null)
        {
            return;
        }

        label.AddThemeColorOverride("font_color", color);
        label.AddThemeConstantOverride("outline_size", 0);
        label.CustomMinimumSize = new Vector2(108.0f, 22.0f);
        label.VerticalAlignment = VerticalAlignment.Center;
    }

    private static void ConfigureLongLineLabel(Label? label, Color color)
    {
        ConfigureLabel(label, color);
        if (label == null)
        {
            return;
        }

        label.ClipText = true;
        label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        label.CustomMinimumSize = new Vector2(640.0f, 22.0f);
        label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
    }

    private static void SetLabel(Label? label, string text, double delta = 0.0)
    {
        if (label == null)
        {
            return;
        }

        label.Text = text;
        if (delta > 0.0)
        {
            label.AddThemeColorOverride("font_color", PositiveDeltaColor);
        }
        else if (delta < 0.0)
        {
            label.AddThemeColorOverride("font_color", NegativeDeltaColor);
        }
    }

    private static string FormatMoneyMetric(string label, double value, double delta)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} \u00a5{1:0} ({2:+0;-0;0})",
            label,
            value,
            delta
        );
    }

    private static string FormatOutcome(PhaseOutcomeKind outcomeKind, ProductStage productStage)
    {
        return $"\u9636\u6bb5 {FormatProductStage(productStage)} / {FormatOutcomeKind(outcomeKind)}";
    }

    private static string FormatProductStage(ProductStage productStage)
    {
        return productStage switch
        {
            ProductStage.MvpReady => "MVP \u5c31\u7eea",
            ProductStage.Launched => "\u5df2\u53d1\u5e03",
            _ => "\u539f\u578b",
        };
    }

    private static string FormatOutcomeKind(PhaseOutcomeKind outcomeKind)
    {
        return outcomeKind switch
        {
            PhaseOutcomeKind.MvpCompleted => "MVP \u5b8c\u6210",
            PhaseOutcomeKind.FirstUsersAcquired => "\u83b7\u5f97\u9996\u6279\u7528\u6237",
            PhaseOutcomeKind.RevenuePositive => "\u6536\u5165\u8f6c\u6b63",
            PhaseOutcomeKind.FailedCashDepleted => "\u73b0\u91d1\u8017\u5c3d",
            _ => "\u8fdb\u884c\u4e2d",
        };
    }

    private static string FormatFirstLoopObjective(
        CoreCompanySimulationTotals totals,
        PhaseOutcomeKind outcomeKind
    )
    {
        if (outcomeKind == PhaseOutcomeKind.FirstUsersAcquired)
        {
            return "\u76ee\u6807 \u9996\u6279\u7528\u6237\u5df2\u83b7\u5f97";
        }

        if (outcomeKind == PhaseOutcomeKind.MvpCompleted || totals.ProductStage == ProductStage.MvpReady)
        {
            return "\u76ee\u6807 \u51c6\u5907\u9500\u552e\u4e0e\u9996\u6279\u7528\u6237";
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "\u76ee\u6807 \u63a8\u8fdb MVP {0:0.#}/{1:0.#}",
            totals.CurrentProjectProgress,
            totals.ProjectRequiredProgress
        );
    }

    private static string FormatNextObjective(
        CoreCompanySimulationTotals totals,
        PhaseOutcomeKind outcomeKind
    )
    {
        if (outcomeKind == PhaseOutcomeKind.FirstUsersAcquired)
        {
            return "\u4e0b\u4e00\u6b65 \u67e5\u770b\u6708\u62a5\u4e0e\u9636\u6bb5\u590d\u76d8";
        }

        if (outcomeKind == PhaseOutcomeKind.MvpCompleted || totals.ProductStage == ProductStage.MvpReady)
        {
            return "\u4e0b\u4e00\u6b65 \u8ba9\u5e02\u573a/\u8fd0\u8425\u5458\u5de5\u4f7f\u7528\u767d\u677f\u6216\u9500\u552e\u8bbe\u65bd";
        }

        return "\u4e0b\u4e00\u6b65 \u89c2\u5bdf\u5de5\u7a0b\u5e08\u524d\u5f80\u529e\u516c\u684c\u7814\u53d1";
    }

    private static string FormatRecentCoreEvent(CoreOfficeSimulationResult result)
    {
        var eventSummary = result.PresentationEvents.LastOrDefault();
        return eventSummary == null
            ? "\u6700\u8fd1\u4e8b\u4ef6 \u672c\u6b21 tick \u65e0\u65b0\u4e8b\u4ef6"
            : $"\u6700\u8fd1\u4e8b\u4ef6 {FormatEventKind(eventSummary.Kind)}: {eventSummary.Message}";
    }

    private static string FormatPhaseRecapTitle(CoreOfficeSimulationResult result)
    {
        if (result.OutcomeKind == PhaseOutcomeKind.FirstUsersAcquired)
        {
            return "\u9636\u6bb5\u590d\u76d8 \u9996\u6279\u7528\u6237\u9a8c\u8bc1\u901a\u8fc7";
        }

        if (result.OutcomeKind == PhaseOutcomeKind.MvpCompleted)
        {
            return "\u9636\u6bb5\u590d\u76d8 MVP \u5df2\u5b8c\u6210";
        }

        if (result.OutcomeKind == PhaseOutcomeKind.RevenuePositive)
        {
            return "\u9636\u6bb5\u590d\u76d8 \u6536\u5165\u8f6c\u6b63";
        }

        if (result.OutcomeKind == PhaseOutcomeKind.FailedCashDepleted)
        {
            return "\u9636\u6bb5\u590d\u76d8 \u73b0\u91d1\u8017\u5c3d";
        }

        return result.CompanyTotals.ProductStage == ProductStage.MvpReady
            ? "\u9636\u6bb5\u590d\u76d8 \u51c6\u5907\u83b7\u53d6\u7528\u6237"
            : "\u9636\u6bb5\u590d\u76d8 \u539f\u578b\u63a8\u8fdb\u4e2d";
    }

    private static string FormatPhaseRecapSummary(CoreOfficeSimulationResult result)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "\u590d\u76d8\u6458\u8981 MVP {0:0.#}/{1:0.#}\uff0c\u7528\u6237 {2}\uff0cMRR \u00a5{3:0}\uff0c\u73b0\u91d1 \u00a5{4:0}",
            result.CompanyTotals.CurrentProjectProgress,
            result.CompanyTotals.ProjectRequiredProgress,
            result.CompanyTotals.CurrentActiveUsers,
            result.CompanyTotals.CurrentMonthlyRecurringRevenue,
            result.CompanyTotals.CurrentCash
        );
    }

    private static string FormatPhaseRecapReason(CoreOfficeSimulationResult result)
    {
        var phaseEvent = result.PresentationEvents.LastOrDefault(
            eventSummary => eventSummary.Kind == SimulationEventKind.PhaseOutcomeReached
        );
        if (phaseEvent != null)
        {
            return $"\u590d\u76d8\u539f\u56e0 {phaseEvent.Message}";
        }

        var monthlyReport = result.PresentationEvents.LastOrDefault(
            eventSummary => eventSummary.Kind == SimulationEventKind.MonthlyReportReady
        );
        if (monthlyReport != null)
        {
            return $"\u590d\u76d8\u539f\u56e0 {monthlyReport.Message}";
        }

        var lastEvent = result.PresentationEvents.LastOrDefault();
        return lastEvent == null
            ? "\u590d\u76d8\u539f\u56e0 Core tick \u5df2\u66f4\u65b0\uff0c\u7ee7\u7eed\u89c2\u5bdf\u5458\u5de5\u884c\u52a8\u4e0e\u6307\u6807\u53d8\u5316"
            : $"\u590d\u76d8\u539f\u56e0 {FormatEventKind(lastEvent.Kind)}: {lastEvent.Message}";
    }

    private static string FormatLastEvent(CoreOfficeSimulationResult result)
    {
        var lastEvent = result.PresentationEvents.LastOrDefault();
        return lastEvent == null
            ? "\u672c\u6b21 tick \u65e0\u65b0\u4e8b\u4ef6"
            : lastEvent.Kind == SimulationEventKind.MetricChanged
                ? FormatBusinessTickSummary(result)
            : $"\u6700\u8fd1 {FormatEventKind(lastEvent.Kind)}: {lastEvent.Message}";
    }

    private static string FormatBusinessTickSummary(CoreOfficeSimulationResult result)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "\u6700\u8fd1 tick: MVP {0:+0.#;-0.#;0}, \u73b0\u91d1 {1:+0;-0;0}, \u6536\u5165 {2:+0;-0;0}",
            result.ProjectProgressDelta,
            result.CashDelta,
            result.RevenueDelta
        );
    }

    private static string FormatEventKind(SimulationEventKind kind)
    {
        return kind switch
        {
            SimulationEventKind.IntentPlanned => "\u884c\u4e3a",
            SimulationEventKind.ActivityChanged => "\u72b6\u6001",
            SimulationEventKind.FacilityUpdated => "\u8bbe\u65bd",
            SimulationEventKind.MetricChanged => "\u6307\u6807",
            SimulationEventKind.MonthlyReportReady => "\u6708\u62a5",
            SimulationEventKind.PhaseOutcomeReached => "\u9636\u6bb5",
            _ => kind.ToString(),
        };
    }
}
