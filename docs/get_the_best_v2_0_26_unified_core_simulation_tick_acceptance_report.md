# 《壮志凌云 / Get The Best》V2-0.26 Core 统一模拟 tick 桥接验收报告

## 本轮目标

在 Algorithm A0.26 冻结前端消费契约后，Godot 前端从“分别调用 Core 行为意图和员工生命周期”升级为“只调用 Core 统一模拟 tick”。

本轮不做新的经营 UI，不复制经营公式，不改 AI 玩法。目标是把 Godot/Core 边界收紧：

- Godot 组装办公室事实快照。
- Godot 调用一次 `OfficeSimulationEngine.Advance(...)`。
- Godot 消费 `SimulationTickResult` 映射后的结果。
- 员工意图、生命周期、设施占用、经营 delta 和阶段结果的顺序由 Core 统一编排。

## 已完成

1. `V2CoreBridge` 改为统一 tick 入口
   - 新增 `OfficeSimulationEngine` 桥接。
   - 新增 `AdvanceOfficeSimulation(...)`。
   - 使用 `SimulationFrontendContract.Cadence` 读取前端推荐 tick 节奏。
   - 将 Core `SimulationTickResult` 映射为 Godot 可读的 `CoreOfficeSimulationResult`。
   - `PresentationEvents`、`OutcomeKind`、经营 delta、员工状态和设施占用状态统一从 Core 结果读取。

2. `EmployeeAutonomyController` 改为消费统一模拟结果
   - 删除对 `PlanEmployeeIntents(...)` 的调用。
   - 删除对 `AdvanceEmployeeLifecycle(...)` 的调用。
   - 员工自主行动从 `CoreOfficeSimulationResult.Intents` 读取移动意图。
   - 员工使用设施、设施占用释放从 `CoreOfficeSimulationResult.EmployeeStates` 和 `FacilityStates` 读取。
   - 前端 tick 节奏改为 `CoreSimulationTickSeconds`，默认来自 Core `SimulationFrontendContract.Cadence`。

3. Core 补齐文档中承诺的构造入口
   - `OfficeSimulationEngine` 新增 `OfficeSimulationEngine(SimulationTickOptions options)`。
   - Godot 不再需要知道或手动实例化 Core 内部的 behavior、lifecycle、business、reducer 顺序。

## 自动化验证

新增 V2-0.26 静态回归测试，先红后绿验证：

- `V2CoreBridge` 必须引用 `OfficeSimulationEngine`。
- `V2CoreBridge` 必须公开 `AdvanceOfficeSimulation(...)`。
- `V2CoreBridge` 必须消费 `SimulationFrontendContract.Cadence`。
- Godot 侧不再出现 `PlanEmployeeIntents(...)` 和 `AdvanceEmployeeLifecycle(...)`。
- `EmployeeAutonomyController` 必须通过 `AdvanceOfficeSimulation(...)` 消费统一 Core tick。

已执行并通过：

- `pytest tests/test_godot_v2_scaffold.py -q -k v2_0_26`
- `pytest tests/test_godot_v2_scaffold.py -q`
- `dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug`
- `dotnet build godot\GetTheBestGodot\GetTheBestGodot.csproj --configuration Debug`

## 边界确认

- Godot 不再手动拼接 `EmployeeBehaviorEngine`、`EmployeeLifecycleEngine`、`FirstLoopBusinessEngine` 或 `OfficeStateReducer`。
- Godot 仍然负责可见表现：寻路、站位、路径动画、活动标签、设施使用表现。
- Core 仍然负责事实结果：意图、生命周期、设施占用、经营 tick、阶段结果和可播放事件。
- V2-0.26 暂不接月末 UI，`IsMonthEnd` 按 A0.26 契约保持默认 `false`。

## 实机验收要点

使用 Godot MCP 运行 `res://scenes/main.tscn` 后已确认：

- `/root/Main/V2CoreBridge` 运行时节点存在。
- 员工仍可依据 Core 统一 tick 结果自主前往设施。
- `Employee_1` 到达 `Facility_1` 相邻交互格：员工位置观测为 `x=-65,z=-35`，设施位置为 `x=-65,z=-45`。
- `Employee_2` 也出现了由统一 tick 驱动后的自主位置变化，运行时位置观测为 `x=-75,z=-15`。
- Godot 错误面板保持 `0 error`。
- 验证截图暂存到 `godot/GetTheBestGodot/addons/godot_mcp/cache/v2_0_26_unified_tick.png`，提交前清理。

## 后续建议

下一阶段可以开始消费 `PresentationEvents` 和经营 delta 的最小可见反馈：

- 先做只读 HUD 或轻提示，不做复杂面板。
- 优先显示 MVP 进度、现金变化、阶段结果。
- 继续保持经营公式只在 Core 中存在。
