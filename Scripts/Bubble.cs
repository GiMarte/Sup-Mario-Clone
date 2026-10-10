using Godot;

public partial class Bubble : Area2D
{
	[Export]
	public float Speed = 100.0f;

	[Export]
	public float TrapDuration = 3.0f;

	[Export]
	public Vector2 CenterOffset = new Vector2(0, 0);

	private bool isTrapping = false;
	private Node2D trappedPlayer = null;
	private float trapTimer = 0.0f;

	public override void _Ready()
	{
		this.BodyEntered += OnBodyEntered;
		Modulate = new Color(1, 1, 1, 1.0f);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!isTrapping)
		{
			Position += Vector2.Up * Speed * (float)delta;
		}
		else if (trappedPlayer != null)
		{
			trappedPlayer.GlobalPosition = GlobalPosition + CenterOffset;

			trapTimer += (float)delta;
			if (trapTimer >= TrapDuration)
			{
				if (trappedPlayer is CharacterBody2D playerBody)
				{
					playerBody.SetProcess(true);
					playerBody.SetPhysicsProcess(true);
				}
				QueueFree();
			}
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!isTrapping && body.Name == "Player")
		{
			isTrapping = true;
			trappedPlayer = body;
			Modulate = new Color(1, 1, 1, 0.5f);

			if (body is CharacterBody2D playerBody)
			{
				playerBody.SetPhysicsProcess(false);
			}
		}
	}
}
