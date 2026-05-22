namespace StartupSim.Core;

public static class SimulationFrontendContract
{
    public static IReadOnlyList<SimulationResultFieldContract> ResultFields { get; } =
    [
        new SimulationResultFieldContract(
            "Tick",
            "本 tick 的意图、员工 delta、设施 delta、公司 delta、产品市场 delta 和月报。",
            "现金、收入、用户、MVP 等经营 delta"
        ),
        new SimulationResultFieldContract(
            "NextSnapshot",
            "Core 计算后的下一帧办公室事实快照，可继续作为下一次 Advance 输入。",
            "员工位置/活动状态、设施占用的表现事实"
        ),
        new SimulationResultFieldContract(
            "Outcome",
            "Core 判定的经营阶段结果。",
            "经营阶段胜利或失败结果"
        ),
        new SimulationResultFieldContract(
            "PresentationEvents",
            "前端可播放的一次性提示或状态变化事件。",
            "可播放提示和一次性表现事件"
        ),
    ];

    public static IReadOnlyList<SimulationResultFieldContract> CompanyTotals { get; } =
    [
        new SimulationResultFieldContract(
            "CurrentCash",
            "当前总现金",
            "HUD 当前现金总量"
        ),
        new SimulationResultFieldContract(
            "CurrentProjectProgress",
            "当前 MVP 总进度",
            "HUD 当前 MVP 总进度"
        ),
        new SimulationResultFieldContract(
            "ProjectRequiredProgress",
            "MVP 所需总进度",
            "HUD MVP 进度上限或进度条分母"
        ),
        new SimulationResultFieldContract(
            "CurrentActiveUsers",
            "当前活跃用户",
            "HUD 当前活跃用户总量"
        ),
        new SimulationResultFieldContract(
            "CurrentMonthlyRecurringRevenue",
            "当前月经常收入",
            "HUD 当前 MRR 总量"
        ),
        new SimulationResultFieldContract(
            "ProductStage",
            "当前产品阶段",
            "HUD 或阶段提示的当前产品阶段"
        ),
    ];

    public static IReadOnlyList<SimulationEventSemanticContract> EventSemantics { get; } =
    [
        new SimulationEventSemanticContract(
            SimulationEventKind.IntentPlanned,
            SimulationEventLifetime.Instant,
            TextDisplayPolicy.Optional,
            SimulationEventSubjectKind.Employee,
            "EmployeeId",
            CanIgnoreIfSubjectMissing: true
        ),
        new SimulationEventSemanticContract(
            SimulationEventKind.ActivityChanged,
            SimulationEventLifetime.StateChange,
            TextDisplayPolicy.None,
            SimulationEventSubjectKind.Employee,
            "EmployeeId",
            CanIgnoreIfSubjectMissing: true
        ),
        new SimulationEventSemanticContract(
            SimulationEventKind.FacilityUpdated,
            SimulationEventLifetime.StateChange,
            TextDisplayPolicy.None,
            SimulationEventSubjectKind.Facility,
            "FacilityId",
            CanIgnoreIfSubjectMissing: true
        ),
        new SimulationEventSemanticContract(
            SimulationEventKind.MetricChanged,
            SimulationEventLifetime.Instant,
            TextDisplayPolicy.Optional,
            SimulationEventSubjectKind.Metric,
            "Metric or project id",
            CanIgnoreIfSubjectMissing: true
        ),
        new SimulationEventSemanticContract(
            SimulationEventKind.MonthlyReportReady,
            SimulationEventLifetime.Instant,
            TextDisplayPolicy.Recommended,
            SimulationEventSubjectKind.Report,
            "Report period label",
            CanIgnoreIfSubjectMissing: true
        ),
        new SimulationEventSemanticContract(
            SimulationEventKind.PhaseOutcomeReached,
            SimulationEventLifetime.Instant,
            TextDisplayPolicy.Recommended,
            SimulationEventSubjectKind.Outcome,
            "PhaseOutcomeKind",
            CanIgnoreIfSubjectMissing: true
        ),
    ];

    public static SimulationTickCadenceContract Cadence { get; } = new(
        RecommendedRealSecondsPerTick: 2.0,
        DefaultTickHours: 1.0,
        PauseDuringEmployeeWalkAnimation: false,
        UseMonthEndInV026Bridge: false
    );
}
