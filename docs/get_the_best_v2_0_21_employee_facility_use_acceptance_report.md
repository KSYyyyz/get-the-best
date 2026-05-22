# 《壮志凌云 / Get The Best》V2-0.21 员工设施使用行为基线验收报告

## 本轮目标

在 V2-0.20 员工自主行动基线之上，把员工从“随机近距离移动”推进到“有前端行为目标的自主移动”。

本轮仍不做玩家指定员工行动，不做完整排班、疲劳、满意度、产能或经营收益计算。Godot 只负责表现层行为基线：员工可以按岗位寻找合适设施，走到设施相邻交互格，进入短暂使用状态，然后释放设施。

## 已完成

1. 员工设施目标选择
   - `EmployeeAutonomyController` 接入 `FacilityPlacementStore` 与 `Facility3DRenderer`。
   - 员工空闲时优先尝试 `TryStartFacilityUseBehavior`。
   - 程序岗位优先寻找 `OfficeDesk`。
   - 策划、市场岗位优先寻找 `ProductWhiteboard`。
   - 目标设施被占用或预订时会跳过，避免多个员工同时抢同一个设施。

2. 设施交互格与路径校验
   - 员工不会直接站到设施占用格。
   - 控制器通过设施上下左右相邻格生成交互候选点。
   - 候选点必须通过 `EmployeeStore.CanMoveEmployee`。
   - 候选路径必须通过 `OfficeNavigationStore.FindPath`，确保行为继续建立在 V2-0.19 的通行基础上。

3. 行为状态扩展
   - `EmployeeActivityKind` 增加 `WalkingToFacility` 与 `UsingFacility`。
   - `EmployeeAutonomyState` 增加 `FacilityId`，用于追踪当前设施行为。
   - 员工抵达设施交互格后进入 `UsingFacility`。
   - 使用计时结束后员工回到 `Idle`，设施预订和使用状态被释放。

4. 设施使用表现
   - `Facility3DRenderer` 增加 `_usingFacilityIds`。
   - 新增 `SetFacilityUseState(int facilityId, bool isInUse)`。
   - 设施被员工使用时保持独立描边，不依赖鼠标悬停或玩家选中。

## 自动化验证

新增静态回归测试覆盖：

- 自主控制器必须读取 `FacilityPlacementStore` 与 `Facility3DRenderer`。
- 员工自主行为必须包含 `WalkingToFacility` 与 `UsingFacility`。
- 员工设施行为必须通过 `FindFacilityUseTarget`。
- 设施交互点必须来自 `GetFacilityInteractionCells(facility)`。
- 设施目标路径必须调用 `OfficeNavigationStore.FindPath`。
- 员工移动必须继续使用 `Employee3DRenderer.PlayEmployeePathMove` 的平滑路径动画。
- 设施渲染器必须提供使用中描边状态。

## 实机验收

使用 Godot MCP 运行 `res://scenes/main.tscn` 后已验证：

- `InteractionRoot/EmployeeAutonomyController` 运行时节点存在。
- `Employee_1` 会自主移动到办公桌相邻交互格，例如观察到位置 `x=-65,z=-35`。
- 对应 `Facility_1` 办公桌位于 `x=-65,z=-45`，员工没有站到设施占用格。
- 等待约 5 秒后，设施使用状态释放，员工继续进入下一轮自主行为。
- 观察到 `Employee_2` 后续移动到白板附近交互区域，例如 `x=15,z=-35`。
- Godot 错误面板保持 `0 error`。
- 验证截图暂存到 `godot/GetTheBestGodot/addons/godot_mcp/cache`，提交前清理。

## 后续建议

下一轮建议继续做“员工自主行为系统”的地基，而不是玩家手动控制：

- 为员工增加前端行为意图优先级，例如工作、短暂停顿、返回岗位附近。
- 为设施增加更明确的可用交互点和容量概念。
- 将“正在使用设施”的表现状态扩展到员工头顶状态标记或简短动作反馈。
- 等 C# Core 行为调度接入后，再由规则核心决定员工为什么去某个设施、使用多久、产生什么经营影响。
