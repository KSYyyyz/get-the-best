# 《壮志凌云 / Get The Best》经营算法执行索引

状态：Algorithm A0.23 独立基线
日期：2026-05-22
适用范围：C# Core 经营规则、算法契约、算法测试、算法验收文档

## 版本规则

算法线使用 `Algorithm A主版本.次版本.补丁版本`，简称 `A0.23.x`。

算法版本只描述 C# Core 规则和契约，不占用 Godot 表现层 `V2-0.x` 版本号。

## 当前基线

- `docs/get_the_best_algorithm_a0_23_baseline.md`

## 本轮验收文档

- `docs/get_the_best_algorithm_a0_23_1_state_reducer_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_23_2_bridge_contract_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_23_3_lifecycle_acceptance_report.md`
- `docs/get_the_best_algorithm_a0_23_4_rest_recovery_acceptance_report.md`

## 边界

- C# Core 是经营规则唯一来源。
- Godot 只负责 DTO 转换、表现层播放和交互。
- 算法测试必须独立运行，不依赖 Godot API。
- 每个算法版本必须有确定性测试和中文验收记录。
