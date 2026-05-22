# 《壮志凌云 / Get The Best》V2-0.22 员工自主行为与经营计算核心算法骨架验收报告

## 本轮目标

本轮从 Godot 表现层切回 C# Core，建立“员工行为意图 -> 设施使用 -> 产能、疲劳、满意度、现金成本、项目进度变化”的第一版确定性规则闭环。

本轮不改 Godot 表现层、不改 UI、不做美术、不加入 AI 玩法。Godot 后续只需要把当前办公室事实快照转换为 Core 输入，并读取 Core 输出的员工意图和 tick delta。

## 已完成

1. 新增独立 C# Core 工程
   - 路径：`csharp/StartupSim.Core/StartupSim.Core.csproj`
   - 目标框架：`net8.0`
   - 不引用 Godot API，不依赖 Godot 节点、场景或渲染对象。

2. 新增规则数据契约
   - `OfficeRuleSnapshot`：办公室事实快照入口。
   - `EmployeeState`：岗位、技能、精力、疲劳、满意度、当前活动、房间和逻辑格引用。
   - `FacilityState`：设施类型、所在房间、容量、占用员工、效率修正。
   - `RoomState`：房间类型、舒适度、噪音、容量、设施列表。
   - `CompanyState` / `ProjectState`：现金、月成本、当前项目进度。
   - `TickResult`：本 tick 的员工、设施和公司经营变化。

3. 新增前端可接入输出
   - `EmployeeIntent`：`Idle` / `MoveToFacility` / `UseFacility` / `Rest` / `Work`。
   - `IntentTarget`：`FacilityId`、`RoomId`、`TargetCell`。
   - `EmployeeTickDelta`：疲劳、精力、满意度、产出变化。
   - `FacilityTickDelta`：设施使用中状态、占用数、效率倍率。
   - `CompanyTickDelta`：项目进度、现金变化、经营成本变化。

4. 新增行为意图引擎
   - `EmployeeBehaviorEngine.PlanIntents(snapshot)`。
   - 按员工岗位选择目标设施：
     - 工程：优先 `OfficeDesk`，其次 `ServerRack`。
     - 设计：优先 `OfficeDesk`，其次 `ProductWhiteboard`。
     - 策划/市场：优先 `ProductWhiteboard`，其次 `OfficeDesk`。
     - 运营：优先 `ServerRack`，其次 `OfficeDesk`。
   - 设施容量已满时跳过。
   - 同一轮规划会预留已分配设施，避免多个员工继续抢同一个满容量目标。
   - 疲劳达到 85 及以上时输出 `Rest` 意图，为后续休息行为接入预留规则入口。

5. 新增经营 tick 引擎
   - `BusinessTickEngine.Tick(snapshot)`。
   - 对处于 `UseFacility` 且确实占用设施的员工结算工作产能。
   - 产能公式为：
     - `输出 = 员工技能 * 综合效率 * tick小时数`
     - `综合效率 = 疲劳倍率 * 精力倍率 * 设施效率修正 * 房间倍率 * 岗位匹配倍率`
   - 疲劳倍率：
     - 疲劳小于等于 50 时为 `1.0`。
     - 疲劳超过 50 后线性下降，最低不低于 `0.4`。
   - 房间倍率：
     - `1.0 + 舒适度 - 噪音`，限制在 `0.75` 到 `1.25`。
   - 岗位不匹配设施时倍率为 `0.75`。
   - 工作每 tick 增加疲劳、降低精力，并根据房间舒适度、噪音和高疲劳压力调整满意度。
   - 公司现金按月成本折算为 tick 成本，项目进度按本 tick 产能推进。

## 测试覆盖

新增无外部测试框架依赖的 Core 测试运行器：

- 路径：`csharp/StartupSim.Core.Tests/StartupSim.Core.Tests.csproj`
- 本地命令：
  - `dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug`

覆盖用例：

- 工程岗位会选择 `OfficeDesk`。
- 设施容量满时不会继续分配到该设施。
- 使用设施会推进项目进度。
- 工作会增加疲劳。
- 高疲劳会降低有效产出。
- 同一快照重复 tick 结果确定一致。
- Core 程序集不引用 `GodotSharp`、`GodotSharpEditor` 或 `Godot`。

## CI 更新

`.github/workflows/docs.yml` 新增 C# Core 规则测试步骤：

```text
dotnet run --project csharp/StartupSim.Core.Tests/StartupSim.Core.Tests.csproj --configuration Debug
```

这样后续 push 和 pull request 会同时检查文档、Python 静态脚手架测试、Core 规则测试和 Godot C# 编译。

## Godot 后续接入方式

后续 Godot 桥接层建议只做 DTO 转换：

1. 从 `EmployeeStore`、`FacilityPlacementStore`、`RoomFootprintStore` 和公司状态源组装 `OfficeRuleSnapshot`。
2. 调用 `EmployeeBehaviorEngine.PlanIntents(snapshot)` 获取员工下一步行为意图。
3. 调用 `BusinessTickEngine.Tick(snapshot)` 获取经营计算结果。
4. Godot 根据 `EmployeeIntent` 播放移动、使用设施、休息等表现。
5. Godot 根据 `TickResult` 刷新状态展示，但不在 UI 或控制器里复制产能、疲劳、满意度、现金、项目进度公式。

## 边界确认

- 未修改 Godot 表现层逻辑。
- 未把经营公式写入 Godot UI。
- 未引入随机数。
- 未做完整复杂经营系统，只完成第一版小闭环。
- 当前仍不做 AI 玩法。

## 后续建议

下一轮可以新增 Godot 到 Core 的最小桥接快照，不直接改 UI 规则：

- 将现有视觉岗位字符串映射到 `EmployeeRole`。
- 将 Godot 设施类型映射到 `FacilityType`。
- 将房间类型和简单舒适度/噪音参数映射到 `RoomState`。
- 让 Godot 前端先消费 `EmployeeIntent` 驱动表现层移动和使用状态，再逐步展示 `TickResult` 中的经营变化。
