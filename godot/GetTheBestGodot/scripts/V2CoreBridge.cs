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
    private readonly EmployeeLifecycleEngine _lifecycleEngine = new();
    private readonly Dictionary<int, CoreEmployeeLifecycleState> _employeeLifecycleStates = [];
    private readonly Dictionary<int, IReadOnlyList<int>> _facilityOccupants = [];

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

    public IReadOnlyList<CoreEmployeeLifecycleState> AdvanceEmployeeLifecycle(
        EmployeeStore employeeStore,
        FacilityPlacementStore facilityPlacementStore,
        RoomFootprintStore roomFootprintStore,
        IReadOnlyList<CoreEmployeeIntent> intents
    )
    {
        var snapshot = BuildSnapshot(employeeStore, facilityPlacementStore, roomFootprintStore);
        var nextSnapshot = _lifecycleEngine.Advance(snapshot, intents.Select(MapGodotIntent).ToArray());
        StoreLifecycleState(nextSnapshot);
        return nextSnapshot.Employees.Select(MapLifecycleState).ToArray();
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

        return ApplyLifecycleState(_bridgeContract.BuildSnapshot(dto));
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

    private GodotFacilityFactDto MapFacility(
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
            OccupiedByEmployeeIds: _facilityOccupants.TryGetValue(
                facility.Id,
                out var occupantIds
            )
                ? occupantIds
                : [],
            EfficiencyModifier: 1.0
        );
    }

    private OfficeRuleSnapshot ApplyLifecycleState(OfficeRuleSnapshot snapshot)
    {
        return snapshot with
        {
            Employees = snapshot
                .Employees
                .Select(employee =>
                {
                    var employeeId = ParseCoreEmployeeId(employee.Id);
                    return _employeeLifecycleStates.TryGetValue(employeeId, out var state)
                        ? employee with
                        {
                            CurrentActivity = state.ActivityKind,
                            ActiveFacilityId = state.FacilityId == null
                                ? null
                                : ToFacilityId(state.FacilityId.Value),
                            RemainingActivityTicks = state.RemainingActivityTicks,
                        }
                        : employee;
                })
                .ToArray(),
        };
    }

    private void StoreLifecycleState(OfficeRuleSnapshot snapshot)
    {
        _employeeLifecycleStates.Clear();
        foreach (var employee in snapshot.Employees)
        {
            _employeeLifecycleStates[ParseCoreEmployeeId(employee.Id)] = MapLifecycleState(employee);
        }

        _facilityOccupants.Clear();
        foreach (var facility in snapshot.Facilities)
        {
            _facilityOccupants[ParseCoreFacilityId(facility.Id)!.Value] = facility
                .OccupiedByEmployeeIds
                .Select(ParseCoreEmployeeId)
                .ToArray();
        }
    }

    private static EmployeeIntent MapGodotIntent(CoreEmployeeIntent intent)
    {
        return new EmployeeIntent(
            ToEmployeeId(intent.EmployeeId),
            intent.Kind,
            new IntentTarget(
                FacilityId: intent.FacilityId == null ? null : ToFacilityId(intent.FacilityId.Value)
            )
        );
    }

    private static CoreEmployeeLifecycleState MapLifecycleState(EmployeeState employee)
    {
        return new CoreEmployeeLifecycleState(
            EmployeeId: ParseCoreEmployeeId(employee.Id),
            ActivityKind: employee.CurrentActivity,
            FacilityId: ParseCoreFacilityId(employee.ActiveFacilityId),
            RemainingActivityTicks: employee.RemainingActivityTicks
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

    private static string ToEmployeeId(int employeeId)
    {
        return $"employee-{employeeId}";
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

public sealed record CoreEmployeeLifecycleState(
    int EmployeeId,
    StartupSim.Core.EmployeeActivityKind ActivityKind,
    int? FacilityId,
    int RemainingActivityTicks
);
