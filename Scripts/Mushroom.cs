using Godot;

public partial class Mushroom : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 200f;

	private float _direction = 1;

	public override void _Ready()
	{
		SetPhysicsProcess(false);
		GetNode<Area2D>("PickupArea").BodyEntered += OnBodyEntered;
	}

	public void Activate()
	{
		GetNode<Area2D>("PickupArea").SetDeferred("monitoring", true);
		GetNode<CollisionShape2D>("CollisionShape2D")
			.SetDeferred("disabled", false);

		ZIndex = 0;
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		velocity.X = Speed * _direction;
		Velocity = velocity;

		MoveAndSlide();

		if (IsOnWall())
		{
			_direction *= -1;
		}
	}
	[Export]
	public Player.PowerType Power { get; set; }
	= Player.PowerType.Ice;

	private bool _collected;

	private void OnBodyEntered(Node2D body)
	{
		if (_collected || body is not Player player)
		{
			return;
		}

		_collected = true;
		player.ReceivePower(Power);
		QueueFree();
	}
}