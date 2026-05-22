# 《壮志凌云 / Get The Best》V2-0.23 员工行为状态标记验收报告

## 本轮目标

在 V2-0.21 员工自主寻找并使用设施的表现层基线，以及 V2-0.22 Core 员工经营 tick 基线之上，增加员工当前行为的轻量可视反馈。

本轮不做经营规则计算，不写产能、满意度、疲劳或收益逻辑。Godot 只显示员工当前前端行为状态，后续可由 C# Core 返回的员工意图来驱动同一套表现接口。

## 已完成

1. 员工行为状态牌
   - `Employee3DRenderer` 新增 `_employeeActivityLabels`。
   - 新增 `SetEmployeeActivityLabel(int employeeId, string? labelText)`。
   - 员工模型刷新时会调用 `AddEmployeeActivityBadge(modelRoot, employee)`。
   - 状态牌使用 `Label3D` 挂到员工实例下，随员工移动和模型刷新保持一致。

2. 自主行为状态同步
   - `EmployeeAutonomyController` 新增 `SetEmployeeActivity` 和 `ClearEmployeeActivity`。
   - 员工进入 `WalkingToTarget` 时显示“移动中”。
   - 员工进入 `WalkingToFacility` 时显示“前往设施”。
   - 员工进入 `UsingFacility` 时显示“正在使用”。
   - 员工回到 `Idle` 时清空状态牌。

3. 视觉约束
   - 状态牌采用世界空间尺寸，不使用固定屏幕巨型文字。
   - 状态牌开启 billboard，确保镜头角度变化时仍朝向玩家。
   - 状态牌开启 no-depth-test，避免被员工模型或设施轻易遮挡。
   - 文字尺寸控制为辅助反馈，不遮挡办公室主画面。

## 自动化验证

新增静态回归测试覆盖：

- 员工渲染器必须保存 `_employeeActivityLabels`。
- 员工渲染器必须提供 `SetEmployeeActivityLabel`。
- 员工模型刷新必须挂载 `Label3D` 状态牌。
- 状态牌必须使用 billboard 与 no-depth-test。
- 自主控制器必须通过 `SetEmployeeActivity` 和 `ClearEmployeeActivity` 同步行为状态。
- 自主控制器必须提供 `GetActivityLabel`，把行为状态映射成前端文案。

## 实机验收

使用 Godot MCP 运行 `res://scenes/main.tscn` 后已验证：

- 员工自主行为触发后，运行时 `Employee_2` 下出现 `ActivityBadge` 子节点。
- `ActivityBadge` 类型为 `Label3D`。
- 运行时状态牌文本为“前往设施”。
- 状态牌位置为员工模型上方，`pixel_size=0.01`、`font_size=22`、`fixed_size=false`。
- Godot 错误面板保持 `0 error`。
- 验证截图暂存到 `godot/GetTheBestGodot/addons/godot_mcp/cache`，提交前清理。

## 后续建议

下一轮可以继续沿“Core 可接入表现层”推进：

- 把行为状态文案从当前前端枚举映射，升级为可接收 Core 返回的 `EmployeeIntent`。
- 为设施增加交互点容量的前端事实快照，供 Core 规则算法消费。
- 为员工状态牌增加简短图标或进度弧，但仍不在 Godot 里计算经营收益。
