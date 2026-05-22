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
    assert "size = 112.0" in scene_text
    assert "6400" in scene_text
    assert "4000" in scene_text
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
    assert "MinCameraSize = 28.0f" in scripts["OfficeCamera3DController.cs"]
    assert "MaxCameraSize = 210.0f" in scripts["OfficeCamera3DController.cs"]
    assert "ProjectRayOrigin" in scripts["OfficeSelection3DController.cs"]
    assert "ProjectRayNormal" in scripts["OfficeSelection3DController.cs"]
    assert "TryScreenPositionToCell" in scripts["OfficeSelection3DController.cs"]
    assert "TryWorldToCell" in scripts["OfficeWorld3DConfig.cs"]
    assert "CellToWorldPosition" in scripts["OfficeWorld3DConfig.cs"]
    assert "GodotCoreBridgeContract" in scripts["V2CoreBridge.cs"]
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
    assert "AddFacilityModel" in scripts["Facility3DRenderer.cs"]
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


def test_get_the_best_v2_0_17_kenney_character_floor_and_wall_assets_are_registered() -> None:
    asset_index_path = GODOT_ROOT / "assets" / "third_party_placeholder_assets" / "asset-index.json"
    asset_index = json.loads(read_text(asset_index_path))
    assets = {asset["asset_id"]: asset for asset in asset_index["assets"]}

    expected_assets = {
        "kenney_blocky_characters_character_a": "res://assets/third_party_placeholder_assets/kenney_blocky_characters/character-a.glb",
        "kenney_blocky_characters_character_b": "res://assets/third_party_placeholder_assets/kenney_blocky_characters/character-b.glb",
        "kenney_blocky_characters_character_c": "res://assets/third_party_placeholder_assets/kenney_blocky_characters/character-c.glb",
        "kenney_blocky_characters_texture_a": "res://assets/third_party_placeholder_assets/kenney_blocky_characters/Textures/texture-a.png",
        "kenney_blocky_characters_texture_b": "res://assets/third_party_placeholder_assets/kenney_blocky_characters/Textures/texture-b.png",
        "kenney_blocky_characters_texture_c": "res://assets/third_party_placeholder_assets/kenney_blocky_characters/Textures/texture-c.png",
        "kenney_prototype_textures_floor_light_02": "res://assets/third_party_placeholder_assets/kenney_prototype_textures/floor_light_texture_02.png",
    }

    for asset_id, imported_path in expected_assets.items():
        assert asset_id in assets
        asset = assets[asset_id]
        assert asset["source_site"] == "Kenney"
        assert asset["license"] == "CC0-1.0"
        assert asset["commercial_use_allowed"] is True
        assert asset["attribution_required"] is False
        assert asset["imported_path"] == imported_path
        assert (GODOT_ROOT / imported_path.replace("res://", "")).exists()


def test_get_the_best_v2_0_4_facility_definitions_and_model_rendering_exist() -> None:
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

    assert "Texture2D" not in renderer
    assert "ResourceLoader.Load<Texture2D>" not in renderer
    assert "GetFacilityTexture" not in renderer
    assert "Sprite3D" not in renderer
    assert "AddFacilityModel" in renderer
    assert "AddDeskModel" in renderer
    assert "AddProductWhiteboardModel" in renderer
    assert "AddServerRackModel" in renderer


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
    assert (
        "WallHeight = OfficeWorld3DConfig.GridSize * 1.16f"
        in scripts["OfficeBoundary3DRenderer.cs"]
    )
    assert (
        "WallThickness = OfficeWorld3DConfig.GridSize * 0.10f"
        in scripts["OfficeBoundary3DRenderer.cs"]
    )
    assert "OfficeWorld3DConfig.OfficeBounds" in scripts["OfficeBoundary3DRenderer.cs"]

    room_renderer = scripts["RoomOverlay3DRenderer.cs"]
    assert "RoomCarpetHeight" in room_renderer
    assert "RoomWallHeight" in room_renderer
    assert "AddRoomCellWalls" in room_renderer
    assert "AddRoomWall" in room_renderer
    assert "AddRoomDoor" in room_renderer
    assert "HighlightedRoomStroke" in room_renderer

    facility_renderer = scripts["Facility3DRenderer.cs"]
    assert "AddFacilityModel" in facility_renderer
    assert "AddDeskModel" in facility_renderer
    assert "AddProductWhiteboardModel" in facility_renderer
    assert "AddServerRackModel" in facility_renderer
    assert "new BoxMesh" in facility_renderer


def test_get_the_best_v2_interaction_controller_uses_scene_relative_paths() -> None:
    controller = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")

    assert 'GetNodeOrNull<Camera3D>("../../OfficeWorld/OfficeCamera")' in controller
    assert 'GetNodeOrNull<PanelContainer>("../../HudRoot/FloatingTooltip")' in controller
    assert 'GetNodeOrNull<Label>("../../HudRoot/FloatingTooltip/TooltipLabel")' in controller
    assert "GetNodeOrNull<PlacementPreview3DController>(" in controller
    assert '"../PlacementPreview3DController"' in controller
    assert 'GetNodeOrNull<BuildModeController>("../BuildModeController")' in controller
    assert 'GetNodeOrNull<RoomOverlay3DRenderer>("../RoomOverlay3DRenderer")' in controller
    assert 'GetNodeOrNull<Facility3DRenderer>("../Facility3DRenderer")' in controller


def test_get_the_best_v2_room_overlay_renders_actual_cells_after_single_cell_delete() -> None:
    renderer = read_text(GODOT_ROOT / "scripts" / "RoomOverlay3DRenderer.cs")

    assert "foreach (var cell in room.Cells)" in renderer
    assert "AddRoomCellCarpet(room, cell)" in renderer
    assert "AddRoomCellWalls(room, cell)" in renderer
    assert "HasNeighbor(room, cell + Vector2I.Left)" in renderer
    assert "HasNeighbor(room, cell + Vector2I.Right)" in renderer
    assert "HasNeighbor(room, cell + Vector2I.Up)" in renderer
    assert "HasNeighbor(room, cell + Vector2I.Down)" in renderer
    assert "SelectionSize(room.MinCell, room.MaxCell" not in renderer


