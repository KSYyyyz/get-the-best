namespace StartupSim.Core;

public sealed class OfficeSimulationEngine
{
    private readonly SimulationTickOptions _options;
    private readonly EmployeeBehaviorEngine _behaviorEngine;
    private readonly EmployeeLifecycleEngine _lifecycleEngine;
    private readonly OfficeStateReducer _stateReducer;

    public OfficeSimulationEngine()
        : this(
            SimulationTickOptions.Default,
            new EmployeeBehaviorEngine(),
            new EmployeeLifecycleEngine(),
            new OfficeStateReducer()
        ) { }

    public OfficeSimulationEngine(
        SimulationTickOptions options,
        EmployeeBehaviorEngine behaviorEngine,
        EmployeeLifecycleEngine lifecycleEngine,
        OfficeStateReducer stateReducer
    )
    {
        _options = options;
        _behaviorEngine = behaviorEngine;
        _lifecycleEngine = lifecycleEngine;
        _stateReducer = stateReducer;
    }

    public SimulationTickResult Advance(OfficeRuleSnapshot snapshot)
    {
        var intents = _behaviorEngine.PlanIntents(snapshot);
        var lifecycleSnapshot = _lifecycleEngine.Advance(snapshot, intents);
        var businessEngine = new FirstLoopBusinessEngine(
            new FirstLoopBusinessTickOptions(_options.TickHours, _options.IsMonthEnd),
            _behaviorEngine
        );
        var businessTick = businessEngine.Tick(lifecycleSnapshot);
        var tick = businessTick with { Intents = intents };
        var nextSnapshot = _stateReducer.ApplyTickResult(lifecycleSnapshot, tick);
        var outcome = ResolveOutcome(snapshot, nextSnapshot, tick);
        var presentationEvents = BuildPresentationEvents(snapshot, lifecycleSnapshot, nextSnapshot, tick, outcome);

        return new SimulationTickResult(tick, nextSnapshot, outcome, presentationEvents);
    }

    private static PhaseOutcome ResolveOutcome(
        OfficeRuleSnapshot previous,
        OfficeRuleSnapshot next,
        TickResult tick
    )
    {
        if (next.Company.Cash <= 0)
        {
            return new PhaseOutcome(
                PhaseOutcomeKind.FailedCashDepleted,
                ["现金已经耗尽，第一局经营失败。"]
            );
        }

        if (tick.ProductMarketDelta?.ActiveUsersDelta > 0)
        {
            return new PhaseOutcome(
                PhaseOutcomeKind.FirstUsersAcquired,
                ["获得首批活跃用户。"]
            );
        }

        if (tick.CompanyDelta.RevenueDelta > 0 && tick.CompanyDelta.CashDelta > 0)
        {
            return new PhaseOutcome(
                PhaseOutcomeKind.RevenuePositive,
                ["收入已经覆盖本 tick 经营成本。"]
            );
        }

        if (
            previous.Company.ProductMarket?.Stage == ProductStage.Prototype
            && next.Company.ProductMarket?.Stage == ProductStage.MvpReady
        )
        {
            return new PhaseOutcome(PhaseOutcomeKind.MvpCompleted, ["MVP 已完成。"]);
        }

        return PhaseOutcome.InProgress;
    }

    private static IReadOnlyList<SimulationPresentationEvent> BuildPresentationEvents(
        OfficeRuleSnapshot previous,
        OfficeRuleSnapshot lifecycleSnapshot,
        OfficeRuleSnapshot next,
        TickResult tick,
        PhaseOutcome outcome
    )
    {
        var events = new List<SimulationPresentationEvent>();

        foreach (var intent in tick.Intents.OrderBy(intent => intent.EmployeeId, StringComparer.Ordinal))
        {
            events.Add(
                new SimulationPresentationEvent(
                    SimulationEventKind.IntentPlanned,
                    intent.EmployeeId,
                    intent.Kind.ToString()
                )
            );
        }

        var previousEmployees = previous.Employees.ToDictionary(employee => employee.Id);
        foreach (var employee in lifecycleSnapshot.Employees.OrderBy(employee => employee.Id, StringComparer.Ordinal))
        {
            if (
                previousEmployees.TryGetValue(employee.Id, out var previousEmployee)
                && previousEmployee.CurrentActivity != employee.CurrentActivity
            )
            {
                events.Add(
                    new SimulationPresentationEvent(
                        SimulationEventKind.ActivityChanged,
                        employee.Id,
                        employee.CurrentActivity.ToString()
                    )
                );
            }
        }

        foreach (var facilityDelta in tick.FacilityDeltas.OrderBy(delta => delta.FacilityId, StringComparer.Ordinal))
        {
            events.Add(
                new SimulationPresentationEvent(
                    SimulationEventKind.FacilityUpdated,
                    facilityDelta.FacilityId,
                    facilityDelta.CurrentOccupancy.ToString()
                )
            );
        }

        if (tick.CompanyDelta.ProjectProgressDelta != 0 || tick.CompanyDelta.CashDelta != 0)
        {
            events.Add(
                new SimulationPresentationEvent(
                    SimulationEventKind.MetricChanged,
                    next.Company.ActiveProject.Id,
                    next.Company.ActiveProject.Progress.ToString("0.####")
                )
            );
        }

        if (tick.MonthlyReport != null)
        {
            events.Add(
                new SimulationPresentationEvent(
                    SimulationEventKind.MonthlyReportReady,
                    tick.MonthlyReport.PeriodLabel,
                    tick.MonthlyReport.Cash.ToString("0.####")
                )
            );
        }

        if (outcome.Kind != PhaseOutcomeKind.InProgress)
        {
            events.Add(
                new SimulationPresentationEvent(
                    SimulationEventKind.PhaseOutcomeReached,
                    outcome.Kind.ToString(),
                    outcome.Reasons[0]
                )
            );
        }

        return events;
    }
}
