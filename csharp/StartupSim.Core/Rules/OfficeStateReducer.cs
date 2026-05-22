namespace StartupSim.Core;

public sealed class OfficeStateReducer
{
    public OfficeRuleSnapshot ApplyTickResult(OfficeRuleSnapshot snapshot, TickResult result)
    {
        var employeeDeltas = result.EmployeeDeltas.ToDictionary(delta => delta.EmployeeId);
        var nextEmployees = snapshot.Employees
            .Select(employee => ApplyEmployeeDelta(employee, employeeDeltas))
            .ToArray();

        var nextProject = snapshot.Company.ActiveProject with
        {
            Progress = Clamp(
                snapshot.Company.ActiveProject.Progress + result.CompanyDelta.ProjectProgressDelta,
                0,
                snapshot.Company.ActiveProject.RequiredProgress
            ),
        };
        var nextCompany = snapshot.Company with
        {
            Cash = Math.Round(snapshot.Company.Cash + result.CompanyDelta.CashDelta, 4),
            ActiveProject = nextProject,
        };

        return snapshot with { Employees = nextEmployees, Company = nextCompany };
    }

    private static EmployeeState ApplyEmployeeDelta(
        EmployeeState employee,
        IReadOnlyDictionary<string, EmployeeTickDelta> deltas
    )
    {
        if (!deltas.TryGetValue(employee.Id, out var delta))
        {
            return employee;
        }

        return employee with
        {
            CurrentActivity = delta.NextActivity,
            Fatigue = Clamp(employee.Fatigue + delta.FatigueDelta, 0, 100),
            Energy = Clamp(employee.Energy + delta.EnergyDelta, 0, 100),
            Satisfaction = Clamp(employee.Satisfaction + delta.SatisfactionDelta, 0, 100),
        };
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Round(Math.Min(Math.Max(value, min), max), 4);
    }
}
