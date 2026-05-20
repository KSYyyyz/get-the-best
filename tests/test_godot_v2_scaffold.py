import json
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

    assert '[node name="Main" type="Node3D"' in scene_text
    assert "OfficeWorld" in scene_text
    assert "OfficeFloor" in scene_text
    assert "OfficeGrid3D" in scene_text
    assert 'type="Camera3D"' in scene_text
    assert "projection = 1" in scene_text
    assert "size = 36.0" in scene_text
    assert "3200" in scene_text
    assert "2000" in scene_text
    assert "OfficeCamera" in scene_text
    assert "OfficeSelection3DController" in scene_text
    assert "HudRoot" in scene_text
    assert "FloatingTooltip" in scene_text
    assert "TooltipLabel" in scene_text
    assert "BuildModeLabel" not in scene_text
    assert "ContextPanel" not in scene_text
    assert "ContextLabel" not in scene_text
    assert "G2OperationsPanel" not in scene_text
    assert "StartupSimGodot" not in scene_text


def test_get_the_best_v2_scripts_keep_rules_boundary_explicit() -> None:
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "MainController.cs" in scripts
    assert "OfficeCamera3DController.cs" in scripts
    assert "OfficeGrid3DRenderer.cs" in scripts
    assert "OfficeSelection3DController.cs" in scripts
    assert "OfficeWorld3DConfig.cs" in scripts
    assert "OfficeWorldConfig.cs" in scripts
    assert "V2CoreBridge.cs" in scripts
    assert (
        "new(new Vector2(-1600, -1000), new Vector2(3200, 2000))" in scripts["OfficeWorldConfig.cs"]
    )
    assert "public partial class MainController : Node3D" in scripts["MainController.cs"]
    assert "LayoutHud" in scripts["MainController.cs"]
    assert 'GetNodeOrNull<CanvasLayer>("HudRoot")' in scripts["MainController.cs"]
    assert "RemoveHudChrome(childControl)" in scripts["MainController.cs"]
    assert "Camera3D" in scripts["OfficeCamera3DController.cs"]
    assert "ProjectionType.Orthogonal" in scripts["OfficeCamera3DController.cs"]
    assert "MinCameraSize = 12.0f" in scripts["OfficeCamera3DController.cs"]
    assert "MaxCameraSize = 64.0f" in scripts["OfficeCamera3DController.cs"]
    assert "ProjectRayOrigin" in scripts["OfficeSelection3DController.cs"]
    assert "ProjectRayNormal" in scripts["OfficeSelection3DController.cs"]
    assert "TryScreenPositionToCell" in scripts["OfficeSelection3DController.cs"]
    assert "TryWorldToCell" in scripts["OfficeWorld3DConfig.cs"]
    assert "CellToWorldPosition" in scripts["OfficeWorld3DConfig.cs"]
    assert "规则核心桥接待接入" in scripts["V2CoreBridge.cs"]
    assert "G2OperationsPanel" not in "\n".join(scripts.values())


def test_get_the_best_v2_docs_preserve_core_direction() -> None:
    execution_index = read_text(PROJECT_ROOT / "docs" / "get_the_best_v2_execution_index.md")
    architecture_doc = read_text(PROJECT_ROOT / "docs" / "get_the_best_v2_reset_architecture.md")

    assert "办公室空间是主棋盘" in execution_index
    assert "C# Core 是唯一规则核心" in execution_index
    assert "G2OperationsPanel" in architecture_doc


def test_get_the_best_v2_0_1_acceptance_is_recorded() -> None:
    acceptance_doc = PROJECT_ROOT / "docs" / "get_the_best_v2_0_1_acceptance_report.md"
    assert acceptance_doc.exists()

    acceptance_text = read_text(acceptance_doc)
    assert "V2-0.1 办公室沙盒与镜头基线验收报告" in acceptance_text
    assert "已完成" in acceptance_text
    assert "3200x2000" in acceptance_text
    assert "0.25 - 3.25" in acceptance_text
    assert "V2-0.2 不在本轮范围" in acceptance_text


