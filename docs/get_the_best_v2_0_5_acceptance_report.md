# V2-0.5 3D/2.5D 办公室空间基线纠偏验收报告

状态：已完成
日期：2026-05-20
适用项目：《壮志凌云 / Get The Best》

## 1. 本轮背景

项目目标是 3D/2.5D 办公室经营游戏，而不是纯 2D 平面格子游戏。

V2-0.1 到 V2-0.4 为了快速验证建造、删除、设施摆放、对象选择和 HUD 状态机，使用了 2D 场景作为交互原型。这个选择可以帮助验证底层交互，但不能继续作为正式主场景，否则后续员工、设施、房间和镜头都会被 2D 假设绑定。

本轮的任务是纠偏：正式 `main.tscn` 切换到 3D/2.5D 基线，旧 2D 经验只作为交互参考保留。

## 2. 已完成内容

- `main.tscn` 根节点改为 `Node3D`。
- 主世界切换为 `OfficeWorld` 3D 分层。
- 摄像机切换为正交 `Camera3D`。
- 新增 3D 镜头控制脚本：
  - `OfficeCamera3DController.cs`
- 新增 3D 办公室空间配置：
  - `OfficeWorld3DConfig.cs`
- 新增 3D 地面渲染：
  - `OfficeFloor3DRenderer.cs`
- 新增 3D 网格渲染：
  - `OfficeGrid3DRenderer.cs`
- 新增 3D 框选/放置预览：
  - `PlacementPreview3DController.cs`
- 新增 3D 房间覆盖层：
  - `RoomOverlay3DRenderer.cs`
- 新增 3D 设施渲染：
  - `Facility3DRenderer.cs`
- 新增 3D 鼠标 raycast 选格控制：
  - `OfficeSelection3DController.cs`
- HUD 继续使用 `CanvasLayer`，作为 3D 世界上的辅助层。

## 3. 当前保留与降级

保留：

- `BuildModeController`
- `RoomFootprintStore`
- `FacilityPlacementStore`
- `FacilityDefinitionCatalog`
- V2-0.2 到 V2-0.4 已验证的建造、删除、设施定义和素材登记经验

降级为参考：

- 旧 2D `Node2D / Camera2D / Rect2` 主场景路线。
- 旧 2D 渲染脚本只作为交互经验参考，不再作为正式主场景目标。

## 4. 明确未做

- 未接入员工系统。
- 未接入经营规则结算。
- 未把 Kenney 2D PNG 占位素材替换成 3D GLB 模型。
- 未做墙体、门、真实房间高度和等距办公室美术。
- 未引入插件。

## 5. 验收标准

- 主场景必须是 `Node3D`。
- 主摄像机必须是正交 `Camera3D`。
- 鼠标选格必须通过 `Camera3D.ProjectRayOrigin` 和 `ProjectRayNormal` 落到地面平面。
- 办公室网格、房间覆盖、设施渲染和放置预览都必须拥有 3D 版本。
- Godot HUD 仍然只是辅助层，不恢复面板点击游戏方向。
- C# Core 仍是规则核心，Godot 只负责表现层和交互。

## 6. 下一步

下一步建议进入 V2-0.6：3D 设施模型占位与工位锚点。

V2-0.6 应优先把 Kenney Furniture Kit 的 GLB 模型接入设施渲染，并为办公桌建立员工站位/坐位锚点。完成后再进入员工最小行走基线。
