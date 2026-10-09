using Godot;

public partial class Gun : Area2D
{
	[Export]
	public PackedScene BubbleScene;

	public bool IsStolen = false;

	private Node2D shootPoint;

	public override void _Ready()
	{
		shootPoint = GetNode<Node2D>("ShootPoint");
		this.BodyEntered += OnBodyEntered;
	}

	public void Shoot()
	{
		if (BubbleScene != null)
		{
			if (shootPoint != null)
			{
				Bubble newBubble = BubbleScene.Instantiate<Bubble>();
				newBubble.GlobalPosition = shootPoint.GlobalPosition;

				Vector2 shootDirection = new Vector2(-1, 0);
				newBubble.SetDirection(shootDirection);

				GetTree().CurrentScene.AddChild(newBubble);
			}
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (IsStolen == false)
		{
			bool isAnyPlayer = body.Name == "Player" || body.Name == "Player1" || body.Name == "Player2" || (body is CharacterBody2D && body.Name != "polipetto");

			if (isAnyPlayer == true)
			{
				IsStolen = true;
				CallDeferred(MethodName.Reparent, body);
				//this.Position = new Vector2(10, 0);
			}
		}
	}
}
