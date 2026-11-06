using Godot;
using System;
using System.Threading.Tasks;

public partial class PowerCheck : Node
{
    private Node2D _level;
    private Player _player;

    public override async void _Ready()
    {
        try
        {
            using var scene = GD.Load<PackedScene>("res://Scenes/Level1.tscn");
            _level = scene.Instantiate<Node2D>();
            AddChild(_level);
            _player = _level.GetNode<Player>("Player");
            foreach (Node enemy in _level.GetNode("Enemies").GetChildren())
                enemy.ProcessMode = ProcessModeEnum.Disabled;
            await Steps(4);
            var sprite = _player.GetNode<Sprite2D>("Sprite2D");
            var collider = _player.GetNode<CollisionShape2D>("CollisionShape2D");
            float initialHeight = sprite.GetRect().Size.Y * sprite.Scale.Y;
            float initialFoot = collider.GlobalPosition.Y + ((RectangleShape2D)collider.Shape).Size.Y / 2;

            await Collect("res://Scenes/FireMushroom.tscn");
            Require(_player.CurrentPower == Player.PowerType.Fire, "A real fire mushroom must grant Fire.");
            Require(Mathf.IsEqualApprox(sprite.GetRect().Size.Y * sprite.Scale.Y, initialHeight * 1.25f),
                "A mushroom must make the character 25 percent taller.");
            Require(Mathf.IsEqualApprox(((RectangleShape2D)collider.Shape).Size.Y, 40),
                "The powered collider must match the taller sprite.");
            Require(Mathf.Abs(collider.GlobalPosition.Y + ((RectangleShape2D)collider.Shape).Size.Y / 2 - initialFoot) < 0.2f,
                "Growing must keep the character's feet on the floor.");
            Require(sprite.Material is ShaderMaterial fireMaterial && fireMaterial.GetShaderParameter("power_type").AsInt32() == 2,
                "Fire must select the clothing palette.");

            _player.ReceivePower(Player.PowerType.Fire);
            await Steps(3);
            Require(Mathf.IsEqualApprox(((RectangleShape2D)collider.Shape).Size.Y, 40),
                "Collecting the same power again must not stack the growth.");
            var secondLevel = scene.Instantiate<Node2D>();
            var otherPlayer = secondLevel.GetNode<Player>("Player");
            secondLevel.RemoveChild(otherPlayer);
            secondLevel.Free();
            _level.AddChild(otherPlayer);
            otherPlayer.Position = new Vector2(160,581);
            await Steps(2);
            Require(((RectangleShape2D)otherPlayer.GetNode<CollisionShape2D>("CollisionShape2D").Shape).Size.Y == 32,
                "The first player's growth must not resize another instance.");
            otherPlayer.Free();

            Require(InputMap.HasAction("shoot"), "The shoot input action must be configured.");
            var fireTarget = _level.GetNode<Octopus>("Enemies/Octopus0");
            fireTarget.Position = new Vector2(250,597);
            fireTarget.ProcessMode = ProcessModeEnum.Inherit;
            await Shoot();
            Require(FindBall("FireBall") != null, "Fire power must launch a fireball.");
            await Steps(30);
            Require(!GodotObject.IsInstanceValid(fireTarget), "A fireball must defeat an octopus.");

            await Collect("res://Scenes/IceMushroom.tscn");
            Require(_player.CurrentPower == Player.PowerType.Ice, "The ice mushroom must replace the fire power.");
            Require(Mathf.IsEqualApprox(((RectangleShape2D)collider.Shape).Size.Y, 40),
                "Switching powers must preserve the same powered height.");
            Require(sprite.Material is ShaderMaterial iceMaterial && iceMaterial.GetShaderParameter("power_type").AsInt32() == 1,
                "Ice must select a different clothing palette.");
            using var enemyScene = GD.Load<PackedScene>("res://Scenes/octopus.tscn");
            var iceTarget = enemyScene.Instantiate<Octopus>();
            iceTarget.Position = new Vector2(250,597);
            iceTarget.Scale = new Vector2(2.7f,2.7f);
            _level.GetNode("Enemies").AddChild(iceTarget);
            await Shoot();
            Require(FindBall("IceBall") != null, "Ice power must launch an iceball.");
            await Steps(25);
            Vector2 frozenAt = iceTarget.Position;
            await Steps(60);
            Require(iceTarget.Position.DistanceTo(frozenAt) < 0.1f && iceTarget.Modulate != Colors.White,
                "Ice must temporarily freeze and recolor the target.");
            Require(CountBubbles() == 0, "A frozen octopus must not keep shooting bubbles.");
            await Shoot();
            await Steps(100);
            Require(iceTarget.Position.DistanceTo(frozenAt) < 0.1f && iceTarget.Modulate != Colors.White,
                "A second iceball must refresh the freeze instead of thawing on the first hit's deadline.");
            await Steps(110);
            Require(iceTarget.Modulate == Colors.White && iceTarget.Position.X < frozenAt.X - 2,
                "A frozen enemy must return to its normal appearance and movement.");
            iceTarget.Free();

            Input.ActionPress("move_left");
            await Shoot();
            Input.ActionRelease("move_left");
            var leftBall = FindBall("IceBall");
            Require(leftBall != null && leftBall.Velocity.X < 0,
                "Projectiles must follow the character's facing direction.");
            await Steps(22);
            Require(FindBall("IceBall") == null, "A projectile must disappear when it reaches a wall.");

            _player.Position = new Vector2(1000,581);
            _player.Velocity = Vector2.Zero;
            Input.ActionPress("move_right");
            await Steps(1);
            Input.ActionRelease("move_right");
            await Shoot();
            await Steps(19);
            var bouncingBall = FindBall("IceBall");
            Require(bouncingBall != null && bouncingBall.Velocity.Y < 0,
                "A ball that reaches the floor must bounce upward.");

            _player.Position = new Vector2(2224,930);
            await Steps(4);
            Require(_player.CurrentPower == Player.PowerType.None && ((RectangleShape2D)collider.Shape).Size.Y == 32,
                "A fall must restore the normal character and clear the power.");
            Require(FindBall("IceBall") == null && FindBall("FireBall") == null,
                "Respawning must remove remaining power projectiles.");
            await Steps(20);
            await Shoot();
            Require(FindBall("IceBall") == null && FindBall("FireBall") == null,
                "The normal character must not launch elemental balls.");

            var ceiling = new StaticBody2D { Position = new Vector2(96,564), CollisionLayer = 1, CollisionMask = 0 };
            ceiling.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(64,16) } });
            _level.AddChild(ceiling);
            await Steps(2);
            _player.ReceivePower(Player.PowerType.Fire);
            await Steps(3);
            Require(((RectangleShape2D)collider.Shape).Size.Y == 32,
                "Growth must wait instead of embedding the character in a low ceiling.");
            Input.ActionPress("move_right");
            await Steps(14);
            Input.ActionRelease("move_right");
            Require(((RectangleShape2D)collider.Shape).Size.Y == 40,
                "Pending growth must complete once the character moves out from under the ceiling.");
            Require(Mathf.Abs(collider.GlobalPosition.Y + 20 - initialFoot) < 0.2f,
                "Delayed growth must also preserve the feet's position.");
            _player.ReceivePower(Player.PowerType.None);
            await Steps(3);
            _player.Position = new Vector2(105,581);
            _player.Velocity = Vector2.Zero;
            await Steps(2);
            _player.ReceivePower(Player.PowerType.Fire);
            _player.ReceivePower(Player.PowerType.None);
            ceiling.QueueFree();
            await Steps(4);
            Require(_player.CurrentPower == Player.PowerType.None && ((RectangleShape2D)collider.Shape).Size.Y == 32,
                "Resetting a pending transformation must cancel its deferred growth.");

            _player.ReceivePower(Player.PowerType.Fire);
            await Steps(3);
            var pointBlankTarget = enemyScene.Instantiate<Octopus>();
            pointBlankTarget.Position = new Vector2(collider.GlobalPosition.X + 28,597);
            pointBlankTarget.Scale = new Vector2(2.7f,2.7f);
            _level.GetNode("Enemies").AddChild(pointBlankTarget);
            pointBlankTarget.SetPhysicsProcess(false);
            await Shoot();
            await Steps(3);
            Require(!GodotObject.IsInstanceValid(pointBlankTarget),
                "A fireball must also hit an enemy overlapping its spawn point.");

            GD.Print("PASS: real pickups, anchored/isolated growth, clothing palettes, elemental shots, renewed freeze, facing/bounce, respawn, headroom and point-blank hits.");
            foreach (AudioStreamPlayer audio in _level.FindChildren("*", "AudioStreamPlayer", true, false)) audio.Stop();
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
            if (InputMap.HasAction("shoot")) Input.ActionRelease("shoot");
        }
    }

    private async Task Collect(string path)
    {
        using var packed = GD.Load<PackedScene>(path);
        var mushroom = packed.Instantiate<Mushroom>();
        _level.AddChild(mushroom);
        mushroom.Position = _player.GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition;
        mushroom.Activate();
        await Steps(5);
        Require(!GodotObject.IsInstanceValid(mushroom), "The mushroom must be collected through its pickup area.");
    }

    private async Task Shoot()
    {
        Input.ActionPress("shoot");
        await Steps(1);
        Input.ActionRelease("shoot");
        await Steps(1);
    }

    private CharacterBody2D FindBall(string name)
    {
        foreach (Node child in _level.GetChildren())
            if (child is CharacterBody2D ball && child.Name.ToString().StartsWith(name) && !child.IsQueuedForDeletion()) return ball;
        return null;
    }

    private int CountBubbles()
    {
        int count = 0;
        foreach (Node child in GetChildren()) if (child is Bubble) count++;
        foreach (Node child in _level.GetChildren()) if (child is Bubble) count++;
        return count;
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
