# Algorithm A0.28.2 发布后增长验收报告

日期：2026-05-23

## 验收目标

产品发布后，每个 Core tick 都应根据产品质量、用户评分、市场工作和服务器维护产生用户与收入变化。

## 已验收规则

- 已发布产品有市场工作时，会产生新增用户和 MRR 增量。
- 服务器维护会提升或保护留存，降低低评分带来的风险。
- 低评分且缺少服务器维护时，会产生用户流失。
- 用户变化会写入 `ProductMarketTickDelta.ActiveUsersDelta` 和 `MonthlyRecurringRevenueDelta`。

## 测试证据

- `ProductLaunchGrowthTests.LaunchedProductGrowsFromMarketWorkAndServerMaintenanceProtectsRetention`
- `ProductLaunchGrowthTests.LowRatingAndNoServerMaintenanceCanCauseChurn`
- `FirstLoopBusinessTests.MarketingWorkAddsFirstUsersAfterMvp`
- `FirstLoopBusinessTests.RevenueOffsetsOperatingCost`

## 结论

A0.28.2 通过。Core 已具备发布后增长、停滞和流失的第一版确定性模型。
