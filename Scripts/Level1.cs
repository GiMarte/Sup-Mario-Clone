using Godot;
using System.Collections.Generic;

public partial class Level1 : Node2D
{
    private const float WorldWidth = 7168;
    private const float FinishX = 6352;
    private const float EnemyHitRecoverySeconds = 1.0f;
    private Player _player;
    private Camera2D _camera;
    private Vector2 _spawn;
    private Node2D _coins;
    private Label _coinCount;
    private Label _score;
    private Label _time;
    private Label _power;
    private Control _victory;
    private Control _gameOver;
    private Button _retryButton;
    private AudioStreamPlayer _coinSound;
    private readonly List<Block> _coinBlocks = new();
    private float _remaining = 400;
    private float _animationTime;
    private float _enemyHitRecovery;
    private int _collected;
    private int _deaths;
    private bool _finished;

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
        _gameOver = GetNode<Control>("HUD/GameOver");
        _retryButton = GetNode<Button>("%RetryButton");
        _retryButton.Pressed += RestartLevel;
        GetNode<Button>("%LevelMenuButton").Pressed += () =>
            GetTree().ChangeSceneToFile("res://Scenes/LevelSelect.tscn");
        _coinSound = GetNode<AudioStreamPlayer>("CoinSound");
        var enemies = GetNode("Enemies");
        foreach (Node enemy in enemies.GetChildren())
            ConnectEnemyContact(enemy);
        enemies.ChildEnteredTree += ConnectEnemyContact;
        foreach (Area2D coin in _coins.GetChildren())
        {
            coin.BodyEntered += body =>
            {
                if (body != _player || !coin.Visible || _finished) return;
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
            if (body == _player && !_finished) CompleteLevel();
        };
        UpdateCamera(true);
    }

    public override void _Process(double delta)
    {
        if (!_finished)
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
        if (_finished) return;
        _enemyHitRecovery = Mathf.Max(0, _enemyHitRecovery - (float)delta);
        if (_player.Position.Y <= 880 && _remaining > 0) return;
        ClearProjectiles();
        _player.Position = _spawn;
        _player.Velocity = Vector2.Zero;
        _player.ReceivePower(Player.PowerType.None);
        _player.SetProcess(true);
        _player.SetPhysicsProcess(true);
        _remaining = 400;
        _enemyHitRecovery = 0;
        _deaths++;
        UpdateCamera(true);
    }

    public override void _UnhandledKeyInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.R)
            RestartLevel();
    }

    private void ConnectEnemyContact(Node enemy)
    {
        if (enemy.GetNodeOrNull<Area2D>("ContactArea") is Area2D contact)
            contact.BodyEntered += body => OnEnemyContact(enemy, body);
    }

    private void OnEnemyContact(Node enemy, Node2D body)
    {
        if (body != _player || _finished || !CanProcess() || enemy.IsQueuedForDeletion()) return;
        if (_player.IsLandingOn(enemy))
        {
            enemy.QueueFree();
            return;
        }
        if (_enemyHitRecovery > 0) return;

        if (_player.CurrentPower != Player.PowerType.None)
        {
            _enemyHitRecovery = EnemyHitRecoverySeconds;
            _player.ReceivePower(Player.PowerType.None);
            return;
        }

        _finished = true;
        // Disabling the level also disables its collision bodies, so wait for physics to finish.
        Callable.From(ShowGameOver).CallDeferred();
    }

    private void ShowGameOver()
    {
        _player.Velocity = Vector2.Zero;
        _player.Hide();
        ProcessMode = ProcessModeEnum.Disabled;
        _gameOver.Show();
        _retryButton.GrabFocus();
    }

    private void RestartLevel() => GetTree().ReloadCurrentScene();

    private void UpdateCamera(bool snap)
    {
        float halfWidth = GetViewportRect().Size.X / (_camera.Zoom.X * 2);
        float cameraY = Mathf.Min(432, _player.Position.Y + 160);
        _camera.Position = new Vector2(Mathf.Clamp(_player.Position.X + 110, halfWidth, WorldWidth - halfWidth), cameraY);
        if (snap) _camera.ResetSmoothing();
    }

    private void ClearProjectiles()
    {
        foreach (Node child in GetChildren())
        {
            if (child is not Bubble && child is not PowerBall) continue;
            child.SetPhysicsProcess(false);
            child.QueueFree();
        }
    }

    private void CompleteLevel()
    {
        _finished = true;
        ClearProjectiles();
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
