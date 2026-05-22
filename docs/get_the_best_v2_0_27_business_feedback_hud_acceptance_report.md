# 《壮志凌云 / Get The Best》V2-0.27 经营反馈 HUD 验收报告

## 本轮目标

V2-0.27 在 V2-0.26 统一 Core 模拟 tick 的基础上，新增一块最小只读经营反馈 HUD。

本轮目标不是做完整经营面板，也不是在 Godot UI 中复刻经营规则，而是让玩家在办公室主场景中开始看到 Core tick 产出的公司经营状态：

- 当前现金。
- MVP 当前进度与所需进度。
- 当前活跃用户。
- 当前 MRR。
- 当前产品阶段与阶段结果。
- 最近一条 Core 可播放表现事件。

## 已完成

1. Core 前端契约补齐公司总量字段
   - `SimulationFrontendContract.CompanyTotals` 明确列出 HUD 可消费字段。
   - 新增 `CompanyTotalContract`，用于记录字段名、含义和 Godot 消费方式。

2. Godot Core bridge 暴露总量快照
   - `V2CoreBridge` 继续只调用 `OfficeSimulationEngine.Advance(...)`。
   - `CoreOfficeSimulationResult` 新增 `CompanyTotals`。
   - `CompanyTotals` 从 `result.NextSnapshot.Company` 映射，不在 Godot 里重算经营公式。
   - Godot 仍然只消费 Core 输出：经营 delta、阶段结果、表现事件和下一帧公司事实。

3. 新增经营反馈 HUD
   - 新增 `BusinessFeedbackHudController`。
   - 主场景新增 `HudRoot/BusinessFeedbackPanel`。
   - HUD 在 Core tick 结果到达时刷新现金、MVP、用户、MRR、阶段和最近事件。
   - HUD 为只读展示，不提供经营操作入口。

4. 自主行动控制器接入 HUD
   - `EmployeeAutonomyController` 在应用 Core 模拟状态时同步刷新 HUD。
   - 员工行动、设施占用和经营反馈仍来自同一次 Core tick 结果。

## 自动化验证

本轮遵循先红后绿：

- 先新增 `test_get_the_best_v2_0_27_business_feedback_hud_consumes_core_totals`，验证 HUD 脚本、场景节点、Core 总量映射和前端消费入口。
- 先新增 `CompanyTotalsAreDocumentedForHudConsumption`，验证 Core 前端契约暴露公司总量字段。
- 红灯确认后再实现代码。

已执行并通过：

- `pytest tests/test_godot_v2_scaffold.py -q -k v2_0_27`
- `dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug`
- `dotnet build godot\GetTheBestGodot\GetTheBestGodot.csproj --configuration Debug`

完整回归与 Godot MCP 实机验证记录在本轮提交前补齐。

## 边界确认

- Godot 不计算现金、用户、MRR、MVP 进度或阶段结果。
- Godot 不实例化 Core 内部业务引擎，不绕过 `OfficeSimulationEngine.Advance(...)`。
- HUD 只读展示，不把旧面板点击式经营 UI 带回主场景。
- 本轮只做最小经营反馈，不做完整月报、不做经营决策面板、不做 AI 玩法。

## 后续建议

下一步可以把 HUD 的“最近事件”升级为轻提示队列或月末报告入口，但仍需遵守：

- 事件来源必须是 Core `PresentationEvents`。
- 月报来源必须是 Core `MonthlyReport`。
- Godot 只负责排版、动画和玩家可读性。
