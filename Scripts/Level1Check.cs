using Godot;
using System;
using System.Threading.Tasks;

public partial class Level1Check : Node
{
    private Node2D _level;
    private CharacterBody2D _player;

    public override async void _Ready()
    {
        try
        {
            Require(ResourceLoader.Exists("res://Scenes/Level1.tscn"),
                "The new playable Level1 scene must exist.");
            using var levelScene = GD.Load<PackedScene>("res://Scenes/Level1.tscn");
            _level = levelScene.Instantiate<Node2D>();
            AddChild(_level);
            _player = _level.GetNode<CharacterBody2D>("Player");
            foreach (Node enemy in _level.GetNode("Enemies").GetChildren())
            {
                enemy.ProcessMode = ProcessModeEnum.Disabled;
            }
            await Steps(4);
            Require(_player.IsOnFloor(), "The player must spawn on solid ground.");
            Require(_level.GetNode("Terrain").GetChildCount() >= 4,
                "The level must contain separated terrain sections.");

            using var gap = PhysicsRayQueryParameters2D.Create(new Vector2(2224, 600), new Vector2(2224, 670), 1);
            Require(_level.GetWorld2D().DirectSpaceState.IntersectRay(gap).Count == 0,
                "The first gap must not contain an invisible floor.");

            var firstBlock = _level.GetNode<Block>("Blocks/Question16_9");
            _player.Position = new Vector2(firstBlock.Position.X + 9, 581);
            _player.Velocity = Vector2.Zero;
            await Steps(3);
            Input.ActionPress("jump");
            await Steps(1);
            Input.ActionRelease("jump");
            await Steps(35);
            Require(firstBlock.IsUsed, "Jumping from below must activate the existing reward block.");
            Require(_level.GetNode<Label>("HUD/CoinCount").Text.EndsWith("x01"),
                "A coin from a question block must also update the coin counter.");

            var coin = _level.GetNode<Area2D>("Coins/Coin0");
            string coinsBefore = _level.GetNode<Label>("HUD/CoinCount").Text;
            _player.Position = coin.Position - _player.GetNode<CollisionShape2D>("CollisionShape2D").Position;
            _player.Velocity = Vector2.Zero;
            await Steps(4);
            Require(!coin.Visible, "Touching a coin must collect it.");
            string coinsAfter = _level.GetNode<Label>("HUD/CoinCount").Text;
            Require(coinsBefore != coinsAfter, "Collecting a coin must update the HUD.");
            await Steps(4);
            Require(_level.GetNode<Label>("HUD/CoinCount").Text == coinsAfter,
                "The same coin must only be counted once.");

            _player.Position = new Vector2(6065, 325);
            _player.Velocity = Vector2.Zero;
            await Steps(3);
            Require(_player.IsOnFloor(), "The camera test must begin on the highest stair.");
            Input.ActionPress("jump");
            await Steps(1);
            Input.ActionRelease("jump");
            await Steps(34);
            var camera = _level.GetNode<Camera2D>("Camera2D");
            float visibleTop = camera.GetScreenCenterPosition().Y - GetViewport().GetVisibleRect().Size.Y / (2 * camera.Zoom.Y);
            float playerTop = _player.GetNode<Sprite2D>("Sprite2D").GlobalPosition.Y - 16;
            Require(playerTop >= visibleTop,
                "The player must remain visible when jumping from the highest staircase.");

            _player.Position = new Vector2(2224, 930);
            _player.Velocity = Vector2.Zero;
            await Steps(5);
            Require(_player.Position.X < 200 && _player.Position.Y < 650,
                "Falling into a gap must return the player to the start.");
            await Steps(20);
            Require(_player.IsOnFloor(), "The respawn must restore normal physics and floor contact.");

            using var bubbleScene = GD.Load<PackedScene>("res://Scenes/bubble.tscn");
            var bubble = bubbleScene.Instantiate<Bubble>();
            bubble.Speed = 0;
            bubble.TrapDuration = 10;
            bubble.Position = _player.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition;
            _level.AddChild(bubble);
            await Steps(4);
            Require(!_player.IsPhysicsProcessing(), "The bubble must actually trap the player for this regression check.");
            _player.Position = new Vector2(2224,930);
            await Steps(4);
            Require(!GodotObject.IsInstanceValid(bubble) && _player.IsPhysicsProcessing() && _player.Position.X < 200,
                "Respawning while trapped must clear the bubble and restore the player's controls.");

            Input.ActionPress("move_right");
            for (int tick = 0; tick < 4000 && !_level.GetNode<Control>("HUD/Victory").Visible; tick++)
            {
                if (_player.IsOnFloor() && ObstacleAhead()) Input.ActionPress("jump");
                await Steps(1);
                Input.ActionRelease("jump");
                if (tick % 360 == 0) GD.Print($"Route tick {tick}: {_player.Position}");
            }
            Input.ActionRelease("move_right");
            Require(_level.GetNode<Control>("HUD/Victory").Visible,
                $"The whole course must be traversable with movement and jumps; stopped at {_player.Position}.");
            GD.Print("PASS: Level1 spawn, real gap, reward/collectible coins, high-jump camera, falling and trapped respawn, full-course traversal and flag.");
            foreach (AudioStreamPlayer audio in _level.FindChildren("*", "AudioStreamPlayer", true, false))
                audio.Stop();
            // Fixed-FPS tests outrun the audio thread; let it release pending playback before exit.
            await Task.Delay(80);
            await Steps(2);
            GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.PushError(error.ToString());
            GetTree().Quit(1);
        }
        finally
        {
            Input.ActionRelease("jump");
            Input.ActionRelease("move_left");
            Input.ActionRelease("move_right");
        }
    }

    private bool ObstacleAhead()
    {
        Vector2 center = _player.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition;
        using var wall = PhysicsRayQueryParameters2D.Create(center + new Vector2(12, 0), center + new Vector2(78, 0), 1);
        using var floor = PhysicsRayQueryParameters2D.Create(center + new Vector2(78, 14), center + new Vector2(78, 56), 1);
        var space = _level.GetWorld2D().DirectSpaceState;
        return space.IntersectRay(wall).Count > 0 || space.IntersectRay(floor).Count == 0;
    }

    private async Task Steps(int count)
    {
        for (int index = 0; index < count; index++)
            await ToSignal(GetTree().CreateTimer(0, true, true), SceneTreeTimer.SignalName.Timeout);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
