# 《壮志凌云 / Get The Best》经营算法执行索引

状态：Algorithm A0.26 Godot 前端消费契约冻结
日期：2026-05-22
适用范围：C# Core 经营规则、算法契约、算法测试、算法验收文档

## 版本规则

算法线使用 `Algorithm A主版本.次版本.补丁版本`，例如 `A0.24.1`。
算法版本只描述 C# Core 规则和契约，不占用 Godot 表现层 `V2-0.x` 版本号。

## 当前基线

- `docs/get_the_best_algorithm_a0_26_baseline.md`

## 历史基线

- `docs/get_the_best_algorithm_a0_25_baseline.md`
- `docs/get_the_best_algorithm_a0_24_baseline.md`
- `docs/get_the_best_algorithm_a0_23_baseline.md`

## A0.26 本轮验收文档

- `docs/get_the_best_algorithm_a0_26_1_result_field_contract_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_26_2_presentation_event_contract_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_26_3_tick_cadence_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_26_4_v2_0_26_minimum_bridge_acceptance_report.md`

## A0.25 验收文档

- `docs/get_the_best_algorithm_a0_25_1_tick_orchestration_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_25_2_frontend_contract_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_25_3_phase_outcome_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_25_4_determinism_acceptance_report.md`

## A0.24 验收文档

- `docs/get_the_best_algorithm_a0_24_1_mvp_stage_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_24_2_sales_first_users_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_24_3_revenue_cashflow_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_24_4_monthly_report_acceptance_report.md`

## A0.23 验收文档

- `docs/get_the_best_algorithm_a0_23_1_state_reducer_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_23_2_bridge_contract_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_23_3_lifecycle_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_23_4_rest_recovery_acceptance_report.md`

## 边界

- C# Core 是经营规则唯一来源。
- Godot 只负责 DTO 转换、表现层播放和交互。
- 算法测试必须独立运行，不依赖 Godot API。
- 每个算法版本必须有确定性测试和中文验收记录。
- 算法工作默认在 `codex/algorithm-*` 分支推进，再通过 PR 或人工确认合入主线。
