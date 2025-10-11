using Godot;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    [Export] private float SPEED = 175f;
    [Export] private int GRAVITY = 1500;
    [Export] private int FALL_GRAVITY = 1550;
    [Export] private float FRICTION = 6f;
    [Export] private float ACCELERATION = 80f;
    [Export] private float JUMP_FORCE = 400;
    [Export] private float DASH_SPEED = 450;
    private const float DASH_TIME = 0.2f;

    [Export] public int MaxLives = 3;
    public int CurrentLives { get; private set; }

    private float currentSpeed;

    private bool isInAir = false;
    private bool isDashing;
    private double dashTimer = DASH_TIME;
    private bool isCrouching = false;
    private bool isAttacking = false;
    private bool isForcedAnimation = false;

    [Export] private CollisionShape2D standigCollision;
    [Export] private CollisionShape2D crouchingCollision;

    private AnimatedSprite2D animation;
    private Area2D attackArea;
    private HashSet<Node> hitEnemies = new HashSet<Node>();

    [Signal] public delegate void PlayerDiedEventHandler();
    [Signal] public delegate void LivesChangedEventHandler(int newLives);
    [Signal] public delegate void MaskCollectedEventHandler();

    public override void _Ready()
    {
        standigCollision.Disabled = false;
        crouchingCollision.Disabled = true;

        animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animation.AnimationFinished += OnAnimationFinished;

        attackArea = GetNode<Area2D>("AttackArea");
        attackArea.Monitoring = false;
        attackArea.AreaEntered += OnAttackAreaAreaEntered;

        currentSpeed = SPEED;
        CurrentLives = MaxLives;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isForcedAnimation) return;

        Vector2 vel = Velocity;

        if (!isDashing)
        {
            ApplyGravity(ref vel, delta);
            Move(ref vel, delta);
        }

        Jump(ref vel, delta);
        Dash(ref vel, delta);
        Crouch();
        Attack();


        standigCollision.Disabled = isCrouching;
        crouchingCollision.Disabled = !isCrouching;


        DevTools();

        Velocity = vel;
        MoveAndSlide();
    }

    private void ApplyGravity(ref Vector2 vel, double delta)
    {
        if (!IsOnFloor())
            vel.Y += GetGravity(vel) * (float)delta;
        else
            isInAir = false;
    }

    private float GetGravity(Vector2 vel)
    {
        return vel.Y < 0 ? GRAVITY : FALL_GRAVITY;
    }

    private void Move(ref Vector2 vel, double delta)
    {
        int direction = 0;
        if (Input.IsActionPressed("ui_right")) direction += 1;
        if (Input.IsActionPressed("ui_left")) direction -= 1;

        if (direction != 0)
        {
            vel.X = Mathf.Lerp(vel.X, direction * currentSpeed, ACCELERATION * (float)delta);
            if (!isInAir && !isCrouching && !isAttacking)
                animation.Play("Walk");
            animation.FlipH = direction < 0;
        }
        else
        {
            vel.X = Mathf.Lerp(vel.X, 0, FRICTION * (float)delta);
            if (!isInAir && !isCrouching && !isAttacking)
                animation.Play("Idle");
        }
    }

    private void Jump(ref Vector2 vel, double delta)
    {
        if (Input.IsActionJustReleased("jump") && vel.Y < 0)
            vel.Y = -JUMP_FORCE / 4;

        if (Input.IsActionJustPressed("jump") && IsOnFloor() && !isCrouching)
        {
            isInAir = true;
            vel.Y = -JUMP_FORCE;
            animation.Play("Jump");
        }
    }

    private void Crouch()
    {
        if (Input.IsActionPressed("crouch"))
        {
            isCrouching = true;
            currentSpeed = SPEED * 0.5f;
            animation.Play("Crouch");
        }
        else if (Input.IsActionJustReleased("crouch"))
        {
            isCrouching = false;
            currentSpeed = SPEED;
        }
    }

    private void Attack()
    {
        if (isAttacking || isForcedAnimation) return;

        hitEnemies.Clear();

        if (Input.IsActionJustPressed("attack_right"))
        {
            isAttacking = true;
            animation.FlipH = false;
            attackArea.RotationDegrees = 0;
            attackArea.Monitoring = true;
            animation.Play("Attack");
        }

        if (Input.IsActionJustPressed("attack_left"))
        {
            isAttacking = true;
            animation.FlipH = true;
            attackArea.RotationDegrees = 180;
            attackArea.Monitoring = true;
            animation.Play("Attack");
        }
    }

    private void OnAttackAreaAreaEntered(Area2D area)
    {
        Node enemy = area.GetParent();
        if (enemy != null && enemy.IsInGroup("Enemy"))
        {
            if (!hitEnemies.Contains(enemy))
            {
                enemy.Call("TakeDamage", 1);
                hitEnemies.Add(enemy);
            }
        }
    }

    public void PickUpMask()
    {
        if (animation == null) return;

        isForcedAnimation = true;
        animation.Stop();
        animation.Play("Mask");
        EmitSignal(nameof(MaskCollected));
    }

    private void Dash(ref Vector2 vel, double delta)
    {
        if ( (Input.IsActionJustPressed("attack_right") || Input.IsActionJustPressed("attack_left") ) && isInAir)
        {
            if (Input.IsActionPressed("ui_right"))
            {
                vel.X = DASH_SPEED;
                isDashing = true;
            }
            if (Input.IsActionPressed("ui_left"))
            {
                vel.X = -DASH_SPEED;
                isDashing = true;
            }
            dashTimer = DASH_TIME;
        }
        CheckDashing(ref vel, delta);
    }

    private void CheckDashing(ref Vector2 vel, double delta)
    {
        if (isDashing)
        {
            dashTimer -= delta;
            if (dashTimer <= 0)
            {
                isDashing = false;
                vel.X = 0;
                dashTimer = DASH_TIME;
            }
        }
    }

    private void OnAnimationFinished()
    {
        if (animation.Animation == "Mask")
            isForcedAnimation = false;

        if (animation.Animation == "Attack")
        {
            isAttacking = false;
            attackArea.Monitoring = false;
        }

        if (animation.Animation == "Jump" && isInAir && !isAttacking && !isCrouching)
            animation.Play("Fall");
    }

    public void TakeDamage(int damage)
    {
        if (CurrentLives <= 0) return;

        CurrentLives -= damage;
        EmitSignal(nameof(LivesChanged), CurrentLives);

        if (CurrentLives <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animation.Play("Death");
        isForcedAnimation = true;
        EmitSignal(nameof(PlayerDied));
    }

    private void DevTools()
    {
        if (Input.IsActionJustPressed("reset")) CallDeferred("ResetToMainMenu"); // para que deje terminar el frame actual al apretar "reset" 
    }

    private void ResetToMainMenu()
    {
        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }
}
