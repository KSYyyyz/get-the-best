using Godot;

namespace GetTheBestGodot;

public partial class OfficeCameraController : Camera2D
{
    private const float MoveSpeed = 900.0f;
    private const float MinZoom = 0.45f;
    private const float MaxZoom = 2.0f;
    private bool _isDragging;

    public override void _Process(double delta)
    {
        var direction = Vector2.Zero;
        if (Input.IsKeyPressed(Key.W))
        {
            direction.Y -= 1.0f;
        }
        if (Input.IsKeyPressed(Key.S))
        {
            direction.Y += 1.0f;
        }
        if (Input.IsKeyPressed(Key.A))
        {
            direction.X -= 1.0f;
        }
        if (Input.IsKeyPressed(Key.D))
        {
            direction.X += 1.0f;
        }

        if (direction != Vector2.Zero)
        {
            Position += direction.Normalized() * MoveSpeed * (float)delta / Zoom.X;
            ClampToOfficeBounds();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motionEvent && _isDragging)
        {
            Position -= motionEvent.Relative / Zoom.X;
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
            SetZoomLevel(Zoom.X + 0.1f);
        }
        else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
        {
            SetZoomLevel(Zoom.X - 0.1f);
        }
    }

    private void SetZoomLevel(float zoomLevel)
    {
        var clampedZoom = Mathf.Clamp(zoomLevel, MinZoom, MaxZoom);
        Zoom = new Vector2(clampedZoom, clampedZoom);
        ClampToOfficeBounds();
    }

    private void ClampToOfficeBounds()
    {
        var bounds = OfficeWorldConfig.OfficeBounds;
        Position = new Vector2(
            Mathf.Clamp(Position.X, bounds.Position.X, bounds.End.X),
            Mathf.Clamp(Position.Y, bounds.Position.Y, bounds.End.Y)
        );
    }
}
