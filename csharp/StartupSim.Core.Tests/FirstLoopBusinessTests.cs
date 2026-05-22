using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class FirstLoopBusinessTests
{
    public static void Run()
    {
        ResearchWorkCompletesMvpStage();
        ResearchWorkContinuesFromWorkState();
        PlannerDoesNotReserveMarketWhiteboardBeforeMvp();
        MarketingWorkAddsFirstUsersAfterMvp();
        RevenueOffsetsOperatingCost();
        MonthEndReportExplainsProgressUsersRevenueAndCash();
        NonMonthEndDoesNotEmitMonthlyReport();
    }

    private static void ResearchWorkCompletesMvpStage()
    {
        var snapshot = TestSnapshots.FirstLoopEngineerUsingDesk(
            projectProgress: 99,
            requiredProgress: 100
        );

        var result = new FirstLoopBusinessEngine().Tick(snapshot);
        var next = new OfficeStateReducer().ApplyTickResult(snapshot, result);

        Assert.True(
            result.CompanyDelta.ProjectProgressDelta > 0,
            "research work should produce MVP progress"
        );
        Assert.Equal(ProductStage.MvpReady, result.ProductMarketDelta!.NextStage);
        Assert.Equal(ProductStage.MvpReady, next.Company.ProductMarket!.Stage);
        Assert.Equal(100.0, next.Company.ActiveProject.Progress);
    }

    private static void ResearchWorkContinuesFromWorkState()
    {
        var usingSnapshot = TestSnapshots.FirstLoopEngineerUsingDesk(projectProgress: 20);
        var snapshot = usingSnapshot with
        {
            Employees =
            [
                usingSnapshot.Employees[0] with
                {
                    CurrentActivity = EmployeeActivityKind.Work,
                    ActiveFacilityId = "desk-1",
                },
            ],
        };

        var result = new FirstLoopBusinessEngine().Tick(snapshot);

        Assert.True(
            result.CompanyDelta.ProjectProgressDelta > 0,
            "employees in persistent Work state should continue producing MVP progress"
        );
        Assert.True(
            result.FacilityDeltas.Count > 0,
            "persistent Work state should keep the active facility visible to the frontend"
        );
    }

    private static void PlannerDoesNotReserveMarketWhiteboardBeforeMvp()
    {
        var snapshot = TestSnapshots.SingleEngineerWithTwoFacilities() with
        {
            Employees =
            [
                new EmployeeState(
                    Id: "employee-planner-1",
                    DisplayName: "\u9648\u5b50\u822a",
                    Role: EmployeeRole.Planner,
                    Skill: 1.1,
                    Energy: 80,
                    Fatigue: 20,
                    Satisfaction: 70,
                    CurrentActivity: EmployeeActivityKind.Idle,
                    RoomId: "research-room",
                    Cell: new GridCell(10, 7)
                ),
            ],
            Company = new CompanyState(
                Cash: 50_000,
                MonthlyCostRate: 6_000,
                ActiveProject: new ProjectState("mvp-project", Progress: 20, RequiredProgress: 100),
                ProductMarket: new ProductMarketState(
                    ProductStage.Prototype,
                    ActiveUsers: 0,
                    MonthlyRecurringRevenue: 0
                )
            ),
        };

        var intent = new EmployeeBehaviorEngine().PlanIntents(snapshot)[0];

        Assert.Equal(EmployeeActionCandidateKind.WorkAtDesk, intent.SourceAction);
        Assert.Equal("desk-1", intent.Target.FacilityId);
    }

    private static void MarketingWorkAddsFirstUsersAfterMvp()
    {
        var snapshot = TestSnapshots.FirstLoopMarketingUsingWhiteboard(
            projectProgress: 100,
            requiredProgress: 100,
            activeUsers: 0
        );

        var result = new FirstLoopBusinessEngine().Tick(snapshot);
        var next = new OfficeStateReducer().ApplyTickResult(snapshot, result);

        Assert.True(
            result.ProductMarketDelta!.ActiveUsersDelta > 0,
            "marketing work should acquire first users after MVP is ready"
        );
        Assert.Equal(ProductStage.Launched, result.ProductMarketDelta.NextStage);
        Assert.True(next.Company.ProductMarket!.ActiveUsers > 0);
    }

    private static void RevenueOffsetsOperatingCost()
    {
        var snapshot = TestSnapshots.FirstLoopMarketingUsingWhiteboard(
            projectProgress: 100,
            requiredProgress: 100,
            activeUsers: 50
        );

        var result = new FirstLoopBusinessEngine().Tick(snapshot);

        Assert.True(result.CompanyDelta.RevenueDelta > 0, "active users should produce revenue");
        Assert.Equal(
            Math.Round(
                result.CompanyDelta.OperatingCostDelta + result.CompanyDelta.RevenueDelta,
                4
            ),
            result.CompanyDelta.CashDelta,
            "cash delta should combine revenue and operating cost"
        );
    }

    private static void MonthEndReportExplainsProgressUsersRevenueAndCash()
    {
        var snapshot = TestSnapshots.FirstLoopMarketingUsingWhiteboard(
            projectProgress: 100,
            requiredProgress: 100,
            activeUsers: 50
        );
        var engine = new FirstLoopBusinessEngine(
            new FirstLoopBusinessTickOptions(TickHours: 1.0, IsMonthEnd: true),
            new EmployeeBehaviorEngine()
        );

        var result = engine.Tick(snapshot);
        var report = result.MonthlyReport;

        Assert.True(report != null, "month-end tick should emit a monthly report");
        Assert.Equal("A0.24 月报", report!.PeriodLabel);
        Assert.Equal(100.0, report.ProjectProgress);
        Assert.Equal(
            snapshot.Company.ProductMarket!.ActiveUsers + result.ProductMarketDelta!.ActiveUsersDelta,
            report.ActiveUsers
        );
        Assert.Equal(result.CompanyDelta.RevenueDelta, report.Revenue);
        Assert.Equal(
            Math.Round(snapshot.Company.Cash + result.CompanyDelta.CashDelta, 4),
            report.Cash
        );
        Assert.True(report.Reasons.Contains("MVP 已完成，市场工作可以转化为用户。"));
        Assert.True(report.Reasons.Contains("活跃用户产生收入，经营成本按 tick 扣除。"));
    }

    private static void NonMonthEndDoesNotEmitMonthlyReport()
    {
        var snapshot = TestSnapshots.FirstLoopMarketingUsingWhiteboard(activeUsers: 50);

        var result = new FirstLoopBusinessEngine().Tick(snapshot);

        Assert.Equal(null, result.MonthlyReport);
    }
}
