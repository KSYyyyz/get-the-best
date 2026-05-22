namespace StartupSim.Core;

public enum EmployeeRole
{
    Engineer,
    Designer,
    Planner,
    Marketing,
    Operations,
}

public enum EmployeeActivityKind
{
    Idle,
    MoveToFacility,
    UseFacility,
    Rest,
    Work,
}

public enum FacilityType
{
    OfficeDesk,
    ProductWhiteboard,
    ServerRack,
    RestSeat,
}

public enum RoomType
{
    ResearchRoom,
    MarketRoom,
    ServerRoom,
    RestRoom,
}

public enum EmployeeIntentKind
{
    Idle,
    MoveToFacility,
    UseFacility,
    Rest,
    Work,
}

public enum ProductStage
{
    Prototype,
    MvpReady,
    Launched,
}

public enum PhaseOutcomeKind
{
    InProgress,
    MvpCompleted,
    FirstUsersAcquired,
    RevenuePositive,
    FailedCashDepleted,
}

public enum SimulationEventKind
{
    IntentPlanned,
    ActivityChanged,
    FacilityUpdated,
    MetricChanged,
    MonthlyReportReady,
    PhaseOutcomeReached,
}

public enum SimulationEventLifetime
{
    Instant,
    StateChange,
}

public enum SimulationEventSubjectKind
{
    Employee,
    Facility,
    Metric,
    Report,
    Outcome,
}

public enum TextDisplayPolicy
{
    None,
    Optional,
    Recommended,
}

public sealed record GridCell(int X, int Y);

public sealed record EmployeeState(
    string Id,
    string DisplayName,
    EmployeeRole Role,
    double Skill,
    double Energy,
    double Fatigue,
    double Satisfaction,
    EmployeeActivityKind CurrentActivity,
    string? RoomId,
    GridCell? Cell,
    string? ActiveFacilityId = null,
    int RemainingActivityTicks = 0
);

public sealed record FacilityState(
    string Id,
    FacilityType Type,
    string RoomId,
    int Capacity,
    IReadOnlyList<string> OccupiedByEmployeeIds,
    double EfficiencyModifier
)
{
    public bool HasAvailableCapacity => OccupiedByEmployeeIds.Count < Capacity;
}

public sealed record RoomState(
    string Id,
    RoomType Type,
    double Comfort,
    double Noise,
    int Capacity,
    IReadOnlyList<string> FacilityIds
);

public sealed record ProjectState(string Id, double Progress, double RequiredProgress);

public sealed record ProductMarketState(
    ProductStage Stage,
    int ActiveUsers,
    double MonthlyRecurringRevenue
);

public sealed record CompanyState(
    double Cash,
    double MonthlyCostRate,
    ProjectState ActiveProject,
    ProductMarketState? ProductMarket = null
);

public sealed record OfficeRuleSnapshot(
    IReadOnlyList<EmployeeState> Employees,
    IReadOnlyList<FacilityState> Facilities,
    IReadOnlyList<RoomState> Rooms,
    CompanyState Company
);

public sealed record IntentTarget(string? FacilityId = null, string? RoomId = null, GridCell? TargetCell = null);

public sealed record EmployeeIntent(string EmployeeId, EmployeeIntentKind Kind, IntentTarget Target);

public sealed record EmployeeTickDelta(
    string EmployeeId,
    EmployeeActivityKind PreviousActivity,
    EmployeeActivityKind NextActivity,
    double FatigueDelta,
    double EnergyDelta,
    double SatisfactionDelta,
    double WorkOutput
);

public sealed record FacilityTickDelta(
    string FacilityId,
    bool IsInUse,
    int CurrentOccupancy,
    double EfficiencyMultiplier
);

public sealed record CompanyTickDelta(
    double ProjectProgressDelta,
    double CashDelta,
    double OperatingCostDelta,
    double RevenueDelta = 0
);

public sealed record ProductMarketTickDelta(
    ProductStage PreviousStage,
    ProductStage NextStage,
    int ActiveUsersDelta,
    double MonthlyRecurringRevenueDelta
);

public sealed record TickResult(
    IReadOnlyList<EmployeeIntent> Intents,
    IReadOnlyList<EmployeeTickDelta> EmployeeDeltas,
    IReadOnlyList<FacilityTickDelta> FacilityDeltas,
    CompanyTickDelta CompanyDelta,
    ProductMarketTickDelta? ProductMarketDelta = null,
    MonthlyReport? MonthlyReport = null
);

public sealed record BusinessTickOptions(double TickHours)
{
    public static BusinessTickOptions Default { get; } = new(TickHours: 1.0);
}

public sealed record MonthlyReport(
    string PeriodLabel,
    double ProjectProgress,
    int ActiveUsers,
    double Revenue,
    double Cash,
    IReadOnlyList<string> Reasons
);

public sealed record FirstLoopBusinessTickOptions(double TickHours, bool IsMonthEnd = false)
{
    public static FirstLoopBusinessTickOptions Default { get; } = new(TickHours: 1.0);
}

public sealed record SimulationTickOptions(double TickHours, bool IsMonthEnd = false)
{
    public static SimulationTickOptions Default { get; } = new(TickHours: 1.0);
}

public sealed record PhaseOutcome(
    PhaseOutcomeKind Kind,
    IReadOnlyList<string> Reasons
)
{
    public static PhaseOutcome InProgress { get; } = new(PhaseOutcomeKind.InProgress, []);
}

public sealed record SimulationPresentationEvent(
    SimulationEventKind Kind,
    string SubjectId,
    string Message
);

public sealed record SimulationTickResult(
    TickResult Tick,
    OfficeRuleSnapshot NextSnapshot,
    PhaseOutcome Outcome,
    IReadOnlyList<SimulationPresentationEvent> PresentationEvents
);

public sealed record SimulationResultFieldContract(
    string FieldName,
    string Meaning,
    string GodotConsumption
);

public sealed record SimulationEventSemanticContract(
    SimulationEventKind Kind,
    SimulationEventLifetime Lifetime,
    TextDisplayPolicy TextDisplay,
    SimulationEventSubjectKind SubjectKind,
    string SubjectIdMeaning,
    bool CanIgnoreIfSubjectMissing
);

public sealed record SimulationTickCadenceContract(
    double RecommendedRealSecondsPerTick,
    double DefaultTickHours,
    bool PauseDuringEmployeeWalkAnimation,
    bool UseMonthEndInV026Bridge
);
