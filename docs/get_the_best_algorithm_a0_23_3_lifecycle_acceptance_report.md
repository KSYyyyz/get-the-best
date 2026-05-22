# 《壮志凌云 / Get The Best》Algorithm A0.23.3 员工行为生命周期规则验收报告

## 目标

补齐员工行为从意图到设施占用、使用和释放的 Core 生命周期规则。

## 规则内容

- `EmployeeState` 新增：
  - `ActiveFacilityId`：当前行为绑定的设施。
  - `RemainingActivityTicks`：当前行为剩余 tick。
- 新增 `EmployeeLifecycleEngine.Advance(snapshot, intents)`。
- `MoveToFacility` 意图不会立即占用设施，而是先进入 `MoveToFacility` 活动并预留目标设施。
- 员工处于 `MoveToFacility` 且计时结束时，若设施仍有容量，则进入 `UseFacility` 并写入设施占用。
- 员工处于 `UseFacility` 且计时结束时，释放设施并回到 `Idle`。
- 同一 tick 多个员工请求同一设施时，按员工 ID 稳定排序，只允许最先获得预留，保证确定性。

## 测试证据

新增 `LifecycleTests`：

- `MoveIntentStartsMoveBeforeFacilityUse`：移动意图先进入移动状态，不立即占用设施。
- `MovingEmployeeAcquiresFacilityOnNextTick`：移动结束后进入使用状态并占用设施。
- `UseLifecycleReleasesFacilityWhenTimerEnds`：使用结束后释放设施。
- `ConflictingMoveIntentsReserveFacilityDeterministically`：冲突意图按员工 ID 确定性预留。

局部验证命令：

```text
dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug
```

结果：`StartupSim.Core.Tests passed`。

## 边界

- 不计算路径，不接管 Godot 导航。
- 不修改 Godot 表现层。
- 只维护 Core 行为生命周期和设施占用事实。
