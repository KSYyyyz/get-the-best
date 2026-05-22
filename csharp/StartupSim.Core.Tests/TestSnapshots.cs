using StartupSim.Core;

namespace StartupSim.Core.Tests;

internal static class TestSnapshots
{
    public static GodotOfficeSnapshotDto ValidGodotDto()
    {
        return new GodotOfficeSnapshotDto(
            Employees: [
                new GodotEmployeeFactDto(
                    Id: 1,
                    DisplayName: "\u6797\u5c0f\u5b89",
                    RoleLabel: "\u7a0b\u5e8f",
                    Skill: 1.2,
                    Energy: 80,
                    Fatigue: 20,
                    Satisfaction: 70,
                    ActivityCode: "Idle",
                    RoomId: "research-room",
                    CellX: 9,
                    CellY: 7
                ),
            ],
            Facilities: [
                new GodotFacilityFactDto(
                    Id: 1,
                    FacilityTypeCode: "OfficeDesk",
                    RoomId: "research-room",
                    Capacity: 1,
                    OccupiedByEmployeeIds: [],
                    EfficiencyModifier: 1.0
                ),
            ],
            Rooms: [
                new GodotRoomFactDto(
                    Id: "research-room",
                    RoomTypeCode: "ResearchRoom",
                    Comfort: 0.1,
                    Noise: 0.05,
                    Capacity: 4,
                    FacilityIds: ["facility-1"]
                ),
            ],
            Company: new GodotCompanyFactDto(
                Cash: 50_000,
                MonthlyCostRate: 6_000,
                ProjectId: "project-1",
                ProjectProgress: 10,
                ProjectRequiredProgress: 100
            )
        );
    }

    public static OfficeRuleSnapshot SingleEngineerWithTwoFacilities(
        IReadOnlyList<string>? deskOccupants = null
    )
    {
        return new OfficeRuleSnapshot(
            Employees: [
                new EmployeeState(
                    Id: "employee-1",
                    DisplayName: "\u6797\u5c0f\u5b89",
                    Role: EmployeeRole.Engineer,
                    Skill: 1.2,
                    Energy: 80,
                    Fatigue: 20,
                    Satisfaction: 70,
                    CurrentActivity: EmployeeActivityKind.Idle,
                    RoomId: "research-room",
                    Cell: new GridCell(9, 7)
                ),
            ],
            Facilities: [
                new FacilityState(
                    Id: "desk-1",
                    Type: FacilityType.OfficeDesk,
                    RoomId: "research-room",
                    Capacity: 1,
                    OccupiedByEmployeeIds: deskOccupants ?? [],
                    EfficiencyModifier: 1.0
                ),
                new FacilityState(
                    Id: "desk-2",
                    Type: FacilityType.OfficeDesk,
                    RoomId: "research-room",
                    Capacity: 1,
                    OccupiedByEmployeeIds: [],
                    EfficiencyModifier: 0.95
                ),
                new FacilityState(
                    Id: "whiteboard-1",
                    Type: FacilityType.ProductWhiteboard,
                    RoomId: "market-room",
                    Capacity: 2,
                    OccupiedByEmployeeIds: [],
                    EfficiencyModifier: 1.1
                ),
            ],
            Rooms: [
                new RoomState(
                    Id: "research-room",
                    Type: RoomType.ResearchRoom,
                    Comfort: 0.1,
                    Noise: 0.05,
                    Capacity: 4,
                    FacilityIds: ["desk-1", "desk-2"]
                ),
                new RoomState(
                    Id: "market-room",
                    Type: RoomType.MarketRoom,
                    Comfort: 0.05,
                    Noise: 0.08,
                    Capacity: 4,
                    FacilityIds: ["whiteboard-1"]
                ),
            ],
            Company: new CompanyState(
                Cash: 50_000,
                MonthlyCostRate: 6_000,
                ActiveProject: new ProjectState("project-1", Progress: 10, RequiredProgress: 100)
            )
        );
    }

    public static OfficeRuleSnapshot SingleEngineerUsingDesk(double fatigue)
    {
        var snapshot = SingleEngineerWithTwoFacilities();
        return snapshot with
        {
            Employees =
            [
                snapshot.Employees[0] with
                {
                    Fatigue = fatigue,
                    CurrentActivity = EmployeeActivityKind.UseFacility,
                },
            ],
            Facilities =
            [
                snapshot.Facilities[0] with { OccupiedByEmployeeIds = ["employee-1"] },
                snapshot.Facilities[1],
                snapshot.Facilities[2],
            ],
        };
    }

    public static OfficeRuleSnapshot TwoEngineersOneDesk()
    {
        var snapshot = SingleEngineerWithTwoFacilities();
        return snapshot with
        {
            Employees =
            [
                snapshot.Employees[0],
                snapshot.Employees[0] with
                {
                    Id = "employee-2",
                    DisplayName = "\u9648\u5b50\u822a",
                    Cell = new GridCell(10, 7),
                },
            ],
            Facilities =
            [
                snapshot.Facilities[0],
            ],
        };
    }

    public static OfficeRuleSnapshot EngineerWithOnlyFullDesk()
    {
        var snapshot = SingleEngineerWithTwoFacilities();
        return snapshot with
        {
            Facilities =
            [
                snapshot.Facilities[0] with { OccupiedByEmployeeIds = ["existing-engineer"] },
            ],
            Rooms =
            [
                snapshot.Rooms[0] with { FacilityIds = ["desk-1"] },
            ],
        };
    }

