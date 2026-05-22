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

public sealed record CompanyState(double Cash, double MonthlyCostRate, ProjectState ActiveProject);

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
    double OperatingCostDelta
);

public sealed record TickResult(
    IReadOnlyList<EmployeeIntent> Intents,
    IReadOnlyList<EmployeeTickDelta> EmployeeDeltas,
    IReadOnlyList<FacilityTickDelta> FacilityDeltas,
    CompanyTickDelta CompanyDelta
);

public sealed record BusinessTickOptions(double TickHours)
{
    public static BusinessTickOptions Default { get; } = new(TickHours: 1.0);
}
