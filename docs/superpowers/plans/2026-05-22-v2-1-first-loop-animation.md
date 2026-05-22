# V2-1.0 第一局目标链路与基础动画实施计划

> **给执行代理：** 必须使用 `superpowers:subagent-driven-development`（推荐）或 `superpowers:executing-plans` 逐项执行本计划。步骤使用复选框（`- [ ]`）跟踪。

**目标：** 建立 V2-1 第一局目标链路基线，同时引入可替换的员工动画状态层，覆盖行走、坐下办公和站立使用设施。

**架构：** 保持 C# Core 作为唯一经营状态来源。Godot 从 `CoreOfficeSimulationResult`、`CoreEmployeeIntent` 和员工生命周期事实派生轻量目标文案与动画状态，再通过现有 HUD 和员工渲染节点表现。动画先实现为前端表现状态层，后续 GLB/AnimationPlayer 资产可以替换程序化动作，而不改 Core 桥接逻辑。

**技术栈：** Godot 4.6 C# 脚本、`StartupSim.Core`、现有 pytest 脚手架检查、Godot MCP 实机验证。

---

## 文件结构

- 修改 `tests/test_godot_v2_scaffold.py`: add focused scaffold tests for V2-1.0 objective HUD fields, animation state definitions, Core intent mapping, and cleanup behavior.
- 修改 `godot/GetTheBestGodot/scripts/BusinessFeedbackHudController.cs`: add objective labels and formatting helpers derived from `CoreOfficeSimulationResult`.
- 修改 `godot/GetTheBestGodot/scenes/Main.tscn`: add objective/next-action labels under the existing `BusinessFeedbackPanel`; avoid adding a large new panel.
- 修改 `godot/GetTheBestGodot/scripts/Employee3DRenderer.cs`: add `EmployeePresentationAnimationState` and a single `SetEmployeeAnimationState` entrypoint; route current procedural walking/typing behavior through this state.
- 修改 `godot/GetTheBestGodot/scripts/EmployeeAutonomyController.cs`: map `EmployeeActionCandidateKind` to renderer animation states, set walking/sitting/standing states at the same moments activity labels change, and clear states when manual movement or Core lifecycle returns to idle.
- 新增 `docs/get_the_best_v2_1_0_first_loop_animation_acceptance_report.md`: Chinese acceptance report with scope, evidence, and remaining asset limitations.
- 修改 `docs/get_the_best_v2_execution_index.md`: list the V2-1.0 acceptance report.

---

### 任务 1: Add Failing Scaffold Tests For V2-1.0 Contract

**文件：**
- Modify: `tests/test_godot_v2_scaffold.py`
- 测试： `tests/test_godot_v2_scaffold.py`

- [ ] **步骤 1: Write the failing tests**

Append these tests after the V2-0.30 test:

