using Godot;
using System;

public partial class SkullProjectile : Node2D
{
    [Export] private double speed;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    private void _OnDetectionAreaBodyShapeEntered(object bodyEntered)
    {
        
    }
}
