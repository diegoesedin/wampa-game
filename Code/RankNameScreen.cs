using Godot;

public partial class RankNameScreen : Control
{
    private VirtualKeyboard keyboard;
    
    public override void _Ready()
    {
        keyboard = GetNode<VirtualKeyboard>("VirtualKeyboard");
        keyboard.OnSubmit += OnPlayerNameSaved;
    }

    private void OnPlayerNameSaved()
    {
        var name = keyboard.Text;
        var score = SessionManager.Instance.GetCalculatedScore();
        GameManager.Instance.RankingManager.TryAddEntry(name, score);
        
        GetTree().CallDeferred("change_scene_to_file", "res://Scenes/Menus/RankingScreen.tscn");
    }
}