def test_get_the_best_v2_0_2_grid_interaction_scaffold_exists() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "InteractionRoot" in scene_text
    assert "OfficeSelection3DController" in scene_text
    assert "PlacementPreview3DController" in scene_text
    assert "BuildModeController" in scene_text
    assert 'parent="InteractionRoot"' in scene_text

    assert "PlacementPreview3DController.cs" in scripts
    assert "BuildModeController.cs" in scripts
    assert "TryWorldToCell" in scripts["OfficeWorldConfig.cs"]
    assert "CellsToWorldRect" in scripts["OfficeWorldConfig.cs"]
    assert "ShowHoverCell" in scripts["PlacementPreview3DController.cs"]
    assert "ShowSelectionRect" in scripts["PlacementPreview3DController.cs"]
    assert "ClearPreview" in scripts["PlacementPreview3DController.cs"]
    assert "MouseButton.Left" in scripts["OfficeSelection3DController.cs"]
    assert "Key.Escape" in scripts["OfficeSelection3DController.cs"]
    assert "Plane(Vector3.Up, 0.0f)" in scripts["OfficeSelection3DController.cs"]
    assert "ShowPointerTooltip" in scripts["OfficeSelection3DController.cs"]


def test_get_the_best_v2_0_2_room_footprint_baseline_exists() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "RoomFootprintStore" in scene_text
    assert "RoomOverlay3DRenderer" in scene_text
    assert 'parent="InteractionRoot"' in scene_text

    assert "RoomFootprintStore.cs" in scripts
    assert "RoomOverlay3DRenderer.cs" in scripts
    assert "CanReserve" in scripts["RoomFootprintStore.cs"]
    assert "TryReserve" in scripts["RoomFootprintStore.cs"]
    assert "Overlaps" in scripts["RoomFootprintStore.cs"]
    assert "FindAtCell" in scripts["RoomFootprintStore.cs"]
    assert "GetRooms" in scripts["RoomFootprintStore.cs"]
    assert "RoomFootprintStore" in scripts["BuildModeController.cs"]
    assert "TryCreateRoom" in scripts["BuildModeController.cs"]
    assert "RoomOverlay3DRenderer" in scripts["OfficeSelection3DController.cs"]
    assert "ShowOccupiedRoom" in scripts["OfficeSelection3DController.cs"]
    assert "RefreshRooms" in scripts["RoomOverlay3DRenderer.cs"]


def test_get_the_best_v2_0_2_room_type_build_mode_exists() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "BuildModePanel" in scene_text
    assert "BuildEntryButtons" in scene_text
    assert "BuildMenuButton" in scene_text
    assert "EntrySeparator" in scene_text
    assert "ResearchRoomButton" in scene_text
    assert "MarketRoomButton" in scene_text
    assert "ServerRoomButton" in scene_text
    assert "BuildModeHudController" in scene_text
    assert '[node name="BuildEntryButtons" type="HBoxContainer"' in scene_text
    assert '[node name="RoomTypeButtons" type="VBoxContainer"' in scene_text
    assert scene_text.index('[node name="DeleteRoomButton"') < scene_text.index(
        '[node name="EntrySeparator"'
    )
    assert scene_text.index('[node name="EntrySeparator"') < scene_text.index(
        '[node name="BuildMenuButton"'
    )

    assert "BuildModeHudController.cs" in scripts
    assert "_buildMenuButton" in scripts["BuildModeHudController.cs"]
    assert "_entryButtons" in scripts["BuildModeHudController.cs"]
    assert "_entrySeparator" in scripts["BuildModeHudController.cs"]
    assert "ConfigureEntryButtons" in scripts["BuildModeHudController.cs"]
    assert 'AddThemeConstantOverride("separation", 2)' in scripts["BuildModeHudController.cs"]
    assert "ConfigureSeparator" in scripts["BuildModeHudController.cs"]
    assert 'Text = "|"' in scripts["BuildModeHudController.cs"]
    assert "VBoxContainer? _roomTypeButtons" in scripts["BuildModeHudController.cs"]
    assert "ToggleBuildMenu" in scripts["BuildModeHudController.cs"]
    assert "RefreshRoomTypeVisibility" in scripts["BuildModeHudController.cs"]
    assert "_roomTypeButtons.Visible = _isBuildMenuOpen" in scripts["BuildModeHudController.cs"]
    assert "_buildModeLabel" not in scripts["BuildModeHudController.cs"]
    assert "RefreshBuildModeLabel" not in scripts["BuildModeHudController.cs"]
    assert "MouseEntered" in scripts["BuildModeHudController.cs"]
    assert "ApplyToolButtonState" in scripts["BuildModeHudController.cs"]
    assert "OnToolModeChanged" in scripts["BuildModeHudController.cs"]
    assert "ActiveButtonColor" in scripts["BuildModeHudController.cs"]
    assert "HoverButtonColor" in scripts["BuildModeHudController.cs"]
    assert "GetButtonPrefix" in scripts["BuildModeHudController.cs"]
    assert "minWidth: 46.0f" in scripts["BuildModeHudController.cs"]
    assert "new Vector2(8.0f, 30.0f)" in scripts["BuildModeHudController.cs"]
    assert (
        "CustomMinimumSize = new Vector2(minWidth, 30.0f)" in scripts["BuildModeHudController.cs"]
    )
    assert "viewportSize.X * 0.14f" in scripts["MainController.cs"]
    assert "viewportSize.X - width - 180.0f" in scripts["MainController.cs"]
    assert "new Vector2(width, 180.0f)" in scripts["MainController.cs"]
    assert "enum RoomBuildType" in scripts["BuildModeController.cs"]
    assert "ResearchRoom" in scripts["BuildModeController.cs"]
    assert "MarketRoom" in scripts["BuildModeController.cs"]
    assert "ServerRoom" in scripts["BuildModeController.cs"]
    assert "SetActiveRoomType" in scripts["BuildModeController.cs"]
    assert "GetActiveRoomType" in scripts["BuildModeController.cs"]
    assert "GetActiveRoomTypeLabel" in scripts["BuildModeController.cs"]
    assert "RoomBuildType RoomType" in scripts["RoomFootprintStore.cs"]
    assert "GetRoomFillColor" in scripts["RoomOverlay3DRenderer.cs"]
    assert "ResearchRoomButton" in scripts["BuildModeHudController.cs"]
    assert "MarketRoomButton" in scripts["BuildModeHudController.cs"]
    assert "ServerRoomButton" in scripts["BuildModeHudController.cs"]
    assert "ConfigureButton(" in scripts["BuildModeHudController.cs"]
    assert (
        "BuildModeController.GetRoomTypeLabel(RoomBuildType.ResearchRoom)"
        in scripts["BuildModeHudController.cs"]
    )
    assert (
        "BuildModeController.GetRoomTypeLabel(RoomBuildType.MarketRoom)"
        in scripts["BuildModeHudController.cs"]
    )
    assert (
        "BuildModeController.GetRoomTypeLabel(RoomBuildType.ServerRoom)"
        in scripts["BuildModeHudController.cs"]
    )


