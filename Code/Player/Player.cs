using Godot;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    // -------------------- MOVIMIENTO --------------------
    [Export] private float SPEED = 175f;
    [Export] private int GRAVITY = 1500;
    [Export] private int FALL_GRAVITY = 1550;
    [Export] private float FRICTION = 6f;
    [Export] private float ACCELERATION = 80f;
    [Export] private float JUMP_FORCE = 400;
    [Export] private float COYOTE_TIME = 0.15f;
    private float coyoteTimer = 0f;
    [Export] private float DASH_SPEED = 450;
    private const float DASH_TIME = 0.2f;
    [Export] public float KNOCKBACK_FORCE = 500f;
    [Export] private float KNOCKBACK_DURATION = 0.4f;
    

    // -------------------- VIDA --------------------
    [Export] public int MaxLives = 3;
    public int CurrentLives { get; private set; }

    // -------------------- ESTADOS --------------------
    private float currentSpeed;
    private bool isInAir = false;
    private bool isDashing;
    private double dashTimer = DASH_TIME;
    private bool isCrouching = false;
    private bool isAttacking = false;
    private bool isForcedAnimation = false;
    private bool blockPlayerControl = false;
    private bool isInvulnerable = false;
    private bool isKnockedBack = false;
    private double knockbackTimer = 0;

    // -------------------- COMPONENTES --------------------
    private AnimatedSprite2D animation;
    private Area2D attackArea;
    private HashSet<Node> hitEnemies = new HashSet<Node>();

    private CollisionShape2D standingCollision;
    private CollisionShape2D crouchingCollision;

    private Area2D hurtbox;
    private CollisionShape2D standingHurtBox;
    private CollisionShape2D crouchingHurtBox;

    // -------------------- SEÑALES --------------------
    [Signal] public delegate void PlayerDiedEventHandler();
    [Signal] public delegate void LivesChangedEventHandler(int newLives);
    [Signal] public delegate void MaskCollectedEventHandler();
    [Signal] public delegate void LevelCompletedEventHandler();

    // ==========================================================
    public override void _Ready()
    {
        animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animation.AnimationFinished += OnAnimationFinished;

        attackArea = GetNode<Area2D>("AttackArea");
        attackArea.Monitoring = false;
        attackArea.AreaEntered += OnAttackAreaAreaEntered;

        hurtbox = GetNode<Area2D>("Hurtbox");
        standingHurtBox = GetNode<CollisionShape2D>("Hurtbox/StandingHurtbox");
        crouchingHurtBox = GetNode<CollisionShape2D>("Hurtbox/CrouchingHurtbox");
        hurtbox.BodyEntered += OnHurtboxBodyEntered;

        standingCollision = GetNode<CollisionShape2D>("StandingCollision");
        crouchingCollision = GetNode<CollisionShape2D>("CrouchingCollision");

        currentSpeed = SPEED;

        CurrentLives = MaxLives;
    }
    // ==========================================================
    public override void _PhysicsProcess(double delta)
    {
        DevTools();

        if (blockPlayerControl) return;

        Vector2 vel = Velocity;

        if (isKnockedBack)
        {
            HandleKnockback(ref vel, delta);
        }
        else if (!isDashing)
        {
            ApplyGravity(ref vel, delta);
            Move(ref vel, delta);
            Jump(ref vel, delta);
            Dash(ref vel, delta);
            Crouch();
            Attack();
        }
        else
        {
            ApplyGravity(ref vel, delta);
            CheckDashing(ref vel, delta);
        }


        Velocity = vel;
        MoveAndSlide();
    }

    // ==========================================================
    private void ApplyGravity(ref Vector2 vel, double delta)
    {
        if (!IsOnFloor()) 
        {
            vel.Y += GetGravity(vel) * (float)delta;
            coyoteTimer -= (float)delta;

        }
        else
        {
            isInAir = false;
            coyoteTimer = COYOTE_TIME;
        }
    }
    private float GetGravity(Vector2 vel)
    {
        return vel.Y < 0 ? GRAVITY : FALL_GRAVITY;
    }


    // ========= MOVIMIENTOS =================================================
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

        if (Input.IsActionJustPressed("jump") && coyoteTimer > 0 && !isCrouching)
        {
            isInAir = true;
            vel.Y = -JUMP_FORCE;
            animation.Play("Jump");
            coyoteTimer = 0;
        }
    }

    private void Dash(ref Vector2 vel, double delta)
    {
        if ((Input.IsActionJustPressed("attack_right") || Input.IsActionJustPressed("attack_left")) && isInAir)
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

    private void Crouch()
    {
        if (Input.IsActionPressed("crouch"))
        {
            isCrouching = true;
            currentSpeed = SPEED * 0.5f;
            animation.Play("Crouch");

            standingCollision.Disabled = isCrouching;
            standingHurtBox.Disabled = isCrouching;
            crouchingCollision.Disabled = !isCrouching;
            crouchingHurtBox.Disabled = !isCrouching;
        }
        else if (Input.IsActionJustReleased("crouch"))
        {
            isCrouching = false;
            currentSpeed = SPEED;

            standingCollision.Disabled = isCrouching;
            standingHurtBox.Disabled = isCrouching;
            crouchingCollision.Disabled = !isCrouching;
            crouchingHurtBox.Disabled = !isCrouching;
        }
    }

    // ========= ATAQUES =================================================
    private void Attack()
    {
        if (isAttacking || isForcedAnimation || isInvulnerable) return;

        if (Input.IsActionJustPressed("attack_right"))
        {
            hitEnemies.Clear();

            isAttacking = true;
            animation.FlipH = false;
            attackArea.RotationDegrees = 0;
            attackArea.Monitoring = true;
            animation.Play("Attack");
        }

        if (Input.IsActionJustPressed("attack_left"))
        {
            hitEnemies.Clear();

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

    // ========= RECIBIR DAÑO =================================================
    public void TakeDamage(int damage, Vector2? sourcePosition = null)
    {
        if (CurrentLives <= 0) return;

        if (!isInvulnerable)
        {
            CurrentLives -= damage;
            EmitSignal(nameof(LivesChanged), CurrentLives);

            animation.Play("Hurt");
            isForcedAnimation = true;
            isInvulnerable = true;
            
            if (CurrentLives <= 0)
            {
                Die();
                return;
            }
        }

        if (sourcePosition != null)
            ApplyKnockback(sourcePosition.Value);
    }

    private void ApplyKnockback(Vector2 sourcePosition)
    {
        Vector2 dir = (GlobalPosition - sourcePosition).Normalized();

        Velocity = dir * KNOCKBACK_FORCE;

        isKnockedBack = true;
        knockbackTimer = KNOCKBACK_DURATION;

        isDashing = false;
        isAttacking = false;
        isCrouching = false;
    }

    private void HandleKnockback(ref Vector2 vel, double delta)
    {
        knockbackTimer -= delta;

        if (!IsOnFloor())
        {
            vel.Y += FALL_GRAVITY * (float)delta;
        }

        vel.X = Mathf.Lerp(vel.X, 0, 3f * (float)delta);

        if (knockbackTimer <= 0)
        {
            isKnockedBack = false;
            vel.X = 0;
        }
    }

    private void OnHurtboxBodyEntered(Node2D body)
    {
        // Ignorar colisiones con el propio player
        if (body == this || body.GetParent() == this) return;

        if (body is TileMapLayer tileMapLayer && tileMapLayer.Name == "Traps")
        {
            Vector2 localPos = tileMapLayer.ToLocal(GlobalPosition);
            Vector2I tileCoords = tileMapLayer.LocalToMap(localPos);
            Vector2 tileWorldPos = tileMapLayer.MapToLocal(tileCoords);
            Vector2 trapPosition = tileMapLayer.ToGlobal(tileWorldPos);

            if (!isInvulnerable)
            {
                TakeDamage(1, trapPosition);
            }
            else
            {
                ApplyKnockback(trapPosition);
            }
            return;
        }

        if (body.IsInGroup("Traps"))
        {
            if (!isInvulnerable)
            {
                TakeDamage(1, body.GlobalPosition);
            }
            else
            {
                ApplyKnockback(body.GlobalPosition);
            }
        }
    }
    // ==========================================================
    private void Die()
    {
        isForcedAnimation = true;
        animation.Play("Death");
        blockPlayerControl = true;
        EmitSignal(nameof(PlayerDied));
    }

    public void PickUpMask()
    {
        isForcedAnimation = true;
        animation.Play("Mask");
        blockPlayerControl = true;
        EmitSignal(nameof(MaskCollected));
    }

    public void CompleteLevel()
    {
        GD.Print("Level Completed");

        isForcedAnimation = true;
        animation.Play("Dance");
        blockPlayerControl = true;
        EmitSignal(nameof(LevelCompleted));
    }

    // ==========================================================
    private void OnAnimationFinished()
    {

        if (animation.Animation == "Mask")
        {
            isForcedAnimation = false;
            blockPlayerControl = false;
        }

        if (animation.Animation == "Attack")
        {
            isAttacking = false;
            attackArea.Monitoring = false;
        }

        if (animation.Animation == "Jump" && isInAir && !isAttacking && !isCrouching)
        {
            animation.Play("Fall");
        }

        if (animation.Animation == "Hurt")
        {
            isForcedAnimation = false;
            isInvulnerable = false;
        }

        if (animation.Animation == "Death")
        {
            CallDeferred("ResetToMainMenu");
        }
        
        if (animation.Animation == "Dance")
        {
            CallDeferred("ResetToMainMenu");
        }
    }

    // ========== DEV TOOLS================================================
    private void DevTools()
    {
        if (Input.IsActionJustPressed("reset"))
            CallDeferred("ResetToMainMenu");
    }

    private void ResetToMainMenu()
    {
        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }
}