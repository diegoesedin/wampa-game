using Godot;
using System;

public partial class RankingScreen : Control
{
    [Export]
    private RichTextLabel[] _rankingNameList;
    [Export]
    private RichTextLabel[] _rankingScoreList;

    public override void _Ready()
    {
        var rankingList = GameManager.Instance.RankingManager.RankingEntries;
        for (int i = 0; i < rankingList.Count; i++)
        {
            _rankingNameList[i].Text = rankingList[i].Name;
            _rankingScoreList[i].Text = rankingList[i].Score.ToString();
        }
        
        if (SessionManager.Instance.HasPlayerWon())
        {
            GD.Print("[GameManager] Todas las medallas obtenidas");
            GetTree().CallDeferred("change_scene_to_file", "res://Scenes/Menus/VictoryScreen.tscn");
            return;
        }
        
        var t = new Timer { WaitTime = 6, OneShot = true, Autostart = true };
        AddChild(t);

        t.Timeout += () =>
        {
            GetTree().CallDeferred("change_scene_to_file", "res://Scenes/Menus/main_menu.tscn");
        };
    }
}
