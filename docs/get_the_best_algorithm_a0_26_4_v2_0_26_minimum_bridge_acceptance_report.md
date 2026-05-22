# Algorithm A0.26.4 V2-0.26 最小桥接验收报告

## 验收目标

确认 Godot 用当前预设办公室快照调用一次 `OfficeSimulationEngine.Advance` 后，可以获得最小可播放结果，并且 `NextSnapshot` 能继续作为下一帧输入。

## 最小验收预期

一次 `Advance` 后至少应出现：

- 一个员工意图事件。
- 一个员工活动变化。
- 可能的设施占用变化。
- 一个可继续输入下一次 `Advance` 的 `NextSnapshot`。

## Godot 侧最小桥接建议

1. 从 Godot 当前办公室事实组装 `OfficeRuleSnapshot`。
2. 调用 `OfficeSimulationEngine.Advance(snapshot, options)`。
3. 用 `NextSnapshot.Employees` 刷新员工表现状态。
4. 用 `NextSnapshot.Facilities` 刷新设施占用表现。
5. 用 `PresentationEvents` 播放轻提示或状态变化。
6. 暂不接月末 UI，保持 `IsMonthEnd=false`。

## 测试证据

测试用例：

- `SimulationFrontendContractTests.PresetSnapshotProducesMinimumFrontendAcceptanceEvents`

覆盖内容：

- 预设快照产生 `IntentPlanned`。
- 预设快照产生 `ActivityChanged`。
- 预设快照产生 `FacilityUpdated`。
- `NextSnapshot` 可以再次传给 `Advance`。
