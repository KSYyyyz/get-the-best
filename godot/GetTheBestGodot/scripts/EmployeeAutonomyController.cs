using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class EmployeeAutonomyController : Node
{
    private const float AutonomousMoveIntervalSeconds = 2.4f;
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

    private readonly Dictionary<int, EmployeeAutonomyState> _employeeStates = [];
    private EmployeeStore? _employeeStore;
    private Employee3DRenderer? _employeeRenderer;
    private OfficeNavigationStore? _officeNavigationStore;
    private float _autonomyTimer = AutonomousMoveIntervalSeconds;
    private int _nextEmployeeIndex;
    private bool _isEmployeeMoveInProgress;

    public override void _Ready()
    {
        _employeeStore = GetNodeOrNull<EmployeeStore>("../EmployeeStore");
        _employeeRenderer = GetNodeOrNull<Employee3DRenderer>("../Employee3DRenderer");
        _officeNavigationStore = GetNodeOrNull<OfficeNavigationStore>("../OfficeNavigationStore");
        InitializeEmployeeStates();
    }

    public override void _Process(double delta)
    {
        if (_isEmployeeMoveInProgress)
        {
            return;
        }

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
            _employeeStates[employee.Id] = new EmployeeAutonomyState(
                employee.Id,
                EmployeeActivityKind.Idle,
                TargetCell: null
            );
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
            if (!FindAutonomousTarget(employee, out var targetCell, out var path))
            {
                continue;
            }

            _isEmployeeMoveInProgress = true;
            _employeeStates[employee.Id] = new EmployeeAutonomyState(
                employee.Id,
                EmployeeActivityKind.WalkingToTarget,
                targetCell
            );
            _employeeRenderer?.PlayEmployeePathMove(employee, path, () =>
                FinishAutonomousMove(employee.Id, targetCell)
            );
            return true;
        }

        return false;
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

        _employeeStates[employeeId] = new EmployeeAutonomyState(
            employeeId,
            EmployeeActivityKind.Idle,
            TargetCell: null
        );
        _isEmployeeMoveInProgress = false;
    }
}

public enum EmployeeActivityKind
{
    Idle,
    WalkingToTarget,
}

public sealed record EmployeeAutonomyState(
    int EmployeeId,
    EmployeeActivityKind ActivityKind,
    Vector2I? TargetCell
);
