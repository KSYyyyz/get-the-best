# 《壮志凌云 / Get The Best》V2-1.12 发布原型软件经营动作验收报告

日期：2026-05-23

## 本轮目标

把底部“发行部 -> 发布软件”从占位菜单改成第二项可执行经营动作，延续 V2-1.11 的“玩家操作 -> C# Core 规则 -> Godot 前端反馈”链路。

本轮重点不是做完整发行系统，而是让第一局闭环出现明确的经营节点：MVP 达到可发布状态后，玩家可以发布原型软件，由 Core 结算发布成本、首批用户和阶段变化。

## 实现范围

- C# Core 新增玩家命令：
  - `PlayerCommandKind.PublishPrototype`
- `FirstLoopBusinessEngine` 处理发布命令：
  - MVP 未完成时返回发布失败说明，不扣费、不增加用户。
  - MVP 达到发布条件后扣除固定发布成本。
  - 发布后获得首批活跃用户。
  - 产品阶段由 Core 推进到 `Launched`。
- `OfficeSimulationEngine` 沿用 `PlayerCommandCompleted` 展示事件把发布结果交给前端。
- Godot 前端只提交玩家意图：
  - `BuildModeHudController` 点击“发布软件”发出 `PublishPrototypeRequested` 信号。
  - `EmployeeAutonomyController` 接收信号后让 `V2CoreBridge` 排队发布命令，并立即推进一次 Core tick。
  - `V2CoreBridge` 只创建 `PlayerCommand(PlayerCommandKind.PublishPrototype)`，不在 Godot 中计算发布成本、首批用户或阶段变化。
- 月报说明区域可优先展示 Core 返回的发布事件。

## 验收

- 新增 Core 回归：`PublishPrototypeCommandLaunchesReadyMvpWithInitialUsers`。
- 新增 Godot 结构回归：`test_get_the_best_v2_1_12_publish_prototype_command_enters_core_loop`。
- 已验证发布命令接入复用 V2-1.11 的玩家命令契约，没有在 Godot UI 中复制经营规则。

## 后续

发布动作打通后，V2-1 的下一步可以围绕“发布后的可读反馈”继续推进：

- 让右上核心概要更清楚地表达“已发布 / 首批用户 / 用户评分”。
- 让月报在发布后解释首批用户、现金变化和下一步增长方向。
- 后续再扩展发行部的价格、内购、下架、版本更新等动作，但每项都必须继续由 C# Core 结算。
