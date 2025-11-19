using Godot;
using System;

public partial class WorldBounds : Area2D
{
    [Export]
    public Marker2D RespawnPoint { get; set; }

    public override void _Ready()
    {
        if (RespawnPoint == null)
        {
            GD.PrintErr("No RespawnPoint set in WorldBounds.");
        }
        
        BodyExited += OnBodyExited;
    }

    private async void OnBodyExited(Node2D body)
    {
        if (RespawnPoint == null || !IsInstanceValid(body)) return;
        
        GD.Print($"Player exited world bounds. Respawning at {RespawnPoint.GlobalPosition}. {body.Name}");

        if (body is Player player)
        {
            await ToSignal(GetTree(), "physics_frame");
            if (!GetOverlappingBodies().Contains(body))
            {
                player.GlobalPosition = RespawnPoint.GlobalPosition;
                player.Velocity = Vector2.Zero;
            }
        }
    }
}
