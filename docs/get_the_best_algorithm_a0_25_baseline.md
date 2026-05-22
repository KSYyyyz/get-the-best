# 《壮志凌云 / Get The Best》Algorithm A0.25 基线

## 目标

A0.25 建立 Core 统一 tick 编排器，让 Godot 后续只需要提交 `OfficeRuleSnapshot`，再读取 `SimulationTickResult`，不需要知道或手工组合 Core 内部的行为、生命周期、经营计算和 reducer 顺序。

本基线不做 Godot 表现层、不改 UI、不做 AI 玩法。

## 新增核心契约

- `OfficeSimulationEngine`
  - 第一局 Core 统一入口。
  - 固定执行顺序：行为意图 -> 员工生命周期 -> 第一局经营计算 -> 状态 reducer -> 阶段结果 -> 前端播放事件。
- `SimulationTickOptions`
  - `TickHours` 控制 tick 时间长度。
  - `IsMonthEnd` 控制是否输出月报。
- `SimulationTickResult`
  - `Tick`：Core 经营 tick delta。
  - `NextSnapshot`：Core 计算后的下一帧事实快照。
  - `Outcome`：阶段胜利或失败判断。
  - `PresentationEvents`：前端可播放事件，不包含经营公式。
- `PhaseOutcome`
  - 输出第一局阶段结果。
- `SimulationPresentationEvent`
  - 把意图、活动变化、设施变化、指标变化、月报和阶段结果转成稳定播放事件。

## 编排顺序

1. `EmployeeBehaviorEngine.PlanIntents(snapshot)`
2. `EmployeeLifecycleEngine.Advance(snapshot, intents)`
3. `FirstLoopBusinessEngine.Tick(lifecycleSnapshot)`
4. `OfficeStateReducer.ApplyTickResult(lifecycleSnapshot, tick)`
5. `OfficeSimulationEngine` 根据 tick 和 next snapshot 生成 `PhaseOutcome`
6. `OfficeSimulationEngine` 生成 `PresentationEvents`

## 阶段结果

- `InProgress`：没有达成阶段变化。
- `MvpCompleted`：MVP 从未完成推进到完成。
- `FirstUsersAcquired`：本 tick 获得首批活跃用户。
- `RevenuePositive`：本 tick 收入覆盖经营成本后现金正向增长。
- `FailedCashDepleted`：tick 后现金小于等于 0。

失败优先级最高；同一 tick 内如果现金耗尽，即使有其他正向事件，也输出失败。

## Godot 接入方式

Godot 后续只需要：

1. 组装办公室事实快照。
2. 调用 `OfficeSimulationEngine.Advance(snapshot, options)`。
3. 用 `Tick.Intents` 和 `PresentationEvents` 播放员工行为和设施表现。
4. 用 `NextSnapshot` 更新表现层缓存。
5. 用 `MonthlyReport` 和 `Outcome` 展示经营反馈。

Godot 不应自行调用多个 Core engine 来拼装 tick 顺序，也不应复制现金、收入、用户、项目进度或阶段胜败公式。

## 测试入口

```powershell
$env:PATH='D:\Get The Best\.work\dotnet;' + $env:PATH
dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug
```
