using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class Facility3DRenderer : Node3D
{
    private const float CellInnerSize = OfficeWorld3DConfig.GridSize * 0.72f;
    private const float HighlightStrokeSize = OfficeWorld3DConfig.GridSize * 0.86f;
    private static readonly Color DeskFill = new(0.72f, 0.50f, 0.28f, 1.0f);
    private static readonly Color DeskDarkFill = new(0.38f, 0.24f, 0.14f, 1.0f);
    private static readonly Color ChairFill = new(0.80f, 0.58f, 0.22f, 1.0f);
    private static readonly Color ScreenFill = new(0.18f, 0.24f, 0.26f, 1.0f);
    private static readonly Color WhiteboardFill = new(0.86f, 0.92f, 0.90f, 1.0f);
    private static readonly Color WhiteboardFrameFill = new(0.30f, 0.34f, 0.36f, 1.0f);
    private static readonly Color ServerRackFill = new(0.18f, 0.26f, 0.36f, 1.0f);
    private static readonly Color ServerPanelFill = new(0.34f, 0.46f, 0.70f, 1.0f);
    private static readonly Color StatusLightFill = new(0.38f, 0.90f, 0.52f, 1.0f);
    private static readonly Color HighlightStroke = new(1.0f, 0.95f, 0.42f, 1.0f);
    private readonly List<Node> _renderedFacilities = [];
    private FacilityPlacementStore? _facilityPlacementStore;
    private FacilityPlacement? _highlightedFacility;

    public override void _Ready()
    {
        _facilityPlacementStore = GetNodeOrNull<FacilityPlacementStore>("../FacilityPlacementStore");
    }

    public void RefreshFacilities()
    {
        foreach (var renderedFacility in _renderedFacilities)
        {
            renderedFacility.QueueFree();
        }
        _renderedFacilities.Clear();

        if (_facilityPlacementStore == null)
        {
            return;
        }

        foreach (var facility in _facilityPlacementStore.GetFacilities())
        {
            AddFacilityModel(facility);
        }
    }

    public void HighlightFacility(FacilityPlacement? facility)
    {
        _highlightedFacility = facility;
        RefreshFacilities();
    }

    private void AddFacilityModel(FacilityPlacement facility)
    {
        var position = OfficeWorld3DConfig.CellToWorldPosition(facility.Cell);
        switch (facility.FacilityType)
        {
            case FacilityBuildType.ProductWhiteboard:
                AddProductWhiteboardModel(position);
                break;
            case FacilityBuildType.ServerRack:
                AddServerRackModel(position);
                break;
            default:
                AddDeskModel(position);
                break;
        }

        if (_highlightedFacility?.Id == facility.Id)
        {
            AddHighlight(position);
        }
    }

    private void AddDeskModel(Vector3 position)
    {
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize,
                    OfficeWorld3DConfig.GridSize * 0.12f,
                    CellInnerSize * 0.58f
                ),
            },
            DeskFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.35f, -CellInnerSize * 0.05f)
        );
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.82f,
                    OfficeWorld3DConfig.GridSize * 0.20f,
                    CellInnerSize * 0.08f
                ),
            },
            DeskDarkFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.22f, -CellInnerSize * 0.32f)
        );

        AddDeskLeg(position, -0.32f, -0.25f);
        AddDeskLeg(position, 0.32f, -0.25f);
        AddDeskLeg(position, -0.32f, 0.21f);
        AddDeskLeg(position, 0.32f, 0.21f);

        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.32f,
                    OfficeWorld3DConfig.GridSize * 0.24f,
                    CellInnerSize * 0.05f
                ),
            },
            ScreenFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.52f, -CellInnerSize * 0.11f)
        );
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.18f,
                    OfficeWorld3DConfig.GridSize * 0.04f,
                    CellInnerSize * 0.14f
                ),
            },
            ScreenFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.40f, -CellInnerSize * 0.03f)
        );

        AddMeshPart(
            new CylinderMesh
            {
                TopRadius = CellInnerSize * 0.18f,
                BottomRadius = CellInnerSize * 0.18f,
                Height = OfficeWorld3DConfig.GridSize * 0.10f,
                RadialSegments = 18,
            },
            ChairFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.24f, CellInnerSize * 0.32f)
        );
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.40f,
                    OfficeWorld3DConfig.GridSize * 0.34f,
                    CellInnerSize * 0.06f
                ),
            },
            ChairFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.42f, CellInnerSize * 0.50f)
        );
    }

    private void AddDeskLeg(Vector3 position, float xRatio, float zRatio)
    {
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.07f,
                    OfficeWorld3DConfig.GridSize * 0.30f,
                    CellInnerSize * 0.07f
                ),
            },
            DeskDarkFill,
            position + new Vector3(CellInnerSize * xRatio, OfficeWorld3DConfig.GridSize * 0.18f, CellInnerSize * zRatio)
        );
    }

    private void AddProductWhiteboardModel(Vector3 position)
    {
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.88f,
                    OfficeWorld3DConfig.GridSize * 0.62f,
                    CellInnerSize * 0.08f
                ),
            },
            WhiteboardFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.54f, -CellInnerSize * 0.22f)
        );
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.96f,
                    OfficeWorld3DConfig.GridSize * 0.70f,
                    CellInnerSize * 0.04f
                ),
            },
            WhiteboardFrameFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.54f, -CellInnerSize * 0.27f)
        );
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.70f,
                    OfficeWorld3DConfig.GridSize * 0.04f,
                    CellInnerSize * 0.18f
                ),
            },
            WhiteboardFrameFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.20f, -CellInnerSize * 0.08f)
        );
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.16f,
                    OfficeWorld3DConfig.GridSize * 0.12f,
                    CellInnerSize * 0.03f
                ),
            },
            new Color(0.96f, 0.72f, 0.18f, 1.0f),
            position + new Vector3(-CellInnerSize * 0.22f, OfficeWorld3DConfig.GridSize * 0.58f, -CellInnerSize * 0.31f)
        );
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.16f,
                    OfficeWorld3DConfig.GridSize * 0.12f,
                    CellInnerSize * 0.03f
                ),
            },
            new Color(0.50f, 0.78f, 0.96f, 1.0f),
            position + new Vector3(CellInnerSize * 0.12f, OfficeWorld3DConfig.GridSize * 0.45f, -CellInnerSize * 0.31f)
        );
    }

    private void AddServerRackModel(Vector3 position)
    {
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.64f,
                    OfficeWorld3DConfig.GridSize * 0.86f,
                    CellInnerSize * 0.54f
                ),
            },
            ServerRackFill,
            position + new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.48f, 0.0f)
        );

        for (var index = 0; index < 4; index++)
        {
            AddMeshPart(
                new BoxMesh
                {
                    Size = new Vector3(
                        CellInnerSize * 0.54f,
                        OfficeWorld3DConfig.GridSize * 0.12f,
                        CellInnerSize * 0.05f
                    ),
                },
                ServerPanelFill,
                position
                    + new Vector3(
                        0.0f,
                        OfficeWorld3DConfig.GridSize * (0.22f + index * 0.16f),
                        -CellInnerSize * 0.30f
                    )
            );
            AddMeshPart(
                new CylinderMesh
                {
                    TopRadius = CellInnerSize * 0.035f,
                    BottomRadius = CellInnerSize * 0.035f,
                    Height = CellInnerSize * 0.025f,
                    RadialSegments = 12,
                },
                StatusLightFill,
                position
                    + new Vector3(
                        CellInnerSize * 0.21f,
                        OfficeWorld3DConfig.GridSize * (0.22f + index * 0.16f),
                        -CellInnerSize * 0.33f
                    ),
                new Vector3(90.0f, 0.0f, 0.0f)
            );
        }
    }

    private void AddHighlight(Vector3 position)
    {
        AddMeshPart(
            new BoxMesh
            {
                Size = new Vector3(
                    HighlightStrokeSize,
                    OfficeWorld3DConfig.GridSize * 0.035f,
                    HighlightStrokeSize
                ),
            },
            HighlightStroke,
            position - Vector3.Up * (OfficeWorld3DConfig.GridSize * 0.035f)
        );
    }

    private void AddMeshPart(Mesh mesh, Color color, Vector3 position, Vector3? rotationDegrees = null)
    {
        var part = new MeshInstance3D
        {
            Mesh = mesh,
            MaterialOverride = CreateMaterial(color),
            Position = position,
        };

        if (rotationDegrees != null)
        {
            part.RotationDegrees = rotationDegrees.Value;
        }

        AddChild(part);
        _renderedFacilities.Add(part);
    }

    private static StandardMaterial3D CreateMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 0.86f,
        };
    }
}