```python
def test_get_the_best_v2_1_0_first_loop_objective_hud_uses_core_result() -> None:
    scene_text = read_text(GODOT_ROOT / "scenes" / "main.tscn")
    hud = read_text(GODOT_ROOT / "scripts" / "BusinessFeedbackHudController.cs")
    bridge = read_text(GODOT_ROOT / "scripts" / "V2CoreBridge.cs")

    assert 'name="ObjectiveValueLabel"' in scene_text
    assert 'name="NextObjectiveValueLabel"' in scene_text
    assert 'name="RecentCoreEventValueLabel"' in scene_text
    assert "_objectiveValueLabel" in hud
    assert "_nextObjectiveValueLabel" in hud
    assert "_recentCoreEventValueLabel" in hud
    assert "FormatFirstLoopObjective(result.CompanyTotals, result.OutcomeKind)" in hud
    assert "FormatNextObjective(result.CompanyTotals, result.OutcomeKind)" in hud
    assert "FormatRecentCoreEvent(result)" in hud
    assert "result.CompanyTotals.CurrentProjectProgress" in hud
    assert "result.CompanyTotals.ProductStage" in hud
    assert "PhaseOutcomeKind.FirstUsersAcquired" in hud
    assert "CoreOfficeSimulationResult" in bridge


def test_get_the_best_v2_1_0_employee_animation_states_follow_core_intents() -> None:
    employee_renderer = read_text(GODOT_ROOT / "scripts" / "Employee3DRenderer.cs")
    employee_autonomy = read_text(GODOT_ROOT / "scripts" / "EmployeeAutonomyController.cs")

    assert "public enum EmployeePresentationAnimationState" in employee_renderer
    assert "Idle" in employee_renderer
    assert "Walking" in employee_renderer
    assert "SittingDown" in employee_renderer
    assert "WorkingAtDesk" in employee_renderer
    assert "UsingStandingFacility" in employee_renderer
    assert "LeavingFacility" in employee_renderer
    assert "_employeeAnimationStates" in employee_renderer
    assert "SetEmployeeAnimationState(int employeeId, EmployeePresentationAnimationState state)" in employee_renderer
    assert "PlayEmployeeStandingUseAnimation(modelRoot)" in employee_renderer
    assert "PlayEmployeeSittingDownAnimation(modelRoot)" in employee_renderer

    assert "GetAnimationStateForCoreIntent(coreIntent.SourceAction)" in employee_autonomy
    assert "EmployeeActionCandidateKind.WorkAtDesk => EmployeePresentationAnimationState.Walking" in employee_autonomy
    assert "EmployeeActionCandidateKind.UseWhiteboard => EmployeePresentationAnimationState.Walking" in employee_autonomy
    assert "EmployeeActionCandidateKind.MaintainServer => EmployeePresentationAnimationState.Walking" in employee_autonomy
    assert "GetFacilityUseAnimationState(target.Facility)" in employee_autonomy
    assert "EmployeePresentationAnimationState.WorkingAtDesk" in employee_autonomy
    assert "EmployeePresentationAnimationState.UsingStandingFacility" in employee_autonomy
    assert "SetEmployeeAnimationState(employeeId, EmployeePresentationAnimationState.Idle)" in employee_autonomy
```

- [ ] **步骤 2: Run tests to verify they fail**

Run:

```powershell
pytest tests/test_godot_v2_scaffold.py -q -k "v2_1_0"
```

预期： both new tests fail because `ObjectiveValueLabel`, `EmployeePresentationAnimationState`, and `SetEmployeeAnimationState` are not present yet.

- [ ] **步骤 3: Commit nothing**

先不要提交红灯测试。继续任务 2 和任务 3，用小步实现让测试通过。

---

### 任务 2: Add First-Loop Objective HUD Text

**文件：**
- Modify: `godot/GetTheBestGodot/scenes/Main.tscn`
- Modify: `godot/GetTheBestGodot/scripts/BusinessFeedbackHudController.cs`
- 测试： `tests/test_godot_v2_scaffold.py`

- [ ] **步骤 1: Add HUD node references**

In `BusinessFeedbackHudController.cs`, add fields beside the existing label fields:

```csharp
private Label? _objectiveValueLabel;
private Label? _nextObjectiveValueLabel;
private Label? _recentCoreEventValueLabel;
```

In `_Ready()`, add node lookups after `_lastEventValueLabel`:

```csharp
_objectiveValueLabel = GetNodeOrNull<Label>("BusinessRows/ObjectiveValueLabel");
_nextObjectiveValueLabel = GetNodeOrNull<Label>("BusinessRows/NextObjectiveValueLabel");
_recentCoreEventValueLabel = GetNodeOrNull<Label>("BusinessRows/RecentCoreEventValueLabel");
```

Add configuration calls after `ConfigureLabel(_lastEventValueLabel, MutedTextColor);`:

```csharp
ConfigureLabel(_objectiveValueLabel, PrimaryTextColor);
ConfigureLabel(_nextObjectiveValueLabel, MutedTextColor);
ConfigureLabel(_recentCoreEventValueLabel, MutedTextColor);
```

- [ ] **步骤 2: Update reset and apply logic**

