using Godot;

public partial class PowerBall : CharacterBody2D
{
    public Player.PowerType Power { get; set; }
    public int Direction { get; set; } = 1;
    private float _age;

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1;
        AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 5 } });
        Velocity = new Vector2(Direction * 450, -80);
    }

    public override void _PhysicsProcess(double delta)
    {
        _age += (float)delta;
        if (_age > 2.5f || GlobalPosition.Y > 1200)
        {
            QueueFree();
            return;
        }
        Velocity += Vector2.Down * 800 * (float)delta;
        var hit = MoveAndCollide(Velocity * (float)delta, recoveryAsCollision: true);
        if (hit != null)
        {
            if (hit.GetCollider() is Octopus enemy)
            {
                if (Power == Player.PowerType.Fire) enemy.QueueFree();
                else enemy.Freeze(3);
                QueueFree();
            }
            else if (hit.GetNormal().Y < -0.5f)
                Velocity = new Vector2(Direction * 450, -260);
            else QueueFree();
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        Color color = Power == Player.PowerType.Fire ? new Color(1, 0.30f, 0.04f) : new Color(0.15f, 0.62f, 1);
        Color core = Power == Player.PowerType.Fire ? new Color(1, 0.95f, 0.55f) : new Color(0.85f, 1, 1);
        DrawCircle(new Vector2(-Direction * 6, -1), 3, new Color(color, 0.4f));
        DrawCircle(Vector2.Zero, 5, color);
        DrawCircle(new Vector2(-1, -1), 3, core);
        DrawRect(new Rect2(-2, -3, 2, 2), Colors.White);
    }
}
