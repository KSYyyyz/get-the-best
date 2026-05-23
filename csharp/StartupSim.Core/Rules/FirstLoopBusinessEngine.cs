namespace StartupSim.Core;

public sealed class FirstLoopBusinessEngine
{
    private const double UsersPerSalesOutput = 2.0;
    private const double MonthlyRevenuePerUser = 12.0;
    private const double MarketResearchCost = 500.0;
    private const double MarketResearchPrototypeProgress = 2.0;
    private const int MarketResearchLaunchUsers = 5;
    private const double PublishPrototypeCost = 1200.0;

    private readonly FirstLoopBusinessTickOptions _options;
    private readonly EmployeeBehaviorEngine _behaviorEngine;

    public FirstLoopBusinessEngine()
        : this(FirstLoopBusinessTickOptions.Default, new EmployeeBehaviorEngine()) { }

    public FirstLoopBusinessEngine(
        FirstLoopBusinessTickOptions options,
        EmployeeBehaviorEngine behaviorEngine
    )
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
        var salesOutput = 0.0;
        var serverMaintenanceOutput = 0.0;
        var playerCommandResults = ApplyPlayerCommands(snapshot);

        foreach (var employee in snapshot.Employees.OrderBy(employee => employee.Id, StringComparer.Ordinal))
        {
            var facility = FindFacilityUsedBy(snapshot, employee.Id);
            if (facility == null || !IsProductiveActivity(employee.CurrentActivity))
            {
                employeeDeltas.Add(CreateIdleDelta(employee));
                continue;
            }

            var room = snapshot.Rooms.FirstOrDefault(room => room.Id == facility.RoomId);
            var efficiency = BusinessTickEngine.CalculateEfficiency(employee, facility, room);
            var output = Math.Round(employee.Skill * efficiency * _options.TickHours, 4);

            if (ContributesToMvp(employee, facility, room))
            {
                projectProgressDelta += output;
            }
            else if (ContributesToSales(employee, facility, room))
            {
                salesOutput += output;
            }
            else if (ContributesToServerMaintenance(employee, facility, room))
            {
                serverMaintenanceOutput += output;
            }

            employeeDeltas.Add(
                new EmployeeTickDelta(
                    employee.Id,
                    employee.CurrentActivity,
                    EmployeeActivityKind.Work,
                    FatigueDelta: Math.Round(4.0 * _options.TickHours, 4),
                    EnergyDelta: Math.Round(-3.0 * _options.TickHours, 4),
                    SatisfactionDelta: 0,
                    WorkOutput: output
                )
            );
            facilityDeltas.Add(
                new FacilityTickDelta(facility.Id, IsInUse: true, facility.OccupiedByEmployeeIds.Count, efficiency)
            );
        }

        projectProgressDelta = Math.Round(
            projectProgressDelta + playerCommandResults.Sum(result => result.ProjectProgressDelta),
            4
        );
        var productMarket = snapshot.Company.ProductMarket ?? CreateProductMarket(snapshot.Company);
        var nextProgress = Math.Min(
            snapshot.Company.ActiveProject.Progress + projectProgressDelta,
            snapshot.Company.ActiveProject.RequiredProgress
        );
        var commandActiveUsersDelta = playerCommandResults.Sum(result => result.ActiveUsersDelta);
        var launchHappened = playerCommandResults.Any(result =>
            result.Kind == PlayerCommandKind.PublishPrototype && result.ActiveUsersDelta > 0
        );
        var marketPerformanceDelta = CalculateMarketPerformanceDelta(
            productMarket,
            nextProgress,
            snapshot.Company.ActiveProject.RequiredProgress,
            salesOutput,
            serverMaintenanceOutput,
            launchHappened
        );
        var activeUsersDelta = commandActiveUsersDelta + marketPerformanceDelta.ActiveUsersDelta;
        var monthlyRecurringRevenueDelta = Math.Round(activeUsersDelta * MonthlyRevenuePerUser, 4);
        var nextMonthlyRecurringRevenue =
            productMarket.MonthlyRecurringRevenue + monthlyRecurringRevenueDelta;
        var revenueDelta = Math.Round(
            nextMonthlyRecurringRevenue / 30.0 / 8.0 * _options.TickHours,
            4
        );
        var nextStage = ResolveStage(
            nextProgress,
            snapshot.Company.ActiveProject.RequiredProgress,
            productMarket.ActiveUsers + activeUsersDelta
        );
        var operatingCostDelta = Math.Round(
            -snapshot.Company.MonthlyCostRate / 30.0 / 8.0 * _options.TickHours,
            4
        );
        var commandCashDelta = playerCommandResults.Sum(result => result.CashDelta);
        var cashDelta = Math.Round(operatingCostDelta + revenueDelta + commandCashDelta, 4);
        var productMarketDelta = new ProductMarketTickDelta(
            PreviousStage: productMarket.Stage,
            NextStage: nextStage,
            ActiveUsersDelta: activeUsersDelta,
            MonthlyRecurringRevenueDelta: monthlyRecurringRevenueDelta,
            UserRatingDelta: Math.Round(
                playerCommandResults.Sum(result => result.UserRatingDelta)
                    + marketPerformanceDelta.UserRatingDelta,
                4
            ),
            MarketAwarenessDelta: Math.Round(
                playerCommandResults.Sum(result => result.MarketAwarenessDelta)
                    + marketPerformanceDelta.MarketAwarenessDelta,
                4
            ),
            LaunchQualityDelta: Math.Round(
                playerCommandResults.Sum(result => result.LaunchQualityDelta),
                4
            ),
            RetentionDelta: Math.Round(
                playerCommandResults.Sum(result => result.RetentionDelta)
                    + marketPerformanceDelta.RetentionDelta,
                4
            ),
            Reasons: BuildProductMarketReasons(playerCommandResults, marketPerformanceDelta)
        );
        var monthlyReport = _options.IsMonthEnd
            ? CreateMonthlyReport(
                nextProgress,
                productMarket.ActiveUsers + activeUsersDelta,
                revenueDelta,
                Math.Round(snapshot.Company.Cash + cashDelta, 4),
                productMarketDelta
            )
            : null;

