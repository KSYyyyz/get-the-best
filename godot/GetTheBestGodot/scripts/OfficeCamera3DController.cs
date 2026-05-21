using Godot;

namespace GetTheBestGodot;

public partial class OfficeCamera3DController : Camera3D
{
    private const float MoveSpeed = 72.0f;
    private const float DefaultCameraSize = 112.0f;
    private const float MinCameraSize = 28.0f;
    private const float MaxCameraSize = 210.0f;
    private const float ZoomStepFactor = 1.14f;
    private const float RotationSpeedDegrees = 90.0f;
    private const float CameraPitchDegrees = 42.0f;
    private const float MiddleRotateSensitivity = 0.22f;
    private const float EdgePanMarginPixels = 28.0f;
    private const float EdgePanSpeed = 84.0f;
    private const float CameraDistance = 170.0f;
    private const Key RotateLeftKey = Key.Q;
    private const Key RotateRightKey = Key.E;
    private bool _isMiddleRotating;
    private bool _isRightPanning;
    private bool _isMouseInsideViewport = true;
    private Vector2 _lastMousePosition;
    private float YawDegrees = -35.0f;
    private Vector3 _focus = Vector3.Zero;

    public override void _Ready()
    {
        Projection = ProjectionType.Orthogonal;
        Current = true;
        GetViewport().SizeChanged += FitCameraSizeToViewport;
        FitCameraSizeToViewport();
        ApplyCameraPose();
    }

