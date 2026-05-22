namespace StartupSim.Core;

public sealed class FirstLoopBusinessEngine
{
    private const double UsersPerSalesOutput = 2.0;
    private const double MonthlyRevenuePerUser = 12.0;

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

        foreach (var employee in snapshot.Employees.OrderBy(employee => employee.Id, StringComparer.Ordinal))
        {
            var facility = FindFacilityUsedBy(snapshot, employee.Id);
            if (facility == null || employee.CurrentActivity != EmployeeActivityKind.UseFacility)
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

        projectProgressDelta = Math.Round(projectProgressDelta, 4);
        var productMarket = snapshot.Company.ProductMarket ?? CreateProductMarket(snapshot.Company);
        var nextProgress = Math.Min(
            snapshot.Company.ActiveProject.Progress + projectProgressDelta,
            snapshot.Company.ActiveProject.RequiredProgress
        );
        var activeUsersDelta = CalculateActiveUsersDelta(
            productMarket,
            nextProgress,
            snapshot.Company.ActiveProject.RequiredProgress,
            salesOutput
        );
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
        var cashDelta = Math.Round(operatingCostDelta + revenueDelta, 4);
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
            monthlyReport
        );
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
