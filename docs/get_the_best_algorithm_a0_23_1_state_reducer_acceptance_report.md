# 《壮志凌云 / Get The Best》Algorithm A0.23.1 Core 状态推进规则验收报告

## 目标

建立 Core 内部的状态推进规则，让 `OfficeRuleSnapshot + TickResult` 可以得到下一份确定性快照。

## 规则内容

- 新增 `OfficeStateReducer.ApplyTickResult(snapshot, result)`。
- 员工状态根据 `EmployeeTickDelta` 推进：
  - `CurrentActivity` 更新为 `NextActivity`。
  - `Fatigue`、`Energy`、`Satisfaction` 应用 delta 后钳制在 `0..100`。
- 公司状态根据 `CompanyTickDelta` 推进：
  - `Cash` 应用 `CashDelta`。
  - 当前项目 `Progress` 应用 `ProjectProgressDelta`，并钳制在 `0..RequiredProgress`。
- reducer 返回新快照，不修改输入快照。

## 测试证据

新增 `StateReducerTests`：

- `AppliesTickDeltasToNextSnapshot`：工作 tick 后项目进度上升、现金下降、疲劳上升、精力下降、活动变为 `Work`。
- `ClampsEmployeeAndProjectRanges`：员工数值和项目进度不会越界。
- `DoesNotMutateOriginalSnapshot`：原始快照保持不变。

局部验证命令：

```text
dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug
```

结果：`StartupSim.Core.Tests passed`。

## 边界

- 未修改 Godot 表现层。
- 未引入随机数。
- 未把经营状态推进写入 Godot UI。
