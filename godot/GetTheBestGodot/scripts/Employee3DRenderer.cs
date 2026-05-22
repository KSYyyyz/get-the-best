using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class Employee3DRenderer : Node3D
{
    private const float EmployeeModelScale = OfficeWorld3DConfig.GridSize * 0.34f;
    private const float DragPreviewYOffset = OfficeWorld3DConfig.GridSize * 0.18f;
    private const float SmoothMoveDurationSeconds = 0.12f;
    private const float PathMoveStepDurationSeconds = 0.18f;
    private const float CellInnerSize = OfficeWorld3DConfig.GridSize * 0.72f;
    private static readonly Color OutlineStroke = new(0.38f, 0.82f, 1.0f, 1.0f);
    private static readonly Color IllegalDragFill = new(0.95f, 0.32f, 0.28f, 1.0f);
    private static readonly Color ActivityBadgeFill = new(0.96f, 0.98f, 1.0f, 1.0f);
    private static readonly Color ActivityBadgeOutline = new(0.10f, 0.14f, 0.18f, 1.0f);
    private static readonly Color TypingHandFill = new(0.96f, 0.72f, 0.48f, 1.0f);
    private static readonly string[] EmployeeModelScenePaths =
    [
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/character-a.glb",
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/character-b.glb",
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/character-c.glb",
    ];
    private static readonly string[] EmployeeTexturePaths =
    [
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/Textures/texture-a.png",
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/Textures/texture-b.png",
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/Textures/texture-c.png",
    ];
    private readonly HashSet<int> _highlightedEmployeeIds = [];
    private readonly HashSet<int> _workingEmployeeIds = [];
    private readonly Dictionary<int, string> _employeeActivityLabels = [];
    private readonly Dictionary<int, EmployeePresentationAnimationState> _employeeAnimationStates = [];
    private readonly Dictionary<int, Vector2I> _employeeFacingTargets = [];
    private readonly Dictionary<int, EmployeeWorkPose> _employeeWorkPoses = [];
    private readonly Dictionary<int, Vector3> _lastEmployeePositions = [];
    private readonly Dictionary<int, Tween> _employeePathMoveTweens = [];
    private readonly Dictionary<int, List<Tween>> _employeeLoopingTweens = [];
    private readonly List<Node> _renderedEmployees = [];
    private int? _hoveredEmployeeId;
    private int? _dragPreviewEmployeeId;
    private int? _pathMovingEmployeeId;
    private Vector2I _dragPreviewCell;
    private bool _dragPreviewIsLegal = true;
    private float _presentationTimeScale = 1.0f;
    private EmployeeStore? _employeeStore;

    public override void _Ready()
    {
        _employeeStore = GetNodeOrNull<EmployeeStore>("../EmployeeStore");
        RefreshEmployees();
    }

    public void RefreshEmployees()
    {
        var retainedEmployees = new List<Node>();
        foreach (var renderedEmployee in _renderedEmployees)
        {
            if (TryGetEmployeeId(renderedEmployee.Name.ToString(), out var employeeId))
            {
                if (_employeePathMoveTweens.ContainsKey(employeeId))
                {
                    retainedEmployees.Add(renderedEmployee);
                    continue;
                }

                _lastEmployeePositions[employeeId] = ((Node3D)renderedEmployee).Position;
                KillEmployeeLoopingTweens(employeeId);
            }

            RemoveChild(renderedEmployee);
            renderedEmployee.QueueFree();
        }
        _renderedEmployees.Clear();
        _renderedEmployees.AddRange(retainedEmployees);

        if (_employeeStore == null)
        {
            return;
        }

        foreach (var employee in _employeeStore.GetEmployees())
        {
            if (_employeePathMoveTweens.ContainsKey(employee.Id))
            {
                continue;
            }

            AddEmployeeModel(employee);
        }
    }

    public void HighlightEmployee(EmployeeVisual? employee)
    {
        _highlightedEmployeeIds.Clear();
        if (employee != null)
        {
            _highlightedEmployeeIds.Add(employee.Id);
        }

        RefreshEmployees();
    }

    public void HoverEmployee(EmployeeVisual? employee)
    {
        _hoveredEmployeeId = employee?.Id;
        RefreshEmployees();
    }

    public void HighlightEmployees(IReadOnlyList<EmployeeVisual> employees)
    {
        _highlightedEmployeeIds.Clear();
        foreach (var employee in employees)
        {
            _highlightedEmployeeIds.Add(employee.Id);
        }

        RefreshEmployees();
    }

    public void ShowEmployeeDragPreview(
        EmployeeVisual employee,
        Vector2I targetCell,
        bool isLegal
    )
    {
        _dragPreviewEmployeeId = employee.Id;
        _dragPreviewCell = targetCell;
        _dragPreviewIsLegal = isLegal;
        _highlightedEmployeeIds.Clear();
        _highlightedEmployeeIds.Add(employee.Id);
        RefreshEmployees();
    }

    public void ClearEmployeeDragPreview()
    {
        if (_dragPreviewEmployeeId == null)
        {
            return;
        }

        _dragPreviewEmployeeId = null;
        RefreshEmployees();
    }

    public void SetEmployeeActivityLabel(int employeeId, string? labelText)
    {
        if (string.IsNullOrWhiteSpace(labelText))
        {
            if (!_employeeActivityLabels.Remove(employeeId))
            {
                return;
            }
        }
        else
        {
            if (
                _employeeActivityLabels.TryGetValue(employeeId, out var currentLabel)
                && currentLabel == labelText
            )
            {
                return;
            }

            _employeeActivityLabels[employeeId] = labelText;
        }

        RefreshEmployees();
    }

    public void SetEmployeeAnimationState(int employeeId, EmployeePresentationAnimationState state)
    {
        if (state == EmployeePresentationAnimationState.Idle)
        {
            if (!_employeeAnimationStates.Remove(employeeId))
            {
                return;
            }
        }
        else
        {
            if (
                _employeeAnimationStates.TryGetValue(employeeId, out var currentState)
                && currentState == state
            )
            {
                return;
            }

            _employeeAnimationStates[employeeId] = state;
        }

        RefreshEmployees();
    }

    public void SetEmployeeWorkState(
        int employeeId,
        bool isWorking,
        FacilityPlacement? facility = null
    )
    {
        var changed = false;
        if (isWorking)
        {
            changed |= _workingEmployeeIds.Add(employeeId);
            if (facility != null)
            {
                var nextPose = new EmployeeWorkPose(facility);
                if (
                    !_employeeWorkPoses.TryGetValue(employeeId, out var currentPose)
                    || currentPose != nextPose
                )
                {
                    _employeeWorkPoses[employeeId] = nextPose;
                    changed = true;
                }

                if (
                    !_employeeFacingTargets.TryGetValue(employeeId, out var currentTarget)
                    || currentTarget != facility.Cell
                )
                {
                    _employeeFacingTargets[employeeId] = facility.Cell;
                    changed = true;
                }
            }
        }
        else
        {
            changed |= _workingEmployeeIds.Remove(employeeId);
            changed |= _employeeFacingTargets.Remove(employeeId);
            changed |= _employeeWorkPoses.Remove(employeeId);
        }

        if (changed)
        {
            RefreshEmployees();
        }
    }

    public void SetPresentationTimeScale(float timeScale)
    {
        _presentationTimeScale = Mathf.Clamp(timeScale, 0.0f, 3.0f);
        SetActiveTweenSpeedScale();
    }

    public void PlayEmployeePathMove(
        EmployeeVisual employee,
        IReadOnlyList<Vector2I> path,
        System.Action onFinished
    )
    {
        var modelRoot = GetNodeOrNull<Node3D>($"Employee_{employee.Id}");
        if (modelRoot == null || path.Count <= 1)
        {
            onFinished();
            return;
        }

        KillEmployeePathMoveTween(employee.Id);
        KillEmployeeLoopingTweens(employee.Id);
        _pathMovingEmployeeId = employee.Id;
        ApplyEmployeeOutline(modelRoot, OutlineStroke);

        var tween = CreateScaledTween();
        _employeePathMoveTweens[employee.Id] = tween;
        for (var index = 1; index < path.Count; index++)
        {
            TweenEmployeePathStep(tween, modelRoot, path[index]);
        }

        var finalPosition = GetPathCellWorldPosition(path[^1]);
        var didFinish = false;
        tween.Finished += () =>
        {
            if (didFinish)
            {
                return;
            }

            didFinish = true;
            _lastEmployeePositions[employee.Id] = finalPosition;
            _employeePathMoveTweens.Remove(employee.Id);
            if (_pathMovingEmployeeId == employee.Id)
            {
                _pathMovingEmployeeId = null;
            }

            onFinished();
        };
    }

    private void AddEmployeeModel(EmployeeVisual employee)
    {
        var isDragPreviewEmployee = _dragPreviewEmployeeId == employee.Id;
        var renderCell = GetRenderCell(employee);
        var yOffset =
            isDragPreviewEmployee
                ? DragPreviewYOffset
                : 0.02f;
        var targetPosition =
            OfficeWorld3DConfig.CellToWorldPosition(renderCell) + new Vector3(0.0f, yOffset, 0.0f);
        FacilityPlacement? workFacility = null;
        if (_employeeWorkPoses.TryGetValue(employee.Id, out var workPose))
        {
            workFacility = workPose.Facility;
        }
        if (workFacility != null && !isDragPreviewEmployee)
        {
            targetPosition = GetDeskSeatWorldPosition(workFacility);
        }

        var modelScene = GetEmployeeModelScene(employee);
        if (modelScene == null)
        {
            GD.PushWarning($"Unable to load employee model for employee {employee.Id}.");
            return;
        }

        var modelRoot = modelScene.Instantiate<Node3D>();
        modelRoot.Name = $"Employee_{employee.Id}";
        modelRoot.Position = GetEmployeeStartPosition(employee.Id, targetPosition);
        modelRoot.Scale = Vector3.One * GetWorkPoseScale(employee.Id);
        modelRoot.RotationDegrees = workFacility != null
            ? new Vector3(-5.0f, GetDeskWorkYawDegrees(workFacility), 0.0f)
            : new Vector3(0.0f, GetEmployeeFacingYawDegrees(employee), 0.0f);
        AddChild(modelRoot);
        _renderedEmployees.Add(modelRoot);
        TweenEmployeeToTarget(modelRoot, targetPosition);
        ApplyEmployeeTexture(modelRoot, GetEmployeeTexture(employee));

        if (_dragPreviewEmployeeId == employee.Id && !_dragPreviewIsLegal)
        {
            ApplyEmployeeTint(modelRoot, IllegalDragFill);
        }

        if (
            _highlightedEmployeeIds.Contains(employee.Id)
            || _hoveredEmployeeId == employee.Id
            || _dragPreviewEmployeeId == employee.Id
            || _pathMovingEmployeeId == employee.Id
        )
        {
            ApplyEmployeeOutline(modelRoot, GetRenderOutlineColor(employee));
        }

        AddEmployeeActivityBadge(modelRoot, employee);
        if (_workingEmployeeIds.Contains(employee.Id) && !isDragPreviewEmployee)
        {
            _employeeAnimationStates[employee.Id] = EmployeePresentationAnimationState.WorkingAtDesk;
        }

        ApplyEmployeeAnimationState(modelRoot, employee.Id);
    }

    private static PackedScene? GetEmployeeModelScene(EmployeeVisual employee)
    {
        var index = (employee.Id - 1) % EmployeeModelScenePaths.Length;
        if (index < 0)
        {
            index += EmployeeModelScenePaths.Length;
        }

        var modelScenePath = EmployeeModelScenePaths[index];
        return GD.Load<PackedScene>(modelScenePath);
    }

    private Vector3 GetEmployeeStartPosition(int employeeId, Vector3 targetPosition)
    {
        return _lastEmployeePositions.TryGetValue(employeeId, out var previousPosition)
            ? previousPosition
            : targetPosition;
    }

    private void TweenEmployeeToTarget(Node3D modelRoot, Vector3 targetPosition)
    {
        if (modelRoot.Position.DistanceTo(targetPosition) < 0.01f)
        {
            modelRoot.Position = targetPosition;
            return;
        }

        CreateScaledTween()
            .TweenProperty(modelRoot, "position", targetPosition, SmoothMoveDurationSeconds)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private static void TweenEmployeePathStep(Tween tween, Node3D modelRoot, Vector2I pathCell)
    {
        tween
            .TweenProperty(
                modelRoot,
                "position",
                GetPathCellWorldPosition(pathCell),
                PathMoveStepDurationSeconds
            )
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }

    private static Vector3 GetPathCellWorldPosition(Vector2I pathCell)
    {
        return OfficeWorld3DConfig.CellToWorldPosition(pathCell) + new Vector3(0.0f, 0.02f, 0.0f);
    }

    private void KillEmployeePathMoveTween(int employeeId)
    {
        if (!_employeePathMoveTweens.Remove(employeeId, out var tween))
        {
            return;
        }

        tween.Kill();
        if (_pathMovingEmployeeId == employeeId)
        {
            _pathMovingEmployeeId = null;
        }
    }

    private void RegisterEmployeeLoopingTween(int employeeId, Tween tween)
    {
        if (!_employeeLoopingTweens.TryGetValue(employeeId, out var tweens))
        {
            tweens = [];
            _employeeLoopingTweens[employeeId] = tweens;
        }

        tweens.Add(tween);
    }

    private Tween CreateScaledTween()
    {
        var tween = CreateTween();
        ApplyTweenTimeScale(tween);
        return tween;
    }

    private void SetActiveTweenSpeedScale()
    {
        foreach (var tween in _employeePathMoveTweens.Values)
        {
            ApplyTweenTimeScale(tween);
        }

        foreach (var tweens in _employeeLoopingTweens.Values)
        {
            foreach (var tween in tweens)
            {
                ApplyTweenTimeScale(tween);
            }
        }
    }

    private void ApplyTweenTimeScale(Tween tween)
    {
        if (_presentationTimeScale <= 0.0f)
        {
            tween.Pause();
            return;
        }

        tween.SetSpeedScale(_presentationTimeScale);
        tween.Play();
    }

    private void KillEmployeeLoopingTweens(int employeeId)
    {
        if (!_employeeLoopingTweens.Remove(employeeId, out var tweens))
        {
            return;
        }

        foreach (var tween in tweens)
        {
            tween.Kill();
        }
    }

    private static bool TryGetEmployeeId(string nodeName, out int employeeId)
    {
        employeeId = 0;
        const string prefix = "Employee_";
        return nodeName.StartsWith(prefix)
            && int.TryParse(nodeName[prefix.Length..], out employeeId);
    }

    private static Texture2D? GetEmployeeTexture(EmployeeVisual employee)
    {
        var index = (employee.Id - 1) % EmployeeTexturePaths.Length;
        if (index < 0)
        {
            index += EmployeeTexturePaths.Length;
        }

        return GD.Load<Texture2D>(EmployeeTexturePaths[index]);
    }

    private Vector2I GetRenderCell(EmployeeVisual employee)
    {
        return _dragPreviewEmployeeId == employee.Id ? _dragPreviewCell : employee.Cell;
    }

    private Color GetRenderOutlineColor(EmployeeVisual employee)
    {
        return _dragPreviewEmployeeId == employee.Id && !_dragPreviewIsLegal
            ? IllegalDragFill
            : OutlineStroke;
    }

    private float GetEmployeeFacingYawDegrees(EmployeeVisual employee)
    {
        if (!_employeeFacingTargets.TryGetValue(employee.Id, out var targetCell))
        {
            return 180.0f;
        }

        var delta = targetCell - employee.Cell;
        if (Mathf.Abs(delta.X) > Mathf.Abs(delta.Y))
        {
            return delta.X > 0 ? 90.0f : 270.0f;
        }

        return delta.Y < 0 ? 0.0f : 180.0f;
    }

    private void PlayEmployeeWorkAnimation(Node3D modelRoot, int employeeId)
    {
        PlayEmployeeTypingAnimation(modelRoot, employeeId);
    }

    private void ApplyEmployeeAnimationState(Node3D modelRoot, int employeeId)
    {
        if (!_employeeAnimationStates.TryGetValue(employeeId, out var state))
        {
            return;
        }

        switch (state)
        {
            case EmployeePresentationAnimationState.SittingDown:
                PlayEmployeeSittingDownAnimation(modelRoot);
                break;
            case EmployeePresentationAnimationState.WorkingAtDesk:
                AddTypingHands(modelRoot);
                PlayEmployeeWorkAnimation(modelRoot, employeeId);
                break;
            case EmployeePresentationAnimationState.UsingStandingFacility:
                PlayEmployeeStandingUseAnimation(modelRoot);
                break;
            case EmployeePresentationAnimationState.Walking:
            case EmployeePresentationAnimationState.LeavingFacility:
                PlayEmployeeWalkingAnimation(modelRoot);
                break;
        }
    }

    private void PlayEmployeeWalkingAnimation(Node3D modelRoot)
    {
        if (!TryGetEmployeeId(modelRoot.Name.ToString(), out var employeeId))
        {
            return;
        }

        PlayEmployeeWalkingAnimation(modelRoot, employeeId);
    }

    private void PlayEmployeeWalkingAnimation(Node3D modelRoot, int employeeId)
    {
        var baseRotation = modelRoot.RotationDegrees;
        var tween = CreateScaledTween().SetLoops();
        RegisterEmployeeLoopingTween(employeeId, tween);
        tween
            .TweenProperty(
                modelRoot,
                "rotation_degrees",
                baseRotation + new Vector3(0.0f, 0.0f, 4.0f),
                0.18f
            )
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        tween
            .TweenProperty(
                modelRoot,
                "rotation_degrees",
                baseRotation + new Vector3(0.0f, 0.0f, -4.0f),
                0.18f
            )
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        tween
            .TweenProperty(modelRoot, "rotation_degrees", baseRotation, 0.12f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private void PlayEmployeeSittingDownAnimation(Node3D modelRoot)
    {
        var baseRotation = modelRoot.RotationDegrees;
        var targetRotation = baseRotation + new Vector3(-6.0f, 0.0f, 0.0f);
        var targetScale = modelRoot.Scale * 0.94f;

        CreateScaledTween()
            .TweenProperty(modelRoot, "rotation_degrees", targetRotation, 0.22f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        CreateScaledTween()
            .TweenProperty(modelRoot, "scale", targetScale, 0.22f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private void PlayEmployeeStandingUseAnimation(Node3D modelRoot)
    {
        if (!TryGetEmployeeId(modelRoot.Name.ToString(), out var employeeId))
        {
            return;
        }

        PlayEmployeeStandingUseAnimation(modelRoot, employeeId);
    }

    private void PlayEmployeeStandingUseAnimation(Node3D modelRoot, int employeeId)
    {
        var baseRotation = modelRoot.RotationDegrees;
        var tween = CreateScaledTween().SetLoops();
        RegisterEmployeeLoopingTween(employeeId, tween);
        tween
            .TweenProperty(
                modelRoot,
                "rotation_degrees",
                baseRotation + new Vector3(-3.0f, 2.0f, 0.0f),
                0.36f
            )
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        tween
            .TweenProperty(modelRoot, "rotation_degrees", baseRotation, 0.36f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private void PlayEmployeeTypingAnimation(Node3D modelRoot, int employeeId)
    {
        var baseRotation = modelRoot.RotationDegrees;
        var tween = CreateScaledTween().SetLoops();
        RegisterEmployeeLoopingTween(employeeId, tween);
        tween
            .TweenProperty(
                modelRoot,
                "rotation_degrees",
                baseRotation + new Vector3(-2.0f, 1.4f, 0.0f),
                0.32f
            )
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        tween
            .TweenProperty(modelRoot, "rotation_degrees", baseRotation, 0.32f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        var typingHands = modelRoot.GetNodeOrNull<Node3D>("TypingHands");
        if (typingHands == null)
        {
            return;
        }

        var baseHandsPosition = typingHands.Position;
        var handsTween = CreateScaledTween().SetLoops();
        RegisterEmployeeLoopingTween(employeeId, handsTween);
        handsTween
            .TweenProperty(
                typingHands,
                "position",
                baseHandsPosition + new Vector3(0.0f, 0.0f, -0.08f / modelRoot.Scale.X),
                0.16f
            )
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        handsTween
            .TweenProperty(typingHands, "position", baseHandsPosition, 0.16f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    private static void AddTypingHands(Node3D modelRoot)
    {
        var inverseScale = 1.0f / Mathf.Max(modelRoot.Scale.X, 0.001f);
        var handsRoot = new Node3D
        {
            Name = "TypingHands",
            Position = new Vector3(0.0f, 0.76f * inverseScale, -0.58f * inverseScale),
            Scale = Vector3.One * inverseScale,
        };
        modelRoot.AddChild(handsRoot);

        AddTypingHand(handsRoot, -0.26f);
        AddTypingHand(handsRoot, 0.26f);
    }

    private static void AddTypingHand(Node3D parent, float xOffset)
    {
        parent.AddChild(
            new MeshInstance3D
            {
                Mesh = new BoxMesh
                {
                    Size = new Vector3(0.30f, 0.16f, 0.36f),
                },
                MaterialOverride = CreateMaterial(TypingHandFill),
                Position = new Vector3(xOffset, 0.0f, 0.0f),
            }
        );
    }

    private float GetWorkPoseScale(int employeeId)
    {
        return _employeeWorkPoses.ContainsKey(employeeId)
            ? EmployeeModelScale * 0.82f
            : EmployeeModelScale;
    }

    private static Vector3 GetDeskSeatWorldPosition(FacilityPlacement facility)
    {
        var facilityCenter = OfficeWorld3DConfig.CellToWorldPosition(facility.Cell);
        var localSeatOffset = new Vector3(0.0f, 0.0f, CellInnerSize * 0.40f);
        return facilityCenter
            + RotateLocalOffsetByFacing(localSeatOffset, facility.Facing)
            + new Vector3(0.0f, 0.04f, 0.0f);
    }

    private static float GetDeskWorkYawDegrees(FacilityPlacement facility)
    {
        return facility.Facing switch
        {
            FacilityFacing.North => 180.0f,
            FacilityFacing.East => 270.0f,
            FacilityFacing.South => 0.0f,
            _ => 90.0f,
        };
    }

    private static Vector3 RotateLocalOffsetByFacing(Vector3 offset, FacilityFacing facing)
    {
        return facing switch
        {
            FacilityFacing.North => offset,
            FacilityFacing.East => new Vector3(offset.Z, offset.Y, -offset.X),
            FacilityFacing.South => new Vector3(-offset.X, offset.Y, -offset.Z),
            _ => new Vector3(-offset.Z, offset.Y, offset.X),
        };
    }

    private void AddEmployeeActivityBadge(Node3D modelRoot, EmployeeVisual employee)
    {
        if (!_employeeActivityLabels.TryGetValue(employee.Id, out var labelText))
        {
            return;
        }

        var badge = new Label3D
        {
            Name = "ActivityBadge",
            Text = labelText,
            Position = new Vector3(0.0f, 2.35f, 0.0f),
            PixelSize = 0.010f,
            FontSize = 22,
            Modulate = ActivityBadgeFill,
            OutlineModulate = ActivityBadgeOutline,
            OutlineSize = 4,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            NoDepthTest = true,
            FixedSize = false,
        };
        modelRoot.AddChild(badge);
    }

    private static void ApplyEmployeeTint(Node node, Color color)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                ApplyMaterialToMesh(mesh, color);
            }

            ApplyEmployeeTint(child, color);
        }
    }

    private static void ApplyEmployeeTexture(Node node, Texture2D? texture)
    {
        if (texture == null)
        {
            return;
        }

        foreach (var child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                ApplyTextureToMesh(mesh, texture);
            }

            ApplyEmployeeTexture(child, texture);
        }
    }

    private static void ApplyEmployeeOutline(Node node, Color outlineColor)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                ApplyOutlineToMesh(mesh, outlineColor);
            }

            ApplyEmployeeOutline(child, outlineColor);
        }
    }

    private static void ApplyMaterialToMesh(MeshInstance3D mesh, Color color)
    {
        var surfaceCount = mesh.Mesh?.GetSurfaceCount() ?? 0;
        for (var surfaceIndex = 0; surfaceIndex < surfaceCount; surfaceIndex++)
        {
            mesh.SetSurfaceOverrideMaterial(surfaceIndex, CreateMaterial(color));
        }
    }

    private static void ApplyTextureToMesh(MeshInstance3D mesh, Texture2D texture)
    {
        var surfaceCount = mesh.Mesh?.GetSurfaceCount() ?? 0;
        for (var surfaceIndex = 0; surfaceIndex < surfaceCount; surfaceIndex++)
        {
            var material = CreateMaterial(new Color(1.0f, 1.0f, 1.0f, 1.0f));
            material.AlbedoTexture = texture;
            mesh.SetSurfaceOverrideMaterial(surfaceIndex, material);
        }
    }

    private static void ApplyOutlineToMesh(MeshInstance3D mesh, Color outlineColor)
    {
        var surfaceCount = mesh.Mesh?.GetSurfaceCount() ?? 0;
        for (var surfaceIndex = 0; surfaceIndex < surfaceCount; surfaceIndex++)
        {
            var material = DuplicateMaterial(
                mesh.GetSurfaceOverrideMaterial(surfaceIndex)
                    ?? mesh.Mesh?.SurfaceGetMaterial(surfaceIndex)
            );
            material.NextPass = CreateOutlineMaterial(outlineColor);
            mesh.SetSurfaceOverrideMaterial(surfaceIndex, material);
        }
    }

    private static Material DuplicateMaterial(Material? material)
    {
        if (material?.Duplicate() is Material duplicate)
        {
            return duplicate;
        }

        return CreateMaterial(new Color(1.0f, 1.0f, 1.0f, 1.0f));
    }

    private static StandardMaterial3D CreateMaterial(Color color)
    {
        return new StandardMaterial3D
            {
                AlbedoColor = color,
                Roughness = 0.88f,
            };
    }

    private static StandardMaterial3D CreateOutlineMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            EmissionEnabled = true,
            Emission = color,
            EmissionEnergyMultiplier = 0.15f,
            Grow = true,
            GrowAmount = 0.075f,
            CullMode = BaseMaterial3D.CullModeEnum.Front,
            Roughness = 0.55f,
        };
    }
}

public sealed record EmployeeWorkPose(FacilityPlacement Facility);

public enum EmployeePresentationAnimationState
{
    Idle,
    Walking,
    SittingDown,
    WorkingAtDesk,
    UsingStandingFacility,
    LeavingFacility,
}