In `ApplySimulationResult(CoreOfficeSimulationResult result)`, after `SetLabel(_lastEventValueLabel, FormatLastEvent(result));`, add:

```csharp
SetLabel(
    _objectiveValueLabel,
    FormatFirstLoopObjective(result.CompanyTotals, result.OutcomeKind)
);
SetLabel(
    _nextObjectiveValueLabel,
    FormatNextObjective(result.CompanyTotals, result.OutcomeKind)
);
SetLabel(_recentCoreEventValueLabel, FormatRecentCoreEvent(result));
```

In `ResetDisplay()`, add:

```csharp
SetLabel(_objectiveValueLabel, "目标 等待 Core tick");
SetLabel(_nextObjectiveValueLabel, "下一步 观察员工行动");
SetLabel(_recentCoreEventValueLabel, "最近事件 --");
```

- [ ] **步骤 3: Add formatting helpers**

Add these methods below `FormatOutcomeKind`:

```csharp
private static string FormatFirstLoopObjective(
    CoreCompanySimulationTotals totals,
    PhaseOutcomeKind outcomeKind
)
{
    if (outcomeKind == PhaseOutcomeKind.FirstUsersAcquired)
    {
        return "目标 首批用户已获得";
    }

    if (outcomeKind == PhaseOutcomeKind.MvpCompleted || totals.ProductStage == ProductStage.MvpReady)
    {
        return "目标 准备销售与首批用户";
    }

    return string.Format(
        CultureInfo.InvariantCulture,
        "目标 推进 MVP {0:0.#}/{1:0.#}",
        totals.CurrentProjectProgress,
        totals.ProjectRequiredProgress
    );
}

private static string FormatNextObjective(
    CoreCompanySimulationTotals totals,
    PhaseOutcomeKind outcomeKind
)
{
    if (outcomeKind == PhaseOutcomeKind.FirstUsersAcquired)
    {
        return "下一步 查看月报与阶段复盘";
    }

    if (outcomeKind == PhaseOutcomeKind.MvpCompleted || totals.ProductStage == ProductStage.MvpReady)
    {
        return "下一步 让市场/运营员工使用白板或销售设施";
    }

    return "下一步 观察工程师前往办公桌研发";
}

private static string FormatRecentCoreEvent(CoreOfficeSimulationResult result)
{
    var eventSummary = result.PresentationEvents.LastOrDefault();
    return eventSummary == null
        ? "最近事件 本次 tick 无新事件"
        : $"最近事件 {FormatEventKind(eventSummary.Kind)}: {eventSummary.Message}";
}
```

- [ ] **步骤 4: Add scene labels**

In `Main.tscn`, under `HudRoot/BusinessFeedbackPanel/BusinessRows`, add three `Label` nodes after `LastEventValueLabel`:

```text
[node name="ObjectiveValueLabel" type="Label" parent="HudRoot/BusinessFeedbackPanel/BusinessRows"]
layout_mode = 2
text = "目标 等待 Core tick"

[node name="NextObjectiveValueLabel" type="Label" parent="HudRoot/BusinessFeedbackPanel/BusinessRows"]
layout_mode = 2
text = "下一步 观察员工行动"

[node name="RecentCoreEventValueLabel" type="Label" parent="HudRoot/BusinessFeedbackPanel/BusinessRows"]
layout_mode = 2
text = "最近事件 --"
```

If Godot rewrites `unique_id` values later, keep the node names and parent paths stable; the scaffold test checks names, not ids.

- [ ] **步骤 5: Run the HUD test**

Run:

```powershell
pytest tests/test_godot_v2_scaffold.py -q -k "first_loop_objective"
```

预期： the objective HUD test passes, while the animation state test still fails.

---

### 任务 3: Add Replaceable Employee Animation State Layer

**文件：**
- Modify: `godot/GetTheBestGodot/scripts/Employee3DRenderer.cs`
- Modify: `godot/GetTheBestGodot/scripts/EmployeeAutonomyController.cs`
- 测试： `tests/test_godot_v2_scaffold.py`

