using Godot;

public partial class LevelInfoDisplay : NinePatchRect
{
    // -------------------- COMPONENTES UI --------------------
    private Label heartsLabel;
    private Label timeLabel;
    private Label enemiesLabel;
    private Label maskLabel;
    private Label coinsLabel;
    
    private Label heartsMedal;
    private Label timeMedal;
    private Label enemiesMedal;
    private Label maskMedal;
    
    // ==========================================================
    public override void _Ready()
    {
        // Stats
        heartsLabel = GetNode<Label>("Stats/HeartsLabel");
        timeLabel = GetNode<Label>("Stats/TimeLabel");
        enemiesLabel = GetNode<Label>("Stats/EnemiesLabel");
        maskLabel = GetNode<Label>("Stats/MaskLabel");
        coinsLabel = GetNode<Label>("Stats/CoinsLabel");
        
        // Medals
        heartsMedal = GetNode<Label>("Medals/HeartsMedal");
        timeMedal = GetNode<Label>("Medals/TimeMedal");
        enemiesMedal = GetNode<Label>("Medals/EnemiesMedal");
        maskMedal = GetNode<Label>("Medals/MaskMedal");
        
        // Mostrar mensaje por defecto
        ShowDefaultMessage();
    }
    
    // ==========================================================
    public void ShowLevelInfo(string levelName, LevelStats stats, LevelMedals medals)
    {
        // Stats
        heartsLabel.Text = $"❤️: {stats.HeartsRemaining}";
        
        int seconds = (int)stats.BestTime;
        int milliseconds = (int)((stats.BestTime % 1) * 100);
        timeLabel.Text = $"⏳: {seconds:00}.{milliseconds:00}";
        
        enemiesLabel.Text = $"💀: {stats.EnemiesKilled}";
        maskLabel.Text = $"☀️: {(stats.MaskCollected ? "sí" : "no")}";
        coinsLabel.Text = $"💰: {stats.Coins:00}";
        
        // Medals (cambiar opacidad o color para mostrar si está conseguida)
        UpdateMedalDisplay(heartsMedal, medals.HasHeartsMedal);
        UpdateMedalDisplay(timeMedal, medals.HasTimeMedal);
        UpdateMedalDisplay(enemiesMedal, medals.HasEnemiesMedal);
        UpdateMedalDisplay(maskMedal, medals.HasMaskMedal);
    }
    
    public void ShowLockedLevel()
    {
        heartsLabel.Text = "❤️: --";
        timeLabel.Text = "⏳: --:--";
        enemiesLabel.Text = "💀: --";
        maskLabel.Text = "☀️: --";
        coinsLabel.Text = "💰: --";
        
        // Todas las medallas bloqueadas
        UpdateMedalDisplay(heartsMedal, false);
        UpdateMedalDisplay(timeMedal, false);
        UpdateMedalDisplay(enemiesMedal, false);
        UpdateMedalDisplay(maskMedal, false);
    }
    
    public void ShowDefaultMessage()
    {
        heartsLabel.Text = "❤️: -";
        timeLabel.Text = "⏳: -";
        enemiesLabel.Text = "💀: -";
        maskLabel.Text = "☀️: -";
        coinsLabel.Text = "💰: -";
        
        // Ocultar medallas
        heartsMedal.Modulate = new Color(1, 1, 1, 0.2f);
        timeMedal.Modulate = new Color(1, 1, 1, 0.2f);
        enemiesMedal.Modulate = new Color(1, 1, 1, 0.2f);
        maskMedal.Modulate = new Color(1, 1, 1, 0.2f);
    }
    
    // ==========================================================
    private void UpdateMedalDisplay(Label medal, bool unlocked)
    {
        if (unlocked)
        {
            // Medalla desbloqueada - color dorado brillante
            medal.Modulate = new Color(1, 0.84f, 0, 1);
            medal.AddThemeColorOverride("font_outline_color", new Color(0.5f, 0.3f, 0, 1));
        }
        else
        {
            // Medalla bloqueada - gris oscuro
            medal.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }
    }
}

// ==========================================================
// CLASES DE DATOS
// ==========================================================
public class LevelStats
{
    public float BestTime { get; set; } = 0f;
    public int Coins { get; set; } = 0;
    public int EnemiesKilled { get; set; } = 0;
    public bool MaskCollected { get; set; } = false;
    public int HeartsRemaining { get; set; } = 0;
}

public class LevelMedals
{
    public bool HasHeartsMedal { get; set; } = false;    // Completar sin perder vida
    public bool HasTimeMedal { get; set; } = false;      // Completar bajo tiempo récord
    public bool HasEnemiesMedal { get; set; } = false;   // Matar todos los enemigos
    public bool HasMaskMedal { get; set; } = false;      // Conseguir la máscara
}