using Godot;

public partial class LevelSelectButton : Button
{
    [Export] public PackedScene LevelScene { get; set; }
    [Export] public bool IsLocked { get; set; } = false;
    [Export] public bool IsFirstButton { get; set; } = false;
    [Export] public string LevelName { get; set; } = "Level 1";
    
    private LevelInfoDisplay infoDisplay;
    private Label levelNameLabel;
    private string levelPath = "";

    public override void _Ready()
{
    Pressed += OnPressed;
    FocusEntered += OnFocusEntered;
    
    infoDisplay = Owner.GetNode<LevelInfoDisplay>("LevelInfoDisplay");
levelNameLabel = Owner.GetNode<Label>("CenterPanel/Title/LevelName");

    if (LevelScene != null)
    {
        levelPath = LevelScene.ResourcePath;
    }

    if (SessionManager.Instance != null && !string.IsNullOrEmpty(levelPath))
    {
        IsLocked = !SessionManager.Instance.IsLevelUnlocked(levelPath);
    }

    if (IsLocked)
    {
        Disabled = true;  
    }
    else
    {
        Disabled = false;
        
        if (IsFirstButton)
        {
            CallDeferred("grab_focus");
        }
    }
}
    
    private void OnFocusEntered()
    {
        if (infoDisplay == null || levelNameLabel == null) return;
        
        levelNameLabel.Text = LevelName;
        
        if (IsLocked)
        {
            infoDisplay.ShowLockedLevel();
        }
        else
        {
            var data = SessionManager.Instance.GetLevelData(levelPath);
            var medals = SessionManager.Instance.GetLevelMedals(levelPath);
            
            // Convertir LevelData a LevelStats
            var stats = new LevelStats
            {
                BestTime = data.BestTime,
                Coins = data.Coins,
                EnemiesKilled = data.EnemiesKilled,
                MaskCollected = data.MaskCollected,
                HeartsRemaining = data.HeartsRemaining
            };
            
            infoDisplay.ShowLevelInfo(LevelName, stats, medals);
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