# Algorithm A0.24.3 收入和现金流验收报告

## 验收目标

验证活跃用户可以产生收入，收入进入 `CompanyTickDelta`，并和经营成本共同决定现金变化。

## 实现内容

- `CompanyTickDelta` 新增 `RevenueDelta`。
- `FirstLoopBusinessEngine` 根据月经常收入计算 tick 收入。
- 现金变化不再只等于成本，而是收入与成本的合计。

## 公式

- `月经常收入 = 活跃用户 * 12`
- `tick收入 = 月经常收入 / 30 / 8 * TickHours`
- `现金变化 = tick收入 + tick经营成本`
- `tick经营成本 = -MonthlyCostRate / 30 / 8 * TickHours`

## 测试证据

测试用例：`FirstLoopBusinessTests.RevenueOffsetsOperatingCost`

覆盖断言：
- 有活跃用户时 `RevenueDelta > 0`。
- `CashDelta == OperatingCostDelta + RevenueDelta`。
