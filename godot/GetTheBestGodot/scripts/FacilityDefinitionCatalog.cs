using Godot;

namespace GetTheBestGodot;

public sealed record FacilityDefinition(
    FacilityBuildType FacilityType,
    string Label,
    RoomBuildType RequiredRoomType,
    Vector2I Footprint,
    bool IsWorkstation,
    string TexturePath
);

public static class FacilityDefinitionCatalog
{
    private static readonly FacilityDefinition OfficeDesk = new(
        FacilityBuildType.OfficeDesk,
        "办公桌",
        RoomBuildType.ResearchRoom,
        Footprint: new Vector2I(1, 1),
        IsWorkstation: true,
        "res://assets/third_party_placeholder_assets/kenney_furniture_kit/desk_SE.png"
    );

    private static readonly FacilityDefinition ProductWhiteboard = new(
        FacilityBuildType.ProductWhiteboard,
        "产品白板",
        RoomBuildType.MarketRoom,
        Footprint: new Vector2I(1, 1),
        IsWorkstation: false,
        "res://assets/third_party_placeholder_assets/kenney_furniture_kit/computerScreen_SE.png"
    );

    private static readonly FacilityDefinition ServerRack = new(
        FacilityBuildType.ServerRack,
        "服务器机柜",
        RoomBuildType.ServerRoom,
        Footprint: new Vector2I(1, 1),
        IsWorkstation: false,
        "res://assets/third_party_placeholder_assets/kenney_furniture_kit/bookcaseClosedDoors_SE.png"
    );

    public static FacilityDefinition GetDefinition(FacilityBuildType facilityType)
    {
        return facilityType switch
        {
            FacilityBuildType.OfficeDesk => OfficeDesk,
            FacilityBuildType.ProductWhiteboard => ProductWhiteboard,
            FacilityBuildType.ServerRack => ServerRack,
            _ => OfficeDesk,
        };
    }
}
