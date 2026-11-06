using Godot;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -600.0f;
	private Sprite2D _sprite;
	private CollisionShape2D _collider;
	private RectangleShape2D _shape;
	private ShaderMaterial _powerMaterial;
	private Vector2 _baseShapeSize;
	private Vector2 _baseColliderPosition;
	private Vector2 _baseSpritePosition;
	private Vector2 _baseSpriteScale;
	private bool _grown;
	private int _facing = 1;
	private float _shotCooldown;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_collider = GetNode<CollisionShape2D>("CollisionShape2D");
		_shape = (RectangleShape2D)_collider.Shape.Duplicate();
		_collider.Shape = _shape;
		_baseShapeSize = _shape.Size;
		_baseColliderPosition = _collider.Position;
		_baseSpritePosition = _sprite.Position;
		_baseSpriteScale = _sprite.Scale;
		_facing = _sprite.FlipH ? -1 : 1;
		_powerMaterial = new ShaderMaterial { Shader = GD.Load<Shader>("res://Assets/PlayerPower.gdshader") };
		_sprite.Material = _powerMaterial;
	}

	public override void _PhysicsProcess(double delta)
	{
		_shotCooldown = Mathf.Max(0, _shotCooldown - (float)delta);
		if (CurrentPower != PowerType.None && !_grown) TryGrow();
		Vector2 velocity = Velocity;
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			GetNode<AudioStreamPlayer>("JumpSound").Play();
		}

		float direction = Input.GetAxis("move_left", "move_right");
		if (direction != 0)
		{
			velocity.X = direction * Speed;
			_facing = direction < 0 ? -1 : 1;
			_sprite.FlipH = direction < 0;
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);
		}

		bool wasMovingUp = velocity.Y < 0;
		Velocity = velocity;
		MoveAndSlide();

		if (wasMovingUp)
		{
			for (int i = 0; i < GetSlideCollisionCount(); i++)
			{
				var collision = GetSlideCollision(i);
				// The underside of the block has a downward-facing normal.
				if (collision.GetNormal().Y > 0.5f && collision.GetCollider() is Block block)
				{
					block.Hit();
				}
			}
		}
		if (Input.IsActionJustPressed("shoot") && CurrentPower != PowerType.None && _shotCooldown <= 0)
		{
			var ball = new PowerBall
			{
				Name = CurrentPower == PowerType.Fire ? "FireBall" : "IceBall",
				Power = CurrentPower,
				Direction = _facing
			};
			GetParent().AddChild(ball, true);
			ball.GlobalPosition = _collider.GlobalPosition + new Vector2(_facing * 24, -2);
			_shotCooldown = 0.25f;
		}
	}

	public enum PowerType { None = 0, Ice = 1, Fire = 2 }

	public PowerType CurrentPower { get; private set; }
		= PowerType.None;

	public void ReceivePower(PowerType power)
	{
		CurrentPower = power;
		// Pickup areas signal while physics queries are flushing.
		Callable.From(ApplyPowerAppearance).CallDeferred();
		GD.Print(power == PowerType.None ? "Power cleared." : $"Power collected: {CurrentPower}");
	}

	private void ApplyPowerAppearance()
	{
		_powerMaterial.SetShaderParameter("power_type", (int)CurrentPower);
		if (CurrentPower == PowerType.None)
		{
			_shape.Size = _baseShapeSize;
			_collider.Position = _baseColliderPosition;
			_sprite.Position = _baseSpritePosition;
			_sprite.Scale = _baseSpriteScale;
			_grown = false;
		}
		else TryGrow();
	}

	private void TryGrow()
	{
		if (_grown) return;
		float growth = _baseShapeSize.Y * 0.25f;
		if (TestMove(GlobalTransform, Vector2.Up * growth)) return;
		_shape.Size = new Vector2(_baseShapeSize.X, _baseShapeSize.Y + growth);
		_collider.Position = _baseColliderPosition + Vector2.Up * growth / 2;
		_sprite.Position = _baseSpritePosition + Vector2.Up * growth / 2;
		_sprite.Scale = _baseSpriteScale * new Vector2(1, 1.25f);
		_grown = true;
	}
}
