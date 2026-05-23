using System.Collections.Generic;
using System.Linq;
using Godot;
using StartupSim.Core;

namespace GetTheBestGodot;

public partial class EmployeeAutonomyController : Node
{
    private const float AutonomousMoveIntervalSeconds = 2.4f;
    private static readonly float CoreSimulationTickSeconds =
        (float)V2CoreBridge.CoreSimulationRealSecondsPerTick;
    private const int MaxAutonomousPathCells = 4;
    private static readonly Vector2I[] CandidateTargetOffsets =
    [
        new(-1, 0),
        new(1, 0),
        new(0, 1),
        new(0, -1),
        new(-2, 0),
        new(2, 0),
        new(0, 2),
        new(0, -2),
        new(-1, 1),
        new(1, 1),
        new(-1, -1),
        new(1, -1),
    ];
    private static readonly Vector2I[] FacilityInteractionOffsets =
    [
        Vector2I.Down,
        Vector2I.Up,
        Vector2I.Left,
        Vector2I.Right,
    ];

    private readonly Dictionary<int, EmployeeAutonomyState> _employeeStates = [];
    private readonly HashSet<int> _reservedFacilityIds = [];
    private EmployeeStore? _employeeStore;
    private Employee3DRenderer? _employeeRenderer;
    private FacilityPlacementStore? _facilityPlacementStore;
    private Facility3DRenderer? _facilityRenderer;
    private RoomFootprintStore? _roomFootprintStore;
    private OfficeNavigationStore? _officeNavigationStore;
    private V2CoreBridge? _v2CoreBridge;
    private BusinessFeedbackHudController? _businessFeedbackHud;
    private CoreSummaryHudController? _coreSummaryHud;
    private BusinessCalendarHudController? _businessCalendarHud;
    private MonthlyReportHudController? _monthlyReportHud;
    private TimeScaleHudController? _timeScaleHud;
    private CoreOfficeSimulationResult? _pendingCoreIntentResult;
    private float _autonomyTimer = AutonomousMoveIntervalSeconds;
    private float _coreSimulationTimer = CoreSimulationTickSeconds;
    private float _simulationTimeScale = 1.0f;
    private int _nextEmployeeIndex;
    private bool _isEmployeeMoveInProgress;

    public override void _Ready()
    {
        _employeeStore = GetNodeOrNull<EmployeeStore>("../EmployeeStore");
        _employeeRenderer = GetNodeOrNull<Employee3DRenderer>("../Employee3DRenderer");
        _facilityPlacementStore = GetNodeOrNull<FacilityPlacementStore>("../FacilityPlacementStore");
        _facilityRenderer = GetNodeOrNull<Facility3DRenderer>("../Facility3DRenderer");
        _roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");
        _officeNavigationStore = GetNodeOrNull<OfficeNavigationStore>("../OfficeNavigationStore");
        _v2CoreBridge = GetNodeOrNull<V2CoreBridge>("../../V2CoreBridge");
        _businessFeedbackHud = GetNodeOrNull<BusinessFeedbackHudController>("../../HudRoot/BusinessFeedbackPanel");
        _coreSummaryHud = GetNodeOrNull<CoreSummaryHudController>("../../HudRoot/CoreSummaryPanel");
        _businessCalendarHud = GetNodeOrNull<BusinessCalendarHudController>("../../HudRoot/BusinessCalendarPanel");
        _monthlyReportHud = GetNodeOrNull<MonthlyReportHudController>("../../HudRoot/MonthlyReportPanel");
        _timeScaleHud = GetNodeOrNull<TimeScaleHudController>("../../HudRoot/TimeScalePanel");
        InitializeEmployeeStates();
    }

    public override void _Process(double delta)
    {
        var scaledDelta = (float)delta * _simulationTimeScale;
        if (scaledDelta <= 0.0f)
        {
            return;
        }

        if (_isEmployeeMoveInProgress)
        {
            return;
        }

        UpdateCoreSimulation(scaledDelta);
        _autonomyTimer -= scaledDelta;
        if (_autonomyTimer > 0.0f)
        {
            return;
        }

        _autonomyTimer = AutonomousMoveIntervalSeconds;
        TryStartNextAutonomousMove();
    }

    public void SetSimulationTimeScale(float timeScale)
    {
        _simulationTimeScale = Mathf.Clamp(timeScale, 0.0f, 3.0f);
        _employeeRenderer?.SetPresentationTimeScale(_simulationTimeScale);
    }

