using Godot;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -600.0f;
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		float direction = Input.GetAxis("move_left", "move_right");
		if (direction != 0)
		{
			velocity.X = direction * Speed;
			GetNode<Sprite2D>("Sprite2D").FlipH = direction < 0;
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
