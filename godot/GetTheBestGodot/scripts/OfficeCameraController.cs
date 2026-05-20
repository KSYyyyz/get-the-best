using Godot;

namespace GetTheBestGodot;

public partial class OfficeCameraController : Camera2D
{
    private const float MoveSpeed = 520.0f;
    private const float MinZoom = 0.7f;
    private const float MaxZoom = 2.0f;
    private static readonly Rect2 CameraBounds = new(new Vector2(120, 80), new Vector2(1040, 620));

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
            Position = new Vector2(
                Mathf.Clamp(Position.X, CameraBounds.Position.X, CameraBounds.End.X),
                Mathf.Clamp(Position.Y, CameraBounds.Position.Y, CameraBounds.End.Y)
            );
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed)
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
    }
}

