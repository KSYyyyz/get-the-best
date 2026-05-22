namespace StartupSim.Core;

public sealed class EmployeeBehaviorEngine
{
    private const double ActivePlanScore = 1.0;

    public IReadOnlyList<EmployeeIntent> PlanIntents(OfficeRuleSnapshot snapshot)
    {
        return PlanDecisions(snapshot).Intents;
    }

    public EmployeeBehaviorPlan PlanDecisions(OfficeRuleSnapshot snapshot)
    {
        var reservedFacilityIds = BuildInitialReservations(snapshot);
        var intents = new List<EmployeeIntent>();
        var explanations = new List<EmployeeDecisionExplanation>();

        foreach (var employee in snapshot.Employees.OrderBy(employee => employee.Id, StringComparer.Ordinal))
        {
            var decision = CreateDecision(snapshot, employee, reservedFacilityIds);
            var intent = CreateIntent(employee, decision);

            if (intent.Target.FacilityId != null)
            {
                reservedFacilityIds.Add(intent.Target.FacilityId);
            }

            intents.Add(intent);
            explanations.Add(decision);
        }

        return new EmployeeBehaviorPlan(intents, explanations);
    }

    private static EmployeeDecisionExplanation CreateDecision(
        OfficeRuleSnapshot snapshot,
        EmployeeState employee,
        HashSet<string> reservedFacilityIds
    )
    {
        var activeDecision = CreateActiveDecision(snapshot, employee);
        if (activeDecision != null)
        {
            return activeDecision;
        }

        var candidates = new[]
        {
            EvaluateFacilityCandidate(
                snapshot,
                employee,
                reservedFacilityIds,
                EmployeeActionCandidateKind.WorkAtDesk,
                FacilityType.OfficeDesk,
                [RoomType.ResearchRoom],
                RoleFitForDesk(employee.Role),
                MvpGoalScore(snapshot),
                "MVP 目标需要研发产出"
            ),
            EvaluateFacilityCandidate(
                snapshot,
                employee,
                reservedFacilityIds,
                EmployeeActionCandidateKind.UseWhiteboard,
                FacilityType.ProductWhiteboard,
                WhiteboardAllowedRoomTypes(snapshot, employee.Role),
                RoleFitForWhiteboard(employee.Role),
                WhiteboardGoalScore(snapshot, employee.Role),
                "白板适合产品方案或获客讨论"
            ),
            EvaluateFacilityCandidate(
                snapshot,
                employee,
                reservedFacilityIds,
                EmployeeActionCandidateKind.MaintainServer,
                FacilityType.ServerRack,
                [RoomType.ServerRoom],
                RoleFitForServer(employee.Role),
                StabilityGoalScore(snapshot),
                "稳定性目标需要维护服务器"
            ),
            EvaluateRestCandidate(snapshot, employee, reservedFacilityIds),
            EvaluateIdleCandidate(employee),
        };

        var selected = candidates
            .Where(candidate => candidate.RejectionReasons.Count == 0)
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.Kind)
            .First();

