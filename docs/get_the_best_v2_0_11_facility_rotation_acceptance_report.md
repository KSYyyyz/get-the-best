# V2-0.11 设施朝向旋转验收报告

## 状态

已完成。

## 本轮目标

在设施放置模式中加入 `R` 键旋转朝向能力，让玩家在放下桌子、产品白板、服务器机柜前可以调整方向。

## 完成内容

- `FacilityPlacement` 增加 `Facing` 字段，设施落地时会保存当前朝向。
- `BuildModeController` 增加当前设施朝向状态，并提供 `RotateActiveFacilityFacing` / `GetActiveFacilityFacing`。
- `OfficeSelection3DController` 在设施放置模式下响应 `R` 键，并立即刷新鼠标所在格子的放置预览。
- `PlacementPreview3DController` 增加设施朝向预览条，玩家能看到当前设施正面方向。
- `Facility3DRenderer` 使用设施根节点整体旋转 3D 模型，后续替换正式模型资产时可以继续复用该朝向字段。

## 边界说明

- `R` 键只在设施放置模式下生效。
- 房间建造、门放置、删除、指针框选、相机旋转不受本轮改动影响。
- 当前设施仍按 1x1 格占地，旋转只影响表现朝向，不改变占地规则。

## 本地验证

- `pytest tests\test_godot_v2_scaffold.py -q`
- `python -m ruff check .`
- `python -m black --check --line-length 100 --target-version py311 .`
- `python -m isort --check-only --profile black --line-length 100 .`
- `dotnet build godot\GetTheBestGodot\GetTheBestGodot.csproj --configuration Debug`
- `pytest tests\ -q`
- `python scripts\check_docs_bootstrap.py`
- `D:\Godot\godot.cmd --headless --path "D:\Get The Best\godot\GetTheBestGodot" --import`
- Godot MCP 运行 `res://scenes/main.tscn`，打开设施菜单并选择办公桌，按 `R` 后设施朝向预览条发生变化，`get_errors` 为 0 error。
