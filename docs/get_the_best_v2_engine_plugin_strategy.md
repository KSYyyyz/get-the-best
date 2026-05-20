# 《壮志凌云 / Get The Best》V2 引擎、语言与插件策略

状态：V2 技术决策基线
日期：2026-05-20
范围：Godot、C#、GDScript、插件、公开素材库和未来技术节点

## 1. 总原则

V2 不采用“先装满插件再开发”的路线。

正确路线：

- 先明确每个未来开发节点需要解决什么问题。
- 优先使用 Godot 内置能力和项目自有代码。
- 插件只在能显著节省时间、降低风险、且不会控制核心架构时引入。
- 所有插件必须记录版本、来源、许可证、使用范围、替换方案和弃用条件。

## 2. 语言策略

### 2.1 C# Core 保留

C# Core 是规则核心，继续保留。

原因：

- 当前项目已经有 C# Core、测试和 Godot C# 桥接经验。
- 规则核心需要可测试、可复用、可与 Python 参考实现对齐。
- 桌面端 Godot .NET 项目适合继续使用 C#。

边界：

- C# Core 负责规则、结算、状态和事实快照。
- Godot 不复制经营规则。
- 所有现金、用户、收入、融资、竞争、客户和结局判断都必须由 C# Core 或其明确授权的规则层处理。

### 2.2 Godot 表现层默认使用 C#

V2 Godot 表现层默认继续使用 C#。

原因：

- 与现有 C# Core 集成成本低。
- 团队已经有 Godot C# 工程和 CI 构建经验。
- 可以减少跨语言边界和重复 DTO。

风险：

- Godot 内置脚本编辑器对 C# 体验不如 GDScript。
- UI 快速迭代可能比 GDScript 稍慢。
- Godot C# 项目不适合 Web 导出，但本项目已明确不做 Web 前端。

### 2.3 GDScript 的允许边界

V2 不禁止 GDScript，但不能让它成为第二套规则层。

允许：

- 非规则性的编辑器辅助脚本。
- 临时调试工具。
- 小型视觉效果或动画控制，前提是与 C# Core 无关。

禁止：

- 在 GDScript 中实现经营规则。
- 在 GDScript 中维护现金、收入、用户、融资、竞争或结局状态。
- 用 GDScript 绕过 C# Core 改变公司事实。

## 3. Godot 内置能力优先级

### 3.1 UI 布局

优先使用 Godot Containers、anchors 和主题系统。

目标：

- 避免硬编码坐标堆叠。
- 避免旧版全屏面板遮挡办公室。
- 让 HUD、dock、对象面板和月报在多窗口尺寸下稳定。

### 3.2 镜头系统

V2-0 使用自研最小 Camera2D：

- WASD 或鼠标拖拽平移。
- 滚轮缩放。
- 限制视野边界。
- 选中对象时可平滑聚焦。

插件只在镜头需求复杂后评估。

### 3.3 小人移动和寻路

早期使用 Godot 内置寻路能力：

- 格子明确时优先评估 AStarGrid2D 或 AStar2D。
- 有导航区域后评估 NavigationAgent2D。
- 员工只需要能走向工位、休息区、厕所、会议区等目标。

早期不引入复杂行为树。

### 3.4 员工行为

V2-1 使用简单 FSM：

- Working
- Resting
- MovingToNeed
- Training
- Idle
- SickOrAbsent

行为树插件等员工行为复杂后再评估。

### 3.5 存档

优先自研稳定 JSON 存档或 Godot Resource 存档。

要求：

- 存档结构可版本化。
- 存档不依赖 Godot 场景节点路径。
- 存档内容以 C# Core 状态和 Godot 表现层必要信息为主。

### 3.6 本地化

早期先保持中文文档和中文玩家文案。

未来建立本地化表：

- UI 文案。
- 房间说明。
- 设施说明。
- 员工特质。
- 月报模板。
- 事件模板。

## 4. 插件候选地图

以下是未来节点的候选插件和评估条件。它们不是当前必须安装项。

