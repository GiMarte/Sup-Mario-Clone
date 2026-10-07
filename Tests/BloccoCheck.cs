using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class BloccoCheck : Node
{
    private Node2D _level;
    private CharacterBody2D _player;
    private StaticBody2D _block;
    private ColorRect _visual;

    public override async void _Ready()
    {
        try
        {
            await SetUp();
            var shape = _block.GetNode<CollisionShape2D>("CollisionShape2D");
            Require(_visual.Size == new Vector2(32, 32), "Il quadrato deve misurare 32 x 32.");
            Require(((RectangleShape2D)shape.Shape).Size == _visual.Size, "Grafica e collisione devono avere la stessa dimensione.");
            Require((_visual.GlobalPosition + _visual.Size / 2).DistanceTo(shape.GlobalPosition) < 0.01f,
                "La collisione non e' centrata sul quadrato visibile.");

            var sprite = _player.GetNode<Sprite2D>("Sprite2D");
            Vector2 spriteSize = sprite.GetRect().Size * sprite.Scale;
            var playerShape = _player.GetNode<CollisionShape2D>("CollisionShape2D");
            Vector2 playerSize = ((RectangleShape2D)playerShape.Shape).Size;
            Require(Mathf.IsEqualApprox(spriteSize.Y, _visual.Size.Y),
                "Mario piccolo deve essere alto quanto un blocco.");
            Require(Mathf.IsEqualApprox(sprite.Scale.X, sprite.Scale.Y),
                "La scala deve mantenere le proporzioni del PNG.");
            Require(Mathf.IsEqualApprox(playerSize.Y, spriteSize.Y) && playerSize.X <= spriteSize.X,
                "La collisione del Player deve seguire le dimensioni dello sprite.");
            Require(sprite.GlobalPosition.DistanceTo(playerShape.GlobalPosition) < 0.01f,
                "Sprite e collisione del Player devono essere centrati insieme.");

            Color startingColor = _visual.Color;
            await JumpIntoBlock();
            Require(_visual.Color != startingColor, "Il primo colpo dal basso deve cambiare il colore del blocco.");
            _visual.Color = Colors.Magenta;
            await JumpIntoBlock();
            Require(_visual.Color == Colors.Magenta, "Un secondo colpo non deve ripetere l'effetto.");

            await SetUp();
            Color initialColor = _visual.Color;
            _player.GlobalPosition = _block.GlobalPosition + new Vector2(-24, -13);
            _player.Velocity = Vector2.Zero;
            Input.ActionPress("move_right");
            Vector2 sideNormal = await Contact();
            Input.ActionRelease("move_right");
            Require(sideNormal.X < -0.5f, "La prova deve toccare il lato sinistro del blocco.");
            Require(_visual.Color == initialColor, "Un contatto laterale non deve attivare il blocco.");

            await SetUp();
            initialColor = _visual.Color;
            _player.GlobalPosition = _block.GlobalPosition + new Vector2(9, -49);
            _player.Velocity = new Vector2(0, 300);
            Vector2 landingNormal = await Contact();
            Require(landingNormal.Y < -0.5f, "La prova deve atterrare sulla faccia superiore.");
            Require(_visual.Color == initialColor, "Atterrare sul blocco non deve attivarlo.");

            await SetUp();
            await JumpIntoBlock();
            var other = GD.Load<PackedScene>("res://Scenes/blocco.tscn").Instantiate<StaticBody2D>();
            other.Position = _block.Position + new Vector2(64, 0);
            _level.AddChild(other);
            Require(other.GetNode<ColorRect>("Visual").Color != _visual.Color,
                "Una nuova istanza deve partire ancora da attivare.");
            Require(other.Call("Colpisci").AsBool(), "La nuova istanza deve accettare il suo primo colpo.");
            Require(!other.Call("Colpisci").AsBool(), "La stessa istanza deve rifiutare un secondo colpo.");

            GD.Print("PASS: allineamento, colpo dal basso, secondo colpo, lato, atterraggio e istanze indipendenti.");
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
        _block = _level.GetChildren().OfType<StaticBody2D>().First(node => node.Name.ToString().StartsWith("Blocco"));
        _visual = _block.GetChildren().OfType<ColorRect>().Single();
        await Step();
        await Step();
    }

    private async Task JumpIntoBlock()
    {
        _player.GlobalPosition = new Vector2(_block.GlobalPosition.X + 9, 581);
        _player.Velocity = Vector2.Zero;
        for (int tick = 0; tick < 3; tick++) await Step();
        Require(_player.IsOnFloor(), "Il salto di prova deve partire dal terreno.");
        Input.ActionPress("jump");
        await Step();
        Input.ActionRelease("jump");
        Require((await Contact()).Y > 0.5f, "Il salto deve colpire la faccia inferiore del blocco.");
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
        throw new Exception("Il Player non ha raggiunto la collisione del blocco.");
    }

    private async Task Step() => await ToSignal(GetTree().CreateTimer(0.0, true, true), SceneTreeTimer.SignalName.Timeout);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
