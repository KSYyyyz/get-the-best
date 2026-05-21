using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class EmployeeStore : Node
{
    private readonly List<EmployeeVisual> _employees =
    [
        new(
            1,
            "\u6797\u5c0f\u5b89",
            "\u7a0b\u5e8f",
            new Vector2I(15, 5),
            new Color(0.30f, 0.58f, 0.95f, 1.0f)
        ),
        new(
            2,
            "\u9648\u5b50\u822a",
            "\u7b56\u5212",
            new Vector2I(16, 5),
            new Color(0.95f, 0.52f, 0.38f, 1.0f)
        ),
        new(
            3,
            "\u5468\u82e5\u6674",
            "\u5e02\u573a",
            new Vector2I(17, 6),
            new Color(0.42f, 0.78f, 0.48f, 1.0f)
        ),
    ];

    public IReadOnlyList<EmployeeVisual> GetEmployees()
    {
        return _employees;
    }

    public EmployeeVisual? FindAtCell(Vector2I cell)
    {
        for (var index = _employees.Count - 1; index >= 0; index--)
        {
            var employee = _employees[index];
            if (employee.Cell == cell)
            {
                return employee;
            }
        }

        return null;
    }

    public IReadOnlyList<EmployeeVisual> FindInSelection(Vector2I startCell, Vector2I endCell)
    {
        var minX = Mathf.Min(startCell.X, endCell.X);
        var maxX = Mathf.Max(startCell.X, endCell.X);
        var minY = Mathf.Min(startCell.Y, endCell.Y);
        var maxY = Mathf.Max(startCell.Y, endCell.Y);
        var matches = new List<EmployeeVisual>();

        foreach (var employee in _employees)
        {
            if (
                employee.Cell.X >= minX
                && employee.Cell.X <= maxX
                && employee.Cell.Y >= minY
                && employee.Cell.Y <= maxY
            )
            {
                matches.Add(employee);
            }
        }

        return matches;
    }
}

public sealed record EmployeeVisual(
    int Id,
    string DisplayName,
    string RoleLabel,
    Vector2I Cell,
    Color AccentColor
);