def test_get_the_best_v2_0_2_room_delete_and_hover_highlight_exists() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "DeleteRoomButton" in scene_text
    assert "CancelActionButton" not in scene_text
    assert 'parent="HudRoot/BuildModePanel/BuildModeRows/BuildEntryButtons"' in scene_text

    assert "enum BuildToolMode" in scripts["BuildModeController.cs"]
    assert "Pointer" in scripts["BuildModeController.cs"]
    assert "DeleteRoom" in scripts["BuildModeController.cs"]
    assert "StartDeleteRoomMode" in scripts["BuildModeController.cs"]
    assert "ToggleDeleteRoomMode" in scripts["BuildModeController.cs"]
    assert "CancelActiveTool" in scripts["BuildModeController.cs"]
    assert "ToolModeChanged" in scripts["BuildModeController.cs"]
    assert "IsPointerMode" in scripts["BuildModeController.cs"]
    assert "TryDeleteRoomAtCell" in scripts["BuildModeController.cs"]
    assert "CanDeleteSelection" in scripts["BuildModeController.cs"]
    assert "TryDeleteRoomsInSelection" in scripts["BuildModeController.cs"]
    assert "SellFixturesInSelection" in scripts["BuildModeController.cs"]
    assert "HasBlockingFixtures" not in scripts["BuildModeController.cs"]
    assert "RemoveAtCell" in scripts["RoomFootprintStore.cs"]
    assert "RemoveCells" in scripts["RoomFootprintStore.cs"]
    assert "IReadOnlyCollection<Vector2I> Cells" in scripts["RoomFootprintStore.cs"]
    assert "HighlightRoom" in scripts["RoomOverlay3DRenderer.cs"]
    assert "HighlightedRoomStroke" in scripts["RoomOverlay3DRenderer.cs"]
    assert "FinishDeleteSelection" in scripts["OfficeSelection3DController.cs"]
    assert "SelectRoomAtPointer" in scripts["OfficeSelection3DController.cs"]
    assert "ClearSelectedRoom();" in scripts["OfficeSelection3DController.cs"]
    assert "_roomOverlayRenderer?.RefreshRooms();" in scripts["OfficeSelection3DController.cs"]
    assert "CancelActiveTool" in scripts["OfficeSelection3DController.cs"]
    assert "if (_isDraggingSelection)" in scripts["OfficeSelection3DController.cs"]
    assert "CancelInteraction();" in scripts["OfficeSelection3DController.cs"]
    assert "ShowPointerTooltip" in scripts["OfficeSelection3DController.cs"]
    assert "PositionPointerTooltip" in scripts["OfficeSelection3DController.cs"]
    assert "TooltipOffset = 6.0f" in scripts["OfficeSelection3DController.cs"]
    assert "ShowPointerTooltip(size, screenPosition)" in scripts["OfficeSelection3DController.cs"]
    assert "HorizontalAlignment.Left" in scripts["OfficeSelection3DController.cs"]
    assert "GetMinimumSize()" in scripts["OfficeSelection3DController.cs"]
    assert "ShowHoverCell(cell)" not in scripts["OfficeSelection3DController.cs"]
    assert "IsDeleteRoomMode" in scripts["OfficeSelection3DController.cs"]
    assert "_deleteRoomButton" in scripts["BuildModeHudController.cs"]
    assert "_cancelActionButton" not in scripts["BuildModeHudController.cs"]
    assert 'AddThemeColorOverride("font_shadow_color"' in scripts["MainController.cs"]
    assert "StyleBoxEmpty" in scripts["MainController.cs"]
    assert ".Flat = true" in scripts["MainController.cs"]


