using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class EmployeeUtilityBehaviorTests
{
    public static void Run()
    {
        EngineerPrioritizesDeskWorkForMvpWhenDeskIsAvailable();
        UnusableDeskDoesNotProduceMvpProgress();
        HighFatigueRaisesRestCandidateScore();
        RestIntentStartsRestFacilityUse();
        OccupiedFacilityIsNotAssignedToSecondEmployee();
        AdvanceEmitsPlayableUtilityExplanationEvents();
    }

    private static void EngineerPrioritizesDeskWorkForMvpWhenDeskIsAvailable()
    {
        var plan = new EmployeeBehaviorEngine().PlanDecisions(
            TestSnapshots.SingleEngineerWithTwoFacilities()
        );
        var intent = plan.Intents.Single();
        var explanation = plan.Explanations.Single();
        var deskCandidate = explanation.Candidates.Single(candidate =>
            candidate.Kind == EmployeeActionCandidateKind.WorkAtDesk
        );

        Assert.Equal(EmployeeIntentKind.MoveToFacility, intent.Kind);
        Assert.Equal("desk-1", intent.Target.FacilityId);
        Assert.Equal(EmployeeActionCandidateKind.WorkAtDesk, explanation.SelectedAction);
        Assert.True(deskCandidate.Score > 0.75, "desk work should score high for MVP");
        Assert.True(
            deskCandidate.Reasons.Any(reason => reason.Contains("MVP", StringComparison.Ordinal)),
            "desk work explanation should mention MVP"
        );
    }

    private static void UnusableDeskDoesNotProduceMvpProgress()
    {
        var snapshot = TestSnapshots.EngineerWithOnlyFullDesk();
        var result = new OfficeSimulationEngine().Advance(snapshot);

        Assert.Equal(0.0, result.Tick.CompanyDelta.ProjectProgressDelta);
        Assert.False(
            result.NextSnapshot.Facilities[0].OccupiedByEmployeeIds.Contains("employee-1"),
            "employee should not occupy a full desk"
        );
    }

    private static void HighFatigueRaisesRestCandidateScore()
    {
        var plan = new EmployeeBehaviorEngine().PlanDecisions(
            TestSnapshots.HighFatigueEngineerWithRestSeat()
        );
        var explanation = plan.Explanations.Single();
        var rest = explanation.Candidates.Single(candidate =>
            candidate.Kind == EmployeeActionCandidateKind.Rest
        );
        var work = explanation.Candidates.Single(candidate =>
            candidate.Kind == EmployeeActionCandidateKind.WorkAtDesk
        );

        Assert.Equal(EmployeeActionCandidateKind.Rest, explanation.SelectedAction);
        Assert.True(rest.Score > work.Score, "rest score should exceed work score when fatigue is high");
        Assert.True(
            rest.Reasons.Any(reason => reason.Contains("疲劳", StringComparison.Ordinal)),
            "rest explanation should mention fatigue pressure"
        );
    }

    private static void RestIntentStartsRestFacilityUse()
    {
        var result = new OfficeSimulationEngine().Advance(
            TestSnapshots.HighFatigueEngineerWithRestSeat()
        );
        var employee = result.NextSnapshot.Employees.Single();
        var restFacility = result.NextSnapshot.Facilities.Single(facility =>
            facility.Id == "rest-seat-1"
        );

        Assert.Equal(EmployeeActivityKind.Rest, employee.CurrentActivity);
        Assert.Equal("rest-seat-1", employee.ActiveFacilityId);
        Assert.True(
            restFacility.OccupiedByEmployeeIds.Contains("employee-1"),
            "rest facility should be occupied by the resting employee"
        );
    }

    private static void OccupiedFacilityIsNotAssignedToSecondEmployee()
    {
        var plan = new EmployeeBehaviorEngine().PlanDecisions(TestSnapshots.TwoEngineersOneDesk());
        var firstIntent = plan.Intents.Single(intent => intent.EmployeeId == "employee-1");
        var secondIntent = plan.Intents.Single(intent => intent.EmployeeId == "employee-2");
        var secondExplanation = plan.Explanations.Single(explanation =>
            explanation.EmployeeId == "employee-2"
        );

        Assert.Equal("desk-1", firstIntent.Target.FacilityId);
        Assert.False(
            secondIntent.Target.FacilityId == "desk-1",
            "second employee should not reserve the same desk"
        );
        Assert.True(
            secondExplanation.Candidates.Any(candidate =>
                candidate.RejectionReasons.Any(reason =>
                    reason.Contains("已被占用", StringComparison.Ordinal)
                    || reason.Contains("已被预留", StringComparison.Ordinal)
                )
            ),
            "second employee should explain why the desk was rejected"
        );
    }

    private static void AdvanceEmitsPlayableUtilityExplanationEvents()
    {
        var result = new OfficeSimulationEngine().Advance(TestSnapshots.MixedIntentAndUsingDesk());
        var messages = result.PresentationEvents.Select(evt => evt.Message).ToArray();

        Assert.True(
            result.Tick.Intents.Any(intent => intent.Explanation != null),
            "simulation intents should expose decision explanations"
        );
        Assert.True(
            messages.Any(message => message.Contains("前往设施", StringComparison.Ordinal)),
            "presentation events should be able to drive move prompts"
        );
        Assert.True(
            messages.Any(message => message.Contains("正在使用", StringComparison.Ordinal)),
            "presentation events should be able to drive use prompts"
        );
        Assert.True(
            messages.Any(message => message.Contains("指标变化", StringComparison.Ordinal)),
            "presentation events should be able to drive metric prompts"
        );
        Assert.True(
            messages.Any(message => message.Contains("原因摘要", StringComparison.Ordinal)),
            "presentation events should expose a concise reason summary"
        );
    }
}
