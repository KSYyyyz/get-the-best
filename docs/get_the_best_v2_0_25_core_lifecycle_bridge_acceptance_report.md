# 《壮志凌云 / Get The Best》V2-0.25 Core 员工生命周期桥接验收报告

## 本轮目标

在 V2-0.24 已经让 Godot 消费 Core 员工意图之后，本轮继续把员工“到设施后使用、设施占用、使用结束释放”的生命周期交回 C# Core。

Godot 仍然只负责表现层：

- 组装办公室事实快照。
- 把玩家可见的员工、房间、设施数据映射给 Core。
- 根据 Core 返回的员工生命周期状态播放路径移动、显示活动状态、切换设施使用表现。
- 不在 Godot 里维护经营规则、岗位偏好、使用时长或设施占用公式。

## 已完成

1. `V2CoreBridge` 增加 Core 生命周期桥接
   - 新增 `EmployeeLifecycleEngine` 实例。
   - 新增 `AdvanceEmployeeLifecycle(...)`，输入 Godot 员工/设施/房间 store 和 Core 意图，输出 Godot 可消费的 `CoreEmployeeLifecycleState`。
   - 新增生命周期状态缓存，把 Core 的 `CurrentActivity`、`ActiveFacilityId`、`RemainingActivityTicks` 保留在后续快照中。
   - 设施 DTO 的 `OccupiedByEmployeeIds` 由 Core 生命周期占用状态回填。

2. `EmployeeAutonomyController` 移除 Godot 自管设施使用计时
   - 删除 `UseFacilityDurationSeconds`。
   - 删除 `_facilityUseTimers`。
   - 新增 `CoreLifecycleTickSeconds`，仅作为前端向 Core 推进生命周期 tick 的表现层节奏。
   - 员工到达设施后调用 Core 生命周期推进，而不是在 Godot 里直接判定使用开始/结束。
   - 设施“使用中”描边/状态只跟 Core `UseFacility` 状态绑定。

3. 保持 Godot/Core 职责边界
   - Godot 继续负责找站位格、播放路径、显示活动标签。
   - Core 负责员工活动状态、设施占用和剩余活动 tick。
   - 后续接入算法分支时，Godot 只需要继续消费 Core 快照，不需要复制经营算法。

## 自动化验证

本轮新增和更新了 `tests/test_godot_v2_scaffold.py` 中的静态回归测试，覆盖：

- Godot 侧存在 `EmployeeLifecycleEngine` 桥接。
- `V2CoreBridge` 公开 `AdvanceEmployeeLifecycle(...)`。
- 生命周期状态包含 `RemainingActivityTicks`。
- `EmployeeAutonomyController` 不再包含 `UseFacilityDurationSeconds` 和 `_facilityUseTimers`。
- Godot 自主行动改为调用 Core 生命周期，并消费 Core `UseFacility` / `Idle` 状态。

已执行的验证命令：

- `pytest tests/test_godot_v2_scaffold.py -q`
- `dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug`
- `dotnet build godot\GetTheBestGodot\GetTheBestGodot.csproj --configuration Debug`
- `D:\Godot\godot.cmd --headless --path "D:\Get The Best\.worktrees\godot-v2-0-25-lifecycle-bridge\godot\GetTheBestGodot" --import`

完整门禁会在提交前再次执行并以最新结果为准。

## 实机验收要点

使用 Godot MCP 运行 `res://scenes/main.tscn` 后已确认：

- `/root/Main/V2CoreBridge` 运行时节点存在。
- 员工仍会根据 Core 意图自主前往设施。
- `Employee_1` 可到达 `Facility_1` 相邻交互格：员工位置观测到 `x=-65,z=-35`，设施位置为 `x=-65,z=-45`。
- 生命周期释放后，`Employee_1` 回到非使用状态，运行时位置再次观测为 `x=-65,z=-55`，活动标签移除。
- Godot 错误面板保持 `0 error`。
- 验证截图暂存到 `godot/GetTheBestGodot/addons/godot_mcp/cache/v2_0_25_lifecycle_bridge.png`，提交前清理。

## 分支协作说明

本轮前端工作放在独立 worktree：

- 路径：`D:\Get The Best\.worktrees\godot-v2-0-25-lifecycle-bridge`
- 分支：`codex/godot-v2-0-25-lifecycle-bridge`

原始工作目录中存在算法分支的未提交改动，本轮不触碰、不回滚。后续建议继续保持：

- `main`：稳定主线。
- `codex/algorithm-*`：规则算法和 Core 独立演进。
- `codex/godot-*`：Godot 前端和 Core 接入演进。
- `.worktrees/*`：并行工作树目录，仅本地使用，不提交。

## 后续建议

下一轮可以在确认算法分支稳定后，挑一个最小经营结果接入点，例如 Core `BusinessTickEngine.Tick` 输出的公司/员工 delta，只在 Godot HUD 或员工状态牌上做只读展示，继续避免把经营公式写入 Godot。
