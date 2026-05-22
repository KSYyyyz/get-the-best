namespace StartupSim.Core;

public sealed class EmployeeLifecycleEngine
{
    private readonly int _useDurationTicks;

    public EmployeeLifecycleEngine(int useDurationTicks = 2)
    {
        if (useDurationTicks < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(useDurationTicks));
        }

        _useDurationTicks = useDurationTicks;
    }

    public OfficeRuleSnapshot Advance(
        OfficeRuleSnapshot snapshot,
        IReadOnlyList<EmployeeIntent> intents
    )
    {
        var intentByEmployeeId = intents
            .GroupBy(intent => intent.EmployeeId)
            .ToDictionary(group => group.Key, group => group.First());
        var occupancy = snapshot.Facilities.ToDictionary(
            facility => facility.Id,
            facility => facility.OccupiedByEmployeeIds.ToList()
        );
        var reservedFacilityIds = BuildInitialReservations(snapshot);
        var nextEmployees = new List<EmployeeState>();

        foreach (var employee in snapshot.Employees.OrderBy(employee => employee.Id, StringComparer.Ordinal))
        {
            nextEmployees.Add(AdvanceEmployee(employee, intentByEmployeeId, snapshot, occupancy, reservedFacilityIds));
        }

        var nextFacilities = snapshot.Facilities
            .Select(facility => facility with { OccupiedByEmployeeIds = occupancy[facility.Id].ToArray() })
            .ToArray();

        return snapshot with { Employees = nextEmployees.ToArray(), Facilities = nextFacilities };
    }

    private EmployeeState AdvanceEmployee(
        EmployeeState employee,
        IReadOnlyDictionary<string, EmployeeIntent> intentByEmployeeId,
        OfficeRuleSnapshot snapshot,
        Dictionary<string, List<string>> occupancy,
        HashSet<string> reservedFacilityIds
    )
    {
        if (employee.CurrentActivity == EmployeeActivityKind.UseFacility)
        {
            return AdvanceUsingEmployee(employee, occupancy, reservedFacilityIds);
        }

        if (employee.CurrentActivity == EmployeeActivityKind.MoveToFacility)
        {
            return AdvanceMovingEmployee(employee, snapshot, occupancy, reservedFacilityIds);
        }

        if (!intentByEmployeeId.TryGetValue(employee.Id, out var intent))
        {
            return employee;
        }

        return intent.Kind == EmployeeIntentKind.MoveToFacility
            ? TryStartFacilityMove(employee, intent, snapshot, reservedFacilityIds)
            : employee;
    }

    private EmployeeState AdvanceUsingEmployee(
        EmployeeState employee,
        Dictionary<string, List<string>> occupancy,
        HashSet<string> reservedFacilityIds
    )
    {
        if (employee.ActiveFacilityId == null)
        {
            return employee with
            {
                CurrentActivity = EmployeeActivityKind.Idle,
                RemainingActivityTicks = 0,
            };
        }

        if (employee.RemainingActivityTicks > 1)
        {
            return employee with { RemainingActivityTicks = employee.RemainingActivityTicks - 1 };
        }

        if (occupancy.TryGetValue(employee.ActiveFacilityId, out var occupants))
        {
            occupants.Remove(employee.Id);
        }

        reservedFacilityIds.Remove(employee.ActiveFacilityId);
        return employee with
        {
            CurrentActivity = EmployeeActivityKind.Idle,
            ActiveFacilityId = null,
            RemainingActivityTicks = 0,
        };
    }

    private EmployeeState AdvanceMovingEmployee(
        EmployeeState employee,
        OfficeRuleSnapshot snapshot,
        Dictionary<string, List<string>> occupancy,
        HashSet<string> reservedFacilityIds
    )
    {
        if (employee.ActiveFacilityId == null)
        {
            return employee with
            {
                CurrentActivity = EmployeeActivityKind.Idle,
                RemainingActivityTicks = 0,
            };
        }

        if (employee.RemainingActivityTicks > 1)
        {
            return employee with { RemainingActivityTicks = employee.RemainingActivityTicks - 1 };
        }

        var facility = snapshot.Facilities.FirstOrDefault(facility =>
            facility.Id == employee.ActiveFacilityId
        );
        if (facility == null || occupancy[facility.Id].Count >= facility.Capacity)
        {
            reservedFacilityIds.Remove(employee.ActiveFacilityId);
            return employee with
            {
                CurrentActivity = EmployeeActivityKind.Idle,
                ActiveFacilityId = null,
                RemainingActivityTicks = 0,
            };
        }

        if (!occupancy[facility.Id].Contains(employee.Id))
        {
            occupancy[facility.Id].Add(employee.Id);
        }

        reservedFacilityIds.Add(facility.Id);
        return employee with
        {
            CurrentActivity = EmployeeActivityKind.UseFacility,
            ActiveFacilityId = facility.Id,
            RemainingActivityTicks = _useDurationTicks,
        };
    }

    private static EmployeeState TryStartFacilityMove(
        EmployeeState employee,
        EmployeeIntent intent,
        OfficeRuleSnapshot snapshot,
        HashSet<string> reservedFacilityIds
    )
    {
        var facilityId = intent.Target.FacilityId;
        if (facilityId == null || reservedFacilityIds.Contains(facilityId))
        {
            return employee;
        }

        var facility = snapshot.Facilities.FirstOrDefault(facility => facility.Id == facilityId);
        if (facility == null || !facility.HasAvailableCapacity)
        {
            return employee;
        }

        reservedFacilityIds.Add(facilityId);
        return employee with
        {
            CurrentActivity = EmployeeActivityKind.MoveToFacility,
            ActiveFacilityId = facilityId,
            RemainingActivityTicks = 1,
        };
    }

    private static HashSet<string> BuildInitialReservations(OfficeRuleSnapshot snapshot)
    {
        var reservedFacilityIds = snapshot.Facilities
            .Where(facility => facility.OccupiedByEmployeeIds.Count >= facility.Capacity)
            .Select(facility => facility.Id)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var employee in snapshot.Employees)
        {
            if (
                employee.ActiveFacilityId != null
                && employee.CurrentActivity
                    is EmployeeActivityKind.MoveToFacility or EmployeeActivityKind.UseFacility
            )
            {
                reservedFacilityIds.Add(employee.ActiveFacilityId);
            }
        }

        return reservedFacilityIds;
    }
}
