using Godot;

public partial class Fungo : CharacterBody2D
{
	[Export]
	public float Velocita { get; set; } = 200f;

	private float _direzione = 1;

	public override void _Ready()
	{
		SetPhysicsProcess(false);
	}

	public void Avvia()
	{
		GetNode<CollisionShape2D>("CollisionShape2D")
			.SetDeferred("disabled", false);

		ZIndex = 0;
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocita = Velocity;

		if (!IsOnFloor())
		{
			velocita += GetGravity() * (float)delta;
		}

		velocita.X = Velocita * _direzione;
		Velocity = velocita;

		MoveAndSlide();

		if (IsOnWall())
		{
			_direzione *= -1;
		}
	}
}