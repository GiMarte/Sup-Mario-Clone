using Godot;

public partial class Gun : area2D
{
	[Export]
	public PackedScene BubbleScene;
	public bool IsStolen = false;
	private Node2D shootpoint;
	
	public override void _Ready()
	{	shootPoint = getNode<Node2D>("ShootPoint");
		this.BodyEntered += OnBodyEntered;
	}
	
	public void Shoot()
	{
		if(BubbleScene != null)
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
if (IsStolen == false)
{ bool isAnyPlayer = body.name == "Player || body.Name" || body.Nae == "Player2" || (body is CharacterBody && body.Name != "polipetto");
if (isAnyPlayer == true)
{ IsStolen = true;
CallDeferred(MethodName.Reparent, body);
this.Position = new Vector2(10, 0);}
