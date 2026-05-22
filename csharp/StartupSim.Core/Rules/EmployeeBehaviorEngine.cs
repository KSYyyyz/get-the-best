namespace StartupSim.Core;

public sealed class EmployeeBehaviorEngine
{
    private const double RestFatigueThreshold = 85.0;

    public IReadOnlyList<EmployeeIntent> PlanIntents(OfficeRuleSnapshot snapshot)
    {
        var reservedFacilityIds = new HashSet<string>();
        var intents = new List<EmployeeIntent>();

        foreach (var employee in snapshot.Employees.OrderBy(employee => employee.Id, StringComparer.Ordinal))
        {
            if (employee.Fatigue >= RestFatigueThreshold)
            {
                intents.Add(new EmployeeIntent(employee.Id, EmployeeIntentKind.Rest, new IntentTarget()));
                continue;
            }

            if (employee.CurrentActivity is EmployeeActivityKind.UseFacility or EmployeeActivityKind.Work)
            {
                intents.Add(new EmployeeIntent(employee.Id, EmployeeIntentKind.Work, new IntentTarget()));
                continue;
            }

            var facility = FindBestAvailableFacility(snapshot, employee, reservedFacilityIds);
            if (facility == null)
            {
                intents.Add(new EmployeeIntent(employee.Id, EmployeeIntentKind.Idle, new IntentTarget()));
                continue;
            }

            reservedFacilityIds.Add(facility.Id);
            intents.Add(
                new EmployeeIntent(
                    employee.Id,
                    EmployeeIntentKind.MoveToFacility,
                    new IntentTarget(FacilityId: facility.Id, RoomId: facility.RoomId)
                )
            );
        }

        return intents;
    }

    private static FacilityState? FindBestAvailableFacility(
        OfficeRuleSnapshot snapshot,
        EmployeeState employee,
        HashSet<string> reservedFacilityIds
    )
    {
        var desiredTypes = GetDesiredFacilityTypes(employee.Role);
        return snapshot.Facilities
            .Where(facility =>
                desiredTypes.Contains(facility.Type)
                && facility.HasAvailableCapacity
                && !reservedFacilityIds.Contains(facility.Id)
            )
            .OrderBy(facility => Array.IndexOf(desiredTypes, facility.Type))
            .ThenByDescending(facility => facility.EfficiencyModifier)
            .ThenBy(facility => facility.Id, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    public static FacilityType[] GetDesiredFacilityTypes(EmployeeRole role)
    {
        return role switch
        {
            EmployeeRole.Engineer => [FacilityType.OfficeDesk, FacilityType.ServerRack],
            EmployeeRole.Designer => [FacilityType.OfficeDesk, FacilityType.ProductWhiteboard],
            EmployeeRole.Planner => [FacilityType.ProductWhiteboard, FacilityType.OfficeDesk],
            EmployeeRole.Marketing => [FacilityType.ProductWhiteboard, FacilityType.OfficeDesk],
            EmployeeRole.Operations => [FacilityType.ServerRack, FacilityType.OfficeDesk],
            _ => [FacilityType.OfficeDesk],
        };
    }
}