        return new TickResult(
            intents,
            employeeDeltas,
            facilityDeltas.OrderBy(delta => delta.FacilityId, StringComparer.Ordinal).ToArray(),
            new CompanyTickDelta(
                ProjectProgressDelta: projectProgressDelta,
                CashDelta: cashDelta,
                OperatingCostDelta: operatingCostDelta,
                RevenueDelta: revenueDelta
            ),
            productMarketDelta,
            monthlyReport,
            playerCommandResults
        );
    }

    private IReadOnlyList<PlayerCommandResult> ApplyPlayerCommands(OfficeRuleSnapshot snapshot)
    {
        var commands = _options.PlayerCommands ?? [];
        if (commands.Count == 0)
        {
            return [];
        }

        var results = new List<PlayerCommandResult>();
        var productMarket = snapshot.Company.ProductMarket ?? CreateProductMarket(snapshot.Company);
        foreach (var command in commands)
        {
            if (command.Kind == PlayerCommandKind.MarketResearch)
            {
                results.Add(CreateMarketResearchResult(productMarket));
                continue;
            }

            if (command.Kind == PlayerCommandKind.PublishPrototype)
            {
                results.Add(CreatePublishPrototypeResult(snapshot, productMarket));
            }
        }

        return results;
    }

    private static PlayerCommandResult CreateMarketResearchResult(ProductMarketState productMarket)
    {
        var isPrototype = productMarket.Stage == ProductStage.Prototype;
        return new PlayerCommandResult(
            PlayerCommandKind.MarketResearch,
            CashDelta: -MarketResearchCost,
            ProjectProgressDelta: isPrototype ? MarketResearchPrototypeProgress : 0.0,
            ActiveUsersDelta: isPrototype ? 0 : MarketResearchLaunchUsers,
            Message: isPrototype
                ? "市场调研完成：获得用户画像，MVP 方向更清晰。"
                : "市场调研完成：定位首批用户，市场转化提高。",
            MarketAwarenessDelta: isPrototype ? 0.18 : 0.12,
            UserRatingDelta: isPrototype ? 0 : 0.02
        );
    }

    private static PlayerCommandResult CreatePublishPrototypeResult(
        OfficeRuleSnapshot snapshot,
        ProductMarketState productMarket
    )
    {
        var project = snapshot.Company.ActiveProject;
        var isReady =
            productMarket.Stage != ProductStage.Prototype
            || project.Progress >= project.RequiredProgress;
        if (!isReady)
        {
            return new PlayerCommandResult(
                PlayerCommandKind.PublishPrototype,
                CashDelta: 0.0,
                ProjectProgressDelta: 0.0,
                ActiveUsersDelta: 0,
                Message: "发布失败：MVP 尚未完成，先让研发室继续推进原型。"
            );
        }

        return new PlayerCommandResult(
            PlayerCommandKind.PublishPrototype,
            CashDelta: -PublishPrototypeCost,
            ProjectProgressDelta: 0.0,
            ActiveUsersDelta: CalculateLaunchUsers(project, productMarket),
            Message: CreatePublishPrototypeMessage(project, productMarket),
            UserRatingDelta: CalculateLaunchRating(project, productMarket) - productMarket.UserRating,
            LaunchQualityDelta: CalculateLaunchQuality(project, productMarket)
                - productMarket.LaunchQuality,
            RetentionDelta: CalculateLaunchRetention(project, productMarket) - productMarket.Retention
        );
    }

    private static int CalculateLaunchUsers(ProjectState project, ProductMarketState productMarket)
    {
        var launchQuality = CalculateLaunchQuality(project, productMarket);
        return Math.Max(
            1,
            (int)Math.Round(3 + launchQuality * 8 + productMarket.MarketAwareness * 12)
        );
    }

    private static double CalculateLaunchQuality(ProjectState project, ProductMarketState productMarket)
    {
        var readiness = project.RequiredProgress <= 0
            ? 1.0
            : Clamp(project.Progress / project.RequiredProgress, 0, 1);
        return Math.Round(
            Clamp(0.35 + readiness * 0.45 + productMarket.MarketAwareness * 0.2, 0, 1),
            4
        );
    }

    private static double CalculateLaunchRating(ProjectState project, ProductMarketState productMarket)
    {
        var launchQuality = CalculateLaunchQuality(project, productMarket);
        return Math.Round(
            Clamp(2.6 + launchQuality * 1.1 + productMarket.MarketAwareness * 0.4, 1, 5),
            4
        );
    }

    private static double CalculateLaunchRetention(ProjectState project, ProductMarketState productMarket)
    {
        var launchQuality = CalculateLaunchQuality(project, productMarket);
        return Math.Round(
            Clamp(0.62 + launchQuality * 0.22 + productMarket.MarketAwareness * 0.1, 0, 1),
            4
        );
    }

    private static string CreatePublishPrototypeMessage(ProjectState project, ProductMarketState productMarket)
    {
        var users = CalculateLaunchUsers(project, productMarket);
        var rating = CalculateLaunchRating(project, productMarket);
        var quality = CalculateLaunchQuality(project, productMarket);
        return $"发布完成：发布质量 {quality:0.##}，用户评分 {rating:0.#}，首批用户 {users}。";
    }

    private static bool ContributesToSales(
        EmployeeState employee,
        FacilityState facility,
        RoomState? room
    )
    {
        return employee.Role == EmployeeRole.Marketing
            && room?.Type == RoomType.MarketRoom
            && facility.Type == FacilityType.ProductWhiteboard;
    }

    private static bool ContributesToServerMaintenance(
        EmployeeState employee,
        FacilityState facility,
        RoomState? room
    )
    {
        return employee.Role is EmployeeRole.Operations or EmployeeRole.Engineer
            && room?.Type == RoomType.ServerRoom
            && facility.Type == FacilityType.ServerRack;
    }

    private static int CalculateActiveUsersDelta(
        ProductMarketState productMarket,
        double nextProgress,
        double requiredProgress,
        double salesOutput
    )
    {
        if (nextProgress < requiredProgress || salesOutput <= 0)
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Floor(salesOutput * UsersPerSalesOutput));
    }

    private static ProductMarketPerformanceDelta CalculateMarketPerformanceDelta(
        ProductMarketState productMarket,
        double nextProgress,
        double requiredProgress,
        double salesOutput,
        double serverMaintenanceOutput,
        bool launchHappened
    )
    {
        if (productMarket.Stage == ProductStage.Launched && !launchHappened)
        {
            return CalculateLaunchedProductDelta(productMarket, salesOutput, serverMaintenanceOutput);
        }

        var users = CalculateActiveUsersDelta(
            productMarket,
            nextProgress,
            requiredProgress,
            salesOutput
        );
        var awareness = salesOutput > 0 ? Math.Round(Math.Min(0.08, salesOutput * 0.025), 4) : 0;
        var reasons = users > 0
            ? [$"市场工作带来用户增长 +{users}。"]
            : Array.Empty<string>();

        return new ProductMarketPerformanceDelta(
            ActiveUsersDelta: users,
            MarketAwarenessDelta: awareness,
            UserRatingDelta: 0,
            RetentionDelta: 0,
            Reasons: reasons
        );
    }

    private static ProductMarketPerformanceDelta CalculateLaunchedProductDelta(
        ProductMarketState productMarket,
        double salesOutput,
        double serverMaintenanceOutput
    )
    {
        var rating = productMarket.UserRating <= 0 ? 3.0 : productMarket.UserRating;
        var marketingGrowth = salesOutput <= 0
            ? 0
            : Math.Max(
                1,
                (int)Math.Floor(
                    salesOutput
                        * (1.4 + productMarket.MarketAwareness * 1.8)
                        * Clamp(rating / 4.0, 0.35, 1.25)
                )
            );
        var churn = rating < 2.6 && serverMaintenanceOutput <= 0
            ? Math.Max(1, (int)Math.Ceiling(productMarket.ActiveUsers * (2.7 - rating) * 0.05))
            : 0;
        var retentionDelta = serverMaintenanceOutput > 0
            ? Math.Min(0.035, serverMaintenanceOutput * 0.018)
            : rating < 2.6
                ? -0.03
                : 0;
        var ratingDelta = serverMaintenanceOutput > 0
            ? Math.Min(0.04, serverMaintenanceOutput * 0.012)
            : rating < 2.6
                ? -0.06
                : 0;
        var awarenessDelta = salesOutput > 0 ? Math.Min(0.05, salesOutput * 0.018) : 0;
        var reasons = new List<string>();

        if (marketingGrowth > 0)
        {
            reasons.Add($"用户增长 +{marketingGrowth}：市场工作和市场认知带来转化。");
        }
        else
        {
            reasons.Add("增长停滞：缺少市场工作或市场认知不足。");
        }

        if (churn > 0)
        {
            reasons.Add($"用户流失 -{churn}：差评和服务器维护不足拖累留存。");
        }

        if (serverMaintenanceOutput > 0)
        {
            reasons.Add("服务器维护保护评分和留存。");
        }

        return new ProductMarketPerformanceDelta(
            ActiveUsersDelta: marketingGrowth - churn,
            MarketAwarenessDelta: Math.Round(awarenessDelta, 4),
            UserRatingDelta: Math.Round(ratingDelta, 4),
            RetentionDelta: Math.Round(retentionDelta, 4),
            Reasons: reasons
        );
    }

    private static IReadOnlyList<string> BuildProductMarketReasons(
        IReadOnlyList<PlayerCommandResult> playerCommandResults,
        ProductMarketPerformanceDelta marketDelta
    )
    {
        var reasons = new List<string>();
        reasons.AddRange(playerCommandResults.Select(result => result.Message));
        reasons.AddRange(marketDelta.Reasons);
        return reasons;
    }

    private static ProductMarketState CreateProductMarket(CompanyState company)
    {
        return new ProductMarketState(
            ResolveStage(
                company.ActiveProject.Progress,
                company.ActiveProject.RequiredProgress,
                activeUsers: 0
            ),
            ActiveUsers: 0,
            MonthlyRecurringRevenue: 0
        );
    }

    private static bool ContributesToMvp(
        EmployeeState employee,
        FacilityState facility,
        RoomState? room
    )
    {
        if (room?.Type != RoomType.ResearchRoom)
        {
            return false;
        }

        return employee.Role is EmployeeRole.Engineer or EmployeeRole.Designer or EmployeeRole.Planner
            && facility.Type is FacilityType.OfficeDesk or FacilityType.ProductWhiteboard;
    }

    private static ProductStage ResolveStage(double progress, double requiredProgress, int activeUsers)
    {
        if (progress < requiredProgress)
        {
            return ProductStage.Prototype;
        }

        return activeUsers > 0 ? ProductStage.Launched : ProductStage.MvpReady;
    }

    private static MonthlyReport CreateMonthlyReport(
        double projectProgress,
        int activeUsers,
        double revenue,
        double cash,
        ProductMarketTickDelta productMarketDelta
    )
    {
        return new MonthlyReport(
            PeriodLabel: "A0.24 月报",
            ProjectProgress: Math.Round(projectProgress, 4),
            ActiveUsers: activeUsers,
            Revenue: revenue,
            Cash: cash,
            Reasons:
            [
                "MVP 已完成，市场工作可以转化为用户。",
                "活跃用户产生收入，经营成本按 tick 扣除。",
                "A0.28 起，发布质量、用户评分、市场认知和服务器维护会影响增长。",
                .. productMarketDelta.Reasons ?? [],
            ]
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

    private static double Clamp(double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    private sealed record ProductMarketPerformanceDelta(
        int ActiveUsersDelta,
        double MarketAwarenessDelta,
        double UserRatingDelta,
        double RetentionDelta,
        IReadOnlyList<string> Reasons
    );
}
