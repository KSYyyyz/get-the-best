namespace StartupSim.Core;

public sealed class BusinessTickEngine
{
    private readonly BusinessTickOptions _options;
    private readonly EmployeeBehaviorEngine _behaviorEngine;

    public BusinessTickEngine()
        : this(BusinessTickOptions.Default, new EmployeeBehaviorEngine()) { }

    public BusinessTickEngine(BusinessTickOptions options, EmployeeBehaviorEngine behaviorEngine)
    {
        _options = options;
        _behaviorEngine = behaviorEngine;
    }

    public TickResult Tick(OfficeRuleSnapshot snapshot)
    {
        var intents = _behaviorEngine.PlanIntents(snapshot);
        var employeeDeltas = new List<EmployeeTickDelta>();
        var facilityDeltas = new List<FacilityTickDelta>();
        var projectProgressDelta = 0.0;

        foreach (var employee in snapshot.Employees.OrderBy(employee => employee.Id, StringComparer.Ordinal))
        {
            if (employee.CurrentActivity == EmployeeActivityKind.Rest)
            {
                employeeDeltas.Add(CreateRestDelta(snapshot, employee));
                var restFacility = FindActiveFacility(snapshot, employee);
                if (restFacility != null)
                {
                    facilityDeltas.Add(CreateFacilityDelta(restFacility, snapshot, employee));
                }
                continue;
            }

            var facility = FindFacilityUsedBy(snapshot, employee.Id);
            if (facility == null || !IsProductiveActivity(employee.CurrentActivity))
            {
                employeeDeltas.Add(CreateIdleDelta(employee));
                continue;
            }

            var room = snapshot.Rooms.FirstOrDefault(room => room.Id == facility.RoomId);
            var efficiency = CalculateEfficiency(employee, facility, room);
            var output = Math.Round(employee.Skill * efficiency * _options.TickHours, 4);
            var fatigueDelta = Math.Round(4.0 * _options.TickHours, 4);
            var energyDelta = Math.Round(-3.0 * _options.TickHours, 4);
            var satisfactionDelta = Math.Round(CalculateSatisfactionDelta(employee, room), 4);

            projectProgressDelta += output;
            employeeDeltas.Add(
                new EmployeeTickDelta(
                    employee.Id,
                    employee.CurrentActivity,
                    EmployeeActivityKind.Work,
                    fatigueDelta,
                    energyDelta,
                    satisfactionDelta,
                    output
                )
            );

            facilityDeltas.Add(
                new FacilityTickDelta(facility.Id, IsInUse: true, facility.OccupiedByEmployeeIds.Count, efficiency)
            );
        }

        var operatingCostDelta = Math.Round(
            -snapshot.Company.MonthlyCostRate / 30.0 / 8.0 * _options.TickHours,
            4
        );

        return new TickResult(
            intents,
            employeeDeltas,
            facilityDeltas.OrderBy(delta => delta.FacilityId, StringComparer.Ordinal).ToArray(),
            new CompanyTickDelta(
                ProjectProgressDelta: Math.Round(projectProgressDelta, 4),
                CashDelta: operatingCostDelta,
                OperatingCostDelta: operatingCostDelta
            )
        );
    }

    public static double CalculateEfficiency(
        EmployeeState employee,
        FacilityState facility,
        RoomState? room
    )
    {
        var fatigueMultiplier = CalculateFatigueMultiplier(employee.Fatigue);
        var energyMultiplier = Clamp(employee.Energy / 100.0, 0.5, 1.1);
        var roomMultiplier = room == null ? 1.0 : Clamp(1.0 + room.Comfort - room.Noise, 0.75, 1.25);
        var roleMatchMultiplier = EmployeeBehaviorEngine.GetDesiredFacilityTypes(employee.Role)
            .Contains(facility.Type)
            ? 1.0
            : 0.75;

        return Math.Round(
            fatigueMultiplier
                * energyMultiplier
                * facility.EfficiencyModifier
                * roomMultiplier
                * roleMatchMultiplier,
            4
        );
    }

    private static FacilityState? FindFacilityUsedBy(OfficeRuleSnapshot snapshot, string employeeId)
    {
        return snapshot.Facilities
            .OrderBy(facility => facility.Id, StringComparer.Ordinal)
            .FirstOrDefault(facility => facility.OccupiedByEmployeeIds.Contains(employeeId));
    }

    private static bool IsProductiveActivity(EmployeeActivityKind activity)
    {
        return activity is EmployeeActivityKind.UseFacility or EmployeeActivityKind.Work;
    }

    private static EmployeeTickDelta CreateIdleDelta(EmployeeState employee)
    {
        return new EmployeeTickDelta(
            employee.Id,
            employee.CurrentActivity,
            employee.CurrentActivity,
            FatigueDelta: 0,
            EnergyDelta: 0,
            SatisfactionDelta: 0,
            WorkOutput: 0
        );
    }

    private EmployeeTickDelta CreateRestDelta(
        OfficeRuleSnapshot snapshot,
        EmployeeState employee
    )
    {
        var facility = FindActiveFacility(snapshot, employee);
        var room = facility != null
            ? snapshot.Rooms.FirstOrDefault(room => room.Id == facility.RoomId)
            : snapshot.Rooms.FirstOrDefault(room => room.Id == employee.RoomId);
        var facilityMultiplier = facility?.EfficiencyModifier ?? 1.0;
        var roomMultiplier = room == null ? 1.0 : Clamp(1.0 + room.Comfort - room.Noise, 0.8, 1.3);
        var recoveryMultiplier = facilityMultiplier * roomMultiplier;

        return new EmployeeTickDelta(
            employee.Id,
            employee.CurrentActivity,
            EmployeeActivityKind.Rest,
            FatigueDelta: Math.Round(-5.0 * recoveryMultiplier * _options.TickHours, 4),
            EnergyDelta: Math.Round(4.0 * recoveryMultiplier * _options.TickHours, 4),
            SatisfactionDelta: Math.Round(0.2 * recoveryMultiplier, 4),
            WorkOutput: 0
        );
    }

    private static FacilityTickDelta CreateFacilityDelta(
        FacilityState facility,
        OfficeRuleSnapshot snapshot,
        EmployeeState employee
    )
    {
        var room = snapshot.Rooms.FirstOrDefault(room => room.Id == facility.RoomId);
        var efficiency = CalculateEfficiency(employee, facility, room);
        return new FacilityTickDelta(
            facility.Id,
            IsInUse: true,
            facility.OccupiedByEmployeeIds.Count,
            efficiency
        );
    }

    private static FacilityState? FindActiveFacility(OfficeRuleSnapshot snapshot, EmployeeState employee)
    {
        if (employee.ActiveFacilityId != null)
        {
            return snapshot.Facilities.FirstOrDefault(facility =>
                facility.Id == employee.ActiveFacilityId
            );
        }

        return FindFacilityUsedBy(snapshot, employee.Id);
    }

    private static double CalculateFatigueMultiplier(double fatigue)
    {
        if (fatigue <= 50)
        {
            return 1.0;
        }

        return Clamp(1.0 - (fatigue - 50.0) / 100.0, 0.4, 1.0);
    }

    private static double CalculateSatisfactionDelta(EmployeeState employee, RoomState? room)
    {
        var roomComfort = room == null ? 0.0 : room.Comfort - room.Noise;
        var fatiguePressure = employee.Fatigue >= 80 ? -0.5 : 0.0;
        return roomComfort + fatiguePressure;
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }
}
