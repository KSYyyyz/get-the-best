# Algorithm A0.24.1 MVP 阶段验收报告

## 验收目标

验证研发区的员工设施使用产能可以推进 MVP 项目进度，并在达到需求线时把产品阶段从 `Prototype` 推进到 `MvpReady`。

## 实现内容

- 新增 `ProductStage` 和 `ProductMarketState`。
- 新增 `FirstLoopBusinessEngine`。
- 研发房间内的工程、设计、策划岗位使用办公桌或产品白板时，产出计入 MVP 进度。
- `OfficeStateReducer` 可以把 `ProductMarketTickDelta` 应用到下一帧快照。

## 公式

`MVP进度增量 = 员工技能 * 员工疲劳效率 * 员工精力效率 * 设施效率 * 房间效率 * TickHours`

当 `当前项目进度 + MVP进度增量 >= RequiredProgress` 时，阶段进入 `MvpReady`。

## 测试证据

测试用例：`FirstLoopBusinessTests.ResearchWorkCompletesMvpStage`

覆盖断言：
- tick 产生正向项目进度。
- tick delta 的下一阶段为 `MvpReady`。
- reducer 后的公司产品市场阶段为 `MvpReady`。
- reducer 后项目进度被钳制到需求线。
