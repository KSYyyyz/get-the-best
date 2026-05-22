# Algorithm A0.25.3 阶段胜利和失败验收报告

## 验收目标

验证第一局核心阶段结果由 Core 判断，而不是由 Godot UI 或表现层复制规则。

## 实现内容

- 新增 `PhaseOutcomeKind`。
- 新增 `PhaseOutcome`。
- `OfficeSimulationEngine` 在每次 tick 后根据 tick delta 和下一帧快照判断阶段结果。

## 当前阶段结果

- `InProgress`：未达成阶段节点。
- `MvpCompleted`：MVP 完成。
- `FirstUsersAcquired`：获得首批活跃用户。
- `RevenuePositive`：收入覆盖本 tick 成本。
- `FailedCashDepleted`：现金耗尽。

## 优先级

失败优先级最高。只要 tick 后现金小于等于 0，即输出 `FailedCashDepleted`。

## 测试证据

测试用例：
- `SimulationEngineTests.CashDepletionProducesFailureOutcome`
- `SimulationEngineTests.FirstUsersProducePhaseOutcome`

覆盖断言：
- 现金耗尽时输出失败。
- tick 后现金小于等于 0。
- 获得首批用户时输出 `FirstUsersAcquired`。
- 阶段结果原因包含“获得首批活跃用户”。
