using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class ProductLaunchGrowthTests
{
    public static void Run()
    {
        PublishFailsWhenMvpIsNotReady();
        PublishWithoutResearchLaunchesWithLowerUsersAndRating();
        MarketResearchImprovesLaunchUsersAndRating();
        LaunchedProductGrowsFromMarketWorkAndServerMaintenanceProtectsRetention();
        LowRatingAndNoServerMaintenanceCanCauseChurn();
        SimulationTickResultTopLevelContractStaysStable();
    }

    private static void PublishFailsWhenMvpIsNotReady()
    {
        var result = Publish(TestSnapshots.FirstLoopEngineerUsingDesk(projectProgress: 40));
        var next = new OfficeStateReducer().ApplyTickResult(
            TestSnapshots.FirstLoopEngineerUsingDesk(projectProgress: 40),
            result
        );
        var commandResult = result.PlayerCommandResults![0];

        Assert.Equal(0.0, commandResult.CashDelta);
        Assert.Equal(0, commandResult.ActiveUsersDelta);
        Assert.Equal(ProductStage.Prototype, result.ProductMarketDelta!.NextStage);
        Assert.Equal(0, next.Company.ProductMarket!.ActiveUsers);
        Assert.True(commandResult.Message.Contains("MVP", StringComparison.Ordinal));
    }

    private static void PublishWithoutResearchLaunchesWithLowerUsersAndRating()
    {
        var snapshot = TestSnapshots.ReadyMvpWithMarketState(marketAwareness: 0.0);
        var result = Publish(snapshot);
        var next = new OfficeStateReducer().ApplyTickResult(snapshot, result);

        Assert.True(result.ProductMarketDelta!.ActiveUsersDelta > 0);
        Assert.True(
            result.ProductMarketDelta.ActiveUsersDelta < 20,
            "unresearched launch should no longer use the old fixed 20 users"
        );
        Assert.True(next.Company.ProductMarket!.UserRating < 3.8);
        Assert.True(next.Company.ProductMarket.LaunchQuality > 0);
    }

    private static void MarketResearchImprovesLaunchUsersAndRating()
    {
        var cold = Publish(TestSnapshots.ReadyMvpWithMarketState(marketAwareness: 0.0));
        var researchedSnapshot = TestSnapshots.ReadyMvpWithMarketState(marketAwareness: 0.8);

        var researched = Publish(researchedSnapshot);
        var next = new OfficeStateReducer().ApplyTickResult(researchedSnapshot, researched);

        Assert.True(
            researched.ProductMarketDelta!.ActiveUsersDelta > cold.ProductMarketDelta!.ActiveUsersDelta,
            "market awareness should increase launch users"
        );
        Assert.True(
            next.Company.ProductMarket!.UserRating > 3.8,
            "market awareness should improve launch rating"
        );
    }

    private static void LaunchedProductGrowsFromMarketWorkAndServerMaintenanceProtectsRetention()
    {
        var snapshot = TestSnapshots.LaunchedProductWithMarketAndServerWork(
            activeUsers: 40,
            userRating: 4.2,
            marketAwareness: 0.7
        );
        var result = new OfficeSimulationEngine().Advance(snapshot);
        var next = result.NextSnapshot.Company.ProductMarket!;

        Assert.True(result.Tick.ProductMarketDelta!.ActiveUsersDelta > 0);
        Assert.True(
            result.Tick.ProductMarketDelta.RetentionDelta >= 0,
            "server maintenance should protect retention"
        );
        Assert.True(next.ActiveUsers > snapshot.Company.ProductMarket!.ActiveUsers);
        Assert.True(next.MonthlyRecurringRevenue > snapshot.Company.ProductMarket.MonthlyRecurringRevenue);
        Assert.True(
            result.PresentationEvents.Any(evt =>
                evt.Message.Contains("用户增长", StringComparison.Ordinal)
                || evt.Message.Contains("收入变化", StringComparison.Ordinal)
            ),
            "growth and revenue reasons should be playable"
        );
    }

    private static void LowRatingAndNoServerMaintenanceCanCauseChurn()
    {
        var snapshot = TestSnapshots.LaunchedProductWithMarketAndServerWork(
            activeUsers: 40,
            userRating: 2.1,
            marketAwareness: 0.1,
            includeMarketing: false,
            includeServerMaintenance: false
        );

        var result = new OfficeSimulationEngine().Advance(snapshot);

        Assert.True(result.Tick.ProductMarketDelta!.ActiveUsersDelta < 0);
        Assert.True(result.Tick.CompanyDelta.RevenueDelta < snapshot.Company.ProductMarket!.MonthlyRecurringRevenue);
        Assert.True(
            result.PresentationEvents.Any(evt =>
                evt.Message.Contains("流失", StringComparison.Ordinal)
                || evt.Message.Contains("差评", StringComparison.Ordinal)
                || evt.Message.Contains("增长停滞", StringComparison.Ordinal)
            ),
            "churn or poor feedback should be explained"
        );
    }

    private static void SimulationTickResultTopLevelContractStaysStable()
    {
        var propertyNames = typeof(SimulationTickResult)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal("NextSnapshot,Outcome,PresentationEvents,Tick", string.Join(",", propertyNames));
    }

    private static TickResult Publish(OfficeRuleSnapshot snapshot)
    {
        var engine = new FirstLoopBusinessEngine(
            new FirstLoopBusinessTickOptions(
                TickHours: 1.0,
                IsMonthEnd: false,
                PlayerCommands: [new PlayerCommand(PlayerCommandKind.PublishPrototype)]
            ),
            new EmployeeBehaviorEngine()
        );

        return engine.Tick(snapshot);
    }
}
