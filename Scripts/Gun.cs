using Godot;

public partial class Gun : Area2D
{
	[Export]
	public PackedScene BubbleScene;

	public bool IsStolen = false;
	private Node2D shootPoint;
	
	public override void _Ready()
	{
		shootPoint = GetNode<Node2D>("GunSprite/ShootPoint");
		this.BodyEntered += OnBodyEntered;
	}

	public void Shoot()
	{
		if (BubbleScene != null && shootPoint != null)
		{
			Bubble newBubble = BubbleScene.Instantiate<Bubble>();
			Vector2 shootDirection = new Vector2(-1, 0);
			newBubble.SetDirection(shootDirection);
			GetTree().CurrentScene.AddChild(newBubble);
			newBubble.GlobalPosition = shootPoint.GlobalPosition;
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (IsStolen == false)
		{
			bool isAnyPlayer = body.Name == "Player" || body.Name == "Player1" || body.Name == "Player2"
				|| (body is CharacterBody2D && body.Name != "Octopus");
			if (isAnyPlayer == true)
			{
				IsStolen = true;
				Callable.From(() =>
				{
					Reparent(body, false);
					Position = new Vector2(10, 0);
				}).CallDeferred();
			}
		}
	}
}
