using Godot;

public partial class EnemySkull : Node2D
{
    [Export] private int MaxHealth = 3;
    [Export] private float attackCooldown = 2.0f;
    
    private int currentHealth;
    private AnimatedSprite2D animation;
    private Area2D attackArea;
    private AnimatedSprite2D weapon;
    private bool isAttacking = false;
    private float attackTimer = 0f;

    [Signal] public delegate void EnemyDiedEventHandler();

    public override void _Ready()
    {
        currentHealth = MaxHealth;

        animation = GetNode<AnimatedSprite2D>("SkullSprite");
        animation.Play("Idle");
        animation.AnimationFinished += OnAnimationFinished;

        weapon = GetNode<AnimatedSprite2D>("AttackArea/BoneSprite");
        weapon.AnimationFinished += OnAnimationFinished;
        attackArea = GetNode<Area2D>("AttackArea");
        //attackArea.Visible = false;
        attackArea.Monitoring = false;
        attackArea.BodyEntered += OnAttackAreaBodyEntered;
        
        attackTimer = attackCooldown;
    }

    public override void _Process(double delta)
    {
        if (currentHealth <= 0 || isAttacking) return;

        attackTimer -= (float)delta;

        if (attackTimer <= 0)
        {
            Attack();
        }
    }

    private void Attack()
    {
        //attackArea.Visible = true;
        
        isAttacking = true;
        attackArea.Monitoring = true;
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

        if (weapon.Animation == "attack")
        {
            //attackArea.Visible = false;
            
            isAttacking = false;
            attackArea.Monitoring = false;
            animation.Play("Idle");
            weapon.Play("prepare");
        }
    }
}