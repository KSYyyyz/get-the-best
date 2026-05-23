# Algorithm A0.28.1 发布质量验收报告

日期：2026-05-23

## 验收目标

发布原型软件不能再固定给用户，而要由 Core 根据 MVP 准备度、市场认知和发布质量计算。

## 已验收规则

- MVP 未完成时发布失败，不扣发布费，不给用户。
- MVP 完成但市场认知为 0 时可发布，但首批用户少于旧固定 20，评分较低。
- 市场认知较高时，发布获得更多首批用户和更高评分。
- 发布结果通过 `PlayerCommandResult.Message` 解释。

## 测试证据

- `ProductLaunchGrowthTests.PublishFailsWhenMvpIsNotReady`
- `ProductLaunchGrowthTests.PublishWithoutResearchLaunchesWithLowerUsersAndRating`
- `ProductLaunchGrowthTests.MarketResearchImprovesLaunchUsersAndRating`
- `FirstLoopBusinessTests.PublishPrototypeCommandLaunchesReadyMvpWithInitialUsers`

## 结论

A0.28.1 通过。发布命令已从固定结果升级为确定性发布质量模型。