        return new EmployeeDecisionExplanation(
            employee.Id,
            selected.Kind,
            selected.Score,
            candidates,
            BuildSelectedReasons(selected)
        );
    }

    private static EmployeeDecisionExplanation? CreateActiveDecision(
        OfficeRuleSnapshot snapshot,
        EmployeeState employee
    )
    {
        if (
            employee.CurrentActivity is not (
                EmployeeActivityKind.MoveToFacility
                or EmployeeActivityKind.UseFacility
                or EmployeeActivityKind.Rest
                or EmployeeActivityKind.Work
            )
            || employee.ActiveFacilityId == null
        )
        {
            return null;
        }

        var facility = snapshot.Facilities.FirstOrDefault(facility =>
            facility.Id == employee.ActiveFacilityId
        );
        var action = facility == null
            ? EmployeeActionCandidateKind.Idle
            : ActionKindForFacility(facility.Type);
        var candidate = new EmployeeActionCandidate(
            action,
            ActivePlanScore,
            facility?.Id,
            facility?.RoomId ?? employee.RoomId,
            ["继续当前短计划，等待下个 AI tick 重新评估"],
            []
        );

        return new EmployeeDecisionExplanation(
            employee.Id,
            action,
            ActivePlanScore,
            [candidate],
            candidate.Reasons
        );
    }

    private static EmployeeIntent CreateIntent(
        EmployeeState employee,
        EmployeeDecisionExplanation explanation
    )
    {
        var selected = explanation.Candidates.First(candidate =>
            candidate.Kind == explanation.SelectedAction
        );
        var target = new IntentTarget(FacilityId: selected.FacilityId, RoomId: selected.RoomId);
        var intentKind = ResolveIntentKind(employee, explanation.SelectedAction);

        return new EmployeeIntent(
            employee.Id,
            intentKind,
            target,
            explanation.SelectedAction,
            explanation
        );
    }

    private static EmployeeIntentKind ResolveIntentKind(
        EmployeeState employee,
        EmployeeActionCandidateKind selectedAction
    )
    {
        if (employee.CurrentActivity is EmployeeActivityKind.UseFacility or EmployeeActivityKind.Work)
        {
            return EmployeeIntentKind.Work;
        }

        if (employee.CurrentActivity == EmployeeActivityKind.Rest)
        {
            return EmployeeIntentKind.Rest;
        }

        if (employee.CurrentActivity == EmployeeActivityKind.MoveToFacility)
        {
            return EmployeeIntentKind.MoveToFacility;
        }

        return selectedAction switch
        {
            EmployeeActionCandidateKind.Rest => EmployeeIntentKind.Rest,
            EmployeeActionCandidateKind.Idle => EmployeeIntentKind.Idle,
            _ => EmployeeIntentKind.MoveToFacility,
        };
    }

    private static EmployeeActionCandidate EvaluateFacilityCandidate(
        OfficeRuleSnapshot snapshot,
        EmployeeState employee,
        HashSet<string> reservedFacilityIds,
        EmployeeActionCandidateKind kind,
        FacilityType facilityType,
        IReadOnlyCollection<RoomType> allowedRoomTypes,
        double roleFit,
        double goalFit,
        string goalReason
    )
    {
        var rejected = new List<string>();
        var bestFacility = FindBestFacility(
            snapshot,
            facilityType,
            allowedRoomTypes,
            reservedFacilityIds,
            rejected
        );

        if (roleFit <= 0.2)
        {
            rejected.Add($"{employee.Role} 与 {kind} 岗位适配不足");
        }

        if (bestFacility == null)
        {
            rejected.Add($"{kind} 没有可用设施或交互站位");
            return new EmployeeActionCandidate(kind, 0, null, null, [], rejected);
        }

        IReadOnlyList<string> blockingRejections =
            roleFit <= 0.2 ? rejected : Array.Empty<string>();
        var room = snapshot.Rooms.First(room => room.Id == bestFacility.RoomId);
        var fatiguePenalty = employee.Fatigue >= 50 ? (employee.Fatigue - 50) / 160.0 : 0;
        var score = Clamp(
            0.25
                + roleFit * 0.25
                + goalFit * 0.25
                + FacilityQuality(bestFacility, room) * 0.15
                + WorkPressure(employee) * 0.10
                - fatiguePenalty,
            0,
            1
        );
        var reasons = new List<string>
        {
            goalReason,
            $"岗位适配 {roleFit:0.##}",
            $"设施效率 {bestFacility.EfficiencyModifier:0.##}",
        };

        if (rejected.Count > 0)
        {
            reasons.Add($"已跳过不可用设施 {rejected.Count} 个");
        }

        if (employee.Fatigue >= 70)
        {
            reasons.Add("疲劳偏高，工作分数被压低");
        }

        return new EmployeeActionCandidate(
            kind,
            Math.Round(score, 4),
            bestFacility.Id,
            bestFacility.RoomId,
            reasons,
            blockingRejections
        );
    }

    private static EmployeeActionCandidate EvaluateRestCandidate(
        OfficeRuleSnapshot snapshot,
        EmployeeState employee,
        HashSet<string> reservedFacilityIds
    )
    {
        var rejected = new List<string>();
        var restFacility = FindBestFacility(
            snapshot,
            FacilityType.RestSeat,
            [RoomType.RestRoom],
            reservedFacilityIds,
            rejected
        );
        var restPressure = RestPressure(employee);

        if (restFacility == null)
        {
            rejected.Add("Rest 没有可用设施或交互站位");
            return new EmployeeActionCandidate(
                EmployeeActionCandidateKind.Rest,
                0,
                null,
                null,
                [],
                rejected
            );
        }

        var room = snapshot.Rooms.First(room => room.Id == restFacility.RoomId);
        var score = Clamp(
            0.10 + restPressure * 0.75 + FacilityQuality(restFacility, room) * 0.10,
            0,
            1
        );

        return new EmployeeActionCandidate(
            EmployeeActionCandidateKind.Rest,
            Math.Round(score, 4),
            restFacility.Id,
            restFacility.RoomId,
            [
                $"疲劳压力 {restPressure:0.##}",
                $"休息设施效率 {restFacility.EfficiencyModifier:0.##}",
            ],
            []
        );
    }

    private static EmployeeActionCandidate EvaluateIdleCandidate(EmployeeState employee)
    {
        var score = Clamp(0.08 + (employee.NeedFood + employee.NeedToilet) * 0.05, 0.05, 0.25);
        return new EmployeeActionCandidate(
            EmployeeActionCandidateKind.Idle,
            Math.Round(score, 4),
            null,
            employee.RoomId,
            ["没有更高分且可执行的短计划时保持待机"],
            []
        );
    }

    private static FacilityState? FindBestFacility(
        OfficeRuleSnapshot snapshot,
        FacilityType facilityType,
        IReadOnlyCollection<RoomType> allowedRoomTypes,
        HashSet<string> reservedFacilityIds,
        List<string> rejected
    )
    {
        var roomsById = snapshot.Rooms.ToDictionary(room => room.Id);
        var matchingFacilities = snapshot.Facilities
            .Where(facility => facility.Type == facilityType)
            .OrderByDescending(facility => facility.EfficiencyModifier)
            .ThenBy(facility => facility.Id, StringComparer.Ordinal)
            .ToArray();

        if (matchingFacilities.Length == 0)
        {
            rejected.Add($"{facilityType} 不存在");
            return null;
        }

        foreach (var facility in matchingFacilities)
        {
            if (!roomsById.TryGetValue(facility.RoomId, out var room))
            {
                rejected.Add($"{facility.Id} 所在房间不存在");
                continue;
            }

            if (!allowedRoomTypes.Contains(room.Type))
            {
                rejected.Add($"{facility.Id} 房间类型不匹配");
                continue;
            }

            if (!room.FacilityIds.Contains(facility.Id))
            {
                rejected.Add($"{facility.Id} 未登记在房间设施列表，缺少交互站位");
                continue;
            }

            if (room.Capacity <= 0 || facility.Capacity <= 0)
            {
                rejected.Add($"{facility.Id} 缺少可交互站位");
                continue;
            }

            if (!facility.HasAvailableCapacity)
            {
                rejected.Add($"{facility.Id} 已被占用");
                continue;
            }

            if (reservedFacilityIds.Contains(facility.Id))
            {
                rejected.Add($"{facility.Id} 已被预留");
                continue;
            }

            return facility;
        }

        return null;
    }

    private static HashSet<string> BuildInitialReservations(OfficeRuleSnapshot snapshot)
    {
        var reserved = snapshot.Facilities
            .Where(facility => !facility.HasAvailableCapacity)
            .Select(facility => facility.Id)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var employee in snapshot.Employees)
        {
            if (
                employee.ActiveFacilityId != null
                && employee.CurrentActivity
                    is EmployeeActivityKind.MoveToFacility
                        or EmployeeActivityKind.UseFacility
                        or EmployeeActivityKind.Rest
            )
            {
                reserved.Add(employee.ActiveFacilityId);
            }
        }

        return reserved;
    }

    private static IReadOnlyList<string> BuildSelectedReasons(EmployeeActionCandidate selected)
    {
        var reasons = selected.Reasons.Take(2).ToList();
        reasons.Insert(0, $"选择 {selected.Kind}，分数 {selected.Score:0.####}");
        return reasons;
    }

    private static EmployeeActionCandidateKind ActionKindForFacility(FacilityType facilityType)
    {
        return facilityType switch
        {
            FacilityType.OfficeDesk => EmployeeActionCandidateKind.WorkAtDesk,
            FacilityType.ProductWhiteboard => EmployeeActionCandidateKind.UseWhiteboard,
            FacilityType.ServerRack => EmployeeActionCandidateKind.MaintainServer,
            FacilityType.RestSeat => EmployeeActionCandidateKind.Rest,
            _ => EmployeeActionCandidateKind.Idle,
        };
    }

    public static FacilityType[] GetDesiredFacilityTypes(EmployeeRole role)
    {
        return role switch
        {
            EmployeeRole.Engineer => [FacilityType.OfficeDesk, FacilityType.ServerRack],
            EmployeeRole.Designer => [FacilityType.OfficeDesk, FacilityType.ProductWhiteboard],
            EmployeeRole.Planner => [FacilityType.ProductWhiteboard, FacilityType.OfficeDesk],
            EmployeeRole.Marketing => [FacilityType.ProductWhiteboard, FacilityType.OfficeDesk],
            EmployeeRole.Operations => [FacilityType.ServerRack, FacilityType.OfficeDesk],
            _ => [FacilityType.OfficeDesk],
        };
    }

    private static double RoleFitForDesk(EmployeeRole role)
    {
        return role switch
        {
            EmployeeRole.Engineer => 1.0,
            EmployeeRole.Designer or EmployeeRole.Planner => 0.85,
            EmployeeRole.Operations => 0.55,
            EmployeeRole.Marketing => 0.45,
            _ => 0.4,
        };
    }

    private static double RoleFitForWhiteboard(EmployeeRole role)
    {
        return role switch
        {
            EmployeeRole.Designer or EmployeeRole.Planner or EmployeeRole.Marketing => 1.0,
            EmployeeRole.Engineer => 0.55,
            EmployeeRole.Operations => 0.35,
            _ => 0.4,
        };
    }

    private static double RoleFitForServer(EmployeeRole role)
    {
        return role switch
        {
            EmployeeRole.Operations => 1.0,
            EmployeeRole.Engineer => 0.75,
            _ => 0.2,
        };
    }

    private static double MvpGoalScore(OfficeRuleSnapshot snapshot)
    {
        return snapshot.Company.ActiveProject.Progress < snapshot.Company.ActiveProject.RequiredProgress
            ? 1.0
            : 0.25;
    }

    private static double WhiteboardGoalScore(OfficeRuleSnapshot snapshot, EmployeeRole role)
    {
        var product = snapshot.Company.ProductMarket;
        if (snapshot.Company.ActiveProject.Progress < snapshot.Company.ActiveProject.RequiredProgress)
        {
            return role is EmployeeRole.Designer or EmployeeRole.Planner ? 0.9 : 0.55;
        }

        if (product?.Stage is ProductStage.MvpReady or ProductStage.Launched)
        {
            return role == EmployeeRole.Marketing ? 1.0 : 0.65;
        }

        return 0.45;
    }

    private static RoomType[] WhiteboardAllowedRoomTypes(OfficeRuleSnapshot snapshot, EmployeeRole role)
    {
        if (snapshot.Company.ActiveProject.Progress < snapshot.Company.ActiveProject.RequiredProgress)
        {
            return role is EmployeeRole.Designer or EmployeeRole.Planner
                ? new[] { RoomType.ResearchRoom }
                : new[] { RoomType.MarketRoom };
        }

        return role == EmployeeRole.Marketing
            ? new[] { RoomType.MarketRoom }
            : new[] { RoomType.ResearchRoom, RoomType.MarketRoom };
    }

    private static double StabilityGoalScore(OfficeRuleSnapshot snapshot)
    {
        var product = snapshot.Company.ProductMarket;
        if (product?.Stage == ProductStage.Launched || product?.ActiveUsers > 0)
        {
            return 0.9;
        }

        return 0.45;
    }

    private static double FacilityQuality(FacilityState facility, RoomState room)
    {
        return Clamp(0.75 + (facility.EfficiencyModifier - 1.0) * 0.4 + room.Comfort - room.Noise, 0, 1);
    }

    private static double WorkPressure(EmployeeState employee)
    {
        return Clamp((employee.Energy / 100.0) * 0.65 + (employee.Satisfaction / 100.0) * 0.35, 0, 1);
    }

    private static double RestPressure(EmployeeState employee)
    {
        var fatiguePressure = employee.Fatigue / 100.0;
        var energyPressure = (100.0 - employee.Energy) / 100.0;
        return Clamp(
            fatiguePressure * 0.6
                + energyPressure * 0.3
                + employee.NeedRest * 0.1
                + employee.NeedFood * 0.025
                + employee.NeedToilet * 0.025,
            0,
            1
        );
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }
}