def test_get_the_best_v2_0_3_facility_placement_baseline_exists() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "FacilityPlacementStore" in scene_text
    assert "Facility3DRenderer" in scene_text
    assert "FacilityTypeButtons" in scene_text
    assert "FacilityMenuButton" in scene_text
    assert "DeskFacilityButton" in scene_text
    assert "WhiteboardFacilityButton" in scene_text
    assert "ServerRackFacilityButton" in scene_text

    assert "FacilityPlacementStore.cs" in scripts
    assert "Facility3DRenderer.cs" in scripts
    assert "enum FacilityBuildType" in scripts["BuildModeController.cs"]
    assert "PlaceFacility" in scripts["BuildModeController.cs"]
    assert "IsPlaceFacilityMode" in scripts["BuildModeController.cs"]
    assert "CanPlaceFacility" in scripts["BuildModeController.cs"]
    assert "TryPlaceFacility" in scripts["BuildModeController.cs"]
    assert "FindFacilityAtCell" in scripts["BuildModeController.cs"]
    assert "DeleteFacilitiesInSelection" in scripts["BuildModeController.cs"]
    assert "GetRequiredRoomType" in scripts["BuildModeController.cs"]

    assert "TryPlace" in scripts["FacilityPlacementStore.cs"]
    assert "CanPlace" in scripts["FacilityPlacementStore.cs"]
    assert "RemoveInSelection" in scripts["FacilityPlacementStore.cs"]
    assert "FindAtCell" in scripts["FacilityPlacementStore.cs"]
    assert "RoomFootprintStore" in scripts["FacilityPlacementStore.cs"]
    assert "OfficeDesk" in scripts["FacilityPlacementStore.cs"]
    assert "ProductWhiteboard" in scripts["FacilityPlacementStore.cs"]
    assert "ServerRack" in scripts["FacilityPlacementStore.cs"]

    assert "HighlightFacility" in scripts["Facility3DRenderer.cs"]
    assert "GetFacilityFillColor" in scripts["Facility3DRenderer.cs"]
    assert "RefreshFacilities" in scripts["Facility3DRenderer.cs"]
    assert "ShowFacilityCell" in scripts["PlacementPreview3DController.cs"]
    assert "FinishFacilityPlacement" in scripts["OfficeSelection3DController.cs"]
    assert "SelectFacilityAtPointer" in scripts["OfficeSelection3DController.cs"]
    assert "ShowFacilityTooltip" in scripts["OfficeSelection3DController.cs"]
    assert "_facilityRenderer?.RefreshFacilities();" in scripts["OfficeSelection3DController.cs"]
    assert (
        "_facilityRenderer?.HighlightFacility(null);" in scripts["OfficeSelection3DController.cs"]
    )

    assert "FacilityMenuText" in scripts["BuildModeHudController.cs"]
    assert "_facilityMenuButton" in scripts["BuildModeHudController.cs"]
    assert "VBoxContainer? _facilityTypeButtons" in scripts["BuildModeHudController.cs"]
    assert "ToggleFacilityMenu" in scripts["BuildModeHudController.cs"]
    assert "SetFacilityType" in scripts["BuildModeHudController.cs"]
    assert "RefreshToolMenuVisibility" in scripts["BuildModeHudController.cs"]
    assert "BuildModeController.GetFacilityTypeLabel" in scripts["BuildModeHudController.cs"]


