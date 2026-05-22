# Algorithm A0.26.3 Tick 节奏验收报告

## 验收目标

给 Godot V2-0.26 一个明确、保守的 Core tick 调用建议，避免前端用表现动画节奏反向控制经营规则。

## 当前建议

- 前端每 `2.0` 秒调用一次 `OfficeSimulationEngine.Advance`。
- `TickHours` 默认 `1.0`。
- 员工走路动画期间不暂停 tick。
- V2-0.26 暂不接月末，`IsMonthEnd` 默认 `false`。

## 边界说明

Core tick 是经营事实推进；Godot 动画是表现插值。动画未播完时，Godot 可以选择延迟视觉更新，但不应要求 Core 暂停规则时间。

## 测试证据

测试用例：

- `SimulationFrontendContractTests.TickCadenceMatchesV026BridgeRecommendation`

覆盖内容：

- `SimulationFrontendContract.Cadence.RecommendedRealSecondsPerTick == 2.0`
- `DefaultTickHours == 1.0`
- `PauseDuringEmployeeWalkAnimation == false`
- `UseMonthEndInV026Bridge == false`
