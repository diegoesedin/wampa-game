using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private const int SPEED = 2000;
	private const int GRAVITY = 2000;
	private const float FRICTION = .01f;
	private const float ACCELERATION = 1.5f;
	private const float JUMP_FORCE = 500;
	
	public override void _Ready()
	{
		
	}

	public override void _Process(double delta)
	{
		Vector2 vel = new Vector2(0, 0);
		int direction = 0;
		if (Input.IsActionPressed("ui_right"))
			direction += 1;
		else if (Input.IsActionPressed("ui_left"))
			direction -= 1;
		
		if (direction != 0)
			vel.X = Mathf.Lerp(vel.X, direction * SPEED, ACCELERATION * (float)delta);
		else
			vel.X = Mathf.Lerp(vel.X, 0, FRICTION * (float)delta);

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			vel.Y -= JUMP_FORCE;
		}
		vel.Y += GRAVITY * (float)delta;

		Velocity = vel;
		MoveAndSlide();
	}
}
