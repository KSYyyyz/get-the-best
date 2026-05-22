# Algorithm A0.26.5 公司总量桥接验收报告

## 验收目标

确认 Godot HUD 可以直接读取当前总现金、当前 MVP 总进度、当前活跃用户和当前 MRR，而不是只拿本 tick delta 自己推算总量。

## 实现内容

- Core 契约新增 `SimulationFrontendContract.CompanyTotals`。
- Godot bridge 新增 `CoreCompanySimulationTotals`。
- `CoreOfficeSimulationResult` 新增 `CompanyTotals` 字段。
- `V2CoreBridge.MapSimulationResult` 从 `result.NextSnapshot.Company` 映射总量。

## 字段来源

- `CurrentCash`：`NextSnapshot.Company.Cash`
- `CurrentProjectProgress`：`NextSnapshot.Company.ActiveProject.Progress`
- `ProjectRequiredProgress`：`NextSnapshot.Company.ActiveProject.RequiredProgress`
- `CurrentActiveUsers`：`NextSnapshot.Company.ProductMarket.ActiveUsers`
- `CurrentMonthlyRecurringRevenue`：`NextSnapshot.Company.ProductMarket.MonthlyRecurringRevenue`
- `ProductStage`：`NextSnapshot.Company.ProductMarket.Stage`

## 边界说明

- `Tick.CompanyDelta` 继续表达本 tick 增量。
- `CompanyTotals` 表达 tick 后总量。
- Godot HUD 应优先显示 `CompanyTotals`，只在需要播放变化动画时使用 delta。

## 测试证据

测试用例：

- `SimulationFrontendContractTests.CompanyTotalsAreDocumentedForHudConsumption`
- `test_get_the_best_v2_0_26_bridge_exposes_company_totals_for_hud`

覆盖内容：

- Core 契约表列出 HUD 总量字段。
- Godot bridge 映射 `result.NextSnapshot.Company` 到 `CoreOfficeSimulationResult.CompanyTotals`。
