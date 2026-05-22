# 《壮志凌云 / Get The Best》Algorithm A0.23.2 Godot 到 Core 桥接契约验收报告

## 目标

建立 Godot 事实快照到 Core `OfficeRuleSnapshot` 的稳定契约，保证 Godot 只做 DTO 转换，不承载经营规则。

## 规则内容

- 新增 `GodotOfficeSnapshotDto`。
- 新增员工、设施、房间、公司 DTO：
  - `GodotEmployeeFactDto`
  - `GodotFacilityFactDto`
  - `GodotRoomFactDto`
  - `GodotCompanyFactDto`
- 新增 `GodotCoreBridgeContract.BuildSnapshot(dto)`。
- Godot 侧 `V2CoreBridge` 通过 project reference 调用 Core，只做快照组装和 intent 转换。
- `EmployeeAutonomyController` 消费 `V2CoreBridge.PlanEmployeeIntents(...)`，不再在 Godot 内维护岗位到设施的经营规则映射。
- 映射规则：
  - Godot 员工整数 ID 转为 `employee-{id}`。
  - Godot 设施整数 ID 转为 `facility-{id}`。
  - 中文岗位标签和英文代码都可以映射到 `EmployeeRole`。
  - 设施类型、房间类型、员工活动使用 Core 枚举代码。
- 未知岗位、未知活动、未知设施类型、未知房间类型会抛出 `ArgumentException`，避免静默落入错误规则。

## 测试证据

新增 `BridgeContractTests`：

- `MapsGodotFactDtoToCoreSnapshot`：Godot DTO 能映射为 Core 快照。
- `RejectsUnknownEmployeeRole`：未知岗位会被拒绝。
- `BridgeContractDoesNotReferenceGodot`：桥接契约所在程序集不引用 Godot。
- Python 脚手架测试验证 Godot C# 工程引用 Core，并且自主行为控制器消费 Core intent。

局部验证命令：

```text
dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug
```

结果：`StartupSim.Core.Tests passed`。

## 边界

- 本轮只做 Godot 到 Core 的最小桥接消费，不新增 UI 或美术表现。
- Core 仍不引用 Godot API。
- 桥接层只定义 DTO 和映射规则，不计算产能、现金、疲劳或项目进度。
