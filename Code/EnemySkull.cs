using Godot;
using System;

public partial class EnemySkull : Node2D
{
    [Export] private int MaxHealth = 3;
    private int currentHealth;

    private AnimatedSprite2D animation;

    public override void _Ready()
    {
        currentHealth = MaxHealth;

        animation = GetNode<AnimatedSprite2D>("SkullSprite");
        animation.Play("Idle");

        animation.AnimationFinished += OnAnimationFinished;
    }


    public void TakeDamage(int damage)
    {
        GD.Print($"Enemy hit! Current health: {currentHealth}");

        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            animation.Play("Hurt");
        }
        else
        {
            animation.Play("Death");
        }
    }


    private void OnAnimationFinished()
    {
        if (animation.Animation == "Death")
        {
            QueueFree();
        }
        else if (animation.Animation == "Hurt")
        {
            animation.Play("Idle");
        }
    }
}