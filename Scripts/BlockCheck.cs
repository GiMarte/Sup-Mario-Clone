using Godot;
using System;
using System.Threading.Tasks;

public partial class BlockCheck : Node
{
    private Node2D _level;
    private CharacterBody2D _player;
    private Block _block;
    private Sprite2D _visual;

    public override async void _Ready()
    {
        try
        {
            await SetUp();
            var shape = _block.GetNode<CollisionShape2D>("CollisionShape2D");
            Vector2 blockSize = ((RectangleShape2D)shape.Shape).Size;
            Require(blockSize == new Vector2(32, 32), "The block collider must measure 32 x 32.");
            Require(_block.UsedTexture != null, "The used texture must remain assigned in the scene.");
            Require(!_block.IsUsed, "A block must start unused.");

            var sprite = _player.GetNode<Sprite2D>("Sprite2D");
            Vector2 spriteSize = sprite.GetRect().Size * sprite.Scale;
            var playerShape = _player.GetNode<CollisionShape2D>("CollisionShape2D");
            Vector2 playerSize = ((RectangleShape2D)playerShape.Shape).Size;
            Require(Mathf.IsEqualApprox(spriteSize.Y, blockSize.Y),
                "Small Mario must be as tall as one block.");
            Require(Mathf.IsEqualApprox(sprite.Scale.X, sprite.Scale.Y),
                "The sprite scale must preserve the PNG proportions.");
            Require(Mathf.IsEqualApprox(playerSize.Y, spriteSize.Y) && playerSize.X <= spriteSize.X,
                "The player collider must follow the sprite dimensions.");
            Require(sprite.GlobalPosition.DistanceTo(playerShape.GlobalPosition) < 0.01f,
                "The player sprite and collider must share a center.");

            Texture2D startingTexture = _visual.Texture;
            await JumpIntoBlock();
            Require(_block.IsUsed && _visual.Texture == _block.UsedTexture && _visual.Texture != startingTexture,
                "The first hit from below must change the question texture to the used texture.");
            await JumpIntoBlock();
            Require(_visual.Texture == _block.UsedTexture && !_block.Hit(),
                "A second hit must be rejected and preserve the used texture.");

            await SetUp();
            Texture2D initialTexture = _visual.Texture;
            _player.GlobalPosition = _block.GlobalPosition + new Vector2(-24, -13);
            _player.Velocity = Vector2.Zero;
            Input.ActionPress("move_right");
            Vector2 sideNormal = await Contact();
            Input.ActionRelease("move_right");
            Require(sideNormal.X < -0.5f, "The test must touch the block's left side.");
            Require(!_block.IsUsed && _visual.Texture == initialTexture, "A side contact must leave the block unused.");

            await SetUp();
            initialTexture = _visual.Texture;
            _player.GlobalPosition = _block.GlobalPosition + new Vector2(9, -49);
            _player.Velocity = new Vector2(0, 300);
            Vector2 landingNormal = await Contact();
            Require(landingNormal.Y < -0.5f, "The test must land on the block's top face.");
            Require(!_block.IsUsed && _visual.Texture == initialTexture, "Landing must leave the block unused.");

            await SetUp();
            await JumpIntoBlock();
            var other = GD.Load<PackedScene>("res://Scenes/Block.tscn").Instantiate<Block>();
            other.Position = _block.Position + new Vector2(64, 0);
            other.UsedTexture = _block.UsedTexture;
            _level.AddChild(other);
            Require(!other.IsUsed && other.GetNode<Sprite2D>("Sprite2D").Texture != _visual.Texture,
                "A new instance must start unused.");
            Require(other.Hit(), "A new instance must accept its first hit.");
            Require(!other.Hit(), "The same instance must reject a second hit.");

            GD.Print("PASS: proportions, underside hit, texture swap, second hit, side contact, landing and independent instances.");
            var jumpSound = _player.GetNode<AudioStreamPlayer>("JumpSound");
            if (jumpSound.Playing)
                await ToSignal(jumpSound, AudioStreamPlayer.SignalName.Finished);
            await Step();
            await Step();
            GetTree().Quit();
        }
        catch (Exception error)
        {
            Input.ActionRelease("move_right");
            Input.ActionRelease("jump");
            GD.PushError(error.Message);
            GetTree().Quit(1);
        }
    }

    private async Task SetUp()
    {
        _level?.Free();
        _level = GD.Load<PackedScene>("res://main.tscn").Instantiate<Node2D>();
        AddChild(_level);
        _player = _level.GetNode<CharacterBody2D>("Player");
        _block = _level.GetNode<Block>("Block");
        _block.RewardScene = null;
        _visual = _block.GetNode<Sprite2D>("Sprite2D");
        await Step();
        await Step();
    }

    private async Task JumpIntoBlock()
    {
        _player.GlobalPosition = new Vector2(_block.GlobalPosition.X + 9, 581);
        _player.Velocity = Vector2.Zero;
        for (int tick = 0; tick < 3; tick++) await Step();
        Require(_player.IsOnFloor(), "The test jump must start on the ground.");
        Input.ActionPress("jump");
        await Step();
        Input.ActionRelease("jump");
        Require((await Contact()).Y > 0.5f, "The jump must hit the block's underside.");
    }

    private async Task<Vector2> Contact()
    {
        for (int tick = 0; tick < 40; tick++)
        {
            await Step();
            for (int index = 0; index < _player.GetSlideCollisionCount(); index++)
            {
                var collision = _player.GetSlideCollision(index);
                if (collision.GetCollider().GetInstanceId() == _block.GetInstanceId()) return collision.GetNormal();
            }
        }
        throw new Exception("The player did not reach the block collider.");
    }

    private async Task Step() => await ToSignal(GetTree().CreateTimer(0.0, true, true), SceneTreeTimer.SignalName.Timeout);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
