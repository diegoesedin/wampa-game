using Godot;
using System;

public partial class IntroScene : Control
{
    [Export] public PackedScene SceneToLoad;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsPressed())
        {
            GetTree().CallDeferred("change_scene_to_packed", SceneToLoad);
        }
    }
}
