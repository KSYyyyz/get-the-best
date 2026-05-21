# 《壮志凌云》V2-0.17 Kenney 美术资源替换验收报告

## 本轮目标

根据项目文档中的免费美术资源策略，从 Kenney 的 CC0 资源中选取可商用占位美术，替换当前办公室沙盒里过于程序化的员工、地砖和墙面表现。

## 资源来源

- 员工模型：Kenney `Blocky Characters`
- 地砖与墙面：Kenney `Prototype Textures`
- 授权：CC0-1.0，可商用，无需署名

所有新增第三方占位资源已经登记到：

- `godot/GetTheBestGodot/assets/third_party_placeholder_assets/asset-index.json`

## 已完成

1. 员工实例改用 Kenney 角色模型。
   - 新增 `character-a.glb`、`character-b.glb`、`character-c.glb`。
   - 新增对应贴图 `texture-a.png`、`texture-b.png`、`texture-c.png`。
   - `Employee3DRenderer` 不再用 `CylinderMesh` / `SphereMesh` 生成员工。

2. 地砖改用 Kenney 纹理。
   - `OfficeGrid3DRenderer` 的格子材质加载 `floor_light_texture_02.png`。
   - `OfficeFloor3DRenderer` 的底层地面也使用同一套地砖纹理，避免底图和格子脱节。

3. 墙面改用 Kenney 纹理。
   - `RoomOverlay3DRenderer` 的房间墙体加载 `wall_dark_texture_03.png`。
   - `OfficeBoundary3DRenderer` 的办公室外边界墙体加载同一墙面纹理。

4. 员工描边适配真实模型。
   - 选中、悬停、拿起移动时保留模型材质，只在 `NextPass` 添加外扩描边。
   - 修复刷新员工渲染时节点名被 Godot 改成 `@Node3D@...` 的问题，运行时仍能稳定查询 `Employee_1`、`Employee_2`、`Employee_3`。

## 实机验证

使用 Godot MCP 运行 `res://scenes/main.tscn` 验证：

- 默认办公室预设场景能正常显示 Kenney 地砖、墙体与员工模型。
- 员工点击拿起后仍保持模型实例显示，提示文本为“点击放下”。
- 运行时 `Employee3DRenderer` 下稳定存在 `Employee_1`、`Employee_2`、`Employee_3`。
- Godot 错误面板：0 error。

## 自动化验证

本轮新增和更新的测试覆盖：

- Kenney 资源必须登记在 `asset-index.json`。
- 所有新增资源必须是 Kenney / CC0-1.0 / 可商用 / 无需署名。
- 员工渲染器必须加载 GLB 模型和对应贴图。
- 地砖、底层地面、房间墙体和外边界墙体必须加载 Kenney 纹理。
- 员工渲染器不得回退到程序生成圆柱 / 球体模型。

验证命令见本轮提交记录。
