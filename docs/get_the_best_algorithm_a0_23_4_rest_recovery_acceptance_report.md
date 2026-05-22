# 《壮志凌云 / Get The Best》Algorithm A0.23.4 休息与恢复规则验收报告

## 目标

补齐高疲劳员工的休息意图和 Core tick 恢复公式，让疲劳不再只有积累没有恢复出口。

## 规则内容

- `EmployeeBehaviorEngine` 在员工疲劳达到阈值时输出 `Rest` 意图。
- 如果存在可用 `RestSeat`，休息意图会带上 `FacilityId` 和 `RoomId`。
- `BusinessTickEngine` 对 `Rest` 活动结算恢复：
  - 疲劳下降。
  - 精力上升。
  - 工作产出为 0。
  - 满意度按休息环境小幅上升。
  - 使用休息设施时输出对应 `FacilityTickDelta`。
- 恢复倍率由休息设施效率和房间舒适度/噪音共同决定。
- 公式使用 `BusinessTickOptions.TickHours`，保持不同 tick 粒度下结果可解释。

## 测试证据

新增 `RestRecoveryTests`：

- `HighFatigueEmployeeTargetsRestFacility`：高疲劳员工优先选择可用休息设施。
- `RestTickRecoversFatigueAndEnergy`：休息 tick 降低疲劳、恢复精力且不产生工作产出。
- `RestFacilityAndRoomImproveRecovery`：休息设施和房间环境会提高恢复效果。
- `RestTickReportsRestFacilityUse`：休息设施使用会进入设施 delta。

局部验证命令：

```text
dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug
```

结果：`StartupSim.Core.Tests passed`。

## 边界

- 休息规则仍在 C# Core。
- Godot 后续只需要根据 `Rest` 意图表现移动或休息动画。
- 本轮不做员工养成、离职风险或完整情绪系统。
