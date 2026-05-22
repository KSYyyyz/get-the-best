using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class SimulationFrontendContractTests
{
    public static void Run()
    {
        SimulationTickResultKeepsStableFrontendFields();
        GodotFactSourcesAreDocumentedInCoreContract();
        CompanyTotalsAreDocumentedForHudConsumption();
        EverySimulationEventKindHasPlayableSemantics();
        TickCadenceMatchesV026BridgeRecommendation();
        OfficeSimulationEngineAcceptsFrontendTickOptions();
        PresetSnapshotProducesMinimumFrontendAcceptanceEvents();
    }

    private static void SimulationTickResultKeepsStableFrontendFields()
    {
        var propertyNames = typeof(SimulationTickResult)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal("NextSnapshot,Outcome,PresentationEvents,Tick", string.Join(",", propertyNames));
    }

    private static void GodotFactSourcesAreDocumentedInCoreContract()
    {
        var fields = SimulationFrontendContract.ResultFields.ToDictionary(field => field.FieldName);

        Assert.Equal("现金、收入、用户、MVP 等经营 delta", fields["Tick"].GodotConsumption);
        Assert.Equal("员工位置/活动状态、设施占用的表现事实", fields["NextSnapshot"].GodotConsumption);
        Assert.Equal("经营阶段胜利或失败结果", fields["Outcome"].GodotConsumption);
        Assert.Equal("可播放提示和一次性表现事件", fields["PresentationEvents"].GodotConsumption);
    }

    private static void CompanyTotalsAreDocumentedForHudConsumption()
    {
        var totals = SimulationFrontendContract.CompanyTotals.ToDictionary(total => total.FieldName);

        Assert.Equal("当前总现金", totals["CurrentCash"].Meaning);
        Assert.Equal("当前 MVP 总进度", totals["CurrentProjectProgress"].Meaning);
        Assert.Equal("MVP 所需总进度", totals["ProjectRequiredProgress"].Meaning);
        Assert.Equal("当前活跃用户", totals["CurrentActiveUsers"].Meaning);
        Assert.Equal("当前月经常收入", totals["CurrentMonthlyRecurringRevenue"].Meaning);
        Assert.Equal("当前产品阶段", totals["ProductStage"].Meaning);
    }

    private static void EverySimulationEventKindHasPlayableSemantics()
    {
        var semantics = SimulationFrontendContract.EventSemantics.ToDictionary(item => item.Kind);
        var eventKinds = Enum.GetValues<SimulationEventKind>();

        Assert.Equal(eventKinds.Length, semantics.Count);
        foreach (var kind in eventKinds)
        {
            Assert.True(semantics.ContainsKey(kind), $"{kind} should have frontend semantics");
            Assert.True(
                !string.IsNullOrWhiteSpace(semantics[kind].SubjectIdMeaning),
                $"{kind} should document SubjectId"
            );
        }

        Assert.Equal(SimulationEventLifetime.Instant, semantics[SimulationEventKind.IntentPlanned].Lifetime);
        Assert.Equal(SimulationEventSubjectKind.Employee, semantics[SimulationEventKind.IntentPlanned].SubjectKind);
        Assert.Equal(TextDisplayPolicy.Optional, semantics[SimulationEventKind.IntentPlanned].TextDisplay);

        Assert.Equal(SimulationEventLifetime.StateChange, semantics[SimulationEventKind.ActivityChanged].Lifetime);
        Assert.Equal(SimulationEventSubjectKind.Employee, semantics[SimulationEventKind.ActivityChanged].SubjectKind);
        Assert.Equal(TextDisplayPolicy.None, semantics[SimulationEventKind.ActivityChanged].TextDisplay);

        Assert.Equal(SimulationEventLifetime.StateChange, semantics[SimulationEventKind.FacilityUpdated].Lifetime);
        Assert.Equal(SimulationEventSubjectKind.Facility, semantics[SimulationEventKind.FacilityUpdated].SubjectKind);

        Assert.Equal(SimulationEventSubjectKind.Metric, semantics[SimulationEventKind.MetricChanged].SubjectKind);
        Assert.Equal(SimulationEventSubjectKind.Report, semantics[SimulationEventKind.MonthlyReportReady].SubjectKind);
        Assert.Equal(SimulationEventSubjectKind.Outcome, semantics[SimulationEventKind.PhaseOutcomeReached].SubjectKind);
    }

    private static void TickCadenceMatchesV026BridgeRecommendation()
    {
        var cadence = SimulationFrontendContract.Cadence;

        Assert.Equal(2.0, cadence.RecommendedRealSecondsPerTick);
        Assert.Equal(1.0, cadence.DefaultTickHours);
        Assert.False(cadence.PauseDuringEmployeeWalkAnimation);
        Assert.False(cadence.UseMonthEndInV026Bridge);
    }

    private static void OfficeSimulationEngineAcceptsFrontendTickOptions()
    {
        var options = new SimulationTickOptions(
            SimulationFrontendContract.Cadence.DefaultTickHours,
            SimulationFrontendContract.Cadence.UseMonthEndInV026Bridge
        );
        var result = new OfficeSimulationEngine(options).Advance(
            TestSnapshots.FirstLoopEngineerMovingToDesk(projectProgress: 99)
        );

        Assert.True(result.PresentationEvents.Count > 0);
    }

    private static void PresetSnapshotProducesMinimumFrontendAcceptanceEvents()
    {
        var snapshot = TestSnapshots.FirstLoopEngineerMovingToDesk(projectProgress: 99);

        var result = new OfficeSimulationEngine().Advance(snapshot);

        Assert.True(
            result.PresentationEvents.Any(evt => evt.Kind == SimulationEventKind.IntentPlanned),
            "preset snapshot should produce one employee intent event"
        );
        Assert.True(
            result.PresentationEvents.Any(evt => evt.Kind == SimulationEventKind.ActivityChanged),
            "preset snapshot should produce one employee activity change"
        );
        Assert.True(
            result.PresentationEvents.Any(evt => evt.Kind == SimulationEventKind.FacilityUpdated),
            "preset snapshot may expose facility occupancy changes when a facility is used"
        );
        var followUp = new OfficeSimulationEngine().Advance(result.NextSnapshot);
        Assert.True(
            followUp.Tick.EmployeeDeltas.Count > 0,
            "NextSnapshot should be valid input for the next Advance call"
        );
    }
}
