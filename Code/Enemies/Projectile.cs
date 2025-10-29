using Godot;
using System;

public partial class Projectile : Node2D
{
    [Export] private double speed = 150;
    [Export] private float lifeSpanSeconds = 5;

    public override void _Ready()
    {
        var area = GetNode<Area2D>("DetectionArea");
        area.BodyEntered += _OnDetectionAreaBodyShapeEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += Transform.X * (float)delta * (float)speed;
        lifeSpanSeconds -= (float)delta;
        if (lifeSpanSeconds < 0)
        {
            QueueFree();
        }
    }

    private void _OnDetectionAreaBodyShapeEntered(object bodyEntered)
    {
        QueueFree();

        if (bodyEntered is CharacterBody2D)
        {
            if (bodyEntered is Player player)
            {
                player.TakeDamage(1, this.GlobalPosition);
            }
        }
    }
}
