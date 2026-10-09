using Godot;

public partial class Coin : Node2D
{
	public override void _Ready()
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		Vector2 initialScale = sprite.Scale;

		var tween = CreateTween();

		tween.TweenMethod(
			Callable.From<float>(angle =>
				sprite.Scale = new Vector2(
					initialScale.X * Mathf.Cos(angle),
					initialScale.Y)),
			0f,
			Mathf.Pi,
			0.25
		);

		tween.TweenInterval(0.25);
		tween.TweenCallback(Callable.From(QueueFree));
	}
}
