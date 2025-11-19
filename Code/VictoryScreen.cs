using Godot;
using System;

public partial class VictoryScreen : Control
{
    [Export] public PackedScene SceneToLoad;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsPressed())
        {
            SessionManager.Instance.ResetProgress();
            GetTree().ChangeSceneToPacked(SceneToLoad);
        }
    }
}