    public static OfficeRuleSnapshot MixedIntentAndUsingDesk()
    {
        var snapshot = FirstLoopEngineerUsingDesk(projectProgress: 25);
        return snapshot with
        {
            Employees =
            [
                snapshot.Employees[0],
                snapshot.Employees[0] with
                {
                    Id = "employee-2",
                    DisplayName = "\u9648\u5b50\u822a",
                    CurrentActivity = EmployeeActivityKind.Idle,
                    ActiveFacilityId = null,
                    RemainingActivityTicks = 0,
                    Cell = new GridCell(10, 7),
                },
            ],
            Facilities =
            [
                snapshot.Facilities[0],
                snapshot.Facilities[1],
                snapshot.Facilities[2],
            ],
        };
    }

    public static OfficeRuleSnapshot HighFatigueEngineerWithRestSeat()
    {
        var snapshot = SingleEngineerWithTwoFacilities();
        return snapshot with
        {
            Employees =
            [
                snapshot.Employees[0] with { Fatigue = 90, Energy = 20 },
            ],
            Facilities =
            [
                snapshot.Facilities[0],
                new FacilityState(
                    Id: "rest-seat-1",
                    Type: FacilityType.RestSeat,
                    RoomId: "rest-room",
                    Capacity: 1,
                    OccupiedByEmployeeIds: [],
                    EfficiencyModifier: 1.2
                ),
            ],
            Rooms =
            [
                snapshot.Rooms[0],
                new RoomState(
                    Id: "rest-room",
                    Type: RoomType.RestRoom,
                    Comfort: 0.2,
                    Noise: 0.02,
                    Capacity: 2,
                    FacilityIds: ["rest-seat-1"]
                ),
            ],
        };
    }

    public static OfficeRuleSnapshot EngineerResting(double fatigue, bool useRestSeat = true)
    {
        var snapshot = HighFatigueEngineerWithRestSeat();
        return snapshot with
        {
            Employees =
            [
                snapshot.Employees[0] with
                {
                    CurrentActivity = EmployeeActivityKind.Rest,
                    ActiveFacilityId = useRestSeat ? "rest-seat-1" : null,
                    RemainingActivityTicks = 1,
                },
            ],
            Facilities = useRestSeat
                ?
                [
                    snapshot.Facilities[0],
                    snapshot.Facilities[1] with { OccupiedByEmployeeIds = ["employee-1"] },
                ]
                :
                [
                    snapshot.Facilities[0],
                    snapshot.Facilities[1],
                ],
        };
    }

    public static OfficeRuleSnapshot FirstLoopEngineerUsingDesk(
        double projectProgress = 10,
        double requiredProgress = 100
    )
    {
        var snapshot = SingleEngineerUsingDesk(fatigue: 20);
        return snapshot with
        {
            Company = new CompanyState(
                Cash: 50_000,
                MonthlyCostRate: 6_000,
                ActiveProject: new ProjectState(
                    "mvp-project",
                    Progress: projectProgress,
                    RequiredProgress: requiredProgress
                ),
                ProductMarket: new ProductMarketState(
                    Stage: ProductStage.Prototype,
                    ActiveUsers: 0,
                    MonthlyRecurringRevenue: 0
                )
            ),
        };
    }

    public static OfficeRuleSnapshot FirstLoopEngineerMovingToDesk(double projectProgress = 10)
    {
        var snapshot = FirstLoopEngineerUsingDesk(projectProgress);
        return snapshot with
        {
            Employees =
            [
                snapshot.Employees[0] with
                {
                    CurrentActivity = EmployeeActivityKind.MoveToFacility,
                    ActiveFacilityId = "desk-1",
                    RemainingActivityTicks = 1,
                },
            ],
            Facilities =
            [
                snapshot.Facilities[0] with { OccupiedByEmployeeIds = [] },
                snapshot.Facilities[1],
                snapshot.Facilities[2],
            ],
        };
    }

    public static OfficeRuleSnapshot FirstLoopMarketingUsingWhiteboard(
        double projectProgress = 100,
        double requiredProgress = 100,
        int activeUsers = 0
    )
    {
        return new OfficeRuleSnapshot(
            Employees: [
                new EmployeeState(
                    Id: "employee-marketing-1",
                    DisplayName: "\u5468\u5c0f\u5b81",
                    Role: EmployeeRole.Marketing,
                    Skill: 1.4,
                    Energy: 85,
                    Fatigue: 20,
                    Satisfaction: 72,
                    CurrentActivity: EmployeeActivityKind.UseFacility,
                    RoomId: "market-room",
                    Cell: new GridCell(5, 8),
                    ActiveFacilityId: "market-whiteboard-1",
                    RemainingActivityTicks: 2
                ),
            ],
            Facilities: [
                new FacilityState(
                    Id: "market-whiteboard-1",
                    Type: FacilityType.ProductWhiteboard,
                    RoomId: "market-room",
                    Capacity: 1,
                    OccupiedByEmployeeIds: ["employee-marketing-1"],
                    EfficiencyModifier: 1.1
                ),
            ],
            Rooms: [
                new RoomState(
                    Id: "market-room",
                    Type: RoomType.MarketRoom,
                    Comfort: 0.08,
                    Noise: 0.04,
                    Capacity: 4,
                    FacilityIds: ["market-whiteboard-1"]
                ),
            ],
            Company: new CompanyState(
                Cash: 50_000,
                MonthlyCostRate: 6_000,
                ActiveProject: new ProjectState(
                    "mvp-project",
                    Progress: projectProgress,
                    RequiredProgress: requiredProgress
                ),
                ProductMarket: new ProductMarketState(
                    Stage: projectProgress >= requiredProgress
                        ? ProductStage.MvpReady
                        : ProductStage.Prototype,
                    ActiveUsers: activeUsers,
                    MonthlyRecurringRevenue: activeUsers * 12.0
                )
            )
        );
    }
}
