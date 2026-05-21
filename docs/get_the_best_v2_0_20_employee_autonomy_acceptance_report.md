# 《壮志凌云 / Get The Best》V2-0.20 员工自主行动基线验收报告

## 本轮目标

在 V2-0.19 办公室通行与占用导航基础之上，建立员工自主行动系统的第一层。

本轮不做玩家指定员工行动，不做完整工作、休息、厕所、吃饭、会议等调度，也不把经营规则写进 Godot。目标是让员工从“可拖动实例”开始变成“能由前端行为调度驱动的空间实体”。

## 已完成

1. 新增 `EmployeeAutonomyController`
   - 挂载到 `InteractionRoot/EmployeeAutonomyController`。
   - 读取 `EmployeeStore`、`Employee3DRenderer` 和 `OfficeNavigationStore`。
   - 维护 `EmployeeAutonomyState`，当前包含 `Idle` 与 `WalkingToTarget`。
   - 预留 `EmployeeActivityKind`，后续可扩展为工作、休息、去厕所、吃饭、开会、培训等行为。

2. 自主移动调度
   - 控制器按固定间隔尝试选择一名员工。
   - 通过候选偏移格寻找近距离目标。
   - 目标必须通过 `EmployeeStore.CanMoveEmployee` 与 `OfficeNavigationStore.FindPath`。
   - 当前限制 `MaxAutonomousPathCells`，只做近距离自主微移动，避免员工无目的远离工作区域。

3. 路径动画播放
   - `Employee3DRenderer` 新增 `PlayEmployeePathMove`。
   - 路径按格逐段 tween，而不是直接瞬移到终点。
   - 移动过程中保留员工实例描边。
   - 动画完成后由 `EmployeeStore.TryMoveEmployee` 更新员工最终格子。

4. 规则边界
   - Godot 只做表现层、自主行动展示和交互验证。
   - 本轮没有加入经营产能、满意度、疲劳、岗位效率等规则。
   - 后续完整员工行为调度仍应由 C# Core 或其桥接意图来驱动。

## 自动化验证

新增静态回归测试覆盖：

- 主场景必须包含 `EmployeeAutonomyController`。
- 自主控制器必须使用 `EmployeeActivityKind` 与 `EmployeeAutonomyState`。
- 自主移动必须使用 `OfficeNavigationStore.FindPath`。
- 自主移动必须通过 `EmployeeStore.CanMoveEmployee` 和 `TryMoveEmployee`。
- 员工渲染器必须提供 `PlayEmployeePathMove`，并按路径逐格 tween。

## 实机验收

使用 Godot MCP 运行 `res://scenes/main.tscn` 后已验证：

- `EmployeeAutonomyController` 运行时节点存在。
- 员工无需玩家指定目标，会按自主调度发生近距离移动。
- 运行时观察到员工位置随时间更新，例如 `Employee_1` 从默认区域移动到 `x=-75,z=-15`，随后继续移动到 `x=-55,z=-15`。
- 员工仍保留在预设办公室附近，没有出现远距离无目的跑偏。
- Godot 错误面板保持 `0 error`。
- 验证截图暂存到 `godot/GetTheBestGodot/addons/godot_mcp/cache`，提交前清理。

## 后续建议

下一轮可以把自主行动从“近距离微移动”推进到“行为目标”：

- 为员工状态增加 `NeedWorkSeat` / `MoveToFacility` 一类前端行为意图。
- 先让研发员工自动寻找同房间空闲办公桌。
- 移动到设施后只显示“正在使用设施”的表现状态，不计算经营产能。
- 等 C# Core 行为调度接入后，再由规则核心决定行为原因和收益。
