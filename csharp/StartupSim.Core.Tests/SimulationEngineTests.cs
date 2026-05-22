using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class SimulationEngineTests
{
    public static void Run()
    {
        AdvancePlansLifecycleBusinessAndReducerInOrder();
        AdvanceReturnsStableFrontendContract();
        CashDepletionProducesFailureOutcome();
        FirstUsersProducePhaseOutcome();
        MonthEndOptionEmitsReportThroughSimulationResult();
        SimulationAdvanceIsDeterministic();
    }

    private static void AdvancePlansLifecycleBusinessAndReducerInOrder()
    {
        var snapshot = TestSnapshots.FirstLoopEngineerMovingToDesk(projectProgress: 99);

        var result = new OfficeSimulationEngine().Advance(snapshot);

        Assert.Equal("employee-1", result.Tick.Intents[0].EmployeeId);
        Assert.Equal(EmployeeActivityKind.Work, result.NextSnapshot.Employees[0].CurrentActivity);
        Assert.Equal("desk-1", result.NextSnapshot.Employees[0].ActiveFacilityId);
        Assert.True(
            result.Tick.CompanyDelta.ProjectProgressDelta > 0,
            "business tick should run after lifecycle has acquired the facility"
        );
        Assert.Equal(100.0, result.NextSnapshot.Company.ActiveProject.Progress);
    }

    private static void AdvanceReturnsStableFrontendContract()
    {
        var snapshot = TestSnapshots.FirstLoopEngineerMovingToDesk(projectProgress: 99);

        var result = new OfficeSimulationEngine().Advance(snapshot);

        Assert.True(result.Tick.Intents.Count > 0, "frontend contract should expose intents");
        Assert.True(result.Tick.EmployeeDeltas.Count > 0, "frontend contract should expose employee deltas");
        Assert.True(result.Tick.FacilityDeltas.Count > 0, "frontend contract should expose facility deltas");
        Assert.True(result.PresentationEvents.Count > 0, "frontend contract should expose presentation events");
        Assert.Equal(SimulationEventKind.IntentPlanned, result.PresentationEvents[0].Kind);
    }

    private static void CashDepletionProducesFailureOutcome()
    {
        var snapshot = TestSnapshots.FirstLoopMarketingUsingWhiteboard(activeUsers: 0) with
        {
            Company = TestSnapshots.FirstLoopMarketingUsingWhiteboard(activeUsers: 0).Company with
            {
                Cash = 1,
                MonthlyCostRate = 10_000,
            },
        };

        var result = new OfficeSimulationEngine().Advance(snapshot);

        Assert.Equal(PhaseOutcomeKind.FailedCashDepleted, result.Outcome.Kind);
        Assert.True(result.NextSnapshot.Company.Cash <= 0);
    }

    private static void FirstUsersProducePhaseOutcome()
    {
        var snapshot = TestSnapshots.FirstLoopMarketingUsingWhiteboard(activeUsers: 0);

        var result = new OfficeSimulationEngine().Advance(snapshot);

        Assert.Equal(PhaseOutcomeKind.FirstUsersAcquired, result.Outcome.Kind);
        Assert.True(result.Outcome.Reasons.Contains("获得首批活跃用户。"));
    }

    private static void MonthEndOptionEmitsReportThroughSimulationResult()
    {
        var snapshot = TestSnapshots.FirstLoopMarketingUsingWhiteboard(activeUsers: 50);
        var engine = new OfficeSimulationEngine(
            new SimulationTickOptions(TickHours: 1.0, IsMonthEnd: true),
            new EmployeeBehaviorEngine(),
            new EmployeeLifecycleEngine(),
            new OfficeStateReducer()
        );

        var result = engine.Advance(snapshot);

        Assert.True(result.Tick.MonthlyReport != null, "simulation result should expose month-end report");
        Assert.True(
            result.PresentationEvents.Any(evt => evt.Kind == SimulationEventKind.MonthlyReportReady),
            "month-end report should be exposed as a presentation event"
        );
    }

    private static void SimulationAdvanceIsDeterministic()
    {
        var snapshot = TestSnapshots.FirstLoopEngineerMovingToDesk(projectProgress: 99);
        var engine = new OfficeSimulationEngine();

        var first = engine.Advance(snapshot);
        var second = engine.Advance(snapshot);

        Assert.Equal(first.Tick.CompanyDelta.ProjectProgressDelta, second.Tick.CompanyDelta.ProjectProgressDelta);
        Assert.Equal(first.NextSnapshot.Company.ActiveProject.Progress, second.NextSnapshot.Company.ActiveProject.Progress);
        Assert.Equal(first.Outcome.Kind, second.Outcome.Kind);
        Assert.Equal(first.PresentationEvents.Count, second.PresentationEvents.Count);
    }
}
