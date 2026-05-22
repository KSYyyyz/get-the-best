using System.Linq;
using Godot;
using StartupSim.Core;

namespace GetTheBestGodot;

public partial class BusinessCalendarHudController : PanelContainer
{
    private const int DaysPerMonth = 20;
    private static readonly Color PanelColor = new(0.05f, 0.07f, 0.08f, 0.58f);
    private static readonly Color TextColor = new(0.92f, 0.96f, 0.94f, 0.96f);
    private static readonly Color ReportReadyColor = new(1.0f, 0.86f, 0.46f, 0.96f);

    private Label? _calendarStatusLabel;
    private int _currentMonth = 1;
    private int _currentDay = 1;
    private int? _pendingMonthlyReportMonth;
    private bool _isWaitingForMonthlyReport;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _calendarStatusLabel = GetNodeOrNull<Label>("CalendarStatusLabel");
        ConfigurePanel();
        ConfigureLabel(_calendarStatusLabel);
        UpdateCalendarLabel();
    }

    public bool AdvanceBusinessDay(out BusinessCalendarTick tick)
    {
        if (_isWaitingForMonthlyReport)
        {
            tick = new BusinessCalendarTick(_currentMonth, _currentDay, IsMonthEnd: false);
            return false;
        }

        var tickMonth = _currentMonth;
        var tickDay = _currentDay;
        tick = new BusinessCalendarTick(tickMonth, tickDay, tickDay >= DaysPerMonth);

        if (tick.IsMonthEnd)
        {
            _isWaitingForMonthlyReport = true;
            _pendingMonthlyReportMonth = tickMonth;
            SetCalendarText($"第 {tickMonth} 月 月报待查看", ReportReadyColor);
            _currentMonth++;
            _currentDay = 1;
            return true;
        }

        _currentDay++;
        UpdateCalendarLabel();
        return true;
    }

    public void MarkMonthlyReportReady(CoreOfficeSimulationResult result)
    {
        var monthlyReport = result.PresentationEvents.LastOrDefault(
            eventSummary => eventSummary.Kind == SimulationEventKind.MonthlyReportReady
        );
        if (monthlyReport == null)
        {
            return;
        }

        _isWaitingForMonthlyReport = true;
        SetCalendarText(
            $"第 {_pendingMonthlyReportMonth ?? _currentMonth - 1} 月 月报待查看",
            ReportReadyColor
        );
    }

    public void ResumeAfterMonthlyReport()
    {
        _isWaitingForMonthlyReport = false;
        _pendingMonthlyReportMonth = null;
        UpdateCalendarLabel();
    }

    private void UpdateCalendarLabel()
    {
        SetCalendarText($"第 {_currentMonth} 月 第 {_currentDay} 天", TextColor);
    }

    private void SetCalendarText(string text, Color color)
    {
        if (_calendarStatusLabel == null)
        {
            return;
        }

        _calendarStatusLabel.Text = text;
        _calendarStatusLabel.AddThemeColorOverride("font_color", color);
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

    private static void ConfigureLabel(Label? label)
    {
        if (label == null)
        {
            return;
        }

        label.AddThemeColorOverride("font_color", TextColor);
        label.CustomMinimumSize = new Vector2(146.0f, 26.0f);
        label.VerticalAlignment = VerticalAlignment.Center;
    }
}

public sealed record BusinessCalendarTick(int Month, int Day, bool IsMonthEnd);
