using Godot;

public partial class VictoryScreen : Control
{
    [Export] public PackedScene SceneToLoad;

    public override void _Ready()
    {
        var t = new Timer { WaitTime = 6, OneShot = true, Autostart = true };
        AddChild(t);

        t.Timeout += () =>
        {
            SessionManager.Instance.ResetProgress();
            GetTree().ChangeSceneToPacked(SceneToLoad);
        };
    }
}