def test_get_the_best_v2_room_build_requires_pending_confirmation_and_door() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "BuildConfirmPanel" in scene_text
    assert "ConfirmStatusLabel" in scene_text
    assert "CancelBuildButton" in scene_text
    assert "ConfirmBuildButton" in scene_text
    assert "BuildConfirmationHudController.cs" in scripts

    build_mode = scripts["BuildModeController.cs"]
    selection = scripts["OfficeSelection3DController.cs"]
    confirm_hud = scripts["BuildConfirmationHudController.cs"]

    assert "PlaceRoomDoor" in build_mode
    assert "PendingRoomSelection" in build_mode
    assert "TryStartPendingRoomSelection" in build_mode
    assert "TrySetPendingDoor" in build_mode
    assert "ConfirmPendingRoom" in build_mode
    assert "CancelPendingRoomSelection" in build_mode
    assert "HasPendingRoomSelection" in build_mode
    assert "CanConfirmPendingRoom" in build_mode
    assert "MinimumRoomWidth = 2" in build_mode
    assert "MinimumRoomHeight = 3" in build_mode
    assert "MeetsMinimumRoomSize" in build_mode
    assert "GetRoomBuildFailureMessage" in build_mode
    assert "IsRoomSelectionTooSmall" in build_mode

    assert "TryStartPendingRoomSelection(_dragStartCell, _dragCurrentCell)" in selection
    assert "FinishDoorPlacement" in selection
    assert "ShouldBeginAreaSelection()" in selection
    assert "BeginSelection(mouseEvent.Position)" in selection
    assert "FinishPointerSelection(mouseEvent.Position)" in selection
    assert "SelectObjectAtPointer(screenPosition)" in selection
    assert "IsPointerSelectionDrag()" in selection
    assert "ShowPointerSelectionRect()" in selection
    pointer_selection_block = selection[
        selection.index("private void FinishPointerSelection") : selection.index(
            "private void FinishDoorPlacement"
        )
    ]
    assert "ShowPointerTooltip(size, screenPosition)" not in pointer_selection_block
    assert "HidePointerTooltip();" in pointer_selection_block
    assert "TryCreateRoom(_dragStartCell" not in selection
    assert "RefreshPendingRoomPreview()" in selection
    assert "ShowRoomDoorPreview" in read_text(
        GODOT_ROOT / "scripts" / "PlacementPreview3DController.cs"
    )
    assert "_doorPreviewMesh" in read_text(
        GODOT_ROOT / "scripts" / "PlacementPreview3DController.cs"
    )
    assert "ShowBuildStatus" in confirm_hud
    assert "ClearBuildStatus" in confirm_hud
    assert "ConfirmRows/CancelBuildButton" in confirm_hud
    assert "CustomMinimumSize = new Vector2(28.0f, 28.0f)" in confirm_hud
    assert "new Vector2(320.0f, 42.0f)" in scripts["MainController.cs"]

    assert "CancelBuildButton" in confirm_hud
    assert "ConfirmBuildButton" in confirm_hud
    assert "ConfirmPendingRoom" in confirm_hud
    assert "CancelPendingRoomSelection" in confirm_hud
    assert "CanConfirmPendingRoom" in confirm_hud


def test_get_the_best_v2_room_door_replaces_old_sign_plate_and_clears_on_delete() -> None:
    room_store = read_text(GODOT_ROOT / "scripts" / "RoomFootprintStore.cs")
    room_renderer = read_text(GODOT_ROOT / "scripts" / "RoomOverlay3DRenderer.cs")
    placement_preview = read_text(GODOT_ROOT / "scripts" / "PlacementPreview3DController.cs")
    door_geometry = read_text(GODOT_ROOT / "scripts" / "RoomDoorGeometry.cs")
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")

    assert "RoomDoorSide" in room_store
    assert "RoomDoorPlacement" in room_store
    assert "DoorPlacement" in room_store
    assert "ClearDoorIfRemoved" in room_store
    assert "RemoveDoorOwnerAtAdjacentCell" in room_store
    assert "GetDoorOutsideCell" in room_store
    assert "RemoveDoorOwnerAtWorldPosition" in room_store
    assert "IsWorldPositionOnDoor" in room_store

    assert "AddRoomDoor(room)" in room_renderer
    assert "IsDoorEdge(room, cell, RoomDoorSide.North)" in room_renderer
    assert "IsDoorEdge(room, cell, RoomDoorSide.South)" in room_renderer
    assert "IsDoorEdge(room, cell, RoomDoorSide.West)" in room_renderer
    assert "IsDoorEdge(room, cell, RoomDoorSide.East)" in room_renderer
    assert "RoomDoorGeometry.GetPosition(doorPlacement)" in room_renderer
    assert "RoomDoorGeometry.GetSize(doorPlacement.Side)" in room_renderer
    assert "RoomDoorGeometry.GetPosition(doorPlacement)" in placement_preview
    assert "RoomDoorGeometry.GetSize(doorPlacement.Side)" in placement_preview
    assert "DoorHeight = OfficeWorld3DConfig.GridSize * 0.92f" in door_geometry
    assert "DoorY = DoorHeight / 2.0f" in door_geometry
    assert "RoomSignPlateFill" not in room_renderer
    assert "AddRoomSignPlate" not in room_renderer
    assert "DeleteSingleCellAtPointer(screenPosition)" in selection
    assert "TryDeleteRoomAtCell(cell" in selection
    assert "TryDeleteRoomAtCell(_dragCurrentCell" in selection
    assert "TryDeleteRoomDoorAtWorldPosition(worldPosition" in selection
    assert "RemoveDoorOwnerAtAdjacentCell(cell" in read_text(
        GODOT_ROOT / "scripts" / "BuildModeController.cs"
    )
    assert "TryDeleteRoomDoorAtWorldPosition" in read_text(
        GODOT_ROOT / "scripts" / "BuildModeController.cs"
    )


def test_get_the_best_v2_0_7_camera_composition_and_rotation_baseline_exists() -> None:
    camera = read_text(GODOT_ROOT / "scripts" / "OfficeCamera3DController.cs")
    grid = read_text(GODOT_ROOT / "scripts" / "OfficeGrid3DRenderer.cs")
    boundary = read_text(GODOT_ROOT / "scripts" / "OfficeBoundary3DRenderer.cs")

    assert "DefaultCameraSize = 112.0f" in camera
    assert "MinCameraSize = 28.0f" in camera
    assert "MaxCameraSize = 210.0f" in camera
    assert "YawDegrees" in camera
    assert "RotateLeftKey = Key.Q" in camera
    assert "RotateRightKey = Key.E" in camera
    assert "RotationSpeedDegrees = 90.0f" in camera
    assert "ApplyCameraPose()" in camera
    assert "GetPlanarForward()" in camera
    assert "GetPlanarRight()" in camera
    assert "Size = Mathf.Min(" in camera
    assert "GetViewport().SizeChanged" in camera

    assert "BuildFloorTiles" in grid
    assert "FloorTileA" in grid
    assert "FloorTileB" in grid
    assert "FloorTileTexturePath" in grid
    assert "GD.Load<Texture2D>(FloorTileTexturePath)" in grid
    assert "GridLineThickness" not in grid
    assert "AddLine" not in grid

    assert "BuildingWallScenePath" in boundary
    assert "CornerPostColor" in boundary
    assert "AddCornerPost" in boundary


