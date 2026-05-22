# 《壮志凌云 / Get The Best》Algorithm A0.26 基线

## 目标

A0.26 配合 Godot V2-0.26，冻结 `OfficeSimulationEngine.Advance(snapshot, options)` 的前端消费契约。

本阶段不新增复杂经营规则，不改 Godot 表现层，不做 UI，不做 AI 玩法。核心是让 Godot 后续只消费统一 `Advance` 结果，不再手动拼接 Core 内部 engine。

## 稳定入口

Godot V2-0.26 以后只认：

```csharp
SimulationTickResult result = new OfficeSimulationEngine(options).Advance(snapshot);
```

Godot 不应手动调用：

- `EmployeeBehaviorEngine`
- `EmployeeLifecycleEngine`
- `FirstLoopBusinessEngine`
- `OfficeStateReducer`

## `SimulationTickResult` 稳定字段

短期稳定字段为：

| 字段 | Core 语义 | Godot 消费建议 |
| --- | --- | --- |
| `Tick` | 本 tick 的意图、员工 delta、设施 delta、公司 delta、产品市场 delta 和月报 | 现金、收入、用户、MVP 等 delta |
| `NextSnapshot` | Core 计算后的下一帧办公室事实快照，可继续作为下一次 `Advance` 输入 | 员工位置/活动状态、设施占用的表现事实 |
| `Outcome` | Core 判定的经营阶段结果 | 经营阶段胜利或失败结果 |
| `PresentationEvents` | 前端可播放的一次性提示或状态变化事件 | 可播放提示和一次性表现事件 |

## Godot 表现事实来源

- 员工位置/活动状态：读 `NextSnapshot.Employees`
- 设施占用：读 `NextSnapshot.Facilities`
- 可播放提示：读 `PresentationEvents`
- 经营阶段结果：读 `Outcome`
- 现金、收入、用户、MVP 等 delta：读 `Tick`

## `SimulationEventKind` 可播语义表

| 事件 | 生命周期 | 文字显示 | 关联对象 | `SubjectId` 含义 | 对象缺失时 |
| --- | --- | --- | --- | --- | --- |
| `IntentPlanned` | 瞬时事件 | 可选 | 员工 | `EmployeeId` | 可忽略 |
| `ActivityChanged` | 状态变化 | 不需要 | 员工 | `EmployeeId` | 可忽略 |
| `FacilityUpdated` | 状态变化 | 不需要 | 设施 | `FacilityId` | 可忽略 |
| `MetricChanged` | 瞬时事件 | 可选 | 指标 | 指标名或项目 id | 可忽略 |
| `MonthlyReportReady` | 瞬时事件 | 建议显示 | 月报 | 月报周期标签 | 可忽略 |
| `PhaseOutcomeReached` | 瞬时事件 | 建议显示 | 阶段结果 | `PhaseOutcomeKind` | 可忽略 |

## Tick 节奏建议

- 前端默认每 `2.0` 秒调用一次 Core tick。
- `TickHours` 默认使用 `1.0`。
- 员工走路动画期间不暂停 Core tick；Core 生命周期仍然是事实来源，Godot 只做表现插值。
- V2-0.26 先不接月末；`IsMonthEnd` 默认保持 `false`。

这些建议已写入 `SimulationFrontendContract.Cadence` 并由测试覆盖。

## 前端最小验收场景

使用当前预设办公室快照调用一次 `OfficeSimulationEngine.Advance` 后，算法端确认至少应该出现：

- 一个员工意图事件。
- 一个员工活动变化事件。
- 可能的设施占用变化事件。
- `NextSnapshot` 可以继续作为下一帧 `Advance` 输入。

## 测试入口

```powershell
$env:PATH='D:\Get The Best\.work\dotnet;' + $env:PATH
dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug
```