- [ ] **步骤 1: Define presentation animation states**

At the bottom of `Employee3DRenderer.cs`, before `EmployeeWorkPose`, add:

```csharp
public enum EmployeePresentationAnimationState
{
    Idle,
    Walking,
    SittingDown,
    WorkingAtDesk,
    UsingStandingFacility,
    LeavingFacility,
}
```

Near the existing renderer dictionaries, add:

```csharp
private readonly Dictionary<int, EmployeePresentationAnimationState> _employeeAnimationStates = [];
```

- [ ] **步骤 2: Add renderer state entrypoint**

Add this public method near `SetEmployeeActivityLabel`:

```csharp
public void SetEmployeeAnimationState(int employeeId, EmployeePresentationAnimationState state)
{
    if (
        _employeeAnimationStates.TryGetValue(employeeId, out var currentState)
        && currentState == state
    )
    {
        return;
    }

    if (state == EmployeePresentationAnimationState.Idle)
    {
        _employeeAnimationStates.Remove(employeeId);
    }
    else
    {
        _employeeAnimationStates[employeeId] = state;
    }

    RefreshEmployees();
}
```

- [ ] **步骤 3: Route model setup through animation state**

In `AddEmployeeModel`, after `AddEmployeeActivityBadge(modelRoot, employee);`, replace the working block with:

```csharp
if (_workingEmployeeIds.Contains(employee.Id))
{
    _employeeAnimationStates[employee.Id] = EmployeePresentationAnimationState.WorkingAtDesk;
}

ApplyEmployeeAnimationState(modelRoot, employee.Id);
```

Add this private method near `PlayEmployeeWorkAnimation`:

```csharp
private void ApplyEmployeeAnimationState(Node3D modelRoot, int employeeId)
{
    var state = _employeeAnimationStates.TryGetValue(employeeId, out var animationState)
        ? animationState
        : EmployeePresentationAnimationState.Idle;

    switch (state)
    {
        case EmployeePresentationAnimationState.SittingDown:
            PlayEmployeeSittingDownAnimation(modelRoot);
            break;
        case EmployeePresentationAnimationState.WorkingAtDesk:
            AddTypingHands(modelRoot);
            PlayEmployeeWorkAnimation(modelRoot);
            break;
        case EmployeePresentationAnimationState.UsingStandingFacility:
            PlayEmployeeStandingUseAnimation(modelRoot);
            break;
        case EmployeePresentationAnimationState.Walking:
        case EmployeePresentationAnimationState.LeavingFacility:
            PlayEmployeeWalkingAnimation(modelRoot);
            break;
    }
}
```

- [ ] **步骤 4: Add procedural placeholder animations**

Add these private methods near `PlayEmployeeTypingAnimation`:

```csharp
private void PlayEmployeeWalkingAnimation(Node3D modelRoot)
{
    var baseRotation = modelRoot.RotationDegrees;
    var tween = CreateTween().SetLoops();
    tween
        .TweenProperty(modelRoot, "rotation_degrees", baseRotation + new Vector3(0.0f, 0.0f, 2.2f), 0.18f)
        .SetTrans(Tween.TransitionType.Sine)
        .SetEase(Tween.EaseType.InOut);
    tween
        .TweenProperty(modelRoot, "rotation_degrees", baseRotation + new Vector3(0.0f, 0.0f, -2.2f), 0.18f)
        .SetTrans(Tween.TransitionType.Sine)
        .SetEase(Tween.EaseType.InOut);
}

private void PlayEmployeeSittingDownAnimation(Node3D modelRoot)
{
    var targetScale = modelRoot.Scale * 0.92f;
    CreateTween()
        .TweenProperty(modelRoot, "scale", targetScale, 0.16f)
        .SetTrans(Tween.TransitionType.Cubic)
        .SetEase(Tween.EaseType.Out);
}

private void PlayEmployeeStandingUseAnimation(Node3D modelRoot)
{
    var basePosition = modelRoot.Position;
    var tween = CreateTween().SetLoops();
    tween
        .TweenProperty(modelRoot, "position", basePosition + new Vector3(0.0f, 0.10f, 0.0f), 0.34f)
        .SetTrans(Tween.TransitionType.Sine)
        .SetEase(Tween.EaseType.InOut);
    tween
        .TweenProperty(modelRoot, "position", basePosition, 0.34f)
        .SetTrans(Tween.TransitionType.Sine)
        .SetEase(Tween.EaseType.InOut);
}
```

