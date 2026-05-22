using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class RestRecoveryTests
{
    public static void Run()
    {
        HighFatigueEmployeeTargetsRestFacility();
        RestTickRecoversFatigueAndEnergy();
        RestFacilityAndRoomImproveRecovery();
        RestTickReportsRestFacilityUse();
    }

    private static void HighFatigueEmployeeTargetsRestFacility()
    {
        var snapshot = TestSnapshots.HighFatigueEngineerWithRestSeat();
        var intents = new EmployeeBehaviorEngine().PlanIntents(snapshot);

        Assert.Equal(EmployeeIntentKind.Rest, intents[0].Kind);
        Assert.Equal("rest-seat-1", intents[0].Target.FacilityId);
        Assert.Equal("rest-room", intents[0].Target.RoomId);
    }

    private static void RestTickRecoversFatigueAndEnergy()
    {
        var result = new BusinessTickEngine().Tick(TestSnapshots.EngineerResting(fatigue: 90));
        var employeeDelta = result.EmployeeDeltas[0];

        Assert.Equal(EmployeeActivityKind.Rest, employeeDelta.NextActivity);
        Assert.True(employeeDelta.FatigueDelta < 0);
        Assert.True(employeeDelta.EnergyDelta > 0);
        Assert.Equal(0.0, employeeDelta.WorkOutput);
    }

    private static void RestFacilityAndRoomImproveRecovery()
    {
        var plain = new BusinessTickEngine().Tick(TestSnapshots.EngineerResting(fatigue: 90, useRestSeat: false));
        var boosted = new BusinessTickEngine().Tick(TestSnapshots.EngineerResting(fatigue: 90, useRestSeat: true));

        Assert.True(
            Math.Abs(boosted.EmployeeDeltas[0].FatigueDelta)
                > Math.Abs(plain.EmployeeDeltas[0].FatigueDelta)
        );
        Assert.True(boosted.EmployeeDeltas[0].EnergyDelta > plain.EmployeeDeltas[0].EnergyDelta);
    }

    private static void RestTickReportsRestFacilityUse()
    {
        var result = new BusinessTickEngine().Tick(TestSnapshots.EngineerResting(fatigue: 90));

        Assert.Equal("rest-seat-1", result.FacilityDeltas[0].FacilityId);
        Assert.Equal(true, result.FacilityDeltas[0].IsInUse);
        Assert.Equal(1, result.FacilityDeltas[0].CurrentOccupancy);
    }
}
