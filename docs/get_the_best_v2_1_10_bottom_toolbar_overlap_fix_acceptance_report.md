# 《壮志凌云 / Get The Best》V2-1.10 底部工具栏重叠修复验收报告

日期：2026-05-23

## 问题

V2-1.9 将业务信息区压缩为固定 `360px`，但该面板内部“公司标志、公司名、现金、用户”的实际最小宽度更大。结果是业务信息区视觉上撑开，而建造菜单仍从旧起点绘制，导致底部菜单和业务信息叠在一起。

## 修复

- 业务信息区宽度调整为 `560px`，容纳现金和用户信息。
- 建造菜单区从业务区真实右侧之后开始：`BusinessPanelWidth + BottomHudGap * 2.0f`。
- 建造菜单区宽度改为按右侧时间条剩余空间动态计算，避免再次挤到时间条。
- `MainController` 与 `BuildModeHudController` 使用一致的底部 HUD 常量和宽度计算。

## 验收

- 新增 `test_get_the_best_v2_1_10_bottom_toolbar_sections_do_not_overlap`。
- Godot MCP 实机验证：
  - `BusinessFeedbackPanel`: `x=8, width=560`
  - `BuildModePanel`: `x=576, width=473`
  - `TimeScalePanel`: `x=1057, width=316`
  - 三段之间保留间距，无重叠。
  - 建造菜单浮层可正常打开。
  - `get_errors` 返回 0 error。
