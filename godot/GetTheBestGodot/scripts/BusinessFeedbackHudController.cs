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

        ConfigurePanel();
        ConfigureLabel(_cashValueLabel, PrimaryTextColor);
        ConfigureLabel(_projectProgressValueLabel, PrimaryTextColor);
        ConfigureLabel(_usersValueLabel, PrimaryTextColor);
        ConfigureLabel(_revenueValueLabel, PrimaryTextColor);
        ConfigureLabel(_outcomeValueLabel, PrimaryTextColor);
        ConfigureLabel(_lastEventValueLabel, MutedTextColor);
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
    }

    private void ResetDisplay()
    {
        SetLabel(_cashValueLabel, "\u73b0\u91d1 --");
        SetLabel(_projectProgressValueLabel, "MVP --");
        SetLabel(_usersValueLabel, "\u7528\u6237 --");
        SetLabel(_revenueValueLabel, "MRR --");
        SetLabel(_outcomeValueLabel, "\u9636\u6bb5 --");
        SetLabel(_lastEventValueLabel, "\u7b49\u5f85 Core tick");
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
