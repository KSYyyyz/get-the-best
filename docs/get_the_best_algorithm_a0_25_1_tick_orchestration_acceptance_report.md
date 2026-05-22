# Algorithm A0.25.1 Tick 编排验收报告

## 验收目标

验证 Core 有唯一的第一局 tick 编排入口，并且执行顺序固定为：行为意图 -> 生命周期推进 -> 经营计算 -> 状态 reducer。

## 实现内容

- 新增 `OfficeSimulationEngine`。
- `Advance` 输入 `OfficeRuleSnapshot`，输出 `SimulationTickResult`。
- 编排器先规划员工意图，再推进生命周期，再运行 `FirstLoopBusinessEngine`，最后用 `OfficeStateReducer` 生成下一帧快照。

## 验收要点

移动中的员工在本 tick 先由生命周期进入设施使用状态，再由经营计算产生项目进度。这证明经营 tick 发生在生命周期之后。

## 测试证据

测试用例：`SimulationEngineTests.AdvancePlansLifecycleBusinessAndReducerInOrder`

覆盖断言：
- 输出包含员工 intent。
- 下一帧员工活动为 `Work`。
- 下一帧员工仍绑定目标设施。
- 本 tick 产生项目进度。
- reducer 后项目进度达到需求线。
