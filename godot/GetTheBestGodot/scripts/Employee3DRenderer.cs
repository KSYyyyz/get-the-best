using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class Employee3DRenderer : Node3D
{
    private const float BodyHeight = OfficeWorld3DConfig.GridSize * 0.95f;
    private const float BodyRadius = OfficeWorld3DConfig.GridSize * 0.28f;
    private const float HeadRadius = OfficeWorld3DConfig.GridSize * 0.22f;
    private const float SelectionSize = OfficeWorld3DConfig.GridSize * 0.72f;
    private static readonly Color HeadFill = new(0.90f, 0.76f, 0.60f, 1.0f);
    private static readonly Color LegFill = new(0.16f, 0.18f, 0.22f, 1.0f);
    private static readonly Color SelectionFill = new(1.0f, 0.95f, 0.30f, 1.0f);
    private readonly HashSet<int> _highlightedEmployeeIds = [];
    private readonly List<Node> _renderedEmployees = [];
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

    public void HighlightEmployees(IReadOnlyList<EmployeeVisual> employees)
    {
        _highlightedEmployeeIds.Clear();
        foreach (var employee in employees)
        {
            _highlightedEmployeeIds.Add(employee.Id);
        }

        RefreshEmployees();
    }

    private void AddEmployeeModel(EmployeeVisual employee)
    {
        var position =
            OfficeWorld3DConfig.CellToWorldPosition(employee.Cell)
            + new Vector3(0.0f, 0.02f, 0.0f);
        var modelRoot = new Node3D { Position = position };
        AddChild(modelRoot);
        _renderedEmployees.Add(modelRoot);

        AddMeshPart(
            modelRoot,
            new CylinderMesh
            {
                TopRadius = BodyRadius,
                BottomRadius = BodyRadius * 1.10f,
                Height = BodyHeight,
                RadialSegments = 18,
            },
            employee.AccentColor,
            new Vector3(0.0f, BodyHeight / 2.0f, 0.0f)
        );
        AddMeshPart(
            modelRoot,
            new SphereMesh { Radius = HeadRadius, Height = HeadRadius * 2.0f },
            HeadFill,
            new Vector3(0.0f, BodyHeight + HeadRadius * 0.95f, 0.0f)
        );
        AddMeshPart(
            modelRoot,
            new BoxMesh
            {
                Size = new Vector3(
                    BodyRadius * 0.95f,
                    OfficeWorld3DConfig.GridSize * 0.22f,
                    BodyRadius * 0.80f
                ),
            },
            LegFill,
            new Vector3(-BodyRadius * 0.42f, OfficeWorld3DConfig.GridSize * 0.09f, 0.0f)
        );
        AddMeshPart(
            modelRoot,
            new BoxMesh
            {
                Size = new Vector3(
                    BodyRadius * 0.95f,
                    OfficeWorld3DConfig.GridSize * 0.22f,
                    BodyRadius * 0.80f
                ),
            },
            LegFill,
            new Vector3(BodyRadius * 0.42f, OfficeWorld3DConfig.GridSize * 0.09f, 0.0f)
        );

        if (_highlightedEmployeeIds.Contains(employee.Id))
        {
            AddSelectionRing(position);
        }
    }

    private void AddSelectionRing(Vector3 position)
    {
        AddMeshPart(
            this,
            new BoxMesh
            {
                Size = new Vector3(
                    SelectionSize,
                    OfficeWorld3DConfig.GridSize * 0.035f,
                    SelectionSize
                ),
            },
            SelectionFill,
            position + Vector3.Up * (OfficeWorld3DConfig.GridSize * 0.02f)
        );
    }

    private void AddMeshPart(Node parent, Mesh mesh, Color color, Vector3 position)
    {
        var part = new MeshInstance3D
        {
            Mesh = mesh,
            MaterialOverride = CreateMaterial(color),
            Position = position,
        };
        parent.AddChild(part);
        if (ReferenceEquals(parent, this))
        {
            _renderedEmployees.Add(part);
        }
    }

    private static StandardMaterial3D CreateMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 0.88f,
        };
    }
}
