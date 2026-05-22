using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class StateReducerTests
{
    public static void Run()
    {
        AppliesTickDeltasToNextSnapshot();
        ClampsEmployeeAndProjectRanges();
        DoesNotMutateOriginalSnapshot();
    }

    private static void AppliesTickDeltasToNextSnapshot()
    {
        var snapshot = TestSnapshots.SingleEngineerUsingDesk(fatigue: 20);
        var result = new BusinessTickEngine().Tick(snapshot);

        var next = new OfficeStateReducer().ApplyTickResult(snapshot, result);
        var nextEmployee = next.Employees[0];

        Assert.True(next.Company.ActiveProject.Progress > snapshot.Company.ActiveProject.Progress);
        Assert.True(next.Company.Cash < snapshot.Company.Cash);
        Assert.True(nextEmployee.Fatigue > snapshot.Employees[0].Fatigue);
        Assert.True(nextEmployee.Energy < snapshot.Employees[0].Energy);
        Assert.Equal(EmployeeActivityKind.Work, nextEmployee.CurrentActivity);
    }

    private static void ClampsEmployeeAndProjectRanges()
    {
        var snapshot = TestSnapshots.SingleEngineerUsingDesk(fatigue: 99) with
        {
            Employees =
            [
                (TestSnapshots.SingleEngineerUsingDesk(fatigue: 99).Employees[0] with
                {
                    Energy = 1,
                    Satisfaction = 99,
                }),
            ],
            Company = new CompanyState(
                Cash: 10,
                MonthlyCostRate: 10_000,
                ActiveProject: new ProjectState("project-1", Progress: 99, RequiredProgress: 100)
            ),
        };
        var result = new TickResult(
            Intents: [],
            EmployeeDeltas: [
                new EmployeeTickDelta(
                    "employee-1",
                    EmployeeActivityKind.UseFacility,
                    EmployeeActivityKind.Work,
                    FatigueDelta: 10,
                    EnergyDelta: -10,
                    SatisfactionDelta: 10,
                    WorkOutput: 20
                ),
            ],
            FacilityDeltas: [],
            CompanyDelta: new CompanyTickDelta(
                ProjectProgressDelta: 20,
                CashDelta: -100,
                OperatingCostDelta: -100
            )
        );

        var next = new OfficeStateReducer().ApplyTickResult(snapshot, result);

        Assert.Equal(100.0, next.Employees[0].Fatigue);
        Assert.Equal(0.0, next.Employees[0].Energy);
        Assert.Equal(100.0, next.Employees[0].Satisfaction);
        Assert.Equal(100.0, next.Company.ActiveProject.Progress);
    }

    private static void DoesNotMutateOriginalSnapshot()
    {
        var snapshot = TestSnapshots.SingleEngineerUsingDesk(fatigue: 20);
        var result = new BusinessTickEngine().Tick(snapshot);

        _ = new OfficeStateReducer().ApplyTickResult(snapshot, result);

        Assert.Equal(20.0, snapshot.Employees[0].Fatigue);
        Assert.Equal(10.0, snapshot.Company.ActiveProject.Progress);
        Assert.Equal(50_000.0, snapshot.Company.Cash);
    }
}
