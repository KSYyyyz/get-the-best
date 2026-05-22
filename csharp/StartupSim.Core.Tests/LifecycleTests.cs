using StartupSim.Core;

namespace StartupSim.Core.Tests;

public static class LifecycleTests
{
    public static void Run()
    {
        MoveIntentStartsMoveBeforeFacilityUse();
        MovingEmployeeAcquiresFacilityOnNextTick();
        UseLifecycleReleasesFacilityWhenTimerEnds();
        ConflictingMoveIntentsReserveFacilityDeterministically();
    }

    private static void MoveIntentStartsMoveBeforeFacilityUse()
    {
        var snapshot = TestSnapshots.SingleEngineerWithTwoFacilities();
        var intent = new EmployeeIntent(
            "employee-1",
            EmployeeIntentKind.MoveToFacility,
            new IntentTarget(FacilityId: "desk-1", RoomId: "research-room")
        );

        var next = new EmployeeLifecycleEngine(useDurationTicks: 2).Advance(snapshot, [intent]);

        Assert.Equal(EmployeeActivityKind.MoveToFacility, next.Employees[0].CurrentActivity);
        Assert.Equal("desk-1", next.Employees[0].ActiveFacilityId);
        Assert.Equal(1, next.Employees[0].RemainingActivityTicks);
        Assert.Equal(0, next.Facilities[0].OccupiedByEmployeeIds.Count);
    }

    private static void MovingEmployeeAcquiresFacilityOnNextTick()
    {
        var moving = TestSnapshots.SingleEngineerWithTwoFacilities() with
        {
            Employees =
            [
                TestSnapshots.SingleEngineerWithTwoFacilities().Employees[0] with
                {
                    CurrentActivity = EmployeeActivityKind.MoveToFacility,
                    ActiveFacilityId = "desk-1",
                    RemainingActivityTicks = 1,
                },
            ],
        };

        var next = new EmployeeLifecycleEngine(useDurationTicks: 2).Advance(moving, []);

        Assert.Equal(EmployeeActivityKind.UseFacility, next.Employees[0].CurrentActivity);
        Assert.Equal("desk-1", next.Employees[0].ActiveFacilityId);
        Assert.Equal(2, next.Employees[0].RemainingActivityTicks);
        Assert.Equal("employee-1", next.Facilities[0].OccupiedByEmployeeIds[0]);
    }

    private static void UseLifecycleReleasesFacilityWhenTimerEnds()
    {
        var usingFacility = TestSnapshots.SingleEngineerUsingDesk(fatigue: 20) with
        {
            Employees =
            [
                TestSnapshots.SingleEngineerUsingDesk(fatigue: 20).Employees[0] with
                {
                    ActiveFacilityId = "desk-1",
                    RemainingActivityTicks = 1,
                },
            ],
        };

        var next = new EmployeeLifecycleEngine(useDurationTicks: 2).Advance(usingFacility, []);

        Assert.Equal(EmployeeActivityKind.Idle, next.Employees[0].CurrentActivity);
        Assert.Equal(null, next.Employees[0].ActiveFacilityId);
        Assert.Equal(0, next.Employees[0].RemainingActivityTicks);
        Assert.Equal(0, next.Facilities[0].OccupiedByEmployeeIds.Count);
    }

    private static void ConflictingMoveIntentsReserveFacilityDeterministically()
    {
        var snapshot = TestSnapshots.TwoEngineersOneDesk();
        var intents = new[]
        {
            new EmployeeIntent(
                "employee-2",
                EmployeeIntentKind.MoveToFacility,
                new IntentTarget(FacilityId: "desk-1", RoomId: "research-room")
            ),
            new EmployeeIntent(
                "employee-1",
                EmployeeIntentKind.MoveToFacility,
                new IntentTarget(FacilityId: "desk-1", RoomId: "research-room")
            ),
        };

        var next = new EmployeeLifecycleEngine(useDurationTicks: 2).Advance(snapshot, intents);

        Assert.Equal(EmployeeActivityKind.MoveToFacility, next.Employees[0].CurrentActivity);
        Assert.Equal(EmployeeActivityKind.Idle, next.Employees[1].CurrentActivity);
        Assert.Equal("desk-1", next.Employees[0].ActiveFacilityId);
        Assert.Equal(null, next.Employees[1].ActiveFacilityId);
    }
}
