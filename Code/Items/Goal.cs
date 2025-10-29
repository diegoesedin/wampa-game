using Godot;
using System;

public partial class Goal : Node2D
{
    private Area2D area;

    public override void _Ready()
    {
        area = GetNode<Area2D>("Node2D/Area2D");
        area.BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            var playerScript = body as Player;
            playerScript?.CompleteLevel();

            QueueFree();
        }
    }


}
