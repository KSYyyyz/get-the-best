# 《壮志凌云 / Get The Best》V2-0.19 办公室通行与占用导航基础验收报告

## 本轮目标

在继续推进员工自动行动之前，先建立办公室空间的最小通行数据层，让房间、门、设施和员工拖拽不再各自散写合法性判断。

本轮仍然只做 Godot 桌面前端表现层和交互层：导航 Store 用来服务可视化、拖拽、预览和后续前端验证，不承载经营规则，不替代后续 C# Core。

## 已完成

1. 新增 `OfficeNavigationStore`
   - 挂载到 `InteractionRoot/OfficeNavigationStore`。
   - 读取 `RoomFootprintStore` 与 `FacilityPlacementStore`。
   - 提供 `IsInsideOffice`、`IsWalkable`、`IsBlocked`、`CanStandAt`、`IsDoorCell`、`IsDoorPassage`、`CanMoveBetween` 和 `FindPath`。

2. 门和墙的通行边界
   - 房间内部可行走。
   - 走廊和办公室空地可行走。
   - 房间边界只允许通过房门连接内外。
   - `IsDoorPassage(fromCell, toCell)` 使用门所在格与门外相邻格判断通行边。

3. 设施占用
   - 设施所在格视为占用格。
   - `CanStandAt(cell)` 会拒绝设施占用格。
   - 设施移动时使用 `CanStandAt(targetCell, facility.Id)` 忽略自身原占用。
   - 设施不能放到门口通行格，避免堵住后续员工行为路径。

4. 员工拖拽合法性接入导航
   - 员工目标格必须可站立。
   - 员工目标格不能被其他员工占用。
   - 员工拖拽预览会调用 `FindPath(_dragEmployeeOriginCell, cell)`，只有存在路径时才显示为合法。

5. 简单路径查询
   - `FindPath` 先使用 BFS。
   - 当前只返回格子路径，用于交互合法性和后续调试，不做寻路动画。

## 自动化验证

新增静态回归测试覆盖：

- 主场景必须包含 `OfficeNavigationStore`。
- 导航 Store 必须提供通行、阻挡、门通行和 BFS 路径接口。
- 员工移动必须使用 `OfficeNavigationStore.CanStandAt`。
- 设施放置和移动必须使用 `OfficeNavigationStore.CanStandAt` 和 `IsDoorCell`。
- 员工拖拽合法性必须调用 `OfficeNavigationStore.FindPath`。

## 实机验收

使用 Godot MCP 运行 `res://scenes/main.tscn` 后已验证：

- 默认预设办公室正常显示。
- 点击员工后进入“点击放下”拖拽状态。
- 员工拖到可达、可站立格后可以放下，`Employee_1` 运行时位置更新为 `x=-75,z=-35`。
- 员工拖到设施占用格时会被拒绝，`Employee_1` 运行时位置保持在放下前的 `x=-65,z=-55`。
- Godot 错误面板保持 `0 error`。
- 验证截图暂存到 `godot/GetTheBestGodot/addons/godot_mcp/cache`，提交前清理。

## 后续建议

下一轮可以在此基础上做“员工自动走向目标格”的第一版：

- 仍使用 BFS 路径。
- 先不做完整工作/休息 AI。
- 只做一个可验证的前端行为：选择员工，给一个目标格，员工沿路径逐格平滑移动。
