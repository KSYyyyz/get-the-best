# Algorithm A0.27.2 设施选择与预留验收报告

日期：2026-05-22

## 验收目标

员工选择设施时必须考虑房间、容量、占用、预留和逻辑交互站位，不能让两名员工抢同一个设施。

## 已验收规则

- 满容量设施不会继续分配。
- 本 tick 前序员工已经预留的设施，后序员工会收到拒绝原因。
- 没有可用办公桌时，工程师不会凭空产生 MVP 进度。
- 设施必须登记在房间设施列表中，作为 A0.27 的逻辑交互站位检查。

## 测试证据

- `EmployeeUtilityBehaviorTests.UnusableDeskDoesNotProduceMvpProgress`
- `EmployeeUtilityBehaviorTests.OccupiedFacilityIsNotAssignedToSecondEmployee`
- `EmployeeBehaviorTickTests.FullFacilityCapacityIsNotAssignedAgain`

## 结论

A0.27.2 通过。Core 已能在行为规划阶段排除不可用设施，并在解释字段中说明拒绝原因。
