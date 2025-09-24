using Godot;
using System;

public partial class EnemySkull : CharacterBody2D
{

    [Export] private int MaxHealth = 3;
    private int currentHealth;

    private AnimatedSprite2D animation;

    public override void _Ready()
    {
        currentHealth = MaxHealth;
        animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animation.Play("Idle");

        animation.AnimationFinished += OnAnimationFinished;
    }

    public void TakeDamage(int damage)
    {
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
    }
}