def test_get_the_best_v2_0_9_large_build_cells_and_middle_pitch_baseline_exists() -> None:
    config = read_text(GODOT_ROOT / "scripts" / "OfficeWorld3DConfig.cs")
    camera = read_text(GODOT_ROOT / "scripts" / "OfficeCamera3DController.cs")
    grid = read_text(GODOT_ROOT / "scripts" / "OfficeGrid3DRenderer.cs")
    door = read_text(GODOT_ROOT / "scripts" / "RoomDoorGeometry.cs")
    facility = read_text(GODOT_ROOT / "scripts" / "Facility3DRenderer.cs")
    room_renderer = read_text(GODOT_ROOT / "scripts" / "RoomOverlay3DRenderer.cs")

    assert "SourcePixelWidth = 6400" in config
    assert "SourcePixelHeight = 4000" in config
    assert "Columns = 32" in config
    assert "Rows = 20" in config
    assert "GridSize = 10.0f" in config
    assert "new Vector2(Columns * GridSize, Rows * GridSize)" in config

    assert "CameraPitchDegrees = 42.0f" in camera
    assert "MiddleRotateSensitivity = 0.22f" in camera
    assert "EdgePanMarginPixels = 28.0f" in camera
    assert "EdgePanSpeed = 84.0f" in camera
    assert "AdjustYawFromMiddleDrag" in camera
    assert "PanCameraByMouseDelta" in camera
    assert "PanCameraByDirection" in camera
    assert "GetEdgePanDirection" in camera
    assert "UpdateLastMousePosition" in camera
    assert "_lastMousePosition" in camera
    assert "_isMouseInsideViewport" in camera
    assert "_isMiddleRotating" in camera
    assert "_isRightPanning" in camera
    assert "NotificationWMMouseEnter" in camera
    assert "NotificationWMMouseExit" in camera
    assert "if (!_hasMousePosition || !_isMouseInsideViewport || _isMiddleRotating)" in camera
    assert "AdjustPitchFromMiddleDrag" not in camera
    assert "_isPitchDragging" not in camera
    assert "ApplyStableCameraBasis(lookDirection)" in camera
    assert "Basis.LookingAt(lookDirection, Vector3.Up)" in camera
    assert "RotationDegrees = new Vector3(-_pitchDegrees, -YawDegrees, 0.0f)" not in camera
    assert "_focus += -GetPlanarRight() * motionEvent.Relative.X" not in camera

    assert "MajorGridColor" not in grid
    assert "lineIndex % 5 == 0" not in grid
    assert "BuildFloorTiles" in grid
    assert "TileInset = OfficeWorld3DConfig.GridSize * 0.035f" in grid
    assert "CreateTileMaterial" in grid
    assert "DoorLength = OfficeWorld3DConfig.GridSize * 0.72f" in door
    assert "DoorThickness = OfficeWorld3DConfig.GridSize * 0.11f" in door
    assert "DoorHeight = OfficeWorld3DConfig.GridSize * 0.92f" in door

    assert "CellInnerSize = OfficeWorld3DConfig.GridSize * 0.72f" in facility
    assert "OutlineShellScale = 1.10f" in facility
    assert "RoomWallHeight = OfficeWorld3DConfig.GridSize * 1.16f" in room_renderer
    assert "RoomWallThickness = OfficeWorld3DConfig.GridSize * 0.10f" in room_renderer


def test_get_the_best_v2_interaction_right_click_and_door_delete_are_precise() -> None:
    camera = read_text(GODOT_ROOT / "scripts" / "OfficeCamera3DController.cs")
    build_mode = read_text(GODOT_ROOT / "scripts" / "BuildModeController.cs")
    room_store = read_text(GODOT_ROOT / "scripts" / "RoomFootprintStore.cs")

    assert "mouseEvent.ButtonIndex == MouseButton.Middle" in camera
    assert "mouseEvent.ButtonIndex == MouseButton.Right" in camera
    assert "_isRightPanning = mouseEvent.Pressed" in camera
    assert "CancelInteraction();" in read_text(
        GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs"
    )

    assert "RemoveDoorOwnerAtAdjacentCell(cell" in build_mode
    assert "RemoveDoorOwnerNearCell" not in build_mode
    assert "RemoveDoorOwnerAtWorldPosition" in build_mode

    assert "RemoveDoorOwnerAtAdjacentCell" in room_store
    assert "RemoveDoorOwnerAtWorldPosition" in room_store
    assert "RemoveDoorOwnerNearCell" not in room_store
    assert "Mathf.Abs(outsideCell.X - cell.X) <= 2" not in room_store


def test_get_the_best_v2_0_8_facility_feedback_and_single_sell_baseline_exists() -> None:
    build_mode = read_text(GODOT_ROOT / "scripts" / "BuildModeController.cs")
    facility_store = read_text(GODOT_ROOT / "scripts" / "FacilityPlacementStore.cs")
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")
    preview = read_text(GODOT_ROOT / "scripts" / "PlacementPreview3DController.cs")

    assert "enum FacilityPlacementIssue" in facility_store
    assert (
        "CanPlace(FacilityBuildType facilityType, Vector2I cell, out FacilityPlacementIssue issue)"
        in facility_store
    )
    assert "FacilityPlacementIssue.Occupied" in facility_store
    assert "FacilityPlacementIssue.MissingRequiredRoom" in facility_store
    assert "FacilityPlacementIssue.WrongRoomType" in facility_store

    assert "GetFacilityPlacementIssue" in build_mode
    assert "GetFacilityPlacementFailureMessage" in build_mode
    assert "GetRequiredRoomType(_activeFacilityType)" in build_mode
    assert "\\u9700\\u8981" in build_mode
    assert "\\u683c\\u5b50\\u5df2\\u5360\\u7528" in build_mode

    assert "ShowFacilityCell(" in selection
    assert (
        "FacilityDefinitionCatalog.GetDefinition(_buildModeController.GetActiveFacilityType())"
        in selection
    )
    assert "definition.Footprint" in preview
    assert "ShowFacilityPlacementPreview(cell, screenPosition)" in selection
    assert "GetFacilityPlacementFailureMessage(cell)" in selection
    assert "FinishFacilityPlacement(mouseEvent.Position)" in selection
    assert "ShowFacilityTooltip(facility, screenPosition)" in selection
    assert (
        "return;"
        in selection[
            selection.index("if (deletedFacilities > 0)") : selection.index(
                "if (_buildModeController?.TryDeleteRoomAtCell(cell"
            )
        ]
    )


