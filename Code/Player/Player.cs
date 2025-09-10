using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public override void _Ready()
	{
		
	}

	public override void _Process(double delta)
	{
		Vector2 vel = new Vector2(0, 0);
		if (Input.IsActionPressed("ui_right"))
		{
			vel.X += 50;
		}
		else if (Input.IsActionPressed("ui_left"))
		{
			vel.X -= 50;
		}

		if (Input.IsActionJustPressed("jump"))
		{
			vel.Y -= 2000;
		}
		vel.Y += 400 * (float)delta;

		Velocity = vel;
		MoveAndSlide();
	}
}