| 节点 | 候选方案 | 当前决策 |
| --- | --- | --- |
| 镜头控制 | Phantom Camera | 暂不安装，先自研最小 Camera2D |
| 员工行为树 | LimboAI / Beehave | 暂不安装，先 FSM |
| 事件对话 | Dialogic | 暂不安装，先模板事件 |
| Aseprite 导入 | Aseprite Wizard / Importality | 等美术流程稳定后评估 |
| 地图编辑 | Tiled Importer | 暂不安装，Godot 内直接搭建办公室 |
| 物理增强 | Godot Jolt | 不需要，当前为 2D 办公室经营 |
| 音频管理 | 自研 AudioManager 或轻量插件 | 暂不安装 |
| 存档管理 | 自研 | 暂不安装 |

## 5. 插件引入门槛

引入插件前必须回答：

- 它解决的是否是当前阶段真实问题？
- 它是否支持当前 Godot 版本？
- 它是否支持 .NET/C# 项目工作流？
- 它是否有清晰许可证？
- 它是否会控制核心架构？
- 它是否能在 CI 或 headless 验证中稳定存在？
- 移除它的替代方案是什么？

任意问题无法回答时，不引入。

## 6. 公开素材库策略

公开素材库可以作为占位底座，但不能替代最终风格。

优先级：

1. Kenney
   - 优先选择 CC0 或明确商业可用资源。
   - 适合 UI icon、基础音效、占位 tiles、通用对象。

2. itch.io 免费/CC0 资源
   - 必须逐项检查许可证。
   - 不允许只因为在 itch.io 免费就视为可商用。

3. OpenGameArt
   - 许可证复杂，优先使用 CC0。
   - CC-BY 必须保留署名。
   - 避免 GPL、CC-BY-SA、NC 或不清晰许可证。

4. Freesound
   - 只优先使用 CC0。
   - CC-BY 需要记录署名。

## 7. 占位素材管理规则

所有第三方占位素材必须进入清单，至少记录：

- asset_id
- name
- source_url
- source_site
- author
- license
- commercial_use_allowed
- attribution_required
- current_usage
- replacement_target
- imported_path
- removal_plan

没有清单的第三方素材不得进入 Godot 工程。

## 8. 技术节点与插件复盘表

后续每到一个开发节点，必须先复盘是否需要插件：

| 开发节点 | 默认方案 | 触发插件评估的条件 |
| --- | --- | --- |
| 办公室镜头 | 自研 Camera2D | 需要复杂跟随、轨道、混合镜头或过场 |
| 员工走动 | AStarGrid2D / NavigationAgent2D | 员工移动在复杂房间中频繁卡住 |
| 员工决策 | 简单 FSM | 员工行为超过 8 个状态且互相抢占 |
| 事件叙事 | 模板事件 | 事件链、分支对话和角色关系显著增加 |
| 资产导入 | Godot 原生导入 | 美术源文件批量来自 Aseprite 或固定外部格式 |
| 月报/复盘 UI | 自研 UI | 报表交互复杂到需要通用图表库，但优先避免 |

## 9. 禁止事项

- 不为了“看起来专业”提前安装插件。
- 不把插件作为架构核心。
- 不引入无法在 CI 中稳定构建的插件。
- 不引入许可证不清晰的素材或插件。
- 不让美术占位素材决定最终视觉路线。
- 不让 GDScript 或插件脚本承载经营规则。

## 10. 资料入口

- Godot C# 文档：https://docs.godotengine.org/en/stable/getting_started/scripting/c_sharp/index.html
- Godot C# 差异说明：https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_differences.html
- Godot UI Containers：https://docs.godotengine.org/en/stable/tutorials/ui/gui_containers.html
- Godot 2D Navigation：https://docs.godotengine.org/en/4.0/tutorials/navigation/navigation_introduction_2d.html
- Phantom Camera：https://phantom-camera.dev/
- LimboAI：https://github.com/limbonaut/limboai
- Beehave：https://github.com/bitbrain/beehave
- Dialogic：https://github.com/dialogic-godot/dialogic
