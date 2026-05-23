namespace StartupSim.Core;

public sealed class FirstLoopBusinessEngine
{
    private const double UsersPerSalesOutput = 2.0;
    private const double MonthlyRevenuePerUser = 12.0;
    private const double MarketResearchCost = 500.0;
    private const double MarketResearchPrototypeProgress = 2.0;
    private const int MarketResearchLaunchUsers = 5;

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
        var activeUsersDelta =
            CalculateActiveUsersDelta(
                productMarket,
                nextProgress,
                snapshot.Company.ActiveProject.RequiredProgress,
                salesOutput
            ) + playerCommandResults.Sum(result => result.ActiveUsersDelta);
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
        var monthlyReport = _options.IsMonthEnd
            ? CreateMonthlyReport(
                nextProgress,
                productMarket.ActiveUsers + activeUsersDelta,
                revenueDelta,
                Math.Round(snapshot.Company.Cash + cashDelta, 4)
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
            new ProductMarketTickDelta(
                PreviousStage: productMarket.Stage,
                NextStage: nextStage,
                ActiveUsersDelta: activeUsersDelta,
                MonthlyRecurringRevenueDelta: monthlyRecurringRevenueDelta
            ),
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
            if (command.Kind != PlayerCommandKind.MarketResearch)
            {
                continue;
            }

            var isPrototype = productMarket.Stage == ProductStage.Prototype;
            results.Add(
                new PlayerCommandResult(
                    PlayerCommandKind.MarketResearch,
                    CashDelta: -MarketResearchCost,
                    ProjectProgressDelta: isPrototype ? MarketResearchPrototypeProgress : 0.0,
                    ActiveUsersDelta: isPrototype ? 0 : MarketResearchLaunchUsers,
                    Message: isPrototype
                        ? "市场调研完成：获得用户画像，MVP 方向更清晰。"
                        : "市场调研完成：定位首批用户，市场转化提高。"
                )
            );
        }

        return results;
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
        double cash
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
}
