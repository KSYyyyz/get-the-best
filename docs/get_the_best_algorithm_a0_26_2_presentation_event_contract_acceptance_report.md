# Algorithm A0.26.2 `PresentationEvents` 可播语义验收报告

## 验收目标

确认每种 `SimulationEventKind` 都有明确的前端可播语义，Godot 可以按事件类型做最小表现桥接。

## 语义规则

- `IntentPlanned`：瞬时事件，关联员工，文字可选。
- `ActivityChanged`：状态变化，关联员工，不需要文字。
- `FacilityUpdated`：状态变化，关联设施，不需要文字。
- `MetricChanged`：瞬时事件，关联指标，文字可选。
- `MonthlyReportReady`：瞬时事件，关联月报，建议显示文字。
- `PhaseOutcomeReached`：瞬时事件，关联阶段结果，建议显示文字。

如果 Godot 找不到对应员工、设施、指标或 UI 承载对象，本阶段可以忽略该事件；Core 的事实状态仍以 `NextSnapshot`、`Tick` 和 `Outcome` 为准。

## 测试证据

测试用例：

- `SimulationFrontendContractTests.EverySimulationEventKindHasPlayableSemantics`

覆盖内容：

- 每个 `SimulationEventKind` 都存在一条语义记录。
- 每条语义记录都说明生命周期、文字显示策略、关联对象类型和 `SubjectId` 含义。
- 重点事件的语义值和 V2-0.26 对接约定一致。
