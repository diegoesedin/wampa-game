using System;
using Godot;

public partial class EnemySkull : CharacterBody2D
{
    [Export] private PackedScene projectile;
    [Export] private int MaxHealth = 3;
    [Export] private float attackCooldown = 2.0f;
    [Export] private bool meeleEnemy;
    
    private int currentHealth;
    private AnimatedSprite2D animation;
    private Area2D attackArea;
    private AnimatedSprite2D weapon;
    private bool isAttacking = false;
    private float attackTimer = 0f;

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
        weapon = GetNode<AnimatedSprite2D>("AttackArea/BoneSprite");
        weapon.AnimationFinished += OnAnimationFinished;
        attackArea = GetNode<Area2D>("AttackArea");
        if (meeleEnemy) attackArea.Visible = true;
        attackArea.Monitoring = false;
        attackArea.BodyEntered += OnAttackAreaBodyEntered;
        
        attackTimer = attackCooldown;
    }

    public override void _Process(double delta)
    {
        // dependiendo de que lado está el jugador, flipeamos al enemigo
        if (raycastToPosition != null)
        {
            var angle = GlobalPosition.AngleToPoint(raycastToPosition.GlobalPosition);
            var scale = Mathf.Abs(angle) < Math.PI / 2 ? 1 : -1;
            Scale = new Vector2(scale, 1);
        }
        
        if (meeleEnemy)
        {
            if (currentHealth <= 0 || isAttacking) return;
        
            attackTimer -= (float)delta;

            if (attackTimer <= 0)
            {
                Attack();
            }
        }
        else
        {
            if (playerOnRange)
            {
                if (canShoot && raycastToPosition != null)
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
                        // (1u << 0) es Capa 1 (world)
                        // (1u << 1) es Capa 2 (player)
                        CollisionMask = (1u << 0) | (1u << 1)
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
    }

    private void Attack()
    {
        isAttacking = true;
        attackArea.Monitoring = true;
        animation.Play("Shoot");
        weapon.Play("attack");
        attackTimer = attackCooldown;
    }

    private void OnAttackAreaBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            player.TakeDamage(1, GlobalPosition);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            animation.Play("Hurt");
            AudioManager.Instance.PlaySFX("enemy_death");
        }
        else
        {
            animation.Play("Death");
            AudioManager.Instance.PlaySFX("enemy_death");
            attackArea.Visible = false;
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
        
        if (weapon.Animation == "attack")
        {        
            isAttacking = false;
            attackArea.Monitoring = false;
            weapon.Play("prepare");
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
        // Player left enemy range
        if (body is Player)
        {
            playerOnRange = false;
        }

        
    }
}