def test_get_the_best_v2_facilities_and_rooms_use_procedural_3d_models() -> None:
    facility = read_text(GODOT_ROOT / "scripts" / "Facility3DRenderer.cs")
    room = read_text(GODOT_ROOT / "scripts" / "RoomOverlay3DRenderer.cs")
    boundary = read_text(GODOT_ROOT / "scripts" / "OfficeBoundary3DRenderer.cs")

    assert "Sprite3D" not in facility
    assert "AddDeskModel" in facility
    assert "AddProductWhiteboardModel" in facility
    assert "AddServerRackModel" in facility
    assert "AddMeshPart" in facility
    assert "BoxMesh" in facility
    assert "CylinderMesh" in facility

    assert "RoomWallHeight = OfficeWorld3DConfig.GridSize * 1.16f" in room
    assert "AddRoomWall" in room
    assert "AddDoorFrame" in room
    assert "WallTrimColor" in room
    assert "RoomBoundaryHeight = OfficeWorld3DConfig.GridSize * 0.03f" not in room

    assert "WallHeight = OfficeWorld3DConfig.GridSize * 1.16f" in boundary


def test_get_the_best_v2_facility_placement_supports_r_rotation() -> None:
    build_mode = read_text(GODOT_ROOT / "scripts" / "BuildModeController.cs")
    facility_store = read_text(GODOT_ROOT / "scripts" / "FacilityPlacementStore.cs")
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")
    preview = read_text(GODOT_ROOT / "scripts" / "PlacementPreview3DController.cs")
    renderer = read_text(GODOT_ROOT / "scripts" / "Facility3DRenderer.cs")

    assert "enum FacilityFacing" in facility_store
    assert "FacilityFacing Facing" in facility_store
    assert "FacilityFacing _activeFacilityFacing" in build_mode
    assert "RotateActiveFacilityFacing" in build_mode
    assert "GetActiveFacilityFacing" in build_mode
    assert "TryPlace(_activeFacilityType, cell, _activeFacilityFacing" in build_mode
    assert "Key.R" in selection
    assert "RotateActiveFacilityFacing()" in selection
    assert "ShowFacilityPlacementPreview(_lastHoveredCell.Value" in selection
    assert "GetActiveFacilityFacing()" in selection
    assert "ShowFacilityFacingMarker" in preview
    assert "_facilityFacingPreviewMesh" in preview
    assert "GetFacingYawDegrees(facility.Facing)" in renderer


def test_get_the_best_v2_employee_visual_selection_baseline_exists() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "EmployeeStore" in scene_text
    assert "Employee3DRenderer" in scene_text
    assert "EmployeeStore.cs" in scripts
    assert "Employee3DRenderer.cs" in scripts

    employee_store = scripts["EmployeeStore.cs"]
    employee_renderer = scripts["Employee3DRenderer.cs"]
    selection = scripts["OfficeSelection3DController.cs"]

    assert "EmployeeVisual" in employee_store
    assert "GetEmployees" in employee_store
    assert "FindAtCell" in employee_store
    assert "FindInSelection" in employee_store
    assert "new Vector2I(9, 7)" in employee_store
    assert "new Vector2I(10, 7)" in employee_store
    assert "new Vector2I(11, 7)" in employee_store

    assert "RefreshEmployees" in employee_renderer
    assert "HighlightEmployee" in employee_renderer
    assert "HighlightEmployees" in employee_renderer
    assert "AddEmployeeModel" in employee_renderer
    assert "EmployeeModelScenePaths" in employee_renderer
    assert "PackedScene" in employee_renderer
    assert "ApplyEmployeeOutline" in employee_renderer

    assert 'GetNodeOrNull<EmployeeStore>("../EmployeeStore")' in selection
    assert 'GetNodeOrNull<Employee3DRenderer>("../Employee3DRenderer")' in selection
    assert "SelectEmployeeAtPointer" in selection
    assert "SelectEmployeesInSelection" in selection
    assert "_employeeRenderer?.HighlightEmployee(null);" in selection
    assert "FindInSelection(_dragStartCell, _dragCurrentCell)" in selection
    assert "ShowEmployeeTooltip" in selection


def test_get_the_best_v2_employee_drag_and_default_camera_baseline_exists() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    employee_store = read_text(GODOT_ROOT / "scripts" / "EmployeeStore.cs")
    employee_renderer = read_text(GODOT_ROOT / "scripts" / "Employee3DRenderer.cs")
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")
    camera = read_text(GODOT_ROOT / "scripts" / "OfficeCamera3DController.cs")

    assert "size = 112.0" in scene_text

    assert "CanMoveEmployee" in employee_store
    assert "TryMoveEmployee" in employee_store
    assert "OfficeNavigationStore? _officeNavigationStore" in employee_store
    assert "_officeNavigationStore?.CanStandAt(targetCell) == true" in employee_store
    assert "employee with { Cell = targetCell }" in employee_store

    assert "ShowEmployeeDragPreview" in employee_renderer
    assert "ClearEmployeeDragPreview" in employee_renderer
    assert "_dragPreviewEmployeeId" in employee_renderer
    assert "_dragPreviewCell" in employee_renderer
    assert "_dragPreviewIsLegal" in employee_renderer
    assert "GetRenderCell(employee)" in employee_renderer
    assert "ApplyEmployeeTint(modelRoot, IllegalDragFill)" in employee_renderer
    assert "DragPreviewYOffset" in employee_renderer

    assert "_isDraggingEmployee" in selection
    assert "TryBeginEmployeeDrag(mouseEvent.Position)" in selection
    assert "UpdateEmployeeDragPreview(cell, screenPosition)" in selection
    assert "FinishEmployeeDrag(mouseEvent.Position)" in selection
    assert "CancelEmployeeDrag" in selection
    assert "ShowSelectionRect(cell, cell, _dragEmployeeTargetLegal)" not in selection
    assert "ShowEmployeeDragPreview(_draggedEmployee, cell, _dragEmployeeTargetLegal)" in selection
    assert "ClearEmployeeDragPreview()" in selection
    assert "TryMoveEmployee(" in selection
    assert "_dragEmployeeCurrentCell" in selection

    assert "DefaultCameraSize = 112.0f" in camera
    assert "ZoomStepFactor = 1.08f" in camera
    assert "YawDegrees = 0.0f" in camera
    assert "DefaultFocus = new(0.0f, 0.0f, -40.0f)" in camera
    assert "ApplyDefaultCameraSize" in camera
    assert "_hasMousePosition" in camera
    assert "if (!_hasMousePosition || !_isMouseInsideViewport || _isMiddleRotating)" in camera
    assert "Mathf.Max(heightFit, widthFit)" not in camera


