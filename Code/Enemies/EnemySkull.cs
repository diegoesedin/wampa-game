using Godot;
using System;

public partial class EnemySkull : CharacterBody2D
{
    [Export] private PackedScene projectile;
    [Export] private int MaxHealth = 3;
    private int currentHealth;

    private AnimatedSprite2D animation;

    private Player player;
    private Rid playerAttackArea;
    private Node2D raycastToPosition;
    private bool playerOnRange;
    private bool canShoot;
    private float shootCooldownTimer = 1f;
    private float shootCooldown = 1f;
    private Marker2D projectileSpawn;

    [Signal] public delegate void EnemyDiedEventHandler();

    public override void _Ready()
    {
        currentHealth = MaxHealth;

        projectileSpawn = this.GetNode<Marker2D>("ProjectileSpawn");

        animation = GetNode<AnimatedSprite2D>("SkullSprite");
        animation.Play("Idle");

        animation.AnimationFinished += OnAnimationFinished;

        var area = GetNode<Area2D>("DetectionArea");
        area.BodyEntered += _OnDetectionAreaBodyEntered;
        area.BodyExited += _OnDetectionAreaBodyExited;
    }

    public override void _Process(double delta)
    {
        if (playerOnRange)
        {
            // dependiendo de que lado está el jugador, flipeamos al enemigo
            var angle = GlobalPosition.AngleToPoint(raycastToPosition.GlobalPosition);
            var scale = Mathf.Abs(angle) < Math.PI / 2 ? 1 : -1;
            Scale = new Vector2(scale, 1);
            
            if (canShoot)
            {
                //animation.Play("Shoot");
                var spaceState = GetWorld2D().DirectSpaceState;
                var rayResult = spaceState.IntersectRay(new PhysicsRayQueryParameters2D()
                {
                    From = this.GlobalPosition,
                    To = raycastToPosition.GlobalPosition,
                    CollideWithAreas = true,
                    CollideWithBodies = true,
                    Exclude = new Godot.Collections.Array<Rid> { this.GetRid() },
                    CollisionMask = 1 << 1
                });
                
                if (rayResult.Count > 0 && rayResult.TryGetValue("collider", out var colliderValue))
                {
                    projectileSpawn.LookAt(raycastToPosition.GlobalPosition);
                    var hitCollider = colliderValue.As<GodotObject>();
                    if (ReferenceEquals(hitCollider, player))
                    {
                        var bullet = projectile.Instantiate<Projectile>();
                        Owner.AddChild(bullet);
                        bullet.GlobalTransform = projectileSpawn.GlobalTransform;
                        canShoot = false;
                        shootCooldownTimer = shootCooldown;
                        animation.Play("Shoot");
                    }
                }
                
            }
        }

        if (shootCooldownTimer > 0f)
        {
            shootCooldownTimer -= (float)delta;
        }
        else
        {
            canShoot = true;
        }
    }

    public void TakeDamage(int damage)
    {
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
            EmitSignal(nameof(EnemyDied));
            QueueFree();
        }
        else if (animation.Animation == "Hurt")
        {
            animation.Play("Idle");
        }
        else if (animation.Animation == "Shoot")
        {
            animation.Play("Idle");
        }
    }

    private void _OnDetectionAreaBodyEntered(object body)
    {
        // Player on enemy range
        if (body is Player _player)
        {
            player = _player;
            raycastToPosition = player.GetNode<Node2D>("StandingCollision");
            playerAttackArea = player.GetNode<Area2D>("AttackArea").GetRid();
            playerOnRange = true;
        }
    }

    private void _OnDetectionAreaBodyExited(object body)
    {
        GD.Print($"Exit");
        // Player left enemy range
        if (body is Player)
        {
            playerOnRange = false;
        }
    }
}
