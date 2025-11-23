using Godot;
using System;

public partial class SceneChanger : Area2D
{
    [Export] public string SceneToLoad;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            CallDeferred("Changescene");
        }
    }

    private void Changescene() // daba error si estaba directo en el evento
    {
        GetTree().ChangeSceneToFile(SceneToLoad);
    }
}