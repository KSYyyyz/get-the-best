# Algorithm A0.26.1 `SimulationTickResult` 字段契约验收报告

## 验收目标

确认 Godot V2-0.26 可以稳定消费 `OfficeSimulationEngine.Advance` 的返回值，短期不随便改名或改语义。

## 固定字段

- `Tick`
- `NextSnapshot`
- `Outcome`
- `PresentationEvents`

## Godot 消费建议

- `Tick`：现金、收入、用户、MVP 等经营 delta。
- `NextSnapshot`：员工位置、员工活动状态、设施占用。
- `Outcome`：经营阶段胜利或失败结果。
- `PresentationEvents`：可播放提示和一次性表现事件。

## 测试证据

测试用例：

- `SimulationFrontendContractTests.SimulationTickResultKeepsStableFrontendFields`
- `SimulationFrontendContractTests.GodotFactSourcesAreDocumentedInCoreContract`

覆盖内容：

- 反射检查 `SimulationTickResult` 暴露字段名。
- 检查 `SimulationFrontendContract.ResultFields` 写明 Godot 消费来源。
