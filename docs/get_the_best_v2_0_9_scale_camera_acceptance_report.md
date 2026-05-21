# 《壮志凌云 / Get The Best》V2-0.9 建造尺度与中键视角验收报告

状态：已完成
日期：2026-05-21
范围：Godot 桌面前端，办公室沙盒建造尺度和摄像机输入基线

## 本轮目标

本轮不是加入新经营规则，而是修正 V2 办公室编辑器的底层尺度：

- 办公室物理空间扩大一倍，从 80x50 world unit 扩展为 160x100 world unit。
- 取消旧的小格建造单位，把此前画面里的“大格”作为新的实际建造格。
- 房间、门、设施、后续对象占地都以新的 10 world unit 建造格为标准。
- 鼠标中键不再平移摄像机，改为上下拖动调整俯仰视角。

## 实现记录

- `OfficeWorld3DConfig` 更新为 `6400x4000` 逻辑画布、`16x10` 建造格、`GridSize = 10.0f`。
- `OfficeGrid3DRenderer` 移除每 5 小格一条粗线的旧逻辑，所有可见格线都代表真实建造单位。
- `RoomDoorGeometry`、`RoomOverlay3DRenderer`、`OfficeBoundary3DRenderer`、`Facility3DRenderer` 改为按 `GridSize` 缩放门、墙、房间边界和设施体块。
- `OfficeCamera3DController` 更新默认视野为大办公室尺度，并把中键拖动改为 `42° - 74°` 范围内的俯仰调整。
- `main.tscn` 同步相机默认 `size = 112.0`，世界尺寸 marker 更新为 `WorldSize6400x4000Marker`。

## 验收结果

- 自动测试覆盖了大格建造尺度、相机范围、中键俯仰、网格去小格化、门/设施随新格缩放。
- Godot MCP 真实运行 `res://scenes/main.tscn`，画面中当前可见网格已经是实际建造格。
- Godot MCP 查询相机运行时状态：中键拖动后 `rotation_degrees.x` 从约 `-58°` 变为约 `-67°`，证明中键已经从平移改为俯仰调整。
- Godot 错误面板：0 error。

## 保留问题

- 当前仍是 3D/2.5D 技术骨架，美术层还没有切到最终办公室地板、墙体、家具套件。
- 建造确认、门、设施的交互已经能跟随新建造格，但后续 V2-0.9 员工/角色系统需要继续以 10 world unit 为基础设计占地和寻路。
- 现在默认视角偏沙盒总览，后续需要加入更游戏化的镜头预设和对象跟随视角。