    public void ClearEmployeePresentationState(int employeeId)
    {
        if (
            _employeeStates.TryGetValue(employeeId, out var currentState)
            && currentState.FacilityId != null
        )
        {
            _facilityRenderer?.SetFacilityUseState(currentState.FacilityId.Value, false);
            _reservedFacilityIds.Remove(currentState.FacilityId.Value);
        }

        ClearEmployeeActivity(employeeId);
    }

    public void StartManualFacilityWork(int employeeId, FacilityPlacement facility)
    {
        _reservedFacilityIds.Add(facility.Id);
        SetEmployeeActivity(
            employeeId,
            EmployeeActivityKind.UsingFacility,
            facilityId: facility.Id,
            labelOverride: GetFacilityUseActivityLabel(facility)
        );
        _facilityRenderer?.SetFacilityUseState(facility.Id, true);
        _employeeRenderer?.SetEmployeeAnimationState(
            employeeId,
            GetFacilityUseAnimationState(facility)
        );
        if (facility.FacilityType == FacilityBuildType.OfficeDesk)
        {
            _employeeRenderer?.SetEmployeeWorkState(employeeId, isWorking: true, facility);
        }
        else
        {
            _employeeRenderer?.SetEmployeeWorkState(employeeId, isWorking: false);
        }
    }

    private void InitializeEmployeeStates()
    {
        if (_employeeStore == null)
        {
            return;
        }

        foreach (var employee in _employeeStore.GetEmployees())
        {
            SetEmployeeActivity(employee.Id, EmployeeActivityKind.Idle);
        }
    }

    private bool TryStartNextAutonomousMove()
    {
        if (_employeeStore == null || _employeeRenderer == null || _officeNavigationStore == null)
        {
            return false;
        }

        var employees = _employeeStore.GetEmployees();
        if (employees.Count == 0)
        {
            return false;
        }

        var isReplayingPendingCoreIntents = _pendingCoreIntentResult != null;
        var simulationResult = _pendingCoreIntentResult ?? AdvanceCoreSimulation();
        ApplyCoreSimulationStates(simulationResult);

        for (var attempt = 0; attempt < employees.Count; attempt++)
        {
            var employee = employees[_nextEmployeeIndex % employees.Count];
            _nextEmployeeIndex = (_nextEmployeeIndex + 1) % employees.Count;
            if (!IsEmployeeIdle(employee.Id))
            {
                continue;
            }

            if (TryStartFacilityUseBehavior(employee, simulationResult))
            {
                _pendingCoreIntentResult = simulationResult;
                return true;
            }

            if (!FindAutonomousTarget(employee, out var targetCell, out var path))
            {
                continue;
            }

            _isEmployeeMoveInProgress = true;
            SetEmployeeActivity(employee.Id, EmployeeActivityKind.WalkingToTarget, targetCell);
            _employeeRenderer?.SetEmployeeAnimationState(
                employee.Id,
                EmployeePresentationAnimationState.Walking
            );
            _employeeRenderer?.PlayEmployeePathMove(employee, path, () =>
                FinishAutonomousMove(employee.Id, targetCell)
            );
            return true;
        }

        if (isReplayingPendingCoreIntents)
        {
            _pendingCoreIntentResult = null;
        }

        return false;
    }

    private bool IsEmployeeIdle(int employeeId)
    {
        return !_employeeStates.TryGetValue(employeeId, out var state)
            || state.ActivityKind == EmployeeActivityKind.Idle;
    }

    private bool TryStartFacilityUseBehavior(
        EmployeeVisual employee,
        CoreOfficeSimulationResult? simulationResult
    )
    {
        if (simulationResult == null)
        {
            return false;
        }

        foreach (var coreIntent in simulationResult.Intents)
        {
            if (
                coreIntent.EmployeeId != employee.Id
                || coreIntent.Kind != StartupSim.Core.EmployeeIntentKind.MoveToFacility
                || coreIntent.FacilityId == null
                || !FindFacilityUseTarget(employee, coreIntent.FacilityId, coreIntent.SourceAction, out var target)
            )
            {
                continue;
            }

            StartFacilityMove(employee, target, coreIntent, BuildCoreIntentActivityLabel(coreIntent));
            return true;
        }

        return false;
    }

