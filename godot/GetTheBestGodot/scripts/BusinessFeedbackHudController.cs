using System.Globalization;
using Godot;
using StartupSim.Core;

namespace GetTheBestGodot;

public partial class BusinessFeedbackHudController : PanelContainer
{
    private static readonly Color BarColor = new(0.70f, 0.71f, 0.69f, 0.96f);
    private static readonly Color BorderColor = new(0.34f, 0.36f, 0.35f, 1.0f);
    private static readonly Color LogoColor = new(0.78f, 0.05f, 0.05f, 1.0f);
    private static readonly Color LogoTextColor = new(1.0f, 1.0f, 0.95f, 1.0f);
    private static readonly Color PrimaryTextColor = new(0.10f, 0.12f, 0.12f, 1.0f);
    private static readonly Color PositiveDeltaColor = new(0.12f, 0.55f, 0.18f, 1.0f);
    private static readonly Color NegativeDeltaColor = new(0.82f, 0.16f, 0.14f, 1.0f);

    private Label? _companyLogoLabel;
    private Label? _companyNameLabel;
    private Label? _cashValueLabel;
    private Label? _usersValueLabel;
    private Label? _hiddenProjectLabel;
    private Label? _hiddenRevenueLabel;
    private Label? _hiddenOutcomeLabel;
    private Label? _hiddenLastEventLabel;
    private Label? _hiddenObjectiveLabel;
    private Label? _hiddenNextObjectiveLabel;
    private Label? _hiddenRecentCoreEventLabel;
    private Label? _hiddenPhaseRecapTitleLabel;
    private Label? _hiddenPhaseRecapSummaryLabel;
    private Label? _hiddenPhaseRecapReasonLabel;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _companyLogoLabel = GetNodeOrNull<Label>("BusinessRows/BusinessMetricsRow/CompanyLogoLabel");
        _companyNameLabel = GetNodeOrNull<Label>("BusinessRows/BusinessMetricsRow/CompanyNameLabel");
        _cashValueLabel = GetNodeOrNull<Label>("BusinessRows/BusinessMetricsRow/CashValueLabel");
        _usersValueLabel = GetNodeOrNull<Label>("BusinessRows/BusinessMetricsRow/UsersValueLabel");
        _hiddenProjectLabel = GetNodeOrNull<Label>(
            "BusinessRows/BusinessMetricsRow/ProjectProgressValueLabel"
        );
        _hiddenRevenueLabel = GetNodeOrNull<Label>("BusinessRows/BusinessMetricsRow/RevenueValueLabel");
        _hiddenOutcomeLabel = GetNodeOrNull<Label>("BusinessRows/BusinessMetricsRow/OutcomeValueLabel");
        _hiddenLastEventLabel = GetNodeOrNull<Label>("BusinessRows/LastEventValueLabel");
        _hiddenObjectiveLabel = GetNodeOrNull<Label>("BusinessRows/ObjectiveValueLabel");
        _hiddenNextObjectiveLabel = GetNodeOrNull<Label>("BusinessRows/NextObjectiveValueLabel");
        _hiddenRecentCoreEventLabel = GetNodeOrNull<Label>("BusinessRows/RecentCoreEventValueLabel");
        _hiddenPhaseRecapTitleLabel = GetNodeOrNull<Label>("BusinessRows/PhaseRecapTitleLabel");
        _hiddenPhaseRecapSummaryLabel = GetNodeOrNull<Label>("BusinessRows/PhaseRecapSummaryLabel");
        _hiddenPhaseRecapReasonLabel = GetNodeOrNull<Label>("BusinessRows/PhaseRecapReasonLabel");

        ConfigurePanel();
        ConfigureLogoLabel();
        ConfigureCompanyNameLabel();
        ConfigureMetricLabel(_cashValueLabel, minWidth: 142.0f);
        ConfigureMetricLabel(_usersValueLabel, minWidth: 82.0f);
        HideDebugMetricLabels();
        ResetDisplay();
    }

    public void ApplySimulationResult(CoreOfficeSimulationResult result)
    {
        SetLabel(
            _cashValueLabel,
            FormatCash(result.CompanyTotals.CurrentCash, result.CashDelta),
            result.CashDelta
        );
        SetLabel(_usersValueLabel, FormatUsers(result.CompanyTotals.CurrentActiveUsers));
    }

    private void ResetDisplay()
    {
        SetLabel(_companyLogoLabel, "志");
        SetLabel(_companyNameLabel, "76号");
        SetLabel(_cashValueLabel, "现金 --");
        SetLabel(_usersValueLabel, "用户 --");
    }

    private void HideDebugMetricLabels()
    {
        SetHidden(_hiddenProjectLabel);
        SetHidden(_hiddenRevenueLabel);
        SetHidden(_hiddenOutcomeLabel);
        SetHidden(_hiddenLastEventLabel);
        SetHidden(_hiddenObjectiveLabel);
        SetHidden(_hiddenNextObjectiveLabel);
        SetHidden(_hiddenRecentCoreEventLabel);
        SetHidden(_hiddenPhaseRecapTitleLabel);
        SetHidden(_hiddenPhaseRecapSummaryLabel);
        SetHidden(_hiddenPhaseRecapReasonLabel);
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

    private void ConfigureLogoLabel()
    {
        if (_companyLogoLabel == null)
        {
            return;
        }

        _companyLogoLabel.Text = "志";
        _companyLogoLabel.CustomMinimumSize = new Vector2(38.0f, 36.0f);
        _companyLogoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _companyLogoLabel.VerticalAlignment = VerticalAlignment.Center;
        _companyLogoLabel.AddThemeColorOverride("font_color", LogoTextColor);
        _companyLogoLabel.AddThemeColorOverride("font_shadow_color", LogoColor);
        _companyLogoLabel.AddThemeConstantOverride("shadow_outline_size", 14);
        _companyLogoLabel.AddThemeFontSizeOverride("font_size", 22);
    }

    private void ConfigureCompanyNameLabel()
    {
        if (_companyNameLabel == null)
        {
            return;
        }

        _companyNameLabel.Text = "76号";
        _companyNameLabel.CustomMinimumSize = new Vector2(116.0f, 36.0f);
        _companyNameLabel.VerticalAlignment = VerticalAlignment.Center;
        _companyNameLabel.AddThemeColorOverride("font_color", PrimaryTextColor);
        _companyNameLabel.AddThemeFontSizeOverride("font_size", 18);
    }

    private static void ConfigureMetricLabel(Label? label, float minWidth)
    {
        if (label == null)
        {
            return;
        }

        label.Visible = true;
        label.CustomMinimumSize = new Vector2(minWidth, 36.0f);
        label.VerticalAlignment = VerticalAlignment.Center;
        label.ClipText = true;
        label.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        label.AddThemeColorOverride("font_color", PrimaryTextColor);
        label.AddThemeFontSizeOverride("font_size", 18);
    }

    private static void SetHidden(Control? control)
    {
        if (control != null)
        {
            control.Visible = false;
        }
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
        else
        {
            label.AddThemeColorOverride("font_color", PrimaryTextColor);
        }
    }

    private static string FormatCash(double value, double delta)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "现金 ¥{0:0} {1:+0;-0;0}",
            value,
            delta
        );
    }

    private static string FormatUsers(int users)
    {
        return string.Format(CultureInfo.InvariantCulture, "用户 {0}", users);
    }
}
