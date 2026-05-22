# V2-1.2 持续公司状态与首批用户闭环验收报告

日期：2026-05-22

## 本轮目标

V2-1.1 解决了同一批 Core 员工意图在 Godot 表现层中的连续播放问题，但公司经营状态仍没有形成完整闭环：Godot 每次构造 Core 快照时都会回到默认 MVP 进度，导致员工工作只能产生一次可见变化。本轮目标是让 C# Core 返回的公司状态成为下一次 Core tick 的输入，并修正 Core 中 `Work` 持续状态不继续产出的规则问题。

## 已完成内容

1. Godot-Core 桥接持久化公司状态
   - `V2CoreBridge` 新增 `_companyState`。
   - 每次 Core 计算完成后保存 `result.NextSnapshot.Company`。
   - 下一次 `BuildSnapshot()` 会继续使用 Core 返回的公司状态，而不是重置为默认进度。

2. Core 持续工作状态修正
   - `FirstLoopBusinessEngine` 将 `UseFacility` 和 `Work` 都视为可持续产出的工作状态。
   - `BusinessTickEngine` 同步修正通用业务 tick 的 productive activity 判断。
   - 新增 Core 测试确认员工进入持久 `Work` 状态后仍能继续产出 MVP 进度和设施 delta。

3. 第一局白板占用规则收紧
   - MVP 完成前，策划/设计的产品白板候选只指向研发室白板。
   - 市场室白板留给 MVP 完成后的市场转化动作。
   - 避免开局策划占用市场白板，导致市场员工无法自然获得首批用户。

## 验收证据

- Python 脚手架测试新增 V2-1.2 覆盖：
  - Godot 桥接层存在 `_companyState`。
  - 下一次快照会复用 Core 公司状态。
  - HUD 仍从 `CoreOfficeSimulationResult.CompanyTotals` 读取数据。
- C# Core 测试新增覆盖：
  - `Work` 持续状态继续产生 MVP 进度。
  - MVP 前策划不会预留市场室白板。
- MCP 实机运行确认：
  - MVP 从 `40/100` 持续推进到 `86.5/100`，再到 `100/100`。
  - 阶段进入 `已发布 / 获得首批用户`。
  - 用户数变为 `20`。
  - MRR 变为 `¥240`。
  - `get_errors` 为 0。

## 后续限制

当前已经有第一局“员工行动 -> MVP -> 首批用户 -> MRR”的可见闭环，但月报仍只是 Core 能力，尚未在 Godot 前端中形成专门的月报面板或复盘交互。下一步应把月报/阶段复盘做成玩家能读懂的办公经营反馈，而不是继续堆 HUD 文本。
