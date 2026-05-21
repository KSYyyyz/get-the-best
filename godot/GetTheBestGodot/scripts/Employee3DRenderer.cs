using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class Employee3DRenderer : Node3D
{
    private const float EmployeeModelScale = OfficeWorld3DConfig.GridSize * 0.34f;
    private const float DragPreviewYOffset = OfficeWorld3DConfig.GridSize * 0.18f;
    private const float SmoothMoveDurationSeconds = 0.12f;
    private const float PathMoveStepDurationSeconds = 0.18f;
    private static readonly Color OutlineStroke = new(0.38f, 0.82f, 1.0f, 1.0f);
    private static readonly Color IllegalDragFill = new(0.95f, 0.32f, 0.28f, 1.0f);
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
    private readonly Dictionary<int, Vector3> _lastEmployeePositions = [];
    private readonly List<Node> _renderedEmployees = [];
    private int? _hoveredEmployeeId;
    private int? _dragPreviewEmployeeId;
    private int? _pathMovingEmployeeId;
    private Vector2I _dragPreviewCell;
    private bool _dragPreviewIsLegal = true;
    private EmployeeStore? _employeeStore;

    public override void _Ready()
    {
        _employeeStore = GetNodeOrNull<EmployeeStore>("../EmployeeStore");
        RefreshEmployees();
    }

    public void RefreshEmployees()
    {
        foreach (var renderedEmployee in _renderedEmployees)
        {
            if (TryGetEmployeeId(renderedEmployee.Name.ToString(), out var employeeId))
            {
                _lastEmployeePositions[employeeId] = ((Node3D)renderedEmployee).Position;
            }

            RemoveChild(renderedEmployee);
            renderedEmployee.QueueFree();
        }
        _renderedEmployees.Clear();

        if (_employeeStore == null)
        {
            return;
        }

        foreach (var employee in _employeeStore.GetEmployees())
        {
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

        _pathMovingEmployeeId = employee.Id;
        ApplyEmployeeOutline(modelRoot, OutlineStroke);

        var tween = CreateTween();
        for (var index = 1; index < path.Count; index++)
        {
            TweenEmployeePathStep(tween, modelRoot, path[index]);
        }

        tween.Finished += () =>
        {
            _lastEmployeePositions[employee.Id] = modelRoot.Position;
            _pathMovingEmployeeId = null;
            onFinished();
        };
    }

    private void AddEmployeeModel(EmployeeVisual employee)
    {
        var renderCell = GetRenderCell(employee);
        var yOffset =
            _dragPreviewEmployeeId == employee.Id
                ? DragPreviewYOffset
                : 0.02f;
        var targetPosition =
            OfficeWorld3DConfig.CellToWorldPosition(renderCell) + new Vector3(0.0f, yOffset, 0.0f);

        var modelScene = GetEmployeeModelScene(employee);
        if (modelScene == null)
        {
            GD.PushWarning($"Unable to load employee model for employee {employee.Id}.");
            return;
        }

        var modelRoot = modelScene.Instantiate<Node3D>();
        modelRoot.Name = $"Employee_{employee.Id}";
        modelRoot.Position = GetEmployeeStartPosition(employee.Id, targetPosition);
        modelRoot.Scale = Vector3.One * EmployeeModelScale;
        modelRoot.RotationDegrees = new Vector3(0.0f, 180.0f, 0.0f);
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

        CreateTween()
            .TweenProperty(modelRoot, "position", targetPosition, SmoothMoveDurationSeconds)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private static void TweenEmployeePathStep(Tween tween, Node3D modelRoot, Vector2I pathCell)
    {
        var targetPosition =
            OfficeWorld3DConfig.CellToWorldPosition(pathCell) + new Vector3(0.0f, 0.02f, 0.0f);
        tween
            .TweenProperty(
                modelRoot,
                "position",
                targetPosition,
                PathMoveStepDurationSeconds
            )
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
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