如果行长检查失败，按现有链式调用风格换行。

- [ ] **步骤 5: Set walking state during autonomous movement**

In `EmployeeAutonomyController.cs`, before each `PlayEmployeePathMove` call, set the renderer state:

```csharp
_employeeRenderer?.SetEmployeeAnimationState(
    employee.Id,
    EmployeePresentationAnimationState.Walking
);
```

Use this in both normal autonomous movement and facility movement.

- [ ] **步骤 6: Map Core intent to animation state**

In `StartFacilityMove`, after `SetEmployeeActivity(...)`, add:

```csharp
_employeeRenderer?.SetEmployeeAnimationState(
    employee.Id,
    GetAnimationStateForCoreIntent(target.SourceAction)
);
```

To make this compile, extend `FacilityInteractionTarget` to carry the source action:

```csharp
public sealed record FacilityInteractionTarget(
    FacilityPlacement Facility,
    Vector2I StandCell,
    IReadOnlyList<Vector2I> Path,
    EmployeeActionCandidateKind? SourceAction = null
)
```

When creating the target in `FindFacilityUseTarget`, pass `coreIntent.SourceAction` by changing the method signature:

```csharp
private bool FindFacilityUseTarget(
    EmployeeVisual employee,
    int? preferredFacilityId,
    EmployeeActionCandidateKind? sourceAction,
    out FacilityInteractionTarget target
)
```

and the assignment:

```csharp
target = new FacilityInteractionTarget(facility, standCell, path, sourceAction);
```

Update the caller:

```csharp
|| !FindFacilityUseTarget(
    employee,
    coreIntent.FacilityId,
    coreIntent.SourceAction,
    out var target
)
```

Add helper:

```csharp
private static EmployeePresentationAnimationState GetAnimationStateForCoreIntent(
    EmployeeActionCandidateKind? sourceAction
)
{
    return sourceAction switch
    {
        EmployeeActionCandidateKind.WorkAtDesk => EmployeePresentationAnimationState.Walking,
        EmployeeActionCandidateKind.UseWhiteboard => EmployeePresentationAnimationState.Walking,
        EmployeeActionCandidateKind.MaintainServer => EmployeePresentationAnimationState.Walking,
        EmployeeActionCandidateKind.Rest => EmployeePresentationAnimationState.Walking,
        _ => EmployeePresentationAnimationState.Walking,
    };
}
```

- [ ] **步骤 7: Set facility-use animation after arrival**

In `FinishFacilityArrival`, before or after `StartManualFacilityWork(employeeId, target.Facility);`, set:

```csharp
_employeeRenderer?.SetEmployeeAnimationState(
    employeeId,
    GetFacilityUseAnimationState(target.Facility)
);
```

Add helper:

```csharp
private static EmployeePresentationAnimationState GetFacilityUseAnimationState(
    FacilityPlacement facility
)
{
    return facility.FacilityType == FacilityBuildType.OfficeDesk
        ? EmployeePresentationAnimationState.WorkingAtDesk
        : EmployeePresentationAnimationState.UsingStandingFacility;
}
```

- [ ] **步骤 8: Clear animation states**

In `ClearEmployeeActivity`, after `_employeeRenderer?.SetEmployeeWorkState(employeeId, isWorking: false);`, add:

```csharp
_employeeRenderer?.SetEmployeeAnimationState(employeeId, EmployeePresentationAnimationState.Idle);
```

