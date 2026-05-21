# V2-0.12 员工可视对象与选择基线验收报告

## 状态

已完成。

## 本轮目标

把“员工”从后续规则系统的抽象概念，先落成办公室主场景里的可见对象，并为后续单击员工、框选员工、分配工位、培训等交互预留表现层接口。

## 完成内容

- 新增 `EmployeeStore`，暂时提供 3 名占位员工。
- 新增 `Employee3DRenderer`，用程序化 3D 占位模型渲染员工身体、头部和选中标记。
- `OfficeSelection3DController` 接入员工：
  - 指针模式下 hover 员工显示姓名和岗位。
  - 单击员工优先选中员工。
  - 指针模式框选区域内员工时高亮多名员工。
- `main.tscn` 在 `InteractionRoot` 下接入 `EmployeeStore` 和 `Employee3DRenderer`。

## 边界说明

- 本轮只做表现层和交互入口，不接工资、产能、岗位效率、招聘、培训等经营规则。
- 员工当前不是设施，也不占用设施格；未来接入工位分配时再建立员工与桌子/房间的关系。
- 占位模型只用于交互和比例验证，后续可替换为正式角色模型或第三方临时模型。

## 本地验证

- `pytest tests\test_godot_v2_scaffold.py -q`
- `python -m ruff check .`
- `python -m black --check --line-length 100 --target-version py311 .`
- `python -m isort --check-only --profile black --line-length 100 .`
- `dotnet build godot\GetTheBestGodot\GetTheBestGodot.csproj --configuration Debug`
- `pytest tests\ -q`
- `python scripts\check_docs_bootstrap.py`
- `D:\Godot\godot.cmd --headless --path "D:\Get The Best\godot\GetTheBestGodot" --import`
- Godot MCP 运行 `res://scenes/main.tscn`，默认镜头下可见 3 名员工占位模型，`get_errors` 为 0 error。