    public override void _Process(double delta)
    {
        var direction = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W))
        {
            direction += GetPlanarForward();
        }
        if (Input.IsKeyPressed(Key.S))
        {
            direction -= GetPlanarForward();
        }
        if (Input.IsKeyPressed(Key.A))
        {
            direction -= GetPlanarRight();
        }
        if (Input.IsKeyPressed(Key.D))
        {
            direction += GetPlanarRight();
        }

        if (direction != Vector3.Zero)
        {
            _focus += direction.Normalized() * MoveSpeed * (float)delta * (Size / DefaultCameraSize);
            ClampToOfficeBounds();
            ApplyCameraPose();
        }

        var yawDelta = 0.0f;
        if (Input.IsKeyPressed(RotateLeftKey))
        {
            yawDelta -= RotationSpeedDegrees * (float)delta;
        }
        if (Input.IsKeyPressed(RotateRightKey))
        {
            yawDelta += RotationSpeedDegrees * (float)delta;
        }

        if (!Mathf.IsZeroApprox(yawDelta))
        {
            YawDegrees += yawDelta;
            ApplyCameraPose();
        }

        var edgePanDirection = GetEdgePanDirection();
        if (edgePanDirection != Vector3.Zero)
        {
            PanCameraByDirection(
                edgePanDirection.Normalized(),
                EdgePanSpeed * (float)delta * (Size / DefaultCameraSize)
            );
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMMouseEnter)
        {
            _isMouseInsideViewport = true;
        }
        else if (what == NotificationWMMouseExit)
        {
            _isMouseInsideViewport = false;
            _isMiddleRotating = false;
            _isRightPanning = false;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motionEvent && _isMiddleRotating)
        {
            UpdateLastMousePosition(motionEvent.Position);
            AdjustYawFromMiddleDrag(motionEvent.Relative.X);
            return;
        }

        if (@event is InputEventMouseMotion rightMotionEvent && _isRightPanning)
        {
            UpdateLastMousePosition(rightMotionEvent.Position);
            PanCameraByMouseDelta(rightMotionEvent.Relative);
            return;
        }

        if (@event is InputEventMouseMotion passiveMotionEvent)
        {
            UpdateLastMousePosition(passiveMotionEvent.Position);
            return;
        }

        if (@event is not InputEventMouseButton mouseEvent)
        {
            return;
        }

        UpdateLastMousePosition(mouseEvent.Position);

        if (mouseEvent.ButtonIndex == MouseButton.Middle)
        {
            _isMiddleRotating = mouseEvent.Pressed;
            return;
        }

        if (mouseEvent.ButtonIndex == MouseButton.Right)
        {
            _isRightPanning = mouseEvent.Pressed;
            return;
        }

        if (!mouseEvent.Pressed)
        {
            return;
        }

        if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
        {
            SetCameraSize(Size / ZoomStepFactor);
        }
        else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
        {
            SetCameraSize(Size * ZoomStepFactor);
        }
    }

    private void SetCameraSize(float cameraSize)
    {
        Size = Mathf.Clamp(cameraSize, MinCameraSize, MaxCameraSize);
        ApplyCameraPose();
    }

    private void AdjustYawFromMiddleDrag(float relativeX)
    {
        YawDegrees += relativeX * MiddleRotateSensitivity;
        ApplyCameraPose();
    }

    private void PanCameraByMouseDelta(Vector2 relative)
    {
        var panScale = Size / Mathf.Max(GetViewport().GetVisibleRect().Size.Y, 1.0f);
        PanCameraByDirection(-GetPlanarRight() * relative.X + GetPlanarForward() * relative.Y, panScale);
    }

    private void PanCameraByDirection(Vector3 direction, float distance)
    {
        _focus += direction * distance;
        ClampToOfficeBounds();
        ApplyCameraPose();
    }

    private void FitCameraSizeToViewport()
    {
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var aspect = Mathf.Max(viewportSize.X / Mathf.Max(viewportSize.Y, 1.0f), 1.0f);
        var heightFit = OfficeWorld3DConfig.OfficeBounds.Size.Y + 8.0f;
        var widthFit = (OfficeWorld3DConfig.OfficeBounds.Size.X + 12.0f) / aspect;
        Size = Mathf.Min(MaxCameraSize, Mathf.Max(DefaultCameraSize, Mathf.Max(heightFit, widthFit)));
    }

    private void ApplyCameraPose()
    {
        var yawRadians = Mathf.DegToRad(YawDegrees);
        var pitchRadians = Mathf.DegToRad(CameraPitchDegrees);
        var lookDirection = new Vector3(
            Mathf.Sin(yawRadians) * Mathf.Cos(pitchRadians),
            -Mathf.Sin(pitchRadians),
            -Mathf.Cos(yawRadians) * Mathf.Cos(pitchRadians)
        ).Normalized();

        Position = _focus - lookDirection * CameraDistance;
        ApplyStableCameraBasis(lookDirection);
    }

    private void ApplyStableCameraBasis(Vector3 lookDirection)
    {
        GlobalTransform = new Transform3D(Basis.LookingAt(lookDirection, Vector3.Up), Position);
    }

    private void UpdateLastMousePosition(Vector2 mousePosition)
    {
        _lastMousePosition = mousePosition;
        _isMouseInsideViewport = IsMousePositionInsideViewport(mousePosition);
    }

    private Vector3 GetEdgePanDirection()
    {
        if (!_isMouseInsideViewport || _isMiddleRotating)
        {
            return Vector3.Zero;
        }

        var viewportSize = GetViewport().GetVisibleRect().Size;
        var mousePosition = _lastMousePosition;
        var direction = Vector3.Zero;

        if (!IsMousePositionInsideViewport(mousePosition))
        {
            return Vector3.Zero;
        }

        if (mousePosition.X <= EdgePanMarginPixels)
        {
            direction -= GetPlanarRight();
        }
        else if (mousePosition.X >= viewportSize.X - EdgePanMarginPixels)
        {
            direction += GetPlanarRight();
        }

        if (mousePosition.Y <= EdgePanMarginPixels)
        {
            direction += GetPlanarForward();
        }
        else if (mousePosition.Y >= viewportSize.Y - EdgePanMarginPixels)
        {
            direction -= GetPlanarForward();
        }

        return direction;
    }

    private bool IsMousePositionInsideViewport(Vector2 mousePosition)
    {
        var viewportSize = GetViewport().GetVisibleRect().Size;
        return mousePosition.X >= 0.0f
            && mousePosition.Y >= 0.0f
            && mousePosition.X <= viewportSize.X
            && mousePosition.Y <= viewportSize.Y;
    }

    private Vector3 GetPlanarForward()
    {
        var forward = -GlobalTransform.Basis.Z;
        forward.Y = 0.0f;
        return forward.Normalized();
    }

    private Vector3 GetPlanarRight()
    {
        var right = GlobalTransform.Basis.X;
        right.Y = 0.0f;
        return right.Normalized();
    }

    private void ClampToOfficeBounds()
    {
        var bounds = OfficeWorld3DConfig.OfficeBounds;
        _focus = new Vector3(
            Mathf.Clamp(_focus.X, bounds.Position.X, bounds.End.X),
            0.0f,
            Mathf.Clamp(_focus.Z, bounds.Position.Y, bounds.End.Y)
        );
    }
}