def test_get_the_best_v2_object_selection_uses_instance_outlines_not_grid_bases() -> None:
    employee_renderer = read_text(GODOT_ROOT / "scripts" / "Employee3DRenderer.cs")
    facility_renderer = read_text(GODOT_ROOT / "scripts" / "Facility3DRenderer.cs")
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")

    assert "OutlineStroke" in employee_renderer
    assert "EmployeeModelScenePaths" in employee_renderer
    assert "GD.Load<PackedScene>(modelScenePath)" in employee_renderer
    assert "Instantiate<Node3D>()" in employee_renderer
    assert "GetEmployeeModelScene" in employee_renderer
    assert "ApplyEmployeeOutline" in employee_renderer
    assert "CreateOutlineMaterial" in employee_renderer
    assert "material.NextPass = CreateOutlineMaterial(outlineColor)" in employee_renderer
    assert "CullMode = BaseMaterial3D.CullModeEnum.Front" in employee_renderer
    assert "HoverEmployee" in employee_renderer
    assert "SelectionFill" not in employee_renderer
    assert "AddSelectionRing" not in employee_renderer
    assert "AddOutlinePost" not in employee_renderer
    assert "AddEmployeeOutlineShell" not in employee_renderer
    assert "OutlineHeight" not in employee_renderer
    assert "OutlineRadius" not in employee_renderer

    assert "OutlineStroke" in facility_renderer
    assert "AddFacilityOutlineShell" in facility_renderer
    assert "CreateOutlineMaterial" in facility_renderer
    assert "CullMode = BaseMaterial3D.CullModeEnum.Front" in facility_renderer
    assert "HoverFacility" in facility_renderer
    assert "AddHighlight(position)" not in facility_renderer
    assert "HighlightStrokeSize" not in facility_renderer

    assert "_employeeRenderer?.HoverEmployee(hoveredEmployee);" in selection
    assert "_facilityRenderer?.HoverFacility(hoveredFacility);" in selection
    assert "ShowSelectionRect(cell, cell, _dragEmployeeTargetLegal)" not in selection


def test_get_the_best_v2_employee_uses_kenney_glb_model_resources() -> None:
    employee_renderer = read_text(GODOT_ROOT / "scripts" / "Employee3DRenderer.cs")
    model_dir = (
        GODOT_ROOT / "assets" / "third_party_placeholder_assets" / "kenney_blocky_characters"
    )

    assert (model_dir / "character-a.glb").exists()
    assert (model_dir / "character-b.glb").exists()
    assert (model_dir / "character-c.glb").exists()
    assert (model_dir / "Textures" / "texture-a.png").exists()
    assert (model_dir / "Textures" / "texture-b.png").exists()
    assert (model_dir / "Textures" / "texture-c.png").exists()
    assert (model_dir / "License.txt").exists()
    assert (
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/character-a.glb"
        in employee_renderer
    )
    assert (
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/character-b.glb"
        in employee_renderer
    )
    assert (
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/character-c.glb"
        in employee_renderer
    )
    assert (
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/Textures/texture-a.png"
        in employee_renderer
    )
    assert (
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/Textures/texture-b.png"
        in employee_renderer
    )
    assert (
        "res://assets/third_party_placeholder_assets/kenney_blocky_characters/Textures/texture-c.png"
        in employee_renderer
    )
    assert "ApplyEmployeeTexture(modelRoot, GetEmployeeTexture(employee));" in employee_renderer
    assert "RemoveChild(renderedEmployee);" in employee_renderer
    assert (
        "private const float EmployeeModelScale = OfficeWorld3DConfig.GridSize * 0.34f"
        in employee_renderer
    )
    assert "new CylinderMesh" not in employee_renderer
    assert "new SphereMesh" not in employee_renderer
    assert "new BoxMesh" not in employee_renderer


def test_get_the_best_v2_floor_and_wall_use_kenney_texture_resources() -> None:
    grid = read_text(GODOT_ROOT / "scripts" / "OfficeGrid3DRenderer.cs")
    floor = read_text(GODOT_ROOT / "scripts" / "OfficeFloor3DRenderer.cs")
    room = read_text(GODOT_ROOT / "scripts" / "RoomOverlay3DRenderer.cs")
    boundary = read_text(GODOT_ROOT / "scripts" / "OfficeBoundary3DRenderer.cs")

    texture_dir = (
        GODOT_ROOT / "assets" / "third_party_placeholder_assets" / "kenney_prototype_textures"
    )
    building_dir = GODOT_ROOT / "assets" / "third_party_placeholder_assets" / "kenney_building_kit"
    assert (texture_dir / "floor_light_texture_02.png").exists()
    assert (texture_dir / "License.txt").exists()
    assert (building_dir / "wall.glb").exists()
    assert (building_dir / "License.txt").exists()

    assert "FloorTileTexturePath" in grid
    assert (
        "res://assets/third_party_placeholder_assets/kenney_prototype_textures/floor_light_texture_02.png"
        in grid
    )
    assert "AlbedoTexture = LoadFloorTileTexture()" in grid
    assert "FloorBaseTexturePath" in floor
    assert (
        "res://assets/third_party_placeholder_assets/kenney_prototype_textures/floor_light_texture_02.png"
        in floor
    )
    assert "AlbedoTexture = GD.Load<Texture2D>(FloorBaseTexturePath)" in floor
    assert "BuildingWallScenePath" in room
    assert "BuildingWallScenePath" in boundary
    assert "wall_dark_texture_03.png" not in room
    assert "wall_dark_texture_03.png" not in boundary


def test_get_the_best_v2_0_18_drag_preview_is_smoothed_and_hit_radius_is_tight() -> None:
    employee_renderer = read_text(GODOT_ROOT / "scripts" / "Employee3DRenderer.cs")
    facility_renderer = read_text(GODOT_ROOT / "scripts" / "Facility3DRenderer.cs")
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")

    assert "SmoothMoveDurationSeconds" in employee_renderer
    assert "_lastEmployeePositions" in employee_renderer
    assert "CreateTween()" in employee_renderer
    assert (
        'TweenProperty(modelRoot, "position", targetPosition, SmoothMoveDurationSeconds)'
        in employee_renderer
    )

    assert "SmoothMoveDurationSeconds" in facility_renderer
    assert "_lastFacilityPositions" in facility_renderer
    assert "CreateTween()" in facility_renderer
    assert (
        'TweenProperty(modelRoot, "position", targetPosition, SmoothMoveDurationSeconds)'
        in facility_renderer
    )
    assert 'modelRoot.Name = $"Facility_{facility.Id}"' in facility_renderer
    assert "RemoveChild(renderedFacility);" in facility_renderer

    assert "ObjectHitRadiusPixels" not in selection
    assert "EmployeeHitRadiusPixels = 28.0f" in selection
    assert "FacilityHitRadiusPixels = 34.0f" in selection
    assert "distance > EmployeeHitRadiusPixels" in selection
    assert "distance > FacilityHitRadiusPixels" in selection


