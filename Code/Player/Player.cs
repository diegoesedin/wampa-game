using Godot;

public partial class Player : CharacterBody2D
{

	[Export] private int SPEED = 2000;
    [Export] private int GRAVITY = 1000;
    [Export] private int FALL_GRAVITY = 2500;
    [Export] private float FRICTION = .01f;
	[Export] private float ACCELERATION = 1.5f;
	[Export] private float JUMP_FORCE = 1600;
	[Export] private float DASH_SPEED = 2100;
	private const float DASH_TIME = .2f;

	private bool isInAir = false;
	private bool isDashing;
	private double dashTimer = DASH_TIME;

	private AnimatedSprite2D animation;

	public override void _Ready()
	{
		animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 vel = Vector2.Zero;
		if (!isDashing)
		{
			ApplyGravity(ref vel, delta);
			
			Move(ref vel, delta);
		}
		
		Jump(ref vel, delta);
		
		Dash(ref vel, delta);
		
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
		if (Input.IsActionPressed("ui_right"))
			direction += 1;
		else if (Input.IsActionPressed("ui_left"))
			direction -= 1;
		
		if (direction != 0)
		{
			vel.X = Mathf.Lerp(vel.X, direction * SPEED, ACCELERATION * (float)delta);
			animation.FlipH = direction < 0;
			if (!isInAir)
				animation.Play("Walk");
		}
		else
		{
			vel.X = Mathf.Lerp(vel.X, 0, FRICTION * (float)delta);
			if (!isInAir)
				animation.Play("Idle");
		}
			
	}

	private void Jump(ref Vector2 vel, double delta)
	{
		if (Input.IsActionJustReleased("jump") && vel.Y < 0)
		{
			vel.Y = -JUMP_FORCE / 4;
		}
		
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			isInAir = true;
			vel.Y = -JUMP_FORCE;
			animation.Play("Jump");
		}
	}

	private void Dash(ref Vector2 vel, double delta)
	{
		if (Input.IsActionJustPressed("dash"))
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
				vel = Vector2.Zero;
				dashTimer = DASH_TIME;
			}
		}
	}
}
