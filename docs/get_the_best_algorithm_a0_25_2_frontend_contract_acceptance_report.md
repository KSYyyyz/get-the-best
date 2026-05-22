# Algorithm A0.25.2 前端接入契约验收报告

## 验收目标

验证 Godot 后续可以通过一个稳定 Core 返回值接入经营规则，不需要在表现层知道内部 engine 顺序。

## 实现内容

- 新增 `SimulationTickResult`。
- 新增 `SimulationPresentationEvent` 和 `SimulationEventKind`。
- 统一返回 `Tick`、`NextSnapshot`、`Outcome`、`PresentationEvents`。
- 月末选项通过 `SimulationTickOptions.IsMonthEnd` 传入统一入口。

## 边界

`PresentationEvents` 只表达“可以播放什么”，不携带经营公式。项目进度、现金、收入、用户和阶段结果仍然由 Core 计算。

## 测试证据

测试用例：
- `SimulationEngineTests.AdvanceReturnsStableFrontendContract`
- `SimulationEngineTests.MonthEndOptionEmitsReportThroughSimulationResult`

覆盖断言：
- 返回 intents、员工 delta、设施 delta、播放事件。
- 第一个播放事件是 intent 规划。
- 月末 tick 输出月报。
- 月报可以作为播放事件传给前端。