In `FinishAutonomousMove`, before `_isEmployeeMoveInProgress = false;`, ensure `ClearEmployeeActivity(employeeId);` remains present so walking state clears.

- [ ] **步骤 9: Run the animation state test**

Run:

```powershell
pytest tests/test_godot_v2_scaffold.py -q -k "employee_animation_states"
```

预期： the animation state test passes.

---

### 任务 4: Add V2-1.0 Acceptance Documentation

**文件：**
- Add: `docs/get_the_best_v2_1_0_first_loop_animation_acceptance_report.md`
- Modify: `docs/get_the_best_v2_execution_index.md`
- 测试： `python scripts/check_docs_bootstrap.py`

- [ ] **步骤 1: Create acceptance report**

Create `docs/get_the_best_v2_1_0_first_loop_animation_acceptance_report.md`:

```markdown
# V2-1.0 第一局目标链路与基础动画验收报告

日期：2026-05-22

## 本轮目标

本轮开始从 V2-0 空间沙盒进入 V2-1 第一局闭环。主线目标是让玩家能在办公室空间里理解当前阶段、下一目标、最近 Core 事件，以及员工为什么去设施工作。

本轮同时建立基础动画状态层：不一次性替换全部美术资源，但员工移动、坐下工作、站立使用设施必须有可见状态，并且后续能替换为真实骨骼动画。

## 已完成内容

1. 第一局目标 HUD 基线。
   - 在现有经营反馈面板中增加当前目标、下一步和最近 Core 事件。
   - 目标文案从 `CoreOfficeSimulationResult` 派生，不在 Godot 中计算经营规则。

2. 员工表现动画状态层。
   - 新增 `EmployeePresentationAnimationState`。
   - 支持 `Idle`、`Walking`、`SittingDown`、`WorkingAtDesk`、`UsingStandingFacility`、`LeavingFacility`。
   - 现有 TypingHands 和程序化动作已经挂到状态层，后续可替换为 GLB/AnimationPlayer。

3. Core 意图到表现映射。
   - `WorkAtDesk` 到达后进入办公桌工作状态。
   - 白板、服务器等非办公桌设施到达后进入站立使用状态。
   - 普通移动结束后清理动画状态，避免残留“移动中”。

## 验收证据

- 脚手架测试覆盖 V2-1.0 目标 HUD 与员工动画状态。
- Godot C# 构建通过。
- MCP 实机验证需确认：
  - 当前目标和下一步文字可见。
  - 员工能平滑移动到办公桌并进入工作状态。
  - 至少一种非办公桌设施显示站立使用状态。
  - `get_errors` 为 0。

## 后续限制

当前动画仍是程序化基础表现，不是最终骨骼动画。下一阶段可以在这个状态层后面接入真实角色动作、坐下/站起、走路、开门和打字素材。
```

- [ ] **步骤 2: Link report in execution index**

In `docs/get_the_best_v2_execution_index.md`, add this line after V2-0.30:

```markdown
- `docs/get_the_best_v2_1_0_first_loop_animation_acceptance_report.md`
```

- [ ] **步骤 3: Run doc check**

Run:

```powershell
python scripts\check_docs_bootstrap.py
```

预期： `Get The Best 文档初始化检查通过。`

---

### 任务 5: Local Verification And MCP Runtime Check

**文件：**
- 仅验证。

- [ ] **步骤 1: Run focused pytest checks**

Run:

```powershell
pytest tests/test_godot_v2_scaffold.py -q -k "v2_1_0 or v2_0_30 or v2_0_29"
```

预期： all selected tests pass.

- [ ] **步骤 2: Run full Python checks**

Run:

```powershell
python -m ruff check .
python -m black --check --line-length 100 --target-version py311 .
python -m isort --check-only --profile black --line-length 100 .
pytest tests/ -q
```

预期： ruff passes, black reports unchanged files, isort passes, pytest reports all tests passed.

- [ ] **步骤 3: Run Core and Godot builds**

Run:

