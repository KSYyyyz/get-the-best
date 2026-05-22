using StartupSim.Core;

namespace StartupSim.Core.Tests;

internal static class TestSnapshots
{
    public static OfficeRuleSnapshot SingleEngineerWithTwoFacilities(
        IReadOnlyList<string>? deskOccupants = null
    )
    {
        return new OfficeRuleSnapshot(
            Employees:
            [
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
            Facilities:
            [
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
            Rooms:
            [
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
}
