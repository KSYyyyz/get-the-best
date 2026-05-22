# Algorithm A0.27.4 前端解释事件验收报告

日期：2026-05-22

## 验收目标

Godot 不需要知道内部行为引擎顺序，也不需要手动拼接子引擎；只消费 `OfficeSimulationEngine.Advance(snapshot)` 的输出，就能获得意图、状态、经营 delta 和原因摘要。

## 已验收契约

- `SimulationTickResult` 顶层字段保持 A0.26 稳定：`Tick`、`NextSnapshot`、`Outcome`、`PresentationEvents`。
- 行为解释挂在 `Tick.Intents[*].Explanation`，避免破坏顶层前端合同。
- `PresentationEvents` 可输出“前往设施”“正在使用”“指标变化”“原因摘要”。
- `NextSnapshot` 可以继续作为下一次 `Advance` 的输入。

## 测试证据

- `EmployeeUtilityBehaviorTests.AdvanceEmitsPlayableUtilityExplanationEvents`
- `SimulationFrontendContractTests.SimulationTickResultKeepsStableFrontendFields`
- `SimulationFrontendContractTests.PresetSnapshotProducesMinimumFrontendAcceptanceEvents`
- `SimulationEngineTests.SimulationAdvanceIsDeterministic`

## 结论

A0.27.4 通过。前端仍只认统一 `Advance` 结果，同时可选择消费解释字段来显示调试或提示文字。
