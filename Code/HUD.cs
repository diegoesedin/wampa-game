using Godot;

public partial class HUD : CanvasLayer
{
    private Label livesLabel;
    private Label timeLabel;
    private Label enemiesLabel;
    private Label maskLabel;

    public override void _Ready()
    {
        livesLabel = GetNode<Label>("TopBar/LivesLabel");
        timeLabel = GetNode<Label>("TopBar/TimeLabel");
        enemiesLabel = GetNode<Label>("TopBar/EnemiesLabel");
        maskLabel = GetNode<Label>("TopBar/MaskLabel");
    }

    public void InitializeHUD(int lives, int killedEnemies, int totalEnemies, bool maskCollected)
    {
        UpdateLives(lives);
        UpdateEnemies(killedEnemies, totalEnemies);
        SetMaskCollected(maskCollected);
        UpdateTime(0);
    }

    public void UpdateTime(float time)
    {
        if (timeLabel == null)
            return;

        int minutes = (int)(time / 60);
        float seconds = time % 60;
        timeLabel.Text = $"⏱️: {minutes:00}:{seconds:00.00}";
    }

    public void UpdateLives(int lives)
    {
        if (livesLabel != null)
            livesLabel.Text = $"❤: {lives}";
    }

    public void UpdateEnemies(int killed, int total)
    {
        if (enemiesLabel != null)
            enemiesLabel.Text = $"💀: {killed}/{total}";
    }

    public void SetMaskCollected(bool collected)
    {
        if (maskLabel != null)
            maskLabel.Text = $"☀️: {(collected ? "Sí" : "No")}";
    }
}
