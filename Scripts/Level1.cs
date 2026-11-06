using Godot;
using System.Collections.Generic;

public partial class Level1 : Node2D
{
    private const float WorldWidth = 7168;
    private const float FinishX = 6352;
    private Player _player;
    private Camera2D _camera;
    private Vector2 _spawn;
    private Node2D _coins;
    private Label _coinCount;
    private Label _score;
    private Label _time;
    private Label _power;
    private Control _victory;
    private AudioStreamPlayer _coinSound;
    private readonly List<Block> _coinBlocks = new();
    private float _remaining = 400;
    private float _animationTime;
    private int _collected;
    private int _deaths;
    private bool _completed;

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _spawn = _player.Position;
        _camera = GetNode<Camera2D>("Camera2D");
        _coins = GetNode<Node2D>("Coins");
        _coinCount = GetNode<Label>("HUD/CoinCount");
        _score = GetNode<Label>("HUD/Score");
        _time = GetNode<Label>("HUD/Time");
        _power = GetNode<Label>("HUD/Power");
        _victory = GetNode<Control>("HUD/Victory");
        _coinSound = GetNode<AudioStreamPlayer>("CoinSound");
        foreach (Area2D coin in _coins.GetChildren())
        {
            coin.BodyEntered += body =>
            {
                if (body != _player || !coin.Visible || _completed) return;
                coin.Hide();
                coin.SetDeferred("monitoring", false);
                _collected++;
                _coinSound.Play();
            };
        }
        foreach (Block block in GetNode("Blocks").GetChildren())
        {
            if (block.RewardScene?.ResourcePath == "res://Scenes/Coin.tscn")
                _coinBlocks.Add(block);
        }
        GetNode<Area2D>("Finish").BodyEntered += body =>
        {
            if (body == _player && !_completed) CompleteLevel();
        };
        UpdateCamera(true);
    }

    public override void _Process(double delta)
    {
        if (!_completed)
        {
            _remaining = Mathf.Max(0, _remaining - (float)delta);
            _animationTime += (float)delta;
            foreach (Area2D coin in _coins.GetChildren())
            {
                if (coin.Visible)
                    coin.GetNode<Sprite2D>("Sprite2D").Scale = new Vector2(
                        1.5f * Mathf.Max(0.16f, Mathf.Abs(Mathf.Cos(_animationTime * 4 + coin.Position.X * 0.01f))), 1.5f);
            }
            for (int index = _coinBlocks.Count - 1; index >= 0; index--)
            {
                if (!_coinBlocks[index].IsUsed) continue;
                _collected++;
                _coinBlocks.RemoveAt(index);
            }
        }
        _coinCount.Text = $"MONETE\nx{_collected:00}";
        _score.Text = $"LUIGI\n{_collected * 100:000000}";
        _time.Text = $"TEMPO\n{Mathf.CeilToInt(_remaining):000}";
        _power.Text = _player.CurrentPower switch
        {
            Player.PowerType.Fire => "POTERE FUOCO",
            Player.PowerType.Ice => "POTERE GHIACCIO",
            _ => ""
        };
        UpdateCamera(false);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_completed || (_player.Position.Y <= 880 && _remaining > 0)) return;
        ClearBubbles();
        _player.Position = _spawn;
        _player.Velocity = Vector2.Zero;
        _player.SetProcess(true);
        _player.SetPhysicsProcess(true);
        _remaining = 400;
        _deaths++;
        UpdateCamera(true);
    }

    public override void _UnhandledKeyInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.R)
            GetTree().ReloadCurrentScene();
    }

    private void UpdateCamera(bool snap)
    {
        float halfWidth = GetViewportRect().Size.X / (_camera.Zoom.X * 2);
        _camera.Position = new Vector2(Mathf.Clamp(_player.Position.X + 110, halfWidth, WorldWidth - halfWidth), 432);
        if (snap) _camera.ResetSmoothing();
    }

    private void ClearBubbles()
    {
        foreach (Node child in GetChildren())
        {
            if (child is not Bubble bubble) continue;
            bubble.SetPhysicsProcess(false);
            bubble.QueueFree();
        }
    }

    private void CompleteLevel()
    {
        _completed = true;
        ClearBubbles();
        _player.SetPhysicsProcess(false);
        _player.Velocity = Vector2.Zero;
        _player.GetNode<Sprite2D>("Sprite2D").FlipH = false;
        GetNode("Enemies").ProcessMode = ProcessModeEnum.Disabled;
        GetNode<Label>("HUD/Victory/Result").Text =
            $"MONETE  {_collected:00}     TEMPO  {Mathf.CeilToInt(_remaining):000}\nTENTATIVI  {_deaths + 1}";
        var tween = CreateTween();
        tween.TweenProperty(GetNode("Flag/Pennant"), "position:y", -64, 0.7);
        tween.Parallel().TweenProperty(_player, "position", new Vector2(FinishX - 15, 549), 0.7);
        tween.TweenProperty(_player, "position", new Vector2(6553, 581), 1.2);
        tween.TweenCallback(Callable.From(() =>
        {
            _player.Hide();
            _victory.Show();
        }));
    }
}