def test_get_the_best_v2_0_18_walls_and_doors_use_building_kit_assets() -> None:
    asset_index_path = GODOT_ROOT / "assets" / "third_party_placeholder_assets" / "asset-index.json"
    asset_index = json.loads(read_text(asset_index_path))
    assets = {asset["asset_id"]: asset for asset in asset_index["assets"]}
    room_renderer = read_text(GODOT_ROOT / "scripts" / "RoomOverlay3DRenderer.cs")
    boundary_renderer = read_text(GODOT_ROOT / "scripts" / "OfficeBoundary3DRenderer.cs")
    door_geometry = read_text(GODOT_ROOT / "scripts" / "RoomDoorGeometry.cs")

    expected_assets = {
        "kenney_building_kit_wall": "res://assets/third_party_placeholder_assets/kenney_building_kit/wall.glb",
        "kenney_building_kit_door_rotate_square_a": "res://assets/third_party_placeholder_assets/kenney_building_kit/door-rotate-square-a.glb",
        "kenney_building_kit_colormap": "res://assets/third_party_placeholder_assets/kenney_building_kit/Textures/colormap.png",
    }
    for asset_id, imported_path in expected_assets.items():
        assert asset_id in assets
        asset = assets[asset_id]
        assert asset["source_site"] == "Kenney"
        assert asset["license"] == "CC0-1.0"
        assert asset["commercial_use_allowed"] is True
        assert asset["attribution_required"] is False
        assert asset["imported_path"] == imported_path
        assert (GODOT_ROOT / imported_path.replace("res://", "")).exists()

    assert "BuildingWallScenePath" in room_renderer
    assert "BuildingDoorScenePath" in room_renderer
    assert "BuildingKitColormapPath" in room_renderer
    assert "AddBuildingDoorModel(doorPlacement)" in room_renderer
    assert "GD.Load<PackedScene>(BuildingDoorScenePath)" in room_renderer
    assert "CreateBuildingDoorMaterial" in room_renderer
    assert "WallTexturePath" not in room_renderer
    assert "wall_dark_texture_03.png" not in room_renderer
    assert "RoomWallHeight = OfficeWorld3DConfig.GridSize * 1.16f" in room_renderer

    assert "BuildingWallScenePath" in boundary_renderer
    assert "WallTexturePath" not in boundary_renderer
    assert "wall_dark_texture_03.png" not in boundary_renderer
    assert "WallHeight = OfficeWorld3DConfig.GridSize * 1.16f" in boundary_renderer

    assert "DoorHeight = OfficeWorld3DConfig.GridSize * 0.92f" in door_geometry
    assert "DoorY = DoorHeight / 2.0f" in door_geometry


def test_get_the_best_v2_instance_drag_uses_click_pickup_and_click_drop() -> None:
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")

    left_click_block = selection[
        selection.index("if (mouseEvent.ButtonIndex != MouseButton.Left)") : selection.index(
            "private void BeginSelection"
        )
    ]
    pressed_block = left_click_block[
        left_click_block.index("if (mouseEvent.Pressed)") : left_click_block.index(
            "if (_isDraggingEmployee || _isDraggingFacility)"
        )
    ]

    assert "FinishEmployeeDrag(mouseEvent.Position);" in pressed_block
    assert "FinishFacilityDrag(mouseEvent.Position);" in pressed_block
    assert "TryBeginEmployeeDrag(mouseEvent.Position)" in pressed_block
    assert "TryBeginFacilityDrag(mouseEvent.Position)" in pressed_block

    release_block = left_click_block[
        left_click_block.index("if (_isDraggingEmployee || _isDraggingFacility)") :
    ]
    assert "FinishEmployeeDrag(mouseEvent.Position);" not in release_block
    assert "FinishFacilityDrag(mouseEvent.Position);" not in release_block
    assert '"\\u70b9\\u51fb\\u653e\\u4e0b"' in selection


def test_get_the_best_v2_employee_drag_drop_clears_object_and_grid_preview() -> None:
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")

    employee_finish_block = selection[
        selection.index("private void FinishEmployeeDrag") : selection.index(
            "private void CancelEmployeeDrag"
        )
    ]

    assert "ClearObjectHoverState();" in employee_finish_block
    assert "_employeeRenderer?.HighlightEmployee(null);" in employee_finish_block
    assert "_placementPreviewController?.ClearPreview();" in employee_finish_block
    assert "HighlightEmployee(movedEmployee)" not in employee_finish_block
    assert "ShowEmployeeTooltip(movedEmployee" not in employee_finish_block


def test_get_the_best_v2_facility_instances_support_hover_selection_and_drag_move() -> None:
    facility_store = read_text(GODOT_ROOT / "scripts" / "FacilityPlacementStore.cs")
    facility_renderer = read_text(GODOT_ROOT / "scripts" / "Facility3DRenderer.cs")
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")

    assert "CanMoveFacility" in facility_store
    assert "TryMoveFacility" in facility_store
    assert "FindAtCellExcluding" in facility_store
    assert "facility with { Cell = targetCell }" in facility_store

    assert "ShowFacilityDragPreview" in facility_renderer
    assert "ClearFacilityDragPreview" in facility_renderer
    assert "_dragPreviewFacilityId" in facility_renderer
    assert "GetRenderCell(facility)" in facility_renderer
    assert "GetRenderTint(facility)" in facility_renderer

    assert "_isDraggingFacility" in selection
    assert "TryBeginFacilityDrag(mouseEvent.Position)" in selection
    assert "UpdateFacilityDragPreview(cell, screenPosition)" in selection
    assert "FinishFacilityDrag(mouseEvent.Position)" in selection
    assert "CancelFacilityDrag" in selection
    assert "_facilityRenderer?.ClearFacilityDragPreview();" in selection
    assert "_facilityRenderer?.HighlightFacility(null);" in selection


