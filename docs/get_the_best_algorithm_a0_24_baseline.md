# 《壮志凌云 / Get The Best》Algorithm A0.24 基线

## 目标

A0.24 建立第一局经营闭环核心，让后续 Godot 可以从 Core 获得从“研发区工作”到“MVP 完成”“销售区带来首批用户”“收入抵扣成本”“月报解释因果”的最小可测规则输出。

本基线不做 Godot 表现层、不做 UI、不做 AI 玩法。

## 新增核心契约

- `ProductStage`
  - `Prototype`：MVP 尚未完成。
  - `MvpReady`：项目进度达到需求线，可以进入销售转化。
  - `Launched`：已经获得活跃用户。
- `ProductMarketState`
  - 记录产品阶段、活跃用户数、月经常收入。
- `ProductMarketTickDelta`
  - 记录本 tick 的阶段变化、用户变化、月经常收入变化。
- `MonthlyReport`
  - 月末 tick 输出的经营解释摘要。
- `FirstLoopBusinessEngine`
  - 第一局闭环专用规则引擎，不绑定 Godot 节点。

## 计算公式

- 研发产能：研发房间内，工程、设计、策划岗位使用办公桌或产品白板时，产出进入 MVP 项目进度。
- MVP 完成：`项目进度 + 本 tick 研发产能 >= 项目需求进度` 时，阶段进入 `MvpReady`。
- 销售转用户：MVP 完成后，市场岗位在市场房间使用产品白板时，销售产能转化为活跃用户。
- 用户转化公式：`新增用户 = max(1, floor(销售产能 * 2))`。
- 月经常收入：`月经常收入 = 活跃用户 * 12`。
- tick 收入：`tick收入 = 月经常收入 / 30 / 8 * TickHours`。
- 现金变化：`现金变化 = tick收入 + tick经营成本`，其中经营成本沿用 `-MonthlyCostRate / 30 / 8 * TickHours`。
- 月报：只有 `FirstLoopBusinessTickOptions.IsMonthEnd = true` 时输出。

## 边界

- A0.24 只覆盖第一局最小闭环，不引入竞争、融资、客户分层、服务器稳定性、员工成长或离职风险。
- `BusinessTickEngine` 保持 A0.23 通用员工设施产能规则。
- `FirstLoopBusinessEngine` 是面向 V2-1 第一局闭环的更高层规则入口。
- Godot 后续只需要提交办公室事实快照，并消费 `TickResult`、`ProductMarketTickDelta` 和 `MonthlyReport`。

## 测试入口

Core 算法测试入口：

```powershell
$env:PATH='D:\Get The Best\.work\dotnet;' + $env:PATH
dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug
```
