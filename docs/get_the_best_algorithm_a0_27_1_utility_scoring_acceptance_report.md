# Algorithm A0.27.1 Utility 评分验收报告

日期：2026-05-22

## 验收目标

员工自主行为不再只按角色硬选设施，而是输出可解释的行动候选分数。

## 覆盖范围

- `WorkAtDesk`
- `UseWhiteboard`
- `MaintainServer`
- `Rest`
- `Idle`

## 已验收规则

- 工程师在 MVP 未完成且办公桌可用时，`WorkAtDesk` 分数最高。
- 每个候选都输出分数、原因、拒绝原因和目标引用。
- 高疲劳员工的 `Rest` 分数会超过工作类候选。
- 评分过程不使用随机数，排序规则固定。

## 测试证据

- `EmployeeUtilityBehaviorTests.EngineerPrioritizesDeskWorkForMvpWhenDeskIsAvailable`
- `EmployeeUtilityBehaviorTests.HighFatigueRaisesRestCandidateScore`
- `EmployeeBehaviorTickTests.TickIsDeterministic`

## 结论

A0.27.1 通过。Core 已具备可解释、确定性的员工行动候选评分骨架。
