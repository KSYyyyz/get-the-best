# Algorithm A0.24.2 销售首批用户验收报告

## 验收目标

验证 MVP 完成后，市场岗位在市场房间使用产品白板可以把销售产能转化为首批活跃用户，并把产品阶段推进到 `Launched`。

## 实现内容

- `FirstLoopBusinessEngine` 识别市场岗位、市场房间和产品白板组合。
- MVP 未完成时销售产能不转化用户。
- MVP 完成后销售产能按确定公式转化为用户。

## 公式

`新增用户 = max(1, floor(销售产能 * 2))`

销售产能来自同一套员工、房间、设施效率公式，保持和 A0.23 员工设施使用规则一致。

## 测试证据

测试用例：`FirstLoopBusinessTests.MarketingWorkAddsFirstUsersAfterMvp`

覆盖断言：
- MVP 完成后，市场工作产生正向 `ActiveUsersDelta`。
- tick delta 的下一阶段为 `Launched`。
- reducer 后产品市场状态中的活跃用户数增加。
