# V2-0.30 员工座位锚点、设施旋转与 Core Utility 接入验收报告

日期：2026-05-22

## 本轮目标

本轮修正 V2-0.29 实机反馈的三个问题：

- 员工工作时必须吸附到椅子侧，而不是贴在桌面中心。
- 已放置设施拿起后按 R 原地旋转，再点击放下时必须保留新朝向。
- 接入算法端 A0.27 员工 Utility 自主行动基线，让前端能显示更明确的 Core 意图。

## 已完成内容

1. 合入算法端 A0.27 基线。
   - `EmployeeBehaviorEngine.PlanDecisions(snapshot)` 已进入主线。
   - Core 可输出行动候选分、最终选择、原因摘要、设施预留与可播 `PresentationEvents`。
   - 合入后已通过 Core 测试、Godot C# 构建与脚手架测试。

2. 修正办公桌座位锚点。
   - 员工工作位锚点现在与设施模型朝向使用同一套旋转逻辑。
   - 南向办公桌的椅子侧从错误的下方修正为上方，北向反向处理。
   - 自主行动和手动拖拽都优先选择设施座位格。

3. 修正设施原地旋转提交。
   - 设施拖拽开始时记录原始朝向。
   - 放下时只有“格子没变且朝向也没变”才取消。
   - 格子不变但朝向改变时，会调用 `TryMoveFacility(..., facing, ...)` 提交新朝向。

4. 接入 Core Utility 意图标签。
   - `CoreEmployeeIntent` 增加 `SourceAction` 与 `ReasonSummary`。
   - 员工前往设施时，活动标签可显示“前往办公桌 / 前往白板 / 前往服务器 / 前往休息”以及原因摘要。
   - Godot 仍然不计算经营规则，只消费 `OfficeSimulationEngine.Advance(snapshot)` 的结果。

## 验收证据

- 新增脚手架测试：`test_get_the_best_v2_0_30_seat_anchor_facility_rotation_and_core_intent_labels`
- 本地 Godot C# 构建通过。
- MCP 实机检查：
  - 员工工作状态有 `TypingHands`。
  - 员工工作标签为“工作中”。
  - 员工工作位置已移动到办公桌椅子侧。
  - `get_errors` 为 0。

## 后续限制

当前员工坐姿仍是程序化姿态，不是最终骨骼动画坐姿。真实坐下、站起、行走、开门、打字等动画仍应按 `docs/get_the_best_v2_animation_asset_plan.md` 的素材路线推进。
