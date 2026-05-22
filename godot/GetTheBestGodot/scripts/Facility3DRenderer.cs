using System.Collections.Generic;
using Godot;

namespace GetTheBestGodot;

public partial class Facility3DRenderer : Node3D
{
    private const float CellInnerSize = OfficeWorld3DConfig.GridSize * 0.72f;
    private const float OutlineShellScale = 1.10f;
    private const float DragPreviewYOffset = OfficeWorld3DConfig.GridSize * 0.16f;
    private const float SmoothMoveDurationSeconds = 0.12f;
    private static readonly Color DeskFill = new(0.72f, 0.50f, 0.28f, 1.0f);
    private static readonly Color DeskDarkFill = new(0.38f, 0.24f, 0.14f, 1.0f);
    private static readonly Color ChairFill = new(0.80f, 0.58f, 0.22f, 1.0f);
    private static readonly Color ScreenFill = new(0.18f, 0.24f, 0.26f, 1.0f);
    private static readonly Color WhiteboardFill = new(0.86f, 0.92f, 0.90f, 1.0f);
    private static readonly Color WhiteboardFrameFill = new(0.30f, 0.34f, 0.36f, 1.0f);
    private static readonly Color ServerRackFill = new(0.18f, 0.26f, 0.36f, 1.0f);
    private static readonly Color ServerPanelFill = new(0.34f, 0.46f, 0.70f, 1.0f);
    private static readonly Color StatusLightFill = new(0.38f, 0.90f, 0.52f, 1.0f);
    private static readonly Color OutlineStroke = new(0.38f, 0.82f, 1.0f, 1.0f);
    private static readonly Color InUseStroke = new(0.42f, 1.0f, 0.62f, 1.0f);
    private static readonly Color PlacementPreviewFill = new(0.64f, 0.95f, 0.72f, 1.0f);
    private static readonly Color IllegalDragFill = new(0.95f, 0.32f, 0.28f, 1.0f);
    private readonly Dictionary<int, Vector3> _lastFacilityPositions = [];
    private readonly HashSet<int> _usingFacilityIds = [];
    private readonly List<Node> _renderedFacilities = [];
    private FacilityPlacementStore? _facilityPlacementStore;
    private FacilityPlacement? _highlightedFacility;
    private int? _hoveredFacilityId;
    private int? _dragPreviewFacilityId;
    private FacilityFacing _dragPreviewFacing;
    private FacilityPlacement? _placementPreviewFacility;
    private bool _placementPreviewIsLegal = true;
    private Vector2I _dragPreviewCell;
    private bool _dragPreviewIsLegal = true;

    public override void _Ready()
    {
        _facilityPlacementStore = GetNodeOrNull<FacilityPlacementStore>("../FacilityPlacementStore");
        RefreshFacilities();
    }

