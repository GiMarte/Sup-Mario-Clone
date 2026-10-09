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
		timer = GetNode<Timer>("Timer");

		if (timer != null)
		{timer.Timeout += OnTimerTimeout;}
	}

	public override void _PhysicsProcess(double delta)
	{ Vector2 currentVelocity = Vector2.Zero;

		if (IsOnFloor() == false)
		{ currentVelocity.Y = this.Velocity.Y + (Gravity * (float)delta);}
		else
		{currentVelocity.Y = 0.0f;}
		currentVelocity.X = movementDirection.X * Speed;
		this.Velocity = currentVelocity;

		MoveAndSlide();

		if (IsOnWall() == true)
		{ movementDirection.X = movementDirection.X * -1.0f; }
	}

	private void OnTimerTimeout()
	{
		if (gun != null)
		{ if (gun.IsStolen == false)
			{gun.Shoot();}
		}
	}
}
