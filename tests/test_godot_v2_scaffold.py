from pathlib import Path


PROJECT_ROOT = Path(__file__).resolve().parents[1]
GODOT_ROOT = PROJECT_ROOT / "godot" / "GetTheBestGodot"


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def test_get_the_best_v2_godot_project_shell_exists() -> None:
    project_file = GODOT_ROOT / "project.godot"
    scene_file = GODOT_ROOT / "scenes" / "main.tscn"
    csproj_file = GODOT_ROOT / "GetTheBestGodot.csproj"

    assert project_file.exists()
    assert scene_file.exists()
    assert csproj_file.exists()

    project_text = read_text(project_file)
    assert 'config/name="Get The Best"' in project_text
    assert 'run/main_scene="res://scenes/main.tscn"' in project_text
    assert 'window/stretch/aspect="expand"' in project_text
    assert "window/size/mode=2" in project_text
    assert "window/size/resizable=true" in project_text
    assert 'project/assembly_name="GetTheBestGodot"' in project_text


def test_get_the_best_v2_godot_mcp_is_project_local() -> None:
    project_text = read_text(GODOT_ROOT / "project.godot")
    gitignore_text = read_text(PROJECT_ROOT / ".gitignore")

    assert 'MCPRuntime="*res://addons/godot_mcp/runtime/mcp_runtime.gd"' in project_text
    assert 'enabled=["res://addons/godot_mcp/plugin.cfg"]' in project_text
    assert (GODOT_ROOT / "addons" / "godot_mcp" / "plugin.cfg").exists()
    assert (GODOT_ROOT / "addons" / "godot_mcp" / "runtime" / "mcp_runtime.gd").exists()
    assert "godot/**/addons/godot_mcp/cache/" in gitignore_text


def test_get_the_best_v2_main_scene_is_office_first_not_old_panel_ui() -> None:
    scene_file = GODOT_ROOT / "scenes" / "main.tscn"
    scene_text = read_text(scene_file)

    assert "OfficeWorld" in scene_text
    assert "OfficeFloor" in scene_text
    assert "OfficeGrid" in scene_text
    assert "z_index = 1" in scene_text
    assert "zoom = Vector2(0.65, 0.65)" in scene_text
    assert "3200" in scene_text
    assert "2000" in scene_text
    assert "OfficeCamera" in scene_text
    assert "OfficeSelectionController" in scene_text
    assert "HudRoot" in scene_text
    assert "ContextPanel" in scene_text
    assert "G2OperationsPanel" not in scene_text
    assert "StartupSimGodot" not in scene_text


def test_get_the_best_v2_scripts_keep_rules_boundary_explicit() -> None:
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "MainController.cs" in scripts
    assert "OfficeCameraController.cs" in scripts
    assert "OfficeGridRenderer.cs" in scripts
    assert "OfficeSelectionController.cs" in scripts
    assert "OfficeWorldConfig.cs" in scripts
    assert "V2CoreBridge.cs" in scripts
    assert "new(new Vector2(-1600, -1000), new Vector2(3200, 2000))" in scripts["OfficeWorldConfig.cs"]
    assert "LayoutHud" in scripts["MainController.cs"]
    assert "InputEventMouseMotion" in scripts["OfficeCameraController.cs"]
    assert "规则核心桥接待接入" in scripts["V2CoreBridge.cs"]
    assert "G2OperationsPanel" not in "\n".join(scripts.values())


def test_get_the_best_v2_docs_preserve_core_direction() -> None:
    execution_index = read_text(PROJECT_ROOT / "docs" / "get_the_best_v2_execution_index.md")
    architecture_doc = read_text(PROJECT_ROOT / "docs" / "get_the_best_v2_reset_architecture.md")

    assert "办公室空间是主棋盘" in execution_index
    assert "C# Core 是唯一规则核心" in execution_index
    assert "G2OperationsPanel" in architecture_doc
