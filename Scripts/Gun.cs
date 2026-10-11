using Godot;

public partial class Gun : Area2D
{
	[Export]
	public PackedScene BubbleScene;

	private Node2D shootPoint;

	public override void _Ready()
	{
		shootPoint = GetNode<Node2D>("ShootPoint");
	}

	public void Shoot()
	{
		if (BubbleScene != null && shootPoint != null)
		{
			Bubble newBubble = BubbleScene.Instantiate<Bubble>();
			newBubble.GlobalPosition = shootPoint.GlobalPosition;
			GetTree().CurrentScene.AddChild(newBubble);
			GetNode<AudioStreamPlayer>("BubbleSound").Play();
		}
	}
}
