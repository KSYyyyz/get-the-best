# Get The Best V2-0 Godot 工程骨架实施计划

> **给执行代理的要求：** 必须使用 `superpowers:subagent-driven-development`（推荐）或 `superpowers:executing-plans`，按任务逐项执行。步骤使用复选框语法，方便执行中持续更新状态。

**目标：** 在 `D:\Get The Best` 中创建干净的 Godot 4 .NET 桌面前端骨架，证明《壮志凌云 / Get The Best》可以从办公室主视角开始，而不是继承旧面板点击原型。

**架构：** V2-0 只建立最小可运行工程、主场景结构、镜头、办公室占位空间、基础选中反馈、HUD 空壳和最小 C# Core 接入点。不实现月结、不实现员工 AI、不实现商业化规则，也不迁移旧 `StartupSimGodot/main.tscn`。

**技术栈：** Godot 4 .NET、C#、Godot 内置 `Camera2D`、基础 UI `Control`/Container、未来复用 C# Core 的桥接层、GitHub Actions 文档和工程检查。

---

## 一、范围边界

### 必须完成

- 在 `D:\Get The Best\godot\GetTheBestGodot` 创建新的 Godot 4 .NET 工程。
- 新工程能独立打开、构建和运行。
- 新主场景 `res://scenes/main.tscn` 能显示办公室占位空间。
- 画面以办公室为主体，HUD 只是辅助。
- 支持基础镜头移动和缩放。
- 支持点击办公室占位区域并显示选中反馈。
- 保留后续接入 C# Core 的桥接边界。
- 新增最小自动化检查，防止旧面板 UI 文件被迁入。

### 明确不做

- 不迁移旧 `godot/StartupSimGodot/scenes/main.tscn`。
- 不复制旧 `G2OperationsPanel`、旧底部文字按钮 dock、旧事件日志 feed。
- 不实现经营月结。
- 不实现员工行动。
- 不实现房间划分和设施摆放。
- 不接入 AI 玩法。
- 不做 Web/Vercel。

---

## 二、文件结构

本阶段计划创建或修改：

```text
D:\Get The Best
  godot/
    GetTheBestGodot/
      project.godot
      GetTheBestGodot.csproj
      scenes/
        main.tscn
      scripts/
        MainController.cs
        OfficeCameraController.cs
        OfficeSelectionController.cs
        V2CoreBridge.cs
      data/
        README.md
  tests/
    test_godot_v2_scaffold.py
  scripts/
    check_docs_bootstrap.py
  docs/
    superpowers/
      plans/
        2026-05-20-get-the-best-v2-0-godot-skeleton.md
```

职责说明：

- `project.godot`：Godot 工程入口，设置主场景和基础窗口。
- `main.tscn`：V2-0 主场景，只承载办公室占位空间和 HUD 空壳。
- `MainController.cs`：主场景初始化和调试状态汇总。
- `OfficeCameraController.cs`：镜头平移、缩放和边界限制。
- `OfficeSelectionController.cs`：办公室点击、选中状态和上下文信号。
- `V2CoreBridge.cs`：未来接入 C# Core 的最小桥接占位，只读初始状态，不做规则结算。
- `test_godot_v2_scaffold.py`：文件级脚手架检查，保证工程结构和禁用旧 UI 名称。
- `check_docs_bootstrap.py`：扩展为扫描所有 Markdown 文档，保证文档正文中文优先。

---

## 三、任务拆解

### 任务 1：建立 Godot V2 工程目录

**文件：**

- 创建：`D:\Get The Best\godot\GetTheBestGodot\project.godot`
- 创建：`D:\Get The Best\godot\GetTheBestGodot\GetTheBestGodot.csproj`
- 创建：`D:\Get The Best\godot\GetTheBestGodot\scenes\.gitkeep`
- 创建：`D:\Get The Best\godot\GetTheBestGodot\scripts\.gitkeep`
- 创建：`D:\Get The Best\godot\GetTheBestGodot\data\README.md`

- [ ] **步骤 1：创建目录**

