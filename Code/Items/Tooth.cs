using Godot;
using System;

public partial class Tooth : Node2D
{
    [Export] public float speed = 200f;
    [Export] public Vector2 direction = Vector2.Right;
    [Export] public float lifetime = 3f;

    private float timer = 0f;
    private Area2D area;
    private AnimationPlayer animation;

    public override void _Ready()
    {
        area = GetNode<Area2D>("Area2D");
        area.AreaEntered += OnAreaAreaEntered;

        animation = GetNode<AnimationPlayer>("AnimationPlayer");
        animation.AnimationFinished += OnAnimationFinished;
        animation.Play("shoot");
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += direction * speed * (float)delta;

        timer += (float)delta;
        if (timer >= lifetime)
            QueueFree();
    }

    private void OnAreaAreaEntered(Area2D area)
    {
        if (area.Name != "Hurtbox")
            return;

        if (area.GetParent() is Player player)
        {
            player.TakeDamage(1, GlobalPosition);
            QueueFree();
        }
    }

    private void OnAnimationFinished(StringName animName)
    {
        if (animName == "shoot")
        {
            animation.Play("fly");
        }
    }
}
