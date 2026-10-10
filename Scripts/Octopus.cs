using Godot;

public partial class Octopus : CharacterBody2D
{
	[Export]
	public float Speed = 60.0f;

	[Export]
	public float Gravity = 980.0f;

	private Vector2 movementDirection = new Vector2(-1, 0);
	private Gun gun;
	private Timer timer;

	public override void _Ready()
	{
		gun = GetNodeOrNull<Gun>("Gun");
		timer = GetNodeOrNull<Timer>("Timer");

		if (timer != null)
		{
			timer.Timeout += OnTimerTimeout;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 currentVelocity = Vector2.Zero;

		if (IsOnFloor() == false)
		{
			currentVelocity.Y = this.Velocity.Y + (Gravity * (float)delta);
		}
		else
		{
			currentVelocity.Y = 0.0f;
		}

		currentVelocity.X = movementDirection.X * Speed;
		this.Velocity = currentVelocity;

		MoveAndSlide();

		if (IsOnWall())
		{
			FlipDirection();
		}

		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision2D collision = GetSlideCollision(i);
			Node collider = (Node)collision.GetCollider();

			if (collider != null && collider.Name == "Player")
			{
				FlipDirection();
				break;
			}
		}
	}

	private void FlipDirection()
	{
		movementDirection.X *= -1.0f;
		Scale = new Vector2(-Scale.X, Scale.Y);
	}

	private void OnTimerTimeout()
	{
		if (gun != null)
		{
			gun.Shoot();
		}
	}
}
