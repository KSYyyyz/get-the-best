using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class EmployeeBehaviorTickTests
{
    public static void Run()
    {
        EngineerChoosesOfficeDesk();
        FullFacilityCapacityIsNotAssignedAgain();
        FacilityUseAdvancesProjectProgress();
        FatigueReducesEffectiveOutput();
        TickIsDeterministic();
        CoreAssemblyDoesNotReferenceGodot();
    }

    private static void EngineerChoosesOfficeDesk()
    {
        var snapshot = TestSnapshots.SingleEngineerWithTwoFacilities();
        var intents = new EmployeeBehaviorEngine().PlanIntents(snapshot);

        Assert.Equal(1, intents.Count, "one employee should receive one intent");
        Assert.Equal(EmployeeIntentKind.MoveToFacility, intents[0].Kind, "engineer should move to a facility");
        Assert.Equal("desk-1", intents[0].Target.FacilityId, "engineer should choose an office desk");
    }

    private static void FullFacilityCapacityIsNotAssignedAgain()
    {
        var snapshot = TestSnapshots.SingleEngineerWithTwoFacilities(
            deskOccupants: ["existing-engineer"]
        );
        var intents = new EmployeeBehaviorEngine().PlanIntents(snapshot);

        Assert.Equal("desk-2", intents[0].Target.FacilityId, "full desk should be skipped");
    }

    private static void FacilityUseAdvancesProjectProgress()
    {
        var snapshot = TestSnapshots.SingleEngineerUsingDesk(fatigue: 20);
        var result = new BusinessTickEngine().Tick(snapshot);

        Assert.True(result.CompanyDelta.ProjectProgressDelta > 0, "work tick should advance project");
        Assert.True(result.EmployeeDeltas[0].FatigueDelta > 0, "work tick should add fatigue");
        Assert.Equal("desk-1", result.FacilityDeltas[0].FacilityId, "used facility should be reported");
    }

    private static void FatigueReducesEffectiveOutput()
    {
        var fresh = new BusinessTickEngine().Tick(TestSnapshots.SingleEngineerUsingDesk(fatigue: 10));
        var tired = new BusinessTickEngine().Tick(TestSnapshots.SingleEngineerUsingDesk(fatigue: 85));

        Assert.True(
            fresh.CompanyDelta.ProjectProgressDelta > tired.CompanyDelta.ProjectProgressDelta,
            "high fatigue should reduce progress output"
        );
    }

    private static void TickIsDeterministic()
    {
        var snapshot = TestSnapshots.SingleEngineerUsingDesk(fatigue: 40);
        var first = new BusinessTickEngine().Tick(snapshot);
        var second = new BusinessTickEngine().Tick(snapshot);

        Assert.Equal(first.CompanyDelta.ProjectProgressDelta, second.CompanyDelta.ProjectProgressDelta);
        Assert.Equal(first.EmployeeDeltas[0].FatigueDelta, second.EmployeeDeltas[0].FatigueDelta);
        Assert.Equal(first.FacilityDeltas[0].EfficiencyMultiplier, second.FacilityDeltas[0].EfficiencyMultiplier);
    }

    private static void CoreAssemblyDoesNotReferenceGodot()
    {
        var referencedAssemblies = typeof(BusinessTickEngine)
            .Assembly.GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.False(referencedAssemblies.Contains("GodotSharp"), "Core must not reference GodotSharp");
        Assert.False(referencedAssemblies.Contains("GodotSharpEditor"), "Core must not reference GodotSharpEditor");
        Assert.False(referencedAssemblies.Contains("Godot"), "Core must not reference Godot");
    }
}
