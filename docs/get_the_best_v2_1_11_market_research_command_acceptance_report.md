# 《壮志凌云 / Get The Best》V2-1.11 市场调研经营动作验收报告

日期：2026-05-23

## 本轮目标

把底部“管理部 -> 市场调研”从占位菜单改成第一项可执行经营动作，验证玩家操作可以进入 C# Core 规则循环，并由 Core 返回现金、MVP 进度和可展示事件。

## 实现范围

- C# Core 新增玩家命令契约：
  - `PlayerCommandKind.MarketResearch`
  - `PlayerCommand`
  - `PlayerCommandResult`
- `FirstLoopBusinessEngine` 处理市场调研命令：
  - 扣除固定调研成本。
  - 原型阶段增加少量 MVP 方向进度。
  - MVP 后可转化为少量首批用户。
  - 输出中文事件说明。
- `OfficeSimulationEngine` 把玩家命令结果转为 `PlayerCommandCompleted` 展示事件。
- Godot 前端只提交玩家意图：
  - `BuildModeHudController` 点击“市场调研”发出 `MarketResearchRequested` 信号。
  - `EmployeeAutonomyController` 接收信号后让 `V2CoreBridge` 排队命令，并立即推进一次 Core tick。
  - `V2CoreBridge` 只创建 `PlayerCommand`，不在 Godot 里计算调研成本或收益。
- 月报说明区域优先展示 Core 返回的市场调研事件。

## 验收

- 新增 Core 回归：`MarketResearchCommandCostsCashAndAddsFirstLoopInsight`。
- 新增 Godot 结构回归：`test_get_the_best_v2_1_11_market_research_command_enters_core_loop`。
- 规则边界保持：Godot 不写市场调研成本、用户转化或进度算法。

## 后续

这一步只是打通“玩家经营动作 -> Core 规则 -> 前端反馈”的第一条链路。后续可以沿用同一模式接入发行部发布原型软件、员工管理、融资贷款和统计数据窗口。
