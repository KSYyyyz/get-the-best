# 《壮志凌云 / Get The Best》V2-0.28 设施预览、旋转与工作反馈验收报告

## 本轮目标

V2-0.28 是对 V2-0.27 实机体验问题的补修：

- 放置设施时必须看到设施实例本体的动态预览，而不是只有地面格子。
- 拖拽已有设施时，按 `R` 也能四向旋转，并在放下后保留朝向。
- 员工到达设施后必须有可见的“正在使用/工作中”表现，不再只是移动到旁边。
- 经营反馈 HUD 的最近事件不能只显示含义不明的原始指标值。

## 已完成

1. 设施放置实例预览
   - `Facility3DRenderer` 新增 `ShowFacilityPlacementPreview(...)`。
   - 放置模式下，鼠标移动会显示真实设施模型预览。
   - 合法位置使用绿色预览，非法位置使用红色预览。
   - 退出放置、取消工具或离开有效地块时会清理预览实例。

2. 已有设施拖拽旋转
   - 拖拽设施时按 `R` 会轮换 `North -> East -> South -> West`。
   - 拖拽预览会立即显示新朝向。
   - 放下合法位置后，设施位置和朝向一起写回 `FacilityPlacementStore`。

3. 员工使用设施的可见反馈
   - `Employee3DRenderer` 新增 `SetEmployeeWorkState(...)`。
   - 员工进入 Core `UseFacility` 状态后，会面向目标设施并播放轻微工作动画。
   - 员工恢复 Idle 后清理工作状态。
   - 该表现仍由 Core 生命周期状态驱动，Godot 不自行决定员工应该工作多久。

4. HUD 最近事件文案优化
   - Core `MetricChanged` 事件不再直接显示原始消息。
   - HUD 改为显示本 tick 的 MVP、现金、收入变化摘要。
   - 经营公式仍然来自 Core，Godot 只做显示格式化。

## 自动化验证

本轮遵循先红后绿：

- 新增 `test_get_the_best_v2_0_28_facility_preview_rotation_and_work_feedback`。
- 红灯确认后实现设施预览、设施旋转、员工工作反馈和 HUD 摘要。

已执行并通过：

- `pytest tests/test_godot_v2_scaffold.py -q -k "v2_0_27 or v2_0_28"`
- `dotnet build godot\GetTheBestGodot\GetTheBestGodot.csproj --configuration Debug`

完整回归与 Godot MCP 实机验证记录在本轮提交前补齐。

## 边界确认

- Godot 只显示设施预览、朝向、员工动作和 HUD 文案。
- 员工使用设施的开始、结束、设施占用仍由 Core 统一 tick 驱动。
- 本轮不新增复杂员工调度算法，不在 Godot 中复制需求、动机、产能或收益公式。

## 给算法端的后续方向

本轮暴露的问题是：前端已经能表现员工“去设施”和“使用设施”，但 Core 端还需要更像经营模拟游戏的自主行动系统。

建议算法端下一阶段研究并实现：

- 基于员工需求/动机的 Utility scoring。
- 基于设施能力、房间类型、岗位职责和当前公司目标的 action candidate。
- 短计划或 GOAP-lite，用于把“想做什么”转换为“去哪个设施、站在哪个交互点、持续多久、产出什么 delta”。
- 可解释事件输出，让前端能显示“为什么这个员工去做这件事”。