    private void StartFacilityMove(
        EmployeeVisual employee,
        FacilityInteractionTarget target,
        CoreEmployeeIntent coreIntent,
        string? activityLabel = null
    )
    {
        _reservedFacilityIds.Add(target.Facility.Id);
        _isEmployeeMoveInProgress = true;
        SetEmployeeActivity(
            employee.Id,
            EmployeeActivityKind.WalkingToFacility,
            target.StandCell,
            target.Facility.Id,
            activityLabel
        );
        _employeeRenderer?.SetEmployeeAnimationState(
            employee.Id,
            GetAnimationStateForCoreIntent(coreIntent.SourceAction)
        );
        _employeeRenderer?.PlayEmployeePathMove(employee, target.Path, () =>
            FinishFacilityArrival(employee.Id, target)
        );
    }

    private bool FindAutonomousTarget(
        EmployeeVisual employee,
        out Vector2I targetCell,
        out IReadOnlyList<Vector2I> path
    )
    {
        targetCell = employee.Cell;
        path = System.Array.Empty<Vector2I>();
        if (_employeeStore == null || _officeNavigationStore == null)
        {
            return false;
        }

        foreach (var offset in CandidateTargetOffsets)
        {
            var candidate = employee.Cell + offset;
            if (!_employeeStore.CanMoveEmployee(employee, candidate))
            {
                continue;
            }

            var candidatePath = _officeNavigationStore.FindPath(employee.Cell, candidate);
            if (candidatePath.Count <= 1 || candidatePath.Count > MaxAutonomousPathCells)
            {
                continue;
            }

            targetCell = candidate;
            path = candidatePath;
            return true;
        }

        return false;
    }

    private void FinishAutonomousMove(int employeeId, Vector2I targetCell)
    {
        if (_employeeStore?.TryMoveEmployee(employeeId, targetCell, out var movedEmployee) == true)
        {
            _ = movedEmployee;
            _employeeRenderer?.RefreshEmployees();
        }

        ClearEmployeeActivity(employeeId);
        _isEmployeeMoveInProgress = false;
    }