def test_get_the_best_v2_object_hit_testing_uses_screen_projected_instances() -> None:
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")

    assert "EmployeeHitRadiusPixels = 28.0f" in selection
    assert "FacilityHitRadiusPixels = 34.0f" in selection
    assert "ObjectHitRadiusPixels" not in selection
    assert "TryScreenPositionToEmployee" in selection
    assert "TryScreenPositionToFacility" in selection
    assert "_camera.UnprojectPosition" in selection

    employee_drag_block = selection[
        selection.index("private bool TryBeginEmployeeDrag") : selection.index(
            "private void UpdateEmployeeDragPreview"
        )
    ]
    facility_drag_block = selection[
        selection.index("private bool TryBeginFacilityDrag") : selection.index(
            "private void UpdateFacilityDragPreview"
        )
    ]
    select_block = selection[
        selection.index("private void SelectObjectAtPointer") : selection.index(
            "private void SelectEmployeeAtPointer"
        )
    ]

    assert "TryScreenPositionToEmployee(screenPosition, out var employee)" in employee_drag_block
    assert "TryScreenPositionToFacility(screenPosition, out var facility)" in facility_drag_block
    assert "TryScreenPositionToEmployee(screenPosition, out var employee)" in select_block
    assert "TryScreenPositionToFacility(screenPosition, out var facility)" in select_block


def test_get_the_best_v2_sandbox_has_reasonable_preset_office_scene() -> None:
    room_store = read_text(GODOT_ROOT / "scripts" / "RoomFootprintStore.cs")
    facility_store = read_text(GODOT_ROOT / "scripts" / "FacilityPlacementStore.cs")
    employee_store = read_text(GODOT_ROOT / "scripts" / "EmployeeStore.cs")
    room_renderer = read_text(GODOT_ROOT / "scripts" / "RoomOverlay3DRenderer.cs")
    facility_renderer = read_text(GODOT_ROOT / "scripts" / "Facility3DRenderer.cs")

    assert "SeedPresetOfficeRooms" in room_store
    assert "RoomBuildType.ResearchRoom" in room_store
    assert "RoomBuildType.MarketRoom" in room_store
    assert "RoomBuildType.ServerRoom" in room_store
    assert "RoomDoorPlacement" in room_store

    assert "SeedPresetFacilities" in facility_store
    assert "FacilityBuildType.OfficeDesk" in facility_store
    assert "FacilityBuildType.ProductWhiteboard" in facility_store
    assert "FacilityBuildType.ServerRack" in facility_store

    assert "new Vector2I(9, 7)" in employee_store
    assert "new Vector2I(10, 7)" in employee_store
    assert "new Vector2I(11, 7)" in employee_store

    assert (
        '_roomFootprintStore = GetNodeOrNull<RoomFootprintStore>("../RoomFootprintStore");'
        in room_renderer
    )
    assert (
        "RefreshRooms();"
        in room_renderer[
            room_renderer.index("public override void _Ready") : room_renderer.index(
                "public void RefreshRooms"
            )
        ]
    )
    assert (
        '_facilityPlacementStore = GetNodeOrNull<FacilityPlacementStore>("../FacilityPlacementStore");'
        in facility_renderer
    )
    assert (
        "RefreshFacilities();"
        in facility_renderer[
            facility_renderer.index("public override void _Ready") : facility_renderer.index(
                "public void RefreshFacilities"
            )
        ]
    )


def test_get_the_best_v2_0_19_office_navigation_store_defines_walkability_pathing() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    scripts = {
        path.name: read_text(path)
        for path in (GODOT_ROOT / "scripts").glob("*.cs")
        if path.is_file()
    }

    assert "OfficeNavigationStore" in scene_text
    assert "OfficeNavigationStore.cs" in scripts

    navigation = scripts["OfficeNavigationStore.cs"]
    assert "public partial class OfficeNavigationStore : Node" in navigation
    assert "RoomFootprintStore? _roomFootprintStore" in navigation
    assert "FacilityPlacementStore? _facilityPlacementStore" in navigation
    assert "IsInsideOffice(Vector2I cell)" in navigation
    assert "IsWalkable(Vector2I cell)" in navigation
    assert "IsBlocked(Vector2I cell)" in navigation
    assert "CanStandAt(Vector2I cell)" in navigation
    assert "IsDoorPassage(Vector2I fromCell, Vector2I toCell)" in navigation
    assert "CanMoveBetween(Vector2I fromCell, Vector2I toCell)" in navigation
    assert "FindPath(Vector2I startCell, Vector2I targetCell)" in navigation
    assert "Queue<Vector2I>" in navigation
    assert "ReconstructPath" in navigation
    assert "GetDoorOutsideCell" in navigation


def test_get_the_best_v2_0_19_drag_legality_uses_navigation_store() -> None:
    employee_store = read_text(GODOT_ROOT / "scripts" / "EmployeeStore.cs")
    facility_store = read_text(GODOT_ROOT / "scripts" / "FacilityPlacementStore.cs")
    selection = read_text(GODOT_ROOT / "scripts" / "OfficeSelection3DController.cs")

    assert "OfficeNavigationStore? _officeNavigationStore" in employee_store
    assert 'GetNodeOrNull<OfficeNavigationStore>("../OfficeNavigationStore")' in employee_store
    assert "_officeNavigationStore?.CanStandAt(targetCell) == true" in employee_store
    assert "IsCellInsideOffice" not in employee_store

    assert "OfficeNavigationStore? _officeNavigationStore" in facility_store
    assert 'GetNodeOrNull<OfficeNavigationStore>("../OfficeNavigationStore")' in facility_store
    assert "_officeNavigationStore?.CanStandAt(cell) == true" in facility_store
    assert "_officeNavigationStore?.CanStandAt(targetCell, facility.Id) == true" in facility_store
    assert "_officeNavigationStore?.IsDoorCell(cell) == true" in facility_store
    assert "IsCellInsideOffice" not in facility_store

    assert 'GetNodeOrNull<OfficeNavigationStore>("../OfficeNavigationStore")' in selection
    assert "_officeNavigationStore?.FindPath(_dragEmployeeOriginCell, cell)" in selection


def test_get_the_best_v2_0_20_employee_autonomy_moves_on_navigation_path() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    employee_renderer = read_text(GODOT_ROOT / "scripts" / "Employee3DRenderer.cs")
    employee_autonomy = read_text(GODOT_ROOT / "scripts" / "EmployeeAutonomyController.cs")

    assert "EmployeeAutonomyController" in scene_text
    assert "public partial class EmployeeAutonomyController : Node" in employee_autonomy
    assert "EmployeeActivityKind" in employee_autonomy
    assert "EmployeeAutonomyState" in employee_autonomy
    assert "AutonomousMoveIntervalSeconds" in employee_autonomy
    assert "MaxAutonomousPathCells" in employee_autonomy
    assert "CandidateTargetOffsets" in employee_autonomy
    assert "TryStartNextAutonomousMove" in employee_autonomy
    assert "FindAutonomousTarget" in employee_autonomy
    assert "_officeNavigationStore.FindPath(employee.Cell, candidate)" in employee_autonomy
    assert "CanMoveEmployee(employee, candidate)" in employee_autonomy
    assert "_employeeRenderer?.PlayEmployeePathMove(employee, path, () =>" in employee_autonomy
    assert "TryMoveEmployee(employeeId, targetCell, out var movedEmployee)" in employee_autonomy
    assert "EmployeeActivityKind.WalkingToTarget" in employee_autonomy

    assert "PathMoveStepDurationSeconds" in employee_renderer
    assert "_pathMovingEmployeeId" in employee_renderer
    assert "PlayEmployeePathMove(" in employee_renderer
    assert "IReadOnlyList<Vector2I> path" in employee_renderer
    assert "TweenEmployeePathStep" in employee_renderer
    assert "tween.Finished +=" in employee_renderer


