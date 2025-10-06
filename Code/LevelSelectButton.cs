using Godot;

public partial class LevelSelectButton : Button
{
    [Export] public PackedScene LevelScene { get; set; }
    [Export] public bool IsLocked { get; set; } = false;

    public override void _Ready()
    {
        Pressed += OnPressed;

        if (IsLocked)
        {
            Disabled = true;  
        }
        else
        {
            Disabled = false;
        }
    }

    private void OnPressed()
    {
        if (IsLocked)
        {
            GD.Print("Blocked Level");
            return;
        }

        if (LevelScene != null)
        {
            GD.Print($"Loading level: {LevelScene.ResourcePath}");
            GetTree().ChangeSceneToPacked(LevelScene);
        }
        else
        {
            GD.PrintErr("No scene");
        }
    }
}

