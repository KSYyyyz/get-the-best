# 《壮志凌云 / Get The Best》V2 重启架构执行文档

状态：V2 架构隔离基线
日期：2026-05-20
范围：新 Godot 前端工程、旧原型隔离、C# Core 复用、美术资源复用、验证流程

## 1. 核心判断

现有 Godot 版本已经明显形成“面板点击游戏”的惯性。

V2 不能在旧 `main.tscn` 上继续改，否则旧 HUD、旧控制器、旧事件提示、旧交互状态机会持续污染新方向。

因此，V2 的推荐架构是：

> 新建隔离 Godot 前端工程，保留旧工程作为原型参照和回归经验，不在旧主场景上继续堆功能。

## 2. 推荐目录结构

建议在当前仓库内先形成隔离结构：

```text
godot/
  StartupSimGodot/          # 旧 G1/G2 原型，冻结为参照
  GetTheBestGodot/          # V2 新 Godot 前端工程

csharp/
  StartupSim.Core/          # 规则核心，继续复用
  StartupSim.Core.Tests/    # 规则测试，继续复用

docs/
  get_the_best_v2_*.md      # V2 研究、策略和执行文档

data/
  golden_cases/             # Python 与 C# 对齐用例
  balance_cases/            # 平衡测试用例
```

如果后续新 GitHub 仓库 `get-the-best` 独立初始化，则以上结构可以作为迁移模板。

## 3. 旧工程处理方式

旧工程 `godot/StartupSimGodot/`：

- 保留。
- 不删除。
- 不继续作为主要开发目标。
- 不再往旧 `main.tscn` 里叠加新功能。
- 作为旧经验、资产绑定、MCP 试玩问题和测试样例来源。

允许从旧工程借鉴：

- 美术 atlas 引用方式。
- C# Core 调用经验。
- MCP 验证脚本经验。
- 已发现的 UI 失败案例。
- 已形成的测试保护思路。

禁止直接继承：

- 旧主场景结构。
- 旧全屏 G2OperationsPanel 思路。
- 旧底部文字按钮 dock。
- 旧事件日志式反馈。
- 旧建造/菜单状态交织方式。
- 旧右侧大看板主导体验。

## 4. V2 工程边界

新工程 `godot/GetTheBestGodot/` 必须从第一天遵守以下边界：

- 主画面是办公室空间，不是管理面板。
- HUD 是辅助层，不是主界面。
- 底部操作区是经营工具入口，不是主要游戏内容。
- 选中房间、设施或员工后出现上下文操作，不默认展示大看板。
- 月报是结构化经营反馈，不是日志窗口。
- 所有经营状态来自 C# Core 或其事实快照。

## 5. V2 场景分层

推荐场景结构：

```text
Main.tscn
  GameRoot
    OfficeWorld
      OfficeCamera
      OfficeFloor
      RoomLayer
      FacilityLayer
      EmployeeLayer
      FeedbackFxLayer
    InteractionRoot
      SelectionController
      BuildModeController
      PlacementPreviewController
      CommandRouter
    HudRoot
      TopStatusBar
      BottomToolDock
      ObjectivePanel
      ContextPanel
      ToastFeed
      MonthlyReportModal
```

每层职责：

- `OfficeWorld`：空间、镜头、对象、视觉反馈。
- `InteractionRoot`：输入、选中、建造模式、命令分发。
- `HudRoot`：状态展示、工具入口、对象上下文、月报。
- `CommandRouter`：把玩家意图转为结构化命令，不直接改经营状态。

## 6. 核心组件建议

### 6.1 OfficeWorld

职责：

- 展示办公室空间。
- 管理房间、设施、员工和状态图标的表现层。
- 提供点击命中结果。

不负责：

- 经营结算。
- 现金、收入、用户、融资等核心状态。

### 6.2 SelectionController

职责：

- 管理当前选中对象。
- 支持空地、房间、设施、员工、办公室整体等不同上下文。
- 通知 ContextPanel 切换信息。

要求：

- 点击任何对象都必须给清晰反馈。
- 空白点击必须清除或提示当前状态。

### 6.3 BuildModeController

职责：

- 管理建房间、摆设施、取消、预览。
- 切换一级菜单时自动取消旧模式。
- 支持 Esc/右键取消。

要求：

- 建造模式必须有绿色/红色预览。
- 失败原因必须提前展示，而不是点击后才报错。

### 6.4 CommandRouter

职责：

