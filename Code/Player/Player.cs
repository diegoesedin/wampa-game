using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private const int SPEED = 2000;
	private const int GRAVITY = 1000;
	private const int FALL_GRAVITY = 2500;
	private const float FRICTION = .01f;
	private const float ACCELERATION = 1.5f;
	private const float JUMP_FORCE = 1000;
	private const float DASH_SPEED = 2100;
	private const float DASH_TIME = .2f;

	private bool isDashing;
	private double dashTimer = DASH_TIME;
	
	public override void _Ready()
	{
		
	}

	public override void _Process(double delta)
	{
		Vector2 vel = new Vector2(0, 0);
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
			vel.X = Mathf.Lerp(vel.X, direction * SPEED, ACCELERATION * (float)delta);
		else
			vel.X = Mathf.Lerp(vel.X, 0, FRICTION * (float)delta);
	}

	private void Jump(ref Vector2 vel, double delta)
	{
		if (Input.IsActionJustReleased("jump") && vel.Y < 0)
		{
			vel.Y = -JUMP_FORCE / 4;
		}
		
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			vel.Y = -JUMP_FORCE;
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