运行：

```powershell
New-Item -ItemType Directory -Force -Path 'D:\Get The Best\godot\GetTheBestGodot\scenes' | Out-Null
New-Item -ItemType Directory -Force -Path 'D:\Get The Best\godot\GetTheBestGodot\scripts' | Out-Null
New-Item -ItemType Directory -Force -Path 'D:\Get The Best\godot\GetTheBestGodot\data' | Out-Null
```

预期：三个目录存在。

- [ ] **步骤 2：创建最小 `project.godot`**

写入：

```ini
; Engine configuration file.
; 项目文档使用中文；Godot 配置键保持引擎默认格式。

config_version=5

[application]

config/name="Get The Best"
run/main_scene="res://scenes/main.tscn"
config/features=PackedStringArray("4.6", "C#", "Forward Plus")

[display]

window/size/viewport_width=1280
window/size/viewport_height=720
window/size/mode=0
window/stretch/mode="canvas_items"
window/stretch/aspect="expand"
```

- [ ] **步骤 3：创建最小 C# 项目文件**

写入：

```xml
<Project Sdk="Godot.NET.Sdk/4.6.2">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <RootNamespace>GetTheBestGodot</RootNamespace>
  </PropertyGroup>
</Project>
```

- [ ] **步骤 4：创建数据目录说明**

写入 `data/README.md`：

```markdown
# Get The Best Godot 数据目录

本目录用于存放 V2 Godot 前端可读取的表现层数据。经营规则仍由 C# Core 负责，Godot 数据不能绕过规则核心改写现金、用户、收入、融资、竞争或结局。
```

- [ ] **步骤 5：提交目录骨架**

运行：

```powershell
git add godot/GetTheBestGodot
git commit -m "chore: add get the best godot project shell"
```

预期：提交成功。

---

### 任务 2：创建最小主场景和脚本

**文件：**

- 创建：`D:\Get The Best\godot\GetTheBestGodot\scenes\main.tscn`
- 创建：`D:\Get The Best\godot\GetTheBestGodot\scripts\MainController.cs`

- [ ] **步骤 1：创建 `MainController.cs`**

写入：

```csharp
using Godot;

namespace GetTheBestGodot;

public partial class MainController : Node2D
{
    private Label? _statusLabel;

    public override void _Ready()
    {
        _statusLabel = GetNodeOrNull<Label>("HudRoot/TopStatusBar/StatusLabel");
        if (_statusLabel != null)
        {
            _statusLabel.Text = "Get The Best V2-0：办公室骨架已启动";
        }
    }
}
```

- [ ] **步骤 2：创建 `main.tscn`**

写入一个最小 Node2D 场景，根节点挂载 `MainController.cs`，包含：

- `OfficeWorld`
- `OfficeFloor`
- `OfficeCamera`
- `HudRoot`
- `TopStatusBar`
- `StatusLabel`

场景中的办公室占位地面使用 `ColorRect` 或 `Polygon2D`，必须位于 HUD 下方并保持可点击区域。

- [ ] **步骤 3：运行 Godot 导入**

运行：

```powershell
D:\Godot\godot.cmd --headless --path 'D:\Get The Best\godot\GetTheBestGodot' --import
```

预期：Godot import 成功，没有致命错误。

- [ ] **步骤 4：构建 Godot C# 项目**

运行：

```powershell
$env:PATH='D:\Startup-sim\.work\dotnet;' + $env:PATH
dotnet build 'D:\Get The Best\godot\GetTheBestGodot\GetTheBestGodot.csproj' --configuration Debug
```

预期：0 error。

- [ ] **步骤 5：提交主场景骨架**

运行：

```powershell
git add godot/GetTheBestGodot
git commit -m "feat: add get the best v2 main scene skeleton"
```

预期：提交成功。

---

### 任务 3：实现镜头控制和办公室点击反馈

**文件：**

- 创建：`D:\Get The Best\godot\GetTheBestGodot\scripts\OfficeCameraController.cs`
- 创建：`D:\Get The Best\godot\GetTheBestGodot\scripts\OfficeSelectionController.cs`
- 修改：`D:\Get The Best\godot\GetTheBestGodot\scenes\main.tscn`

