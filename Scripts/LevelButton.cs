using Godot;

public partial class LevelButton : Button
{
    [Export]
    public PackedScene LevelScene { get; set; }

    public override void _Pressed()
    {
        if (LevelScene == null)
        {
            GD.PushError($"Assegna la scena del livello al pulsante {Name}.");
            return;
        }

        Error error = GetTree().ChangeSceneToPacked(LevelScene);
        if (error != Error.Ok)
            GD.PushError($"Impossibile aprire {Text} ({error}).");
    }
}