- 将玩家操作转为结构化经营意图。
- 调用 C# Core 或桥接层。
- 接收结果快照并通知表现层刷新。

禁止：

- 直接在 UI 控制器里计算经营规则。
- 直接修改核心状态绕过 C# Core。

### 6.5 MonthlyReportModal

职责：

- 展示收入、成本、产品、用户、现金流、风险和建议。
- 解释本月结果来源。
- 进入失败或阶段胜利时提供复盘入口。

要求：

- 支持滚动。
- 不遮挡无法关闭。
- 打开时暂停或冻结月结推进。

## 7. 美术资源复用

现有美术资源可以复用，但必须先整理为公共资产源。

建议保留：

- 现有办公室 tile。
- 房间 overlay。
- 员工朝向和动作 atlas。
- 设施 atlas。
- UI core atlas。
- feedback FX。
- 招聘头像。

建议新增公共资源清单：

```text
godot/shared_assets/
  art/
  audio/
  ui/
  third_party_placeholder_assets/
  asset-index.json
```

所有新资源必须记录：

- 索引 ID。
- prompt。
- 源图。
- 导出图。
- 切片指南。
- Godot 导入路径。
- 当前使用场景。

## 8. C# Core 复用方式

V2 必须继续引用 C# Core。

推荐方式：

- 新 Godot 工程通过 project reference 引用 `csharp/StartupSim.Core/StartupSim.Core.csproj`。
- 建立 V2 专用桥接层，例如 `GetTheBestTurnBridge`。
- 桥接层只做 DTO 转换、命令提交和快照接收。

V2 不允许：

- 复制 `DeterministicTurnEngine` 规则。
- 在 Godot UI 里硬编码商业化结算。
- 在表现层私自维护一套公司状态。

## 9. 数据层复用方式

现有 Godot 数据和 Python 参考规则不能直接丢弃。

复用策略：

- 已有房间、设施、员工定义可作为 V2 数据草案。
- V2 需要重新审查字段命名和玩家可见说明。
- Python 参考继续作为规则完整性对照。
- C# Core 未覆盖前，不删除 Python 规则参考。

## 10. 验证流程

V2 每轮有效修改必须验证：

1. Godot MCP 真实运行 V2 主场景。
2. 截图检查办公室主体是否可读。
3. 点击测试检查 HUD 不遮挡办公室。
4. 建造/取消/预览/选中至少走一遍。
5. `get_errors` 确认为 0 error。
6. C# Core 测试通过。
7. Godot C# build 通过。
8. 内容数据校验通过。
9. 相关 pytest 通过。
10. commit、push、检查 GitHub Actions。

## 11. V2-0 验收标准

V2-0 只验收干净骨架，不追求完整玩法。

验收：

- 新 Godot 工程可打开。
- 主场景可运行。
- 办公室空间可见。
- 镜头可平移和缩放。
- 可点击办公室对象。
- 点击对象显示上下文面板。
- HUD 不遮挡办公室主体。
- 可通过桥接层读取一个 C# Core 初始状态快照。
- MCP 截图可证明主场景不是面板点击 UI。

## 12. V2-1 验收标准

V2-1 验收第一局闭环。

验收：

- 玩家能在 20-30 分钟内完成研发 -> MVP -> 销售 -> 首批收入 -> 月报。
- 所有操作发生在办公室空间和对象上下文中。
- 月报解释产品、收入、成本、用户和现金流变化。
- 失败或阶段胜利有复盘。
- 玩家可见文案使用“现金流可支撑时间”。
- Godot 不复制经营规则。

## 13. 迁移到新 GitHub 仓库的建议

新 GitHub 仓库 `get-the-best` 建议先作为空仓库或文档仓库创建。

迁移顺序：

1. 先在 Startup Sim 仓库内完成 V2 文档和工程隔离设计。
2. 新仓库创建后，只迁移明确属于《壮志凌云 / Get The Best》的内容。
3. 迁移 C# Core 前先确认命名空间、包名和历史测试策略。
4. 迁移美术资源前先整理 asset index 和许可证。
5. 不把旧 Godot 原型整包迁移到新仓库作为主工程。

## 14. 反污染检查

每次 V2 实现前必须检查：

- 是否引用了旧 `main.tscn`？
- 是否复用了旧 G2OperationsPanel？
- 是否让 HUD 成为主界面？
- 是否把事件 feed 写成开发日志？
- 是否在 Godot UI 里复制了经营规则？
- 是否让办公室变成不可操作背景？

如果任一项为“是”，该实现不得合入。
