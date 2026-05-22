# Algorithm A0.25.4 确定性验收报告

## 验收目标

验证统一编排器在同一输入下输出确定结果，不依赖随机数或 Godot 节点状态。

## 实现内容

- `OfficeSimulationEngine` 不引用 Godot API。
- 员工、设施、事件输出都按稳定 ID 排序。
- 阶段结果只由输入 snapshot 和 Core tick delta 决定。

## 测试证据

测试用例：`SimulationEngineTests.SimulationAdvanceIsDeterministic`

覆盖断言：
- 同一输入的项目进度 delta 一致。
- 同一输入的下一帧项目进度一致。
- 同一输入的阶段结果一致。
- 同一输入的播放事件数量一致。

完整 Core API 边界仍由既有测试 `EmployeeBehaviorTickTests.CoreAssemblyDoesNotReferenceGodot` 覆盖。