- [ ] **步骤 1：创建镜头控制脚本**

`OfficeCameraController.cs` 必须支持：

- WASD 平移。
- 鼠标滚轮缩放。
- 缩放范围限制在 `0.7` 到 `2.0`。
- 基础移动速度常量，避免魔法数字散落。

- [ ] **步骤 2：创建选中控制脚本**

`OfficeSelectionController.cs` 必须支持：

- 点击办公室地面。
- 更新 `HudRoot/ContextPanel/ContextLabel`。
- 显示“已选中办公室区域”。
- 空白点击不报错。

- [ ] **步骤 3：更新主场景节点**

`main.tscn` 中必须包含：

- `OfficeCamera` 挂载 `OfficeCameraController.cs`。
- `OfficeSelectionController` 挂载 `OfficeSelectionController.cs`。
- `HudRoot/ContextPanel/ContextLabel`。

- [ ] **步骤 4：构建验证**

运行：

```powershell
dotnet build 'D:\Get The Best\godot\GetTheBestGodot\GetTheBestGodot.csproj' --configuration Debug
```

预期：0 error。

- [ ] **步骤 5：提交交互骨架**

运行：

```powershell
git add godot/GetTheBestGodot
git commit -m "feat: add v2 office camera and selection feedback"
```

预期：提交成功。

---

### 任务 4：建立最小 C# Core 桥接边界

**文件：**

- 创建：`D:\Get The Best\godot\GetTheBestGodot\scripts\V2CoreBridge.cs`
- 修改：`D:\Get The Best\godot\GetTheBestGodot\GetTheBestGodot.csproj`
- 修改：`D:\Get The Best\godot\GetTheBestGodot\scenes\main.tscn`

- [ ] **步骤 1：在 csproj 中预留项目引用**

如果继续与旧仓库同盘开发，先使用相对路径引用 `D:\Startup-sim\csharp\StartupSim.Core\StartupSim.Core.csproj`。如果新仓库要完全独立，则本步骤必须暂停，先制定 C# Core 迁移计划。

推荐先不复制规则代码，只加入注释化计划或独立桥接占位，避免仓库间路径耦合。

- [ ] **步骤 2：创建桥接占位脚本**

`V2CoreBridge.cs` 只提供：

- `GetInitialStatusText()`。
- 返回固定中文状态：“规则核心桥接待接入：当前仅验证表现层骨架”。

禁止：

- 不复制旧规则。
- 不计算月结。
- 不修改经营状态。

- [ ] **步骤 3：主场景显示桥接状态**

`MainController.cs` 读取 `V2CoreBridge.GetInitialStatusText()`，把状态显示到顶部 HUD。

- [ ] **步骤 4：构建验证**

运行：

```powershell
dotnet build 'D:\Get The Best\godot\GetTheBestGodot\GetTheBestGodot.csproj' --configuration Debug
```

预期：0 error。

- [ ] **步骤 5：提交桥接边界**

运行：

```powershell
git add godot/GetTheBestGodot
git commit -m "feat: add v2 core bridge boundary"
```

预期：提交成功。

---

### 任务 5：添加 V2 工程脚手架测试

**文件：**

- 创建：`D:\Get The Best\tests\test_godot_v2_scaffold.py`

- [ ] **步骤 1：创建测试目录**

运行：

```powershell
New-Item -ItemType Directory -Force -Path 'D:\Get The Best\tests' | Out-Null
```

- [ ] **步骤 2：创建脚手架测试**

测试必须检查：

- `godot/GetTheBestGodot/project.godot` 存在。
- `godot/GetTheBestGodot/scenes/main.tscn` 存在。
- `project.godot` 主场景指向 `res://scenes/main.tscn`。
- 新工程不包含 `G2OperationsPanel`。
- 新工程不包含旧 `StartupSimGodot` 主场景引用。
- 文档包含“办公室空间是主棋盘”。

