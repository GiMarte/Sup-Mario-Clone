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
			GetNode<AudioStreamPlayer>("JumpSound").Play();
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

		bool wasMovingUp = velocity.Y < 0;
		Velocity = velocity;
		MoveAndSlide();

		if (wasMovingUp)
		{
			for (int i = 0; i < GetSlideCollisionCount(); i++)
			{
				var collision = GetSlideCollision(i);
				// The underside of the block has a downward-facing normal.
				if (collision.GetNormal().Y > 0.5f && collision.GetCollider() is Block block)
				{
					block.Hit();
				}
			}
		}
	}

	public enum PowerType { None = 0, Ice = 1, Fire = 2 }

	public PowerType CurrentPower { get; private set; }
		= PowerType.None;

	public void ReceivePower(PowerType power)
	{
		CurrentPower = power;
		GD.Print($"Power collected: {CurrentPower}");
	}
}
