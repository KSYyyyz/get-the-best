using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using StartupSim.Core;

namespace GetTheBestGodot;

public partial class V2CoreBridge : Node
{
    private const double DefaultCash = 50_000.0;
    private const double DefaultMonthlyCostRate = 6_000.0;
    private const double DefaultProjectProgress = 10.0;
    private const double DefaultProjectRequiredProgress = 100.0;

    private readonly EmployeeBehaviorEngine _behaviorEngine = new();
    private readonly GodotCoreBridgeContract _bridgeContract = new();

    public string GetInitialStatusText()
    {
        return "规则核心桥接已接入：员工意图由 C# Core 规划";
    }

    public IReadOnlyList<CoreEmployeeIntent> PlanEmployeeIntents(
        EmployeeStore employeeStore,
        FacilityPlacementStore facilityPlacementStore,
        RoomFootprintStore roomFootprintStore
    )
    {
        var snapshot = BuildSnapshot(employeeStore, facilityPlacementStore, roomFootprintStore);
        return _behaviorEngine.PlanIntents(snapshot).Select(MapCoreIntent).ToArray();
    }

    public OfficeRuleSnapshot BuildSnapshot(
        EmployeeStore employeeStore,
        FacilityPlacementStore facilityPlacementStore,
        RoomFootprintStore roomFootprintStore
    )
    {
        var dto = new GodotOfficeSnapshotDto(
            Employees: employeeStore
                .GetEmployees()
                .Select(employee => MapEmployee(employee, roomFootprintStore))
                .ToArray(),
            Facilities: facilityPlacementStore
                .GetFacilities()
                .Select(facility => MapFacility(facility, roomFootprintStore))
                .ToArray(),
            Rooms: roomFootprintStore
                .GetRooms()
                .Select(room => MapRoom(room, facilityPlacementStore))
                .ToArray(),
            Company: new GodotCompanyFactDto(
                Cash: DefaultCash,
                MonthlyCostRate: DefaultMonthlyCostRate,
                ProjectId: "project-1",
                ProjectProgress: DefaultProjectProgress,
                ProjectRequiredProgress: DefaultProjectRequiredProgress
            )
        );

        return _bridgeContract.BuildSnapshot(dto);
    }

    private static GodotEmployeeFactDto MapEmployee(
        EmployeeVisual employee,
        RoomFootprintStore roomFootprintStore
    )
    {
        var room = roomFootprintStore.FindAtCell(employee.Cell);
        return new GodotEmployeeFactDto(
            Id: employee.Id,
            DisplayName: employee.DisplayName,
            RoleLabel: employee.RoleLabel,
            Skill: 1.0,
            Energy: 80.0,
            Fatigue: 20.0,
            Satisfaction: 70.0,
            ActivityCode: "Idle",
            RoomId: room == null ? null : ToRoomId(room.Id),
            CellX: employee.Cell.X,
            CellY: employee.Cell.Y
        );
    }

    private static GodotFacilityFactDto MapFacility(
        FacilityPlacement facility,
        RoomFootprintStore roomFootprintStore
    )
    {
        var room = roomFootprintStore.FindAtCell(facility.Cell);
        return new GodotFacilityFactDto(
            Id: facility.Id,
            FacilityTypeCode: facility.FacilityType.ToString(),
            RoomId: room == null ? "room-0" : ToRoomId(room.Id),
            Capacity: 1,
            OccupiedByEmployeeIds: [],
            EfficiencyModifier: 1.0
        );
    }

    private static GodotRoomFactDto MapRoom(
        RoomFootprint room,
        FacilityPlacementStore facilityPlacementStore
    )
    {
        var facilityIds = facilityPlacementStore
            .GetFacilities()
            .Where(facility => room.Contains(facility.Cell))
            .Select(facility => ToFacilityId(facility.Id))
            .ToArray();
        return new GodotRoomFactDto(
            Id: ToRoomId(room.Id),
            RoomTypeCode: room.RoomType.ToString(),
            Comfort: 0.08,
            Noise: 0.04,
            Capacity: room.CellCount,
            FacilityIds: facilityIds
        );
    }

    private static CoreEmployeeIntent MapCoreIntent(EmployeeIntent intent)
    {
        return new CoreEmployeeIntent(
            EmployeeId: ParseCoreEmployeeId(intent.EmployeeId),
            Kind: intent.Kind,
            FacilityId: ParseCoreFacilityId(intent.Target.FacilityId)
        );
    }

    private static string ToRoomId(int roomId)
    {
        return $"room-{roomId}";
    }

    private static string ToFacilityId(int facilityId)
    {
        return $"facility-{facilityId}";
    }

    private static int ParseCoreEmployeeId(string employeeId)
    {
        return ParseCoreId(employeeId, "employee-");
    }

    private static int? ParseCoreFacilityId(string? facilityId)
    {
        return facilityId == null ? null : ParseCoreId(facilityId, "facility-");
    }

    private static int ParseCoreId(string value, string prefix)
    {
        if (!value.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Unexpected Core id '{value}'.", nameof(value));
        }

        return int.Parse(value[prefix.Length..], System.Globalization.CultureInfo.InvariantCulture);
    }
}

public sealed record CoreEmployeeIntent(
    int EmployeeId,
    EmployeeIntentKind Kind,
    int? FacilityId
);
