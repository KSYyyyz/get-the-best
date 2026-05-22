# 《壮志凌云 / Get The Best》V2-0.24 Core 员工意图桥接验收报告

## 本轮目标

在 Algorithm A0.23 独立规则基线完成后，把 Godot 员工自主行动从“前端按岗位挑设施”推进为“读取 Core 员工意图后播放表现”。

本轮只做最小桥接：Godot 组装办公室事实快照，调用 Core 规划员工意图，再用现有导航、动画、状态牌和设施描边表现这些意图。经营公式仍只存在于 C# Core。

## 已完成

1. Godot 工程引用 Core
   - `godot/GetTheBestGodot/GetTheBestGodot.csproj` 增加 `StartupSim.Core` 项目引用。
   - Godot C# 编译会同时编译 Core，但 Godot 侧仍不写经营公式。

2. `V2CoreBridge` 从占位升级为最小桥接
   - 使用 Core 的 `GodotCoreBridgeContract`。
   - 使用 Core 的 `EmployeeBehaviorEngine`。
   - 从 `EmployeeStore`、`FacilityPlacementStore`、`RoomFootprintStore` 组装 `GodotOfficeSnapshotDto`。
   - 将 Godot 员工、设施、房间、公司事实映射成 Core `OfficeRuleSnapshot`。
   - 调用 `PlanEmployeeIntents` 并把 Core 的 `employee-{id}` / `facility-{id}` 转回 Godot 可用的整数 ID。

3. 员工自主行动消费 Core 意图
   - `EmployeeAutonomyController` 增加 `V2CoreBridge` 引用。
   - 员工空闲时优先调用 `_v2CoreBridge.PlanEmployeeIntents(...)`。
   - 当 Core 返回 `MoveToFacility` 且带有 `FacilityId` 时，Godot 只负责寻找该设施相邻可站格、播放路径动画、显示状态牌和设施使用表现。
   - 已移除 Godot 前端里的岗位到设施偏好判断，不再由前端复制这部分规则。

## 自动化验证

新增或更新静态回归测试覆盖：

- Godot 工程必须引用 `StartupSim.Core`。
- `V2CoreBridge` 必须使用 `GodotCoreBridgeContract` 和 `EmployeeBehaviorEngine`。
- `V2CoreBridge` 必须组装 `GodotOfficeSnapshotDto` 及员工、设施、房间 DTO。
- `EmployeeAutonomyController` 必须通过 `V2CoreBridge.PlanEmployeeIntents` 获取 Core 意图。
- Godot 自主行动不再保留 `GetDesiredFacilityTypes` 这类前端岗位设施偏好规则。

## 实机验收

使用 Godot MCP 运行 `res://scenes/main.tscn` 后已验证：

- `/root/Main/V2CoreBridge` 运行时节点存在。
- Core intent 桥接后，员工仍会自主移动到 Core 指定设施附近。
- 观察到 `Employee_1` 到达 `Facility_1` 相邻交互格：员工 `x=-65,z=-35`，设施 `x=-65,z=-45`。
- 观察到 `Employee_2` 出现 `ActivityBadge`，文本为“前往设施”。
- Godot 错误面板保持 `0 error`。
- 验证截图暂存到 `godot/GetTheBestGodot/addons/godot_mcp/cache`，提交前清理。

## 边界确认

- Godot 没有接管产能、疲劳、满意度、现金、项目进度公式。
- Godot 只做 DTO 转换、导航落点、动画播放和状态表现。
- Core 规则算法继续按 Algorithm 版本线独立演进。
- 后续每轮前端小阶段完成时，都应检查新的 Algorithm 版本；在逻辑稳定且测试通过时优先接入主线。

## 后续建议

下一轮可以继续做 Core 接入后的表现闭环：

- 接入 Core `EmployeeLifecycleEngine`，让 Godot 表现层跟随 `MoveToFacility -> UseFacility -> Idle/Rest` 的生命周期。
- 在 Godot 侧保留路径和站位判定，但让行为持续时间、设施占用和释放来自 Core 生命周期快照。
- 等生命周期接稳后，再考虑展示 `BusinessTickEngine.Tick` 的经营 delta。
