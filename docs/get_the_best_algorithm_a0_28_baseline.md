# Algorithm A0.28 发布后增长与产品表现基线

日期：2026-05-23

## 目标

A0.28 把 V2-1.12 的 `PublishPrototype` 从“固定成本、固定首批用户”升级为 Core 内的发布质量与产品表现模型。Godot 仍只提交玩家命令，发布结果、市场反馈、用户增长、流失、评分和收入变化都由 C# Core 结算。

本轮不改 Godot 表现层。

## 新增状态

`ProductMarketState` 追加兼容字段：

- `UserRating`：用户评分，0 到 5，未发布或未知时可为 0。
- `MarketAwareness`：市场认知，0 到 1，由市场调研和市场工作提升。
- `LaunchQuality`：发布质量，0 到 1，由 MVP 准备度和市场认知计算。
- `Retention`：留存，0 到 1，默认 0.75，由发布质量和服务器维护影响。

这些字段均有默认值，旧快照三参数构造仍然可用。

## 发布算法

`PublishPrototype` 不再固定给 20 名用户。

发布失败：

- 当 MVP 未完成且产品仍处于 `Prototype` 阶段时，发布失败。
- 本轮不扣发布费，不给用户。
- `PlayerCommandResult.Message` 说明失败原因。

发布成功：

- `LaunchQuality = clamp(0.35 + MVP准备度 * 0.45 + MarketAwareness * 0.2, 0, 1)`
- `UserRating = clamp(2.6 + LaunchQuality * 1.1 + MarketAwareness * 0.4, 1, 5)`
- `Retention = clamp(0.62 + LaunchQuality * 0.22 + MarketAwareness * 0.1, 0, 1)`
- `首批用户 = max(1, round(3 + LaunchQuality * 8 + MarketAwareness * 12))`

因此，MVP 完成但无调研时仍可发布，但首批用户较少、评分偏低；市场认知越高，发布质量、评分和首批用户越好。

## 发布后增长

已发布产品每个 `OfficeSimulationEngine.Advance(snapshot)` tick 都会进行发布后市场结算：

- 市场员工在市场房间使用白板会带来新增用户和市场认知。
- 运维或工程员工在服务器房间维护服务器会保护评分和留存。
- 低评分且缺少服务器维护时会产生用户流失，并下调评分/留存。
- 月度收入按当前 MRR 折算到 tick，MRR 由活跃用户变化驱动。

## 前端契约

`SimulationTickResult` 顶层字段仍保持：

- `Tick`
- `NextSnapshot`
- `Outcome`
- `PresentationEvents`

新增产品表现数据位于：

- `NextSnapshot.Company.ProductMarket`
- `Tick.ProductMarketDelta`
- `Tick.PlayerCommandResults`
- `PresentationEvents`
- `MonthlyReport.Reasons`

Godot 可以继续只读现有 HUD 总量，也可以后续选择读取新增产品表现字段。

## 验收证据

- `ProductLaunchGrowthTests.PublishFailsWhenMvpIsNotReady`
- `ProductLaunchGrowthTests.PublishWithoutResearchLaunchesWithLowerUsersAndRating`
- `ProductLaunchGrowthTests.MarketResearchImprovesLaunchUsersAndRating`
- `ProductLaunchGrowthTests.LaunchedProductGrowsFromMarketWorkAndServerMaintenanceProtectsRetention`
- `ProductLaunchGrowthTests.LowRatingAndNoServerMaintenanceCanCauseChurn`
- `ProductLaunchGrowthTests.SimulationTickResultTopLevelContractStaysStable`
- `SimulationFrontendContractTests.SimulationTickResultKeepsStableFrontendFields`
