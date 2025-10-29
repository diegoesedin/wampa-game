using Godot;

public partial class LevelSelectButton : Button
{
    [Export] public PackedScene LevelScene { get; set; }
    [Export] public bool IsLocked { get; set; } = false;
    [Export] public bool IsFirstButton { get; set; } = false;
    [Export] public string LevelName { get; set; } = "Level Name";
    [Export] private float Timer;
    [Export] private int Coins;
    [Export] private int EnemiesKilled;
    [Export] private bool MaskCollected = false;
    [Export] private int HeartsRemaining;

    [Export] private bool HasHeartsMedal = false;
    [Export] private bool HasTimeMedal = false;
    [Export] private bool HasEnemiesMedal = false;
    [Export] private bool HasMaskMedal = false;
    
    private LevelInfoDisplay infoDisplay;
    private Label levelNameLabel;

    public override void _Ready()
    {
        Pressed += OnPressed;
        FocusEntered += OnFocusEntered;
        

        infoDisplay = GetTree().Root.GetNode<LevelInfoDisplay>("MainMenu/LevelInfoDisplay");
        levelNameLabel = GetTree().Root.GetNode<Label>("MainMenu/CenterPanel/Title/LevelName");

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
            var stats = GetLevelStats();
            var medals = GetLevelMedals();
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
    
    // TODO: Conectar con SessionManager para obtener stats reales AHORA HARDCODEADO
    private LevelStats GetLevelStats()
    {
        
        return new LevelStats
        {
            Timer = Timer,
            Coins = Coins,
            EnemiesKilled = EnemiesKilled,
            MaskCollected = MaskCollected,
            HeartsRemaining = HeartsRemaining
        };
    }
    
    private LevelMedals GetLevelMedals()
    {
        return new LevelMedals
        {
            HasHeartsMedal = HasHeartsMedal,
            HasTimeMedal = HasTimeMedal,
            HasEnemiesMedal = HasEnemiesMedal,
            HasMaskMedal = HasMaskMedal
        };
    }
}