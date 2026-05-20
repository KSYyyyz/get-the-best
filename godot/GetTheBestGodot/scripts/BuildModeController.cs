using Godot;

namespace GetTheBestGodot;

public partial class BuildModeController : Node
{
    public bool IsSelectionLegal(Vector2I startCell, Vector2I endCell)
    {
        return OfficeWorldConfig.CountCells(startCell, endCell) > 0;
    }

    public string GetSelectionSummary(Vector2I startCell, Vector2I endCell)
    {
        var cellCount = OfficeWorldConfig.CountCells(startCell, endCell);
        return $"预览区域：{cellCount} 格，当前可建造";
    }
}