def test_get_the_best_v2_0_3_product_whiteboard_requires_market_room() -> None:
    build_mode = read_text(GODOT_ROOT / "scripts" / "BuildModeController.cs")
    facility_store = read_text(GODOT_ROOT / "scripts" / "FacilityPlacementStore.cs")

    assert "FacilityBuildType.ProductWhiteboard => RoomBuildType.MarketRoom" in build_mode
    assert "[FacilityBuildType.ProductWhiteboard] = RoomBuildType.MarketRoom" in facility_store


def test_get_the_best_v2_0_4_facility_art_placeholders_are_registered() -> None:
    asset_index_path = GODOT_ROOT / "assets" / "third_party_placeholder_assets" / "asset-index.json"
    assert asset_index_path.exists()

    asset_index = json.loads(read_text(asset_index_path))
    facility_assets = {
        asset["asset_id"]: asset
        for asset in asset_index["assets"]
        if asset["current_usage"].startswith("V2-0.4 facility")
    }

    assert set(facility_assets) == {
        "kenney_furniture_kit_desk_se",
        "kenney_furniture_kit_computer_screen_se",
        "kenney_furniture_kit_bookcase_closed_doors_se",
    }
    for asset in facility_assets.values():
        assert asset["source_site"] == "Kenney"
        assert asset["license"] == "CC0-1.0"
        assert asset["commercial_use_allowed"] is True
        assert asset["attribution_required"] is False
        assert asset["replacement_target"] == "Replace with final Get The Best office art."
        imported_path = GODOT_ROOT / asset["imported_path"].replace("res://", "")
        assert imported_path.exists()


def test_get_the_best_v2_0_4_facility_definitions_and_texture_rendering_exist() -> None:
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "FacilityDefinitionCatalog.cs" in scripts
    catalog = scripts["FacilityDefinitionCatalog.cs"]
    renderer = scripts["Facility3DRenderer.cs"]

    assert "FacilityDefinition" in catalog
    assert "GetDefinition" in catalog
    assert "TexturePath" in catalog
    assert "IsWorkstation" in catalog
    assert "Footprint: new Vector2I(1, 1)" in catalog
    assert "res://assets/third_party_placeholder_assets/kenney_furniture_kit/desk_SE.png" in catalog
    assert (
        "res://assets/third_party_placeholder_assets/kenney_furniture_kit/computerScreen_SE.png"
        in catalog
    )
    assert (
        "res://assets/third_party_placeholder_assets/kenney_furniture_kit/bookcaseClosedDoors_SE.png"
        in catalog
    )
    assert "FacilityBuildType.OfficeDesk" in catalog
    assert "RoomBuildType.ResearchRoom" in catalog
    assert "FacilityBuildType.ProductWhiteboard" in catalog
    assert "RoomBuildType.MarketRoom" in catalog
    assert "FacilityBuildType.ServerRack" in catalog
    assert "RoomBuildType.ServerRoom" in catalog

    assert "Texture2D" in renderer
    assert "ResourceLoader.Load<Texture2D>" in renderer
    assert "GetFacilityTexture" in renderer
    assert "Sprite3D" in renderer
    assert "GetFacilityFillColor" in renderer


def test_get_the_best_v2_0_6_office_space_has_2_5d_depth_cues() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "OfficeBoundary3D" in scene_text
    assert "OfficeBoundary3DRenderer.cs" in scripts
    assert "AddWall" in scripts["OfficeBoundary3DRenderer.cs"]
    assert "WallHeight = 1.4f" in scripts["OfficeBoundary3DRenderer.cs"]
    assert "WallThickness = 0.45f" in scripts["OfficeBoundary3DRenderer.cs"]
    assert "OfficeWorld3DConfig.OfficeBounds" in scripts["OfficeBoundary3DRenderer.cs"]

    room_renderer = scripts["RoomOverlay3DRenderer.cs"]
    assert "RoomCarpetHeight" in room_renderer
    assert "RoomBoundaryHeight" in room_renderer
    assert "AddRoomBoundary" in room_renderer
    assert "AddRoomSignPlate" in room_renderer
    assert "HighlightedRoomStroke" in room_renderer

    facility_renderer = scripts["Facility3DRenderer.cs"]
    assert "AddFacilityVolume" in facility_renderer
    assert "AddFacilitySpriteBillboard" in facility_renderer
    assert "GetFacilityVolumeSize" in facility_renderer
    assert "GetFacilitySpriteHeight" in facility_renderer
    assert "new BoxMesh" in facility_renderer
