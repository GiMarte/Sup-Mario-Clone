using Godot;

public partial class Bubble : Area2D
{
	[Export]
	public float Speed = 180.0f;

	[Export]
	public float TrapDuration = 3.0f;

	private Vector2 direction = new Vector2(-1, 0);

	private Node2D trappedPlayer = null;
	private bool isTrapping = false;

	public void SetDirection(Vector2 newDirection)
	{
		direction = newDirection.Normalized();
	}

	public override void _Ready()
	{
		this.BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isTrapping == true)
		{
			if (GodotObject.IsInstanceValid(trappedPlayer) == true)
			{
				this.GlobalPosition = trappedPlayer.GlobalPosition;
			}
		}
		else
		{
			Vector2 currentPosition = this.Position;
			currentPosition.X = currentPosition.X + (direction.X * Speed * (float)delta);
			currentPosition.Y = currentPosition.Y + (direction.Y * Speed * (float)delta);
			this.Position = currentPosition;
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (isTrapping == true)
		{
			return;
		}

		bool isAnyPlayer = body.Name == "Player" || body.Name == "Player1" || body.Name == "Player2" || (body is CharacterBody2D && body.Name != "Octopus");

		if (isAnyPlayer == true)
		{
			isTrapping = true;
			trappedPlayer = body;

			body.SetPhysicsProcess(false);
			this.GlobalPosition = body.GlobalPosition;

			StartBubblePopTimer(body);
		}
		else
		{
			if (body.Name != "Octopus")
			{
				this.QueueFree();
			}
		}
	}

	private async void StartBubblePopTimer(Node2D playerNode)
	{
		SceneTreeTimer timer = GetTree().CreateTimer(TrapDuration);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		if (GodotObject.IsInstanceValid(playerNode) == true)
		{
			playerNode.SetPhysicsProcess(true);
		}

		this.QueueFree();
	}
}
