# Algorithm A0.27 员工 Utility 自主行动基线

日期：2026-05-22

## 目标

A0.27 固定“员工自主行动系统第一版”的 Core 算法地基：员工在 `OfficeSimulationEngine.Advance(snapshot)` 内通过可解释 Utility 评分选择下一件事，再由生命周期和经营 tick 统一产出下一帧事实、经营 delta 与可播事件。

本轮不改 Godot 表现层，不新增 AI 玩法，不把经营规则复制到前端。

## 新增 Core 契约

- `EmployeeState` 追加简化动机字段：`NeedRest`、`NeedFood`、`NeedToilet`，默认值为 0，兼容旧快照。
- `EmployeeActionCandidateKind` 定义 A0.27 行动候选：`WorkAtDesk`、`UseWhiteboard`、`MaintainServer`、`Rest`、`Idle`。
- `EmployeeActionCandidate` 输出每个候选的 0 到 1 分数、目标设施、目标房间、评分原因和拒绝原因。
- `EmployeeDecisionExplanation` 输出员工最终选择、最终分数、全部候选项和原因摘要。
- `EmployeeBehaviorPlan` 输出 `Intents` 与 `Explanations`。
- `EmployeeIntent` 保持原有字段，并追加 `SourceAction` 与 `Explanation`；因此 A0.26 的 `SimulationTickResult` 顶层字段仍然只保留 `Tick / NextSnapshot / Outcome / PresentationEvents`。

## 行为算法

`EmployeeBehaviorEngine.PlanDecisions(snapshot)` 是 A0.27 新的行为规划入口，`PlanIntents(snapshot)` 保留为兼容包装。

评分顺序固定为员工 `Id` 字典序，设施选择固定为效率降序再按设施 `Id` 字典序，保证结果确定。

候选评分考虑：

- 岗位适配：工程师偏向办公桌和服务器，策划/设计偏向白板，市场偏向白板，运营偏向服务器。
- 公司目标：MVP 未完成时提高研发工作分；MVP 完成后提高市场白板分；已有用户或已发布时提高服务器维护分。
- 设施条件：设施类型、房间类型、容量、占用、预留、房间设施登记、逻辑交互站位。
- 员工动机：疲劳、精力、满意度、休息/食物/厕所需求。
- 环境质量：设施效率、房间舒适度、噪音。

## 计算公式

工作类候选基础公式：

`score = clamp(0.25 + 岗位适配 * 0.25 + 目标适配 * 0.25 + 设施质量 * 0.15 + 工作压力 * 0.10 - 疲劳惩罚, 0, 1)`

休息候选基础公式：

`score = clamp(0.10 + 休息压力 * 0.75 + 休息设施质量 * 0.10, 0, 1)`

其中：

- `工作压力 = clamp(精力 / 100 * 0.65 + 满意度 / 100 * 0.35, 0, 1)`
- `休息压力 = clamp(疲劳 / 100 * 0.6 + 缺精力压力 * 0.3 + NeedRest * 0.1 + NeedFood * 0.025 + NeedToilet * 0.025, 0, 1)`
- `设施质量 = clamp(0.75 + (设施效率 - 1.0) * 0.4 + 房间舒适度 - 房间噪音, 0, 1)`
- `疲劳惩罚 = max(疲劳 - 50, 0) / 160`

## 短计划边界

A0.27 不是完整 GOAP。员工每次只承诺一个短行动：

- 已在移动、使用设施或工作时，继续当前短计划，等待下一次 AI tick 再评估。
- 空闲员工根据候选分数选择一个行动。
- 设施被本轮前序员工预留后，后序员工不会抢同一设施。
- 高疲劳员工选择休息时，生命周期会占用休息设施并把员工置为 `Rest`。

## 前端消费方式

Godot 继续只调用：

`OfficeSimulationEngine.Advance(snapshot)`

前端仍按 A0.26 顶层字段消费：

- 员工活动和设施占用：`NextSnapshot`
- 经营 delta：`Tick.CompanyDelta`
- 行为意图：`Tick.Intents`
- 行为解释：`Tick.Intents[*].Explanation`
- 可播提示：`PresentationEvents`
- 阶段结果：`Outcome`

`PresentationEvents` 的消息已经包含“前往设施”“正在使用”“指标变化”“原因摘要”等最小可播文字，Godot 可显示，也可以只用事件类型和对象 id 播放动画。

## 验收证据

- `EmployeeUtilityBehaviorTests.EngineerPrioritizesDeskWorkForMvpWhenDeskIsAvailable`
- `EmployeeUtilityBehaviorTests.UnusableDeskDoesNotProduceMvpProgress`
- `EmployeeUtilityBehaviorTests.HighFatigueRaisesRestCandidateScore`
- `EmployeeUtilityBehaviorTests.RestIntentStartsRestFacilityUse`
- `EmployeeUtilityBehaviorTests.OccupiedFacilityIsNotAssignedToSecondEmployee`
- `EmployeeUtilityBehaviorTests.AdvanceEmitsPlayableUtilityExplanationEvents`
- `SimulationFrontendContractTests.SimulationTickResultKeepsStableFrontendFields`
- `dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug`
