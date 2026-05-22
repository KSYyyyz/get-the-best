using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class EmployeeAutonomyController : Node
{
    private const float AutonomousMoveIntervalSeconds = 2.4f;
    private const float CoreLifecycleTickSeconds = 1.6f;
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
    private float _autonomyTimer = AutonomousMoveIntervalSeconds;
    private float _coreLifecycleTimer = CoreLifecycleTickSeconds;
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
        InitializeEmployeeStates();
    }

    public override void _Process(double delta)
    {
        if (_isEmployeeMoveInProgress)
        {
            return;
        }

        UpdateCoreLifecycle((float)delta);
        _autonomyTimer -= (float)delta;
        if (_autonomyTimer > 0.0f)
        {
            return;
        }

        _autonomyTimer = AutonomousMoveIntervalSeconds;
        TryStartNextAutonomousMove();
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

        for (var attempt = 0; attempt < employees.Count; attempt++)
        {
            var employee = employees[_nextEmployeeIndex % employees.Count];
            _nextEmployeeIndex = (_nextEmployeeIndex + 1) % employees.Count;
            if (!IsEmployeeIdle(employee.Id))
            {
                continue;
            }

            if (TryStartFacilityUseBehavior(employee))
            {
                return true;
            }

            if (!FindAutonomousTarget(employee, out var targetCell, out var path))
            {
                continue;
            }

            _isEmployeeMoveInProgress = true;
            SetEmployeeActivity(employee.Id, EmployeeActivityKind.WalkingToTarget, targetCell);
            _employeeRenderer?.PlayEmployeePathMove(employee, path, () =>
                FinishAutonomousMove(employee.Id, targetCell)
            );
            return true;
        }

        return false;
    }

    private bool IsEmployeeIdle(int employeeId)
    {
        return !_employeeStates.TryGetValue(employeeId, out var state)
            || state.ActivityKind == EmployeeActivityKind.Idle;
    }

    private bool TryStartFacilityUseBehavior(EmployeeVisual employee)
    {
        if (
            _employeeStore == null
            || _facilityPlacementStore == null
            || _roomFootprintStore == null
            || _v2CoreBridge == null
        )
        {
            return false;
        }

        var coreIntents = _v2CoreBridge.PlanEmployeeIntents(
            _employeeStore,
            _facilityPlacementStore,
            _roomFootprintStore
        );
        foreach (var coreIntent in coreIntents)
        {
            if (
                coreIntent.EmployeeId != employee.Id
                || coreIntent.Kind != StartupSim.Core.EmployeeIntentKind.MoveToFacility
                || coreIntent.FacilityId == null
                || !FindFacilityUseTarget(employee, coreIntent.FacilityId, out var target)
            )
            {
                continue;
            }

            StartFacilityMove(employee, target);
            return true;
        }

        return false;
    }

    private void StartFacilityMove(EmployeeVisual employee, FacilityInteractionTarget target)
    {
        if (
            _employeeStore == null
            || _facilityPlacementStore == null
            || _roomFootprintStore == null
            || _v2CoreBridge == null
        )
        {
            return;
        }

        var lifecycleStates = _v2CoreBridge.AdvanceEmployeeLifecycle(
            _employeeStore,
            _facilityPlacementStore,
            _roomFootprintStore,
            [new CoreEmployeeIntent(employee.Id, StartupSim.Core.EmployeeIntentKind.MoveToFacility, target.Facility.Id)]
        );
        ApplyCoreLifecycleStates(lifecycleStates);
        _reservedFacilityIds.Add(target.Facility.Id);
        _isEmployeeMoveInProgress = true;
        SetEmployeeActivity(
            employee.Id,
            EmployeeActivityKind.WalkingToFacility,
            target.StandCell,
            target.Facility.Id
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

                target = new FacilityInteractionTarget(facility, standCell, path);
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Vector2I> GetFacilityInteractionCells(FacilityPlacement facility)
    {
        foreach (var offset in FacilityInteractionOffsets)
        {
            yield return facility.Cell + offset;
        }
    }

    private void FinishFacilityArrival(int employeeId, FacilityInteractionTarget target)
    {
        if (_employeeStore?.TryMoveEmployee(employeeId, target.StandCell, out var movedEmployee) == true)
        {
            _ = movedEmployee;
            _employeeRenderer?.RefreshEmployees();
            AdvanceAndApplyCoreLifecycle();
        }
        else
        {
            _reservedFacilityIds.Remove(target.Facility.Id);
            ClearEmployeeActivity(employeeId);
        }

        _isEmployeeMoveInProgress = false;
    }

    private void UpdateCoreLifecycle(float delta)
    {
        if (!HasCoreLifecycleActivity())
        {
            return;
        }

        _coreLifecycleTimer -= delta;
        if (_coreLifecycleTimer > 0.0f)
        {
            return;
        }

        _coreLifecycleTimer = CoreLifecycleTickSeconds;
        AdvanceAndApplyCoreLifecycle();
    }

    private void AdvanceAndApplyCoreLifecycle()
    {
        if (
            _employeeStore == null
            || _facilityPlacementStore == null
            || _roomFootprintStore == null
            || _v2CoreBridge == null
        )
        {
            return;
        }

        var lifecycleStates = _v2CoreBridge.AdvanceEmployeeLifecycle(
            _employeeStore,
            _facilityPlacementStore,
            _roomFootprintStore,
            []
        );
        ApplyCoreLifecycleStates(lifecycleStates);
    }

    private bool HasCoreLifecycleActivity()
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

    private void ApplyCoreLifecycleStates(IReadOnlyList<CoreEmployeeLifecycleState> lifecycleStates)
    {
        var activeFacilityIds = new HashSet<int>();
        foreach (var lifecycleState in lifecycleStates)
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
            _coreLifecycleTimer = CoreLifecycleTickSeconds;
        }
    }

    private void SetEmployeeActivity(
        int employeeId,
        EmployeeActivityKind activityKind,
        Vector2I? targetCell = null,
        int? facilityId = null
    )
    {
        _employeeStates[employeeId] = new EmployeeAutonomyState(
            employeeId,
            activityKind,
            targetCell,
            facilityId
        );
        _employeeRenderer?.SetEmployeeActivityLabel(employeeId, GetActivityLabel(activityKind));
    }

    private void ClearEmployeeActivity(int employeeId)
    {
        SetEmployeeActivity(employeeId, EmployeeActivityKind.Idle);
    }

    private static string? GetActivityLabel(EmployeeActivityKind activityKind)
    {
        return activityKind switch
        {
            EmployeeActivityKind.WalkingToFacility => "\u524d\u5f80\u8bbe\u65bd",
            EmployeeActivityKind.UsingFacility => "\u6b63\u5728\u4f7f\u7528",
            EmployeeActivityKind.WalkingToTarget => "\u79fb\u52a8\u4e2d",
            _ => null,
        };
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
    IReadOnlyList<Vector2I> Path
)
{
    public static readonly FacilityInteractionTarget Empty =
        new(new FacilityPlacement(0, FacilityBuildType.OfficeDesk, Vector2I.Zero, FacilityFacing.South), Vector2I.Zero, []);
}
