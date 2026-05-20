using Godot;

namespace GetTheBestGodot;

public partial class OfficeCamera3DController : Camera3D
{
    private const float MoveSpeed = 24.0f;
    private const float MinCameraSize = 12.0f;
    private const float MaxCameraSize = 64.0f;
    private const float ZoomStepFactor = 1.18f;
    private bool _isDragging;

    public override void _Ready()
    {
        Projection = ProjectionType.Orthogonal;
        Size = 36.0f;
        Current = true;
    }

    public override void _Process(double delta)
    {
        var direction = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W))
        {
            direction.Z -= 1.0f;
        }
        if (Input.IsKeyPressed(Key.S))
        {
            direction.Z += 1.0f;
        }
        if (Input.IsKeyPressed(Key.A))
        {
            direction.X -= 1.0f;
        }
        if (Input.IsKeyPressed(Key.D))
        {
            direction.X += 1.0f;
        }

        if (direction != Vector3.Zero)
        {
            Position += direction.Normalized() * MoveSpeed * (float)delta * (Size / 36.0f);
            ClampToOfficeBounds();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motionEvent && _isDragging)
        {
            var panScale = Size / GetViewport().GetVisibleRect().Size.Y;
            Position += new Vector3(-motionEvent.Relative.X * panScale, 0.0f, -motionEvent.Relative.Y * panScale);
            ClampToOfficeBounds();
            return;
        }

        if (@event is not InputEventMouseButton mouseEvent)
        {
            return;
        }

        if (mouseEvent.ButtonIndex is MouseButton.Middle or MouseButton.Right)
        {
            _isDragging = mouseEvent.Pressed;
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
    }

    private void ClampToOfficeBounds()
    {
        var bounds = OfficeWorld3DConfig.OfficeBounds;
        Position = new Vector3(
            Mathf.Clamp(Position.X, bounds.Position.X, bounds.End.X),
            Position.Y,
            Mathf.Clamp(Position.Z, bounds.Position.Y, bounds.End.Y)
        );
    }
}
