using Godot;

public partial class Blocco : StaticBody2D
{
    [Export]
    public Texture2D TextureVuota { get; set; }
    public bool Attivato { get; private set; }
    [Export]
    public PackedScene Premio { get; set; }

    public bool Colpisci()
    {
        if (Attivato)
        {
            return false;
        }

        Attivato = true;
        GetNode<Sprite2D>("Sprite2D").Texture = TextureVuota;
        if (Premio != null)
        {
            var premio = Premio.Instantiate<Node2D>();
            GetParent().AddChild(premio);

            premio.GlobalPosition = GlobalPosition;
            premio.ZIndex = -1;

            premio.CreateTween().TweenProperty(
                premio,
                "global_position",
                GlobalPosition + new Vector2(0, -40),
                0.25
            );
        }

        return true;

    }
}
