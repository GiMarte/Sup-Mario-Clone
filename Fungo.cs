using Godot;

public partial class Fungo : CharacterBody2D
{
	[Export]
	public float Velocita { get; set; } = 200f;

	private float _direzione = 1;

	public override void _Ready()
	{
		SetPhysicsProcess(false);
		GetNode<Area2D>("Raccolta").BodyEntered += QuandoToccaPersonaggio;
	}

	public void Avvia()
	{
		GetNode<Area2D>("Raccolta").SetDeferred("monitoring", true);
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
	[Export]
	public Player.TipoPotere Potere { get; set; }
	= Player.TipoPotere.Ghiaccio;

	private bool _raccolto;

	private void QuandoToccaPersonaggio(Node2D corpo)
	{
		if (_raccolto || corpo is not Player personaggio)
		{
			return;
		}

		_raccolto = true;
		personaggio.RiceviPotere(Potere);
		QueueFree();
	}
}