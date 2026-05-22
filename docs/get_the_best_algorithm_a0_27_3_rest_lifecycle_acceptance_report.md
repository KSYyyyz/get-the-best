# Algorithm A0.27.3 休息短计划验收报告

日期：2026-05-22

## 验收目标

高疲劳员工不仅要“计划休息”，还要通过统一 `Advance` 进入可被前端消费的休息状态。

## 已验收规则

- 高疲劳员工会选择 `Rest` 候选。
- `Rest` 意图会占用休息设施。
- `NextSnapshot.Employees` 会显示 `EmployeeActivityKind.Rest`。
- `NextSnapshot.Facilities` 会显示休息设施占用。
- 休息仍属于 Core 生命周期，不需要 Godot 复制疲劳或设施规则。

## 测试证据

- `EmployeeUtilityBehaviorTests.RestIntentStartsRestFacilityUse`
- `RestRecoveryTests.HighFatigueEmployeeTargetsRestFacility`
- `RestRecoveryTests.RestTickRecoversFatigueAndEnergy`
- `RestRecoveryTests.RestFacilityAndRoomImproveRecovery`
- `RestRecoveryTests.RestTickReportsRestFacilityUse`

## 结论

A0.27.3 通过。员工休息已经从意图延伸到 Core 快照状态，后续 Godot 可以直接播放休息表现。