    public void RefreshFacilities()
    {
        foreach (var renderedFacility in _renderedFacilities)
        {
            if (TryGetFacilityId(renderedFacility.Name.ToString(), out var facilityId))
            {
                _lastFacilityPositions[facilityId] = ((Node3D)renderedFacility).Position;
            }

            RemoveChild(renderedFacility);
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

        if (_placementPreviewFacility != null)
        {
            AddFacilityModel(_placementPreviewFacility);
        }
    }

    public void HighlightFacility(FacilityPlacement? facility)
    {
        _highlightedFacility = facility;
        RefreshFacilities();
    }

    public void HoverFacility(FacilityPlacement? facility)
    {
        _hoveredFacilityId = facility?.Id;
        RefreshFacilities();
    }

    public void ShowFacilityDragPreview(
        FacilityPlacement facility,
        Vector2I targetCell,
        bool isLegal
    )
    {
        _dragPreviewFacilityId = facility.Id;
        _dragPreviewFacing = facility.Facing;
        _dragPreviewCell = targetCell;
        _dragPreviewIsLegal = isLegal;
        _highlightedFacility = facility;
        RefreshFacilities();
    }

    public void ShowFacilityPlacementPreview(
        FacilityBuildType facilityType,
        Vector2I targetCell,
        FacilityFacing facing,
        bool isLegal
    )
    {
        _placementPreviewFacility = new FacilityPlacement(-1000, facilityType, targetCell, facing);
        _placementPreviewIsLegal = isLegal;
        RefreshFacilities();
    }

    public void ClearFacilityPlacementPreview()
    {
        if (_placementPreviewFacility == null)
        {
            return;
        }

        _placementPreviewFacility = null;
        RefreshFacilities();
    }

    public void ClearFacilityDragPreview()
    {
        if (_dragPreviewFacilityId == null)
        {
            return;
        }

        _dragPreviewFacilityId = null;
        RefreshFacilities();
    }

    public void SetFacilityUseState(int facilityId, bool isInUse)
    {
        if (isInUse)
        {
            _usingFacilityIds.Add(facilityId);
        }
        else
        {
            _usingFacilityIds.Remove(facilityId);
        }

        RefreshFacilities();
    }

    private void AddFacilityModel(FacilityPlacement facility)
    {
        var renderCell = GetRenderCell(facility);
        var yOffset =
            _dragPreviewFacilityId == facility.Id
                ? DragPreviewYOffset
                : 0.0f;
        var targetPosition =
            OfficeWorld3DConfig.CellToWorldPosition(renderCell) + new Vector3(0.0f, yOffset, 0.0f);
        var renderTint = GetRenderTint(facility);
        var modelRoot = new Node3D
        {
            Position = GetFacilityStartPosition(facility.Id, targetPosition),
            RotationDegrees = new Vector3(0.0f, GetFacingYawDegrees(GetRenderFacing(facility)), 0.0f),
        };
        modelRoot.Name = $"Facility_{facility.Id}";
        AddChild(modelRoot);
        _renderedFacilities.Add(modelRoot);
        TweenFacilityToTarget(modelRoot, targetPosition);

        if (
            _highlightedFacility?.Id == facility.Id
            || _hoveredFacilityId == facility.Id
            || _dragPreviewFacilityId == facility.Id
            || _placementPreviewFacility?.Id == facility.Id
            || _usingFacilityIds.Contains(facility.Id)
        )
        {
            AddFacilityOutlineShell(modelRoot, facility.FacilityType, GetRenderOutlineColor(facility));
        }

        switch (facility.FacilityType)
        {
            case FacilityBuildType.ProductWhiteboard:
                AddProductWhiteboardModel(modelRoot, renderTint);
                break;
            case FacilityBuildType.ServerRack:
                AddServerRackModel(modelRoot, renderTint);
                break;
            default:
                AddDeskModel(modelRoot, renderTint);
                break;
        }
    }

    private Vector2I GetRenderCell(FacilityPlacement facility)
    {
        return _dragPreviewFacilityId == facility.Id ? _dragPreviewCell : facility.Cell;
    }

    private FacilityFacing GetRenderFacing(FacilityPlacement facility)
    {
        return _dragPreviewFacilityId == facility.Id ? _dragPreviewFacing : facility.Facing;
    }

    private Vector3 GetFacilityStartPosition(int facilityId, Vector3 targetPosition)
    {
        return _lastFacilityPositions.TryGetValue(facilityId, out var previousPosition)
            ? previousPosition
            : targetPosition;
    }

    private void TweenFacilityToTarget(Node3D modelRoot, Vector3 targetPosition)
    {
        if (modelRoot.Position.DistanceTo(targetPosition) < 0.01f)
        {
            modelRoot.Position = targetPosition;
            return;
        }

        CreateTween()
            .TweenProperty(modelRoot, "position", targetPosition, SmoothMoveDurationSeconds)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private static bool TryGetFacilityId(string nodeName, out int facilityId)
    {
        facilityId = 0;
        const string prefix = "Facility_";
        return nodeName.StartsWith(prefix)
            && int.TryParse(nodeName[prefix.Length..], out facilityId);
    }

    private Color? GetRenderTint(FacilityPlacement facility)
    {
        if (_placementPreviewFacility?.Id == facility.Id)
        {
            return _placementPreviewIsLegal ? PlacementPreviewFill : IllegalDragFill;
        }

        if (_dragPreviewFacilityId != facility.Id || _dragPreviewIsLegal)
        {
            return null;
        }

        return IllegalDragFill;
    }

    private Color GetRenderOutlineColor(FacilityPlacement facility)
    {
        return (_dragPreviewFacilityId == facility.Id && !_dragPreviewIsLegal)
                || (_placementPreviewFacility?.Id == facility.Id && !_placementPreviewIsLegal)
            ? IllegalDragFill
            : _placementPreviewFacility?.Id == facility.Id
                ? PlacementPreviewFill
            : _usingFacilityIds.Contains(facility.Id)
                ? InUseStroke
            : OutlineStroke;
    }

    private void AddDeskModel(Node3D parent, Color? tint)
    {
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize,
                    OfficeWorld3DConfig.GridSize * 0.12f,
                    CellInnerSize * 0.58f
                ),
            },
            tint ?? DeskFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.35f, -CellInnerSize * 0.05f)
        );
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.82f,
                    OfficeWorld3DConfig.GridSize * 0.20f,
                    CellInnerSize * 0.08f
                ),
            },
            tint ?? DeskDarkFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.22f, -CellInnerSize * 0.32f)
        );

        AddDeskLeg(parent, -0.32f, -0.25f, tint);
        AddDeskLeg(parent, 0.32f, -0.25f, tint);
        AddDeskLeg(parent, -0.32f, 0.21f, tint);
        AddDeskLeg(parent, 0.32f, 0.21f, tint);

        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.32f,
                    OfficeWorld3DConfig.GridSize * 0.24f,
                    CellInnerSize * 0.05f
                ),
            },
            tint ?? ScreenFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.52f, -CellInnerSize * 0.11f)
        );
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.18f,
                    OfficeWorld3DConfig.GridSize * 0.04f,
                    CellInnerSize * 0.14f
                ),
            },
            tint ?? ScreenFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.40f, -CellInnerSize * 0.03f)
        );

        AddMeshPart(
            parent,
            new CylinderMesh
            {
                TopRadius = CellInnerSize * 0.18f,
                BottomRadius = CellInnerSize * 0.18f,
                Height = OfficeWorld3DConfig.GridSize * 0.10f,
                RadialSegments = 18,
            },
            tint ?? ChairFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.24f, CellInnerSize * 0.32f)
        );
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.40f,
                    OfficeWorld3DConfig.GridSize * 0.34f,
                    CellInnerSize * 0.06f
                ),
            },
            tint ?? ChairFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.42f, CellInnerSize * 0.50f)
        );
    }

    private void AddDeskLeg(Node3D parent, float xRatio, float zRatio, Color? tint)
    {
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.07f,
                    OfficeWorld3DConfig.GridSize * 0.30f,
                    CellInnerSize * 0.07f
                ),
            },
            tint ?? DeskDarkFill,
            new Vector3(
                CellInnerSize * xRatio,
                OfficeWorld3DConfig.GridSize * 0.18f,
                CellInnerSize * zRatio
            )
        );
    }

    private void AddProductWhiteboardModel(Node3D parent, Color? tint)
    {
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.88f,
                    OfficeWorld3DConfig.GridSize * 0.62f,
                    CellInnerSize * 0.08f
                ),
            },
            tint ?? WhiteboardFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.54f, -CellInnerSize * 0.22f)
        );
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.96f,
                    OfficeWorld3DConfig.GridSize * 0.70f,
                    CellInnerSize * 0.04f
                ),
            },
            tint ?? WhiteboardFrameFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.54f, -CellInnerSize * 0.27f)
        );
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.70f,
                    OfficeWorld3DConfig.GridSize * 0.04f,
                    CellInnerSize * 0.18f
                ),
            },
            tint ?? WhiteboardFrameFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.20f, -CellInnerSize * 0.08f)
        );
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.16f,
                    OfficeWorld3DConfig.GridSize * 0.12f,
                    CellInnerSize * 0.03f
                ),
            },
            tint ?? new Color(0.96f, 0.72f, 0.18f, 1.0f),
            new Vector3(
                -CellInnerSize * 0.22f,
                OfficeWorld3DConfig.GridSize * 0.58f,
                -CellInnerSize * 0.31f
            )
        );
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.16f,
                    OfficeWorld3DConfig.GridSize * 0.12f,
                    CellInnerSize * 0.03f
                ),
            },
            tint ?? new Color(0.50f, 0.78f, 0.96f, 1.0f),
            new Vector3(
                CellInnerSize * 0.12f,
                OfficeWorld3DConfig.GridSize * 0.45f,
                -CellInnerSize * 0.31f
            )
        );
    }

    private void AddServerRackModel(Node3D parent, Color? tint)
    {
        AddMeshPart(
            parent,
            new BoxMesh
            {
                Size = new Vector3(
                    CellInnerSize * 0.64f,
                    OfficeWorld3DConfig.GridSize * 0.86f,
                    CellInnerSize * 0.54f
                ),
            },
            tint ?? ServerRackFill,
            new Vector3(0.0f, OfficeWorld3DConfig.GridSize * 0.48f, 0.0f)
        );

        for (var index = 0; index < 4; index++)
        {
            AddMeshPart(
                parent,
                new BoxMesh
                {
                    Size = new Vector3(
                        CellInnerSize * 0.54f,
                        OfficeWorld3DConfig.GridSize * 0.12f,
                        CellInnerSize * 0.05f
                    ),
                },
                tint ?? ServerPanelFill,
                new Vector3(
                    0.0f,
                    OfficeWorld3DConfig.GridSize * (0.22f + index * 0.16f),
                    -CellInnerSize * 0.30f
                )
            );
            AddMeshPart(
                parent,
                new CylinderMesh
                {
                    TopRadius = CellInnerSize * 0.035f,
                    BottomRadius = CellInnerSize * 0.035f,
                    Height = CellInnerSize * 0.025f,
                    RadialSegments = 12,
                },
                tint ?? StatusLightFill,
                new Vector3(
                    CellInnerSize * 0.21f,
                    OfficeWorld3DConfig.GridSize * (0.22f + index * 0.16f),
                    -CellInnerSize * 0.33f
                ),
                new Vector3(90.0f, 0.0f, 0.0f)
            );
        }
    }

    private void AddFacilityOutlineShell(Node3D parent, FacilityBuildType facilityType, Color color)
    {
        var shellRoot = new Node3D
        {
            Scale = new Vector3(OutlineShellScale, OutlineShellScale, OutlineShellScale),
        };
        parent.AddChild(shellRoot);

        switch (facilityType)
        {
            case FacilityBuildType.ProductWhiteboard:
                AddProductWhiteboardModel(shellRoot, color);
                break;
            case FacilityBuildType.ServerRack:
                AddServerRackModel(shellRoot, color);
                break;
            default:
                AddDeskModel(shellRoot, color);
                break;
        }

        ApplyOutlineMaterial(shellRoot, color);
    }

    private void AddMeshPart(
        Node parent,
        Mesh mesh,
        Color color,
        Vector3 position,
        Vector3? rotationDegrees = null
    )
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

        parent.AddChild(part);
        if (ReferenceEquals(parent, this))
        {
            _renderedFacilities.Add(part);
        }
    }

    private static float GetFacingYawDegrees(FacilityFacing facing)
    {
        return facing switch
        {
            FacilityFacing.North => 0.0f,
            FacilityFacing.East => 90.0f,
            FacilityFacing.South => 180.0f,
            _ => 270.0f,
        };
    }

    private static StandardMaterial3D CreateMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            Roughness = 0.86f,
        };
    }

    private static StandardMaterial3D CreateOutlineMaterial(Color color)
    {
        return new StandardMaterial3D
        {
            AlbedoColor = color,
            EmissionEnabled = true,
            Emission = color,
            EmissionEnergyMultiplier = 0.75f,
            Grow = true,
            GrowAmount = 0.025f,
            CullMode = BaseMaterial3D.CullModeEnum.Front,
            Roughness = 0.55f,
        };
    }

    private static void ApplyOutlineMaterial(Node node, Color color)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                mesh.MaterialOverride = CreateOutlineMaterial(color);
            }

            ApplyOutlineMaterial(child, color);
        }
    }
}
