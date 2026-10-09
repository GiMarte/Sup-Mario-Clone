using Godot;

public partial class Block : StaticBody2D
{
    [Export]
    public Texture2D UsedTexture { get; set; }
    public bool IsUsed { get; private set; }
    [Export]
    public PackedScene RewardScene { get; set; }

    public bool Hit()
    {
        if (IsUsed)
        {
            return false;
        }

        IsUsed = true;
        GetNode<Sprite2D>("Sprite2D").Texture = UsedTexture;
        if (RewardScene != null)
        {
            var reward = RewardScene.Instantiate<Node2D>();
            GetParent().AddChild(reward);

            reward.GlobalPosition = GlobalPosition;
            reward.ZIndex = -1;

            var tween = reward.CreateTween();

            tween.TweenProperty(
                reward,
                "global_position",
                GlobalPosition + new Vector2(0, -40),
                0.25
            );

            if (reward is Mushroom mushroom)
            {
                tween.TweenCallback(Callable.From(mushroom.Activate));
            }
        }

        return true;

    }
}