def test_get_the_best_v2_0_21_employee_autonomy_uses_facility_targets() -> None:
    employee_autonomy = read_text(GODOT_ROOT / "scripts" / "EmployeeAutonomyController.cs")
    facility_renderer = read_text(GODOT_ROOT / "scripts" / "Facility3DRenderer.cs")

    assert "FacilityPlacementStore? _facilityPlacementStore" in employee_autonomy
    assert "Facility3DRenderer? _facilityRenderer" in employee_autonomy
    assert "CoreLifecycleTickSeconds" in employee_autonomy
    assert "FacilityInteractionTarget" in employee_autonomy
    assert "WalkingToFacility" in employee_autonomy
    assert "UsingFacility" in employee_autonomy
    assert "TryStartFacilityUseBehavior" in employee_autonomy
    assert "FindFacilityUseTarget" in employee_autonomy
    assert "PlanEmployeeIntents(" in employee_autonomy
    assert "GetFacilityInteractionCells(facility)" in employee_autonomy
    assert "_officeNavigationStore.FindPath(employee.Cell, standCell)" in employee_autonomy
    assert (
        "_employeeRenderer?.PlayEmployeePathMove(employee, target.Path, () =>" in employee_autonomy
    )
    assert "FinishFacilityArrival(employee.Id, target)" in employee_autonomy
    assert (
        "_facilityRenderer?.SetFacilityUseState(facilityId, activeFacilityIds.Contains(facilityId))"
        in employee_autonomy
    )
    assert "ApplyCoreLifecycleStates(lifecycleStates)" in employee_autonomy

    assert "_usingFacilityIds" in facility_renderer
    assert "SetFacilityUseState(int facilityId, bool isInUse)" in facility_renderer
    assert "_usingFacilityIds.Contains(facility.Id)" in facility_renderer


def test_get_the_best_v2_0_23_employee_activity_badges_follow_autonomy_state() -> None:
    employee_autonomy = read_text(GODOT_ROOT / "scripts" / "EmployeeAutonomyController.cs")
    employee_renderer = read_text(GODOT_ROOT / "scripts" / "Employee3DRenderer.cs")

    assert "_employeeActivityLabels" in employee_renderer
    assert "SetEmployeeActivityLabel(int employeeId, string? labelText)" in employee_renderer
    assert "AddEmployeeActivityBadge(modelRoot, employee)" in employee_renderer
    assert "new Label3D" in employee_renderer
    assert "Text = labelText" in employee_renderer
    assert "Billboard = BaseMaterial3D.BillboardModeEnum.Enabled" in employee_renderer
    assert "NoDepthTest = true" in employee_renderer
    assert "FixedSize = false" in employee_renderer

    assert "EmployeeActivityKind.WalkingToFacility" in employee_autonomy
    assert "EmployeeActivityKind.UsingFacility" in employee_autonomy
    assert "ClearEmployeeActivity(employeeId)" in employee_autonomy
    assert "GetActivityLabel(activityKind)" in employee_autonomy


def test_get_the_best_v2_0_24_godot_autonomy_consumes_core_intents() -> None:
    csproj = read_text(GODOT_ROOT / "GetTheBestGodot.csproj")
    bridge = read_text(GODOT_ROOT / "scripts" / "V2CoreBridge.cs")
    employee_autonomy = read_text(GODOT_ROOT / "scripts" / "EmployeeAutonomyController.cs")

    assert "..\\..\\csharp\\StartupSim.Core\\StartupSim.Core.csproj" in csproj
    assert "using StartupSim.Core;" in bridge
    assert "GodotCoreBridgeContract" in bridge
    assert "EmployeeBehaviorEngine" in bridge
    assert "BuildSnapshot(" in bridge
    assert "PlanEmployeeIntents(" in bridge
    assert "GodotOfficeSnapshotDto" in bridge
    assert "GodotEmployeeFactDto" in bridge
    assert "GodotFacilityFactDto" in bridge
    assert "GodotRoomFactDto" in bridge
    assert "CoreEmployeeIntent" in bridge
    assert "ParseCoreFacilityId" in bridge

    assert "V2CoreBridge? _v2CoreBridge" in employee_autonomy
    assert 'GetNodeOrNull<V2CoreBridge>("../../V2CoreBridge")' in employee_autonomy
    assert "_v2CoreBridge.PlanEmployeeIntents(" in employee_autonomy
    assert "FindFacilityUseTarget(employee, coreIntent.FacilityId" in employee_autonomy
    assert "GetDesiredFacilityTypes" not in employee_autonomy


def test_get_the_best_v2_0_25_godot_uses_core_lifecycle_for_facility_use() -> None:
    bridge = read_text(GODOT_ROOT / "scripts" / "V2CoreBridge.cs")
    employee_autonomy = read_text(GODOT_ROOT / "scripts" / "EmployeeAutonomyController.cs")

    assert "EmployeeLifecycleEngine" in bridge
    assert "_lifecycleEngine" in bridge
    assert "_employeeLifecycleStates" in bridge
    assert "_facilityOccupants" in bridge
    assert "AdvanceEmployeeLifecycle(" in bridge
    assert "ApplyLifecycleState(" in bridge
    assert "StoreLifecycleState(" in bridge
    assert "CoreEmployeeLifecycleState" in bridge
    assert "RemainingActivityTicks" in bridge

    assert "UseFacilityDurationSeconds" not in employee_autonomy
    assert "_facilityUseTimers" not in employee_autonomy
    assert "CoreLifecycleTickSeconds" in employee_autonomy
    assert "UpdateCoreLifecycle(" in employee_autonomy
    assert "AdvanceEmployeeLifecycle(" in employee_autonomy
    assert "ApplyCoreLifecycleStates(" in employee_autonomy
    assert "StartupSim.Core.EmployeeActivityKind.UseFacility" in employee_autonomy
    assert "StartupSim.Core.EmployeeActivityKind.Idle" in employee_autonomy
