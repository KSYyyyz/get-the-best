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
    private const float DefaultPitchDegrees = 58.0f;
    private const float MinPitchDegrees = 10.0f;
    private const float MaxPitchDegrees = DefaultPitchDegrees;
    private const float MiddlePitchSensitivity = 0.18f;
    private const float CameraDistance = 170.0f;
    private const Key RotateLeftKey = Key.Q;
    private const Key RotateRightKey = Key.E;
    private bool _isPitchDragging;
    private float _pitchDegrees = DefaultPitchDegrees;
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
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motionEvent && _isPitchDragging)
        {
            AdjustPitchFromMiddleDrag(motionEvent.Relative.Y);
            return;
        }

        if (@event is not InputEventMouseButton mouseEvent)
        {
            return;
        }

        if (mouseEvent.ButtonIndex == MouseButton.Middle)
        {
            _isPitchDragging = mouseEvent.Pressed;
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

    private void AdjustPitchFromMiddleDrag(float relativeY)
    {
        _pitchDegrees = Mathf.Clamp(
            _pitchDegrees + relativeY * MiddlePitchSensitivity,
            MinPitchDegrees,
            MaxPitchDegrees
        );
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
        var pitchRadians = Mathf.DegToRad(_pitchDegrees);
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