```powershell
dotnet run --project csharp\StartupSim.Core.Tests\StartupSim.Core.Tests.csproj --configuration Debug
dotnet build godot\GetTheBestGodot\GetTheBestGodot.csproj --configuration Debug
D:\Godot\godot.cmd --headless --path "D:\Get The Best\.worktrees\godot-v2-0-25-lifecycle-bridge\godot\GetTheBestGodot" --import
```

预期： Core tests print `StartupSim.Core.Tests passed`; Godot C# build succeeds with 0 errors; headless import exits 0. 如果 Godot 重写 `.import` 文件，提交前只还原生成的 `.import` 噪音。

- [ ] **步骤 4: Run MCP scene verification**

Use Godot MCP:

```text
mcp__godot__.run_scene({"scene":"res://scenes/Main.tscn","wait_for_runtime":true})
mcp__godot__.wait({"seconds":3})
mcp__godot__.take_screenshot({"save_to":"res://addons/godot_mcp/cache/screenshots/v2_1_0_runtime.png"})
mcp__godot__.get_errors({"include_warnings":true,"max_errors":50})
```

预期：
- Screenshot shows objective/next-action text in HUD.
- At least one employee has `Walking`, `WorkingAtDesk`, or `UsingStandingFacility` presentation visible through label/pose.
- `get_errors` returns 0 errors.

- [ ] **步骤 5: Clean MCP cache**

Run:

```powershell
$cache = Resolve-Path 'godot\GetTheBestGodot\addons\godot_mcp\cache'
$workspace = Resolve-Path '.'
if (-not $cache.Path.StartsWith($workspace.Path, [System.StringComparison]::OrdinalIgnoreCase)) { throw "Refusing cache cleanup outside workspace: $($cache.Path)" }
Get-ChildItem -LiteralPath $cache.Path -Recurse -Force | Remove-Item -Recurse -Force
git status --short
```

预期： no screenshot cache files are left in git status.

---

### 任务 6: Commit, Push, Merge To Main, And Check Actions

**文件：**
- 提交实现、测试和文档改动。

- [ ] **步骤 1: Commit frontend branch**

Run:

```powershell
git status --short
git add tests\test_godot_v2_scaffold.py godot\GetTheBestGodot\scripts\BusinessFeedbackHudController.cs godot\GetTheBestGodot\scripts\Employee3DRenderer.cs godot\GetTheBestGodot\scripts\EmployeeAutonomyController.cs godot\GetTheBestGodot\scenes\Main.tscn docs\get_the_best_v2_execution_index.md docs\get_the_best_v2_1_0_first_loop_animation_acceptance_report.md
git commit -m "feat: add first loop animation baseline"
git push origin codex/godot-frontend
```

预期： commit succeeds and `codex/godot-frontend` pushes.

- [ ] **步骤 2: Fast-forward main**

Run from `D:\Get The Best\.worktrees\main-stage-merge`:

```powershell
git fetch origin
git status --short
git merge --ff-only origin/codex/godot-frontend
git push origin main
```

预期： `main` fast-forwards and pushes.

- [ ] **步骤 3: Check GitHub Actions**

Run:

```powershell
gh run list --repo KSYyyyz/get-the-best --branch main --limit 5 --json databaseId,headSha,status,conclusion,workflowName,url,createdAt
gh run watch <最新-main-run-id> --repo KSYyyyz/get-the-best --exit-status
```

预期： latest `main` run for the implementation commit completes with `success`.

---

## 自检

- Spec coverage: Task 2 covers first-loop objective HUD; Task 3 covers animation state layer and Core intent mapping; Task 4 covers Chinese acceptance docs; Task 5 covers local and MCP verification; Task 6 covers commit, push, merge to `main`, and Actions.
- 占位检查：没有未完成占位或无边界的“以后处理”要求。
- Type consistency: `EmployeePresentationAnimationState`, `SetEmployeeAnimationState`, `GetFacilityUseAnimationState`, and `CoreOfficeSimulationResult` names are used consistently across tasks.

