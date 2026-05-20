# Get The Best / 壮志凌云

《壮志凌云 / Get The Best》是一款 Godot 桌面端创业公司经营模拟游戏。

当前状态：V2 立项与工程隔离阶段。

## 核心方向

- 办公室空间是主棋盘。
- 公司成长是主目标。
- 房间、设施、员工和经营反馈服务创业主线。
- C# Core 是规则核心，Godot 只负责表现层和交互。
- 玩家可见文案使用“现金流可支撑时间”，不使用“跑道”或 Runway。
- 当前不做 Web/Vercel 前端。
- 当前不做 AI 玩法。

## V2 立项原因

旧 Godot 原型已经明显形成面板点击游戏惯性。V2 不在旧主场景上继续缝补，而是建立干净的 Godot 前端工程，并保留既有美术资源、C# Core、测试经验和 MCP 试玩经验。

## 文档入口

- `docs/get_the_best_v2_execution_index.md`
- `docs/get_the_best_v2_reference_game_study.md`
- `docs/get_the_best_v2_engine_plugin_strategy.md`
- `docs/get_the_best_v2_reset_architecture.md`

## 当前不做

- 不迁移旧 `StartupSimGodot/main.tscn` 作为 V2 主场景。
- 不复制旧 G2OperationsPanel。
- 不让 HUD 和日志成为主游戏体验。
- 不绕过 C# Core 实现经营规则。

## 文档语言

项目文档、执行计划、交接记录和 CI 说明默认使用中文。英文名 Get The Best、仓库名、代码标识符、命令、文件路径和 URL 可以保留原文。