- [ ] **步骤 3：运行测试**

运行：

```powershell
pytest tests/ -q
```

预期：全部通过。

- [ ] **步骤 4：提交测试**

运行：

```powershell
git add tests
git commit -m "test: add get the best v2 scaffold checks"
```

预期：提交成功。

---

### 任务 6：扩展 CI

**文件：**

- 修改：`D:\Get The Best\.github\workflows\docs.yml`

- [ ] **步骤 1：把工作流改名为“项目检查”**

工作流应执行：

- `python scripts/check_docs_bootstrap.py`
- `pytest tests/ -q`
- `dotnet build godot/GetTheBestGodot/GetTheBestGodot.csproj --configuration Debug`

- [ ] **步骤 2：本地运行同等检查**

运行：

```powershell
python scripts\check_docs_bootstrap.py
pytest tests/ -q
dotnet build godot\GetTheBestGodot\GetTheBestGodot.csproj --configuration Debug
```

预期：全部通过。

- [ ] **步骤 3：提交 CI**

运行：

```powershell
git add .github/workflows/docs.yml
git commit -m "ci: check get the best v2 scaffold"
```

预期：提交成功。

---

### 任务 7：MCP 真实运行验证

**文件：**

- 不强制修改文件。

- [ ] **步骤 1：检查 Godot MCP 可用性**

优先使用 Godot MCP 工具或当前可用的 Native MCP bridge。

预期：能连接 Godot 编辑器或明确记录不可用原因。

- [ ] **步骤 2：运行主场景**

运行 `res://scenes/main.tscn`。

预期：

- 主场景能打开。
- 画面中办公室占位区域清晰可见。
- HUD 不遮挡办公室主体。
- 点击办公室区域能更新上下文反馈。

- [ ] **步骤 3：截图并检查错误**

使用 MCP 截图和 `get_errors`。

预期：

- 截图非空。
- 0 error。

- [ ] **步骤 4：如果 MCP 不可用，执行 headless 替代验证**

运行：

```powershell
D:\Godot\godot.cmd --headless --path 'D:\Get The Best\godot\GetTheBestGodot' --import
dotnet build 'D:\Get The Best\godot\GetTheBestGodot\GetTheBestGodot.csproj' --configuration Debug
```

预期：全部通过，并在最终说明中明确 MCP 不可用的原因。

---

### 任务 8：最终提交、推送和 CI 检查

**文件：**

- 不限定文件。

- [ ] **步骤 1：确认工作区状态**

运行：

```powershell
git status --short
git log --oneline -5
```

预期：所有预期改动都已提交；没有意外未跟踪文件。

- [ ] **步骤 2：推送**

运行：

```powershell
git push
```

预期：推送到 `main` 成功。

- [ ] **步骤 3：检查 GitHub Actions**

运行：

```powershell
$run = gh run list --repo KSYyyyz/get-the-best --branch main --limit 1 --json databaseId --jq '.[0].databaseId'
gh run watch $run --repo KSYyyyz/get-the-best --exit-status
```

预期：CI 通过。

---

## 四、自检

覆盖范围：

- 新 Godot 工程骨架由任务 1-2 覆盖。
- 办公室主视角、镜头和选中反馈由任务 3 覆盖。
- C# Core 边界由任务 4 覆盖。
- 防旧 UI 污染由任务 5 覆盖。
- CI 由任务 6 和任务 8 覆盖。
- MCP 真实运行由任务 7 覆盖。

不包含内容：

- 不做经营月结。
- 不做房间系统。
- 不做设施系统。
- 不做员工 AI。
- 不迁移旧主场景。

风险提醒：

- 如果新仓库需要独立拥有 C# Core，必须先写 C# Core 迁移计划，不能直接复制旧规则代码。
- 如果 Godot MCP 对 `D:\Get The Best` 工程不可用，先用 headless import 和 build 验证，并把 MCP 修复作为单独任务。
- 如果场景文件手写不稳定，应优先使用 Godot 工具创建场景，而不是长期手写 `.tscn`。

