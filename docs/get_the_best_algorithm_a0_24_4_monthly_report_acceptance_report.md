# Algorithm A0.24.4 月报归因验收报告

## 验收目标

验证 Core 可以在月末 tick 输出确定性月报，让 Godot 后续只负责展示，不在 UI 中重新计算经营原因。

## 实现内容

- 新增 `MonthlyReport`。
- `FirstLoopBusinessTickOptions` 新增 `IsMonthEnd`。
- 只有月末 tick 才输出月报。
- 月报包含项目进度、活跃用户、收入、现金和原因列表。

## 月报字段

- `PeriodLabel`：当前算法线月报标签。
- `ProjectProgress`：tick 后项目进度。
- `ActiveUsers`：tick 后活跃用户。
- `Revenue`：本 tick 收入。
- `Cash`：tick 后现金。
- `Reasons`：经营原因列表。

## 测试证据

测试用例：
- `FirstLoopBusinessTests.MonthEndReportExplainsProgressUsersRevenueAndCash`
- `FirstLoopBusinessTests.NonMonthEndDoesNotEmitMonthlyReport`

覆盖断言：
- 月末 tick 输出月报。
- 非月末 tick 不输出月报。
- 月报数值来自 Core tick 结果。
- 月报原因包含 MVP 完成、市场转化、收入和成本归因。
