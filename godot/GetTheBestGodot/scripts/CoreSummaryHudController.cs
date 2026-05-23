using Godot;
using StartupSim.Core;

namespace GetTheBestGodot;

public partial class CoreSummaryHudController : PanelContainer
{
    private static readonly Color PanelColor = new(0.05f, 0.07f, 0.08f, 0.72f);
    private static readonly Color PrimaryTextColor = new(0.92f, 0.96f, 0.94f, 0.96f);
    private static readonly Color AccentTextColor = new(0.62f, 0.88f, 1.0f, 0.96f);

    private Label? _stageSummaryLabel;
    private Label? _scoreSummaryLabel;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _stageSummaryLabel = GetNodeOrNull<Label>("CoreSummaryRows/StageSummaryLabel");
        _scoreSummaryLabel = GetNodeOrNull<Label>("CoreSummaryRows/ScoreSummaryLabel");
        ConfigurePanel();
        ConfigureLabel(_stageSummaryLabel, AccentTextColor);
        ConfigureLabel(_scoreSummaryLabel, PrimaryTextColor);
        SetLabel(_stageSummaryLabel, "阶段 原型 / 进行中");
        SetLabel(_scoreSummaryLabel, "用户评分 观察中");
    }

    public void ApplySimulationResult(CoreOfficeSimulationResult result)
    {
        SetLabel(_stageSummaryLabel, FormatStage(result.CompanyTotals.ProductStage, result.OutcomeKind));
        SetLabel(_scoreSummaryLabel, FormatUserScore(result));
    }

    private static string FormatStage(ProductStage stage, PhaseOutcomeKind outcomeKind)
    {
        var stageText = stage switch
        {
            ProductStage.MvpReady => "MVP",
            ProductStage.Launched => "发行",
            _ => "原型",
        };
        var outcomeText = outcomeKind switch
        {
            PhaseOutcomeKind.MvpCompleted => "MVP完成",
            PhaseOutcomeKind.FirstUsersAcquired => "获得用户",
            PhaseOutcomeKind.RevenuePositive => "收入转正",
            PhaseOutcomeKind.FailedCashDepleted => "现金告急",
            _ => "进行中",
        };
        return $"阶段 {stageText} / {outcomeText}";
    }

    private static string FormatUserScore(CoreOfficeSimulationResult result)
    {
        if (result.CompanyTotals.CurrentActiveUsers <= 0)
        {
            return "用户评分 观察中";
        }

        var progressRatio =
            result.CompanyTotals.ProjectRequiredProgress <= 0.0
                ? 0.0
                : result.CompanyTotals.CurrentProjectProgress
                    / result.CompanyTotals.ProjectRequiredProgress;
        var score = Mathf.Clamp(55.0 + progressRatio * 35.0 + result.RevenueDelta * 0.02, 0.0, 100.0);
        return $"用户评分 {score:0.0}";
    }

    private void ConfigurePanel()
    {
        CustomMinimumSize = new Vector2(224.0f, 58.0f);
        var panelStyle = new StyleBoxFlat
        {
            BgColor = PanelColor,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusBottomLeft = 4,
            ContentMarginLeft = 10.0f,
            ContentMarginTop = 7.0f,
            ContentMarginRight = 10.0f,
            ContentMarginBottom = 7.0f,
        };
        panelStyle.SetBorderWidthAll(0);
        AddThemeStyleboxOverride("panel", panelStyle);
    }

    private static void ConfigureLabel(Label? label, Color color)
    {
        if (label == null)
        {
            return;
        }

        label.AddThemeColorOverride("font_color", color);
        label.AddThemeFontSizeOverride("font_size", 14);
        label.VerticalAlignment = VerticalAlignment.Center;
    }

    private static void SetLabel(Label? label, string text)
    {
        if (label != null)
        {
            label.Text = text;
        }
    }
}
