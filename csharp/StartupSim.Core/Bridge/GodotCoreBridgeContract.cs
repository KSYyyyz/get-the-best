namespace StartupSim.Core;

public sealed record GodotOfficeSnapshotDto(
    IReadOnlyList<GodotEmployeeFactDto> Employees,
    IReadOnlyList<GodotFacilityFactDto> Facilities,
    IReadOnlyList<GodotRoomFactDto> Rooms,
    GodotCompanyFactDto Company
);

public sealed record GodotEmployeeFactDto(
    int Id,
    string DisplayName,
    string RoleLabel,
    double Skill,
    double Energy,
    double Fatigue,
    double Satisfaction,
    string ActivityCode,
    string? RoomId,
    int? CellX,
    int? CellY
);

public sealed record GodotFacilityFactDto(
    int Id,
    string FacilityTypeCode,
    string RoomId,
    int Capacity,
    IReadOnlyList<int> OccupiedByEmployeeIds,
    double EfficiencyModifier
);

public sealed record GodotRoomFactDto(
    string Id,
    string RoomTypeCode,
    double Comfort,
    double Noise,
    int Capacity,
    IReadOnlyList<string> FacilityIds
);

public sealed record GodotCompanyFactDto(
    double Cash,
    double MonthlyCostRate,
    string ProjectId,
    double ProjectProgress,
    double ProjectRequiredProgress
);

public sealed class GodotCoreBridgeContract
{
    public OfficeRuleSnapshot BuildSnapshot(GodotOfficeSnapshotDto dto)
    {
        return new OfficeRuleSnapshot(
            dto.Employees.Select(MapEmployee).ToArray(),
            dto.Facilities.Select(MapFacility).ToArray(),
            dto.Rooms.Select(MapRoom).ToArray(),
            new CompanyState(
                dto.Company.Cash,
                dto.Company.MonthlyCostRate,
                new ProjectState(
                    dto.Company.ProjectId,
                    dto.Company.ProjectProgress,
                    dto.Company.ProjectRequiredProgress
                )
            )
        );
    }

    private static EmployeeState MapEmployee(GodotEmployeeFactDto dto)
    {
        return new EmployeeState(
            Id: ToEmployeeId(dto.Id),
            DisplayName: dto.DisplayName,
            Role: MapRole(dto.RoleLabel),
            Skill: dto.Skill,
            Energy: dto.Energy,
            Fatigue: dto.Fatigue,
            Satisfaction: dto.Satisfaction,
            CurrentActivity: MapActivity(dto.ActivityCode),
            RoomId: dto.RoomId,
            Cell: dto.CellX.HasValue && dto.CellY.HasValue
                ? new GridCell(dto.CellX.Value, dto.CellY.Value)
                : null
        );
    }

    private static FacilityState MapFacility(GodotFacilityFactDto dto)
    {
        return new FacilityState(
            Id: ToFacilityId(dto.Id),
            Type: MapFacilityType(dto.FacilityTypeCode),
            RoomId: dto.RoomId,
            Capacity: dto.Capacity,
            OccupiedByEmployeeIds: dto.OccupiedByEmployeeIds.Select(ToEmployeeId).ToArray(),
            EfficiencyModifier: dto.EfficiencyModifier
        );
    }

    private static RoomState MapRoom(GodotRoomFactDto dto)
    {
        return new RoomState(
            dto.Id,
            MapRoomType(dto.RoomTypeCode),
            dto.Comfort,
            dto.Noise,
            dto.Capacity,
            dto.FacilityIds
        );
    }

    private static string ToEmployeeId(int id)
    {
        return $"employee-{id}";
    }

    private static string ToFacilityId(int id)
    {
        return $"facility-{id}";
    }

    private static EmployeeRole MapRole(string roleLabel)
    {
        return roleLabel switch
        {
            "Engineer" or "\u7a0b\u5e8f" => EmployeeRole.Engineer,
            "Designer" or "\u8bbe\u8ba1" => EmployeeRole.Designer,
            "Planner" or "\u7b56\u5212" => EmployeeRole.Planner,
            "Marketing" or "\u5e02\u573a" => EmployeeRole.Marketing,
            "Operations" or "\u8fd0\u8425" => EmployeeRole.Operations,
            _ => throw new ArgumentException($"Unknown employee role: {roleLabel}", nameof(roleLabel)),
        };
    }

    private static EmployeeActivityKind MapActivity(string activityCode)
    {
        return Enum.TryParse<EmployeeActivityKind>(activityCode, ignoreCase: true, out var activity)
            ? activity
            : throw new ArgumentException($"Unknown employee activity: {activityCode}", nameof(activityCode));
    }

    private static FacilityType MapFacilityType(string facilityTypeCode)
    {
        return Enum.TryParse<FacilityType>(facilityTypeCode, ignoreCase: true, out var facilityType)
            ? facilityType
            : throw new ArgumentException(
                $"Unknown facility type: {facilityTypeCode}",
                nameof(facilityTypeCode)
            );
    }

    private static RoomType MapRoomType(string roomTypeCode)
    {
        return Enum.TryParse<RoomType>(roomTypeCode, ignoreCase: true, out var roomType)
            ? roomType
            : throw new ArgumentException($"Unknown room type: {roomTypeCode}", nameof(roomTypeCode));
    }
}