    private bool FindFacilityUseTarget(
        EmployeeVisual employee,
        int? preferredFacilityId,
        EmployeeActionCandidateKind? sourceAction,
        out FacilityInteractionTarget target
    )
    {
        target = FacilityInteractionTarget.Empty;
        if (
            _employeeStore == null
            || _facilityPlacementStore == null
            || _officeNavigationStore == null
        )
        {
            return false;
        }

        foreach (var facility in _facilityPlacementStore.GetFacilities())
        {
            if (
                (preferredFacilityId != null && facility.Id != preferredFacilityId.Value)
                || _reservedFacilityIds.Contains(facility.Id)
            )
            {
                continue;
            }

            foreach (var standCell in GetFacilityInteractionCells(facility))
            {
                if (!_employeeStore.CanMoveEmployee(employee, standCell))
                {
                    continue;
                }

                var path = _officeNavigationStore.FindPath(employee.Cell, standCell);
                if (path.Count <= 1)
                {
                    continue;
                }

                target = new FacilityInteractionTarget(facility, standCell, path, sourceAction);
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Vector2I> GetFacilityInteractionCells(FacilityPlacement facility)
    {
        yield return GetFacilitySeatCell(facility);
        foreach (var offset in FacilityInteractionOffsets)
        {
            var standCell = facility.Cell + offset;
            if (standCell != GetFacilitySeatCell(facility))
            {
                yield return standCell;
            }
        }
    }

    private static Vector2I GetFacilitySeatCell(FacilityPlacement facility)
    {
        return facility.Cell
            + (facility.Facing switch
            {
                FacilityFacing.North => Vector2I.Down,
                FacilityFacing.East => Vector2I.Right,
                FacilityFacing.South => Vector2I.Up,
                _ => Vector2I.Left,
            });
    }

    private void FinishFacilityArrival(int employeeId, FacilityInteractionTarget target)
    {
        if (_employeeStore?.TryMoveEmployee(employeeId, target.StandCell, out var movedEmployee) == true)
        {
            _ = movedEmployee;
            _employeeRenderer?.RefreshEmployees();
            _employeeRenderer?.SetEmployeeAnimationState(
                employeeId,
                GetFacilityUseAnimationState(target.Facility)
            );
            StartManualFacilityWork(employeeId, target.Facility);
            _isEmployeeMoveInProgress = false;
            TryStartNextAutonomousMove();
            return;
        }
        else
        {
            _reservedFacilityIds.Remove(target.Facility.Id);
            ClearEmployeeActivity(employeeId);
        }

        _isEmployeeMoveInProgress = false;
    }

    private void UpdateCoreSimulation(float delta)
    {
        if (!HasCoreSimulationActivity())
        {
            return;
        }

        _coreSimulationTimer -= delta;
        if (_coreSimulationTimer > 0.0f)
        {
            return;
        }

        _coreSimulationTimer = CoreSimulationTickSeconds;
        var isMonthEnd =
            _businessCalendarHud?.AdvanceBusinessDay(out var calendarTick) == true
            && calendarTick.IsMonthEnd;
        AdvanceAndApplyCoreSimulation(isMonthEnd);
    }

    private void AdvanceAndApplyCoreSimulation(bool isMonthEnd = false)
    {
        ApplyCoreSimulationStates(AdvanceCoreSimulation(isMonthEnd));
    }

    private CoreOfficeSimulationResult? AdvanceCoreSimulation(bool isMonthEnd = false)
    {
        if (
            _employeeStore == null
            || _facilityPlacementStore == null
            || _roomFootprintStore == null
            || _v2CoreBridge == null
        )
        {
            return null;
        }

        return _v2CoreBridge.AdvanceOfficeSimulation(
            _employeeStore,
            _facilityPlacementStore,
            _roomFootprintStore,
            isMonthEnd
        );
    }

    private bool HasCoreSimulationActivity()
    {
        foreach (var state in _employeeStates.Values)
        {
            if (
                state.ActivityKind == EmployeeActivityKind.WalkingToFacility
                || state.ActivityKind == EmployeeActivityKind.UsingFacility
            )
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyCoreSimulationStates(CoreOfficeSimulationResult? simulationResult)
    {
        if (simulationResult == null)
        {
            return;
        }

        if (_businessFeedbackHud != null)
        {
            _businessFeedbackHud.ApplySimulationResult(simulationResult);
        }

        _coreSummaryHud?.ApplySimulationResult(simulationResult);

        if (
            simulationResult.PresentationEvents.Any(
                eventSummary => eventSummary.Kind == SimulationEventKind.MonthlyReportReady
            )
        )
        {
            _businessCalendarHud?.MarkMonthlyReportReady(simulationResult);
            _monthlyReportHud?.ShowMonthlyReport(simulationResult);
            _timeScaleHud?.PauseForMonthlyReport();
        }

        var activeFacilityIds = new HashSet<int>();
        foreach (var facilityState in simulationResult.FacilityStates)
        {
            if (facilityState.IsInUse)
            {
                activeFacilityIds.Add(facilityState.FacilityId);
            }
        }

        foreach (var lifecycleState in simulationResult.EmployeeStates)
        {
            if (lifecycleState.ActivityKind == StartupSim.Core.EmployeeActivityKind.UseFacility)
            {
                if (lifecycleState.FacilityId != null)
                {
                    activeFacilityIds.Add(lifecycleState.FacilityId.Value);
                }

                SetEmployeeActivity(
                    lifecycleState.EmployeeId,
                    EmployeeActivityKind.UsingFacility,
                    facilityId: lifecycleState.FacilityId
                );
                var facility = lifecycleState.FacilityId == null
                    ? null
                    : _facilityPlacementStore?.FindById(lifecycleState.FacilityId.Value);
                _employeeRenderer?.SetEmployeeWorkState(
                    lifecycleState.EmployeeId,
                    facility?.FacilityType == FacilityBuildType.OfficeDesk,
                    facility?.FacilityType == FacilityBuildType.OfficeDesk ? facility : null
                );
                if (facility != null)
                {
                    _employeeRenderer?.SetEmployeeAnimationState(
                        lifecycleState.EmployeeId,
                        GetFacilityUseAnimationState(facility)
                    );
                }
                else
                {
                    _employeeRenderer?.SetEmployeeAnimationState(
                        lifecycleState.EmployeeId,
                        EmployeePresentationAnimationState.UsingStandingFacility
                    );
                }
            }
            else if (lifecycleState.ActivityKind == StartupSim.Core.EmployeeActivityKind.Idle)
            {
                ClearEmployeeActivity(lifecycleState.EmployeeId);
            }
        }

        foreach (var facilityId in _reservedFacilityIds)
        {
            _facilityRenderer?.SetFacilityUseState(facilityId, activeFacilityIds.Contains(facilityId));
        }

        _reservedFacilityIds.RemoveWhere(facilityId => !activeFacilityIds.Contains(facilityId));
        if (_reservedFacilityIds.Count == 0)
        {
            _coreSimulationTimer = CoreSimulationTickSeconds;
        }
    }

    private void SetEmployeeActivity(
        int employeeId,
        EmployeeActivityKind activityKind,
        Vector2I? targetCell = null,
        int? facilityId = null,
        string? labelOverride = null
    )
    {
        _employeeStates[employeeId] = new EmployeeAutonomyState(
            employeeId,
            activityKind,
            targetCell,
            facilityId
        );
        _employeeRenderer?.SetEmployeeActivityLabel(
            employeeId,
            labelOverride ?? GetActivityLabel(activityKind)
        );
    }

    private void ClearEmployeeActivity(int employeeId)
    {
        SetEmployeeActivity(employeeId, EmployeeActivityKind.Idle);
        _employeeRenderer?.SetEmployeeWorkState(employeeId, isWorking: false);
        _employeeRenderer?.SetEmployeeAnimationState(employeeId, EmployeePresentationAnimationState.Idle);
    }

    private static EmployeePresentationAnimationState GetAnimationStateForCoreIntent(
        EmployeeActionCandidateKind? sourceAction
    )
    {
        return sourceAction switch
        {
            EmployeeActionCandidateKind.WorkAtDesk => EmployeePresentationAnimationState.Walking,
            EmployeeActionCandidateKind.UseWhiteboard => EmployeePresentationAnimationState.Walking,
            EmployeeActionCandidateKind.MaintainServer => EmployeePresentationAnimationState.Walking,
            _ => EmployeePresentationAnimationState.Walking,
        };
    }

    private static EmployeePresentationAnimationState GetFacilityUseAnimationState(
        FacilityPlacement facility
    )
    {
        return facility.FacilityType == FacilityBuildType.OfficeDesk
            ? EmployeePresentationAnimationState.WorkingAtDesk
            : EmployeePresentationAnimationState.UsingStandingFacility;
    }

    private static string? GetActivityLabel(EmployeeActivityKind activityKind)
    {
        return activityKind switch
        {
            EmployeeActivityKind.WalkingToFacility => "\u524d\u5f80\u8bbe\u65bd",
            EmployeeActivityKind.UsingFacility => "\u5de5\u4f5c\u4e2d",
            EmployeeActivityKind.WalkingToTarget => "\u79fb\u52a8\u4e2d",
            _ => null,
        };
    }

    private static string GetFacilityUseActivityLabel(FacilityPlacement facility)
    {
        return facility.FacilityType switch
        {
            FacilityBuildType.ProductWhiteboard => "讨论方案",
            FacilityBuildType.ServerRack => "维护服务器",
            _ => "工作中",
        };
    }

    private static string BuildCoreIntentActivityLabel(CoreEmployeeIntent coreIntent)
    {
        var actionLabel = coreIntent.SourceAction switch
        {
            EmployeeActionCandidateKind.WorkAtDesk => "\u524d\u5f80\u529e\u516c\u684c",
            EmployeeActionCandidateKind.UseWhiteboard => "\u524d\u5f80\u767d\u677f",
            EmployeeActionCandidateKind.MaintainServer => "\u524d\u5f80\u670d\u52a1\u5668",
            EmployeeActionCandidateKind.Rest => "\u524d\u5f80\u4f11\u606f",
            _ => "\u524d\u5f80\u8bbe\u65bd",
        };

        return string.IsNullOrWhiteSpace(coreIntent.ReasonSummary)
            ? actionLabel
            : $"{actionLabel}: {coreIntent.ReasonSummary}";
    }
}

public enum EmployeeActivityKind
{
    Idle,
    WalkingToTarget,
    WalkingToFacility,
    UsingFacility,
}

public sealed record EmployeeAutonomyState(
    int EmployeeId,
    EmployeeActivityKind ActivityKind,
    Vector2I? TargetCell,
    int? FacilityId
);

public sealed record FacilityInteractionTarget(
    FacilityPlacement Facility,
    Vector2I StandCell,
    IReadOnlyList<Vector2I> Path,
    EmployeeActionCandidateKind? SourceAction = null
)
{
    public static readonly FacilityInteractionTarget Empty =
        new(new FacilityPlacement(0, FacilityBuildType.OfficeDesk, Vector2I.Zero, FacilityFacing.South), Vector2I.Zero, []);
}
