using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class BridgeContractTests
{
    public static void Run()
    {
        MapsGodotFactDtoToCoreSnapshot();
        RejectsUnknownEmployeeRole();
        BridgeContractDoesNotReferenceGodot();
    }

    private static void MapsGodotFactDtoToCoreSnapshot()
    {
        var dto = new GodotOfficeSnapshotDto(
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

        var snapshot = new GodotCoreBridgeContract().BuildSnapshot(dto);

        Assert.Equal("employee-1", snapshot.Employees[0].Id);
        Assert.Equal(EmployeeRole.Engineer, snapshot.Employees[0].Role);
        Assert.Equal(FacilityType.OfficeDesk, snapshot.Facilities[0].Type);
        Assert.Equal(RoomType.ResearchRoom, snapshot.Rooms[0].Type);
        Assert.Equal(50_000.0, snapshot.Company.Cash);
    }

    private static void RejectsUnknownEmployeeRole()
    {
        var dto = TestSnapshots.ValidGodotDto() with
        {
            Employees =
            [
                (TestSnapshots.ValidGodotDto().Employees[0] with { RoleLabel = "UnknownRole" }),
            ],
        };

        Assert.Throws<ArgumentException>(() => new GodotCoreBridgeContract().BuildSnapshot(dto));
    }

    private static void BridgeContractDoesNotReferenceGodot()
    {
        var referencedAssemblies = typeof(GodotCoreBridgeContract)
            .Assembly.GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.False(referencedAssemblies.Contains("GodotSharp"), "Bridge contract must not reference GodotSharp");
        Assert.False(referencedAssemblies.Contains("Godot"), "Bridge contract must not reference Godot");
    }
}
