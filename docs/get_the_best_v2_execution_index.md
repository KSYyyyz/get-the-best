# 《壮志凌云 / Get The Best》V2 执行文档索引

状态：V2 立项执行基线
日期：2026-05-20
适用范围：新 Godot 桌面前端、C# Core 规则核心、既有美术资源复用、新仓库准备

## 1. 立项结论

《壮志凌云 / Get The Best》不是在现有 Godot 面板 UI 上继续缝补，而是启动一个干净的 V2 前端路线。

本轮结论：

- 保留现有美术资源、项目经验、C# Core、Python 规则参考、测试资产和 MCP 试玩经验。
- 不在旧 `godot/StartupSimGodot/scenes/main.tscn` 上继续堆 UI。
- 不复制旧版面板点击式交互。
- 新 Godot 前端必须从第一天开始围绕办公室经营空间、房间、设施、员工和公司成长线设计。
- 新项目前期必须先完成参考游戏研究、插件策略和架构隔离方案，再进入实现。

## 2. 核心命名

- 中文名：《壮志凌云》
- 英文名：Get The Best
- 建议 GitHub 仓库名：`get-the-best`
- 项目定位：创业公司经营模拟游戏
- 目标形态：Godot 桌面端独立游戏

## 3. 不变边界

以下边界继续继承自 Startup Sim 项目：

- 当前只做 Godot 桌面前端，不再维护 Vercel/Web 前端。
- 所有项目文档默认用中文。
- 文档正文、执行计划、README、CI 说明和交接记录都必须使用中文；英文名 Get The Best、仓库名、代码标识符、命令、文件路径和 URL 可保留原文。
- AI 玩法暂时不做。
- C# Core 是规则核心，Godot 只负责表现层和交互。
- Godot UI 不复制经营规则，不绕过 C# Core 改写现金、收入、用户、融资、竞争、客户和结局。
- 玩家可见文案使用“现金流可支撑时间”，不使用“跑道”或 Runway。
- Python CLI/飞书代码暂时作为规则参考和回归测试来源保留，不能在 C# Core 完整覆盖前删除。
- 美术资产新增时必须保留索引、prompt、源图、导出图和切片指南。

## 4. V2 文档组

本索引对应三份执行文档：

1. `docs/get_the_best_v2_reference_game_study.md`
   - 深入拆解《疯狂游戏大亨2》《游戏发展国》《Game Dev Tycoon》《Software Inc.》等同类经营游戏。
   - 明确要学习什么、不能照搬什么。

2. `docs/get_the_best_v2_engine_plugin_strategy.md`
   - 明确 C#、GDScript、Godot 内置系统和插件的使用边界。
   - 提前列出未来节点可评估的插件，而不是一次性装满插件。

3. `docs/get_the_best_v2_reset_architecture.md`
   - 明确 V2 前端与旧 Godot 原型的隔离方式。
   - 明确如何复用 C# Core、美术资源、数据 schema 和测试经验。

阶段验收报告：

- `docs/get_the_best_v2_0_1_acceptance_report.md`
- `docs/get_the_best_v2_0_4_acceptance_report.md`
- `docs/get_the_best_v2_0_5_acceptance_report.md`
- `docs/get_the_best_v2_0_6_acceptance_report.md`
- `docs/get_the_best_v2_0_7_acceptance_report.md`
- `docs/get_the_best_v2_0_8_acceptance_report.md`
- `docs/get_the_best_v2_0_9_scale_camera_acceptance_report.md`
- `docs/get_the_best_v2_0_15_object_outline_and_preset_office_acceptance_report.md`
- `docs/get_the_best_v2_0_16_instance_outline_pickup_drag_acceptance_report.md`
- `docs/get_the_best_v2_0_17_kenney_art_resource_acceptance_report.md`
- `docs/get_the_best_v2_0_18_smooth_drag_precise_hit_wall_door_acceptance_report.md`
- `docs/get_the_best_v2_0_19_office_navigation_acceptance_report.md`
- `docs/get_the_best_v2_0_20_employee_autonomy_acceptance_report.md`
- `docs/get_the_best_v2_0_21_employee_facility_use_acceptance_report.md`
- `docs/get_the_best_v2_0_22_core_employee_behavior_tick_acceptance_report.md`
- `docs/get_the_best_v2_0_23_employee_activity_badge_acceptance_report.md`
- `docs/get_the_best_v2_0_24_core_intent_bridge_acceptance_report.md`

## 5. 第一原则

V2 的第一原则：

> 办公室空间是主棋盘，公司成长是主目标，C# Core 是唯一规则核心。

这意味着：

- 房间不是背景图，而是公司能力入口。
- 设施不是装饰，而是能力放大器和成本来源。
- 员工不是图标，而是产能、风险、成长和组织压力的载体。
- 月报不是日志，而是玩家理解成败因果的经营反馈。
- 失败不是惩罚弹窗，而是一段可复盘的创业故事。

## 6. 阶段路线

### V2-0：干净工程骨架

目标：

- 新建隔离 Godot 前端工程。
- 建立办公室主视角、镜头、点击、选中、HUD 基础。
- 验证 C# Core 引用和最小规则调用。

不做：

- 不移植旧面板 UI。
- 不加入完整商业化规则。
- 不加入复杂员工 AI。

### V2-1：20-30 分钟第一局闭环

目标：

- 研发区 -> MVP -> 销售区 -> 首批用户/收入 -> 月报 -> 阶段胜利或失败复盘。
- 玩家能在第一局理解房间、设施、员工和经营指标的因果关系。

### V2-2：房间系统成型

目标：

- 研发、销售、客服、服务器、休息、厕所等基础房间拥有清晰功能。
- 每个房间都能回答：服务什么公司能力、需要什么设施、适合什么员工、带来什么成本和风险。

### V2-3：员工系统成型

目标：

- 岗位、技能、疲劳、情绪、需求、成长、离职风险和岗位适配形成可读闭环。
- 员工行为要服务公司经营，不做独立养成小游戏。

### V2-4：商业化链路成型

目标：

- 产品能力、用户增长、MRR、客户反馈、服务器稳定和销售能力形成可见因果。
- 玩家能理解“为什么这个月增长/停滞/亏损”。

### V2-5：外部压力系统

目标：

- 竞争公司、董事会、客户、融资和市场趋势进入主循环。
- 外部压力必须基于 C# Core 事实快照，不凭空生成结论。

### V2-6：中后期扩张

目标：

- 办公室升级、多部门、多产品线、组织效率和规模风险。
- 扩张必须带来能力、成本和管理难题，而不是单纯扩大地图。

### V2-7：接近全量经营系统

目标：

- 接近“创业版疯狂游戏大亨2”的长期经营深度。
- 包含市场路线、竞争格局、融资阶段、重大客户、并购或 IPO 等多结局。

## 7. 反对式执行规则

为了避免再次变成面板点击原型，后续每轮设计和实现都必须先回答：

- 这个功能是否让办公室空间更像主场景？
- 这个功能是否让公司成长线更清晰？
- 这个功能是否能被玩家从画面里理解，而不是只靠文字日志？
- 这个功能是否绕过了 C# Core？
- 这个功能是否把旧版面板 UI 惯性带回来了？

任意答案不清楚时，不进入实现。
