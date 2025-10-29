using Godot;

public partial class HUD : CanvasLayer
{
    private Label timeLabel;
    private Label enemiesLabel;
    private Label maskLabel;
    private Label coinsLabel;
    
    private AnimatedSprite2D heart1;
    private AnimatedSprite2D heart2;
    private AnimatedSprite2D heart3;

    public override void _Ready()
    {
        timeLabel = GetNode<Label>("TopBar/TimeLabel");
        enemiesLabel = GetNode<Label>("TopBar/EnemiesLabel");
        maskLabel = GetNode<Label>("TopBar/MaskLabel");
        coinsLabel = GetNode<Label>("TopBar/CoinsLabel");
        
        heart1 = GetNode<AnimatedSprite2D>("TopBar/HeartContainers/Heart_01");
        heart2 = GetNode<AnimatedSprite2D>("TopBar/HeartContainers/Heart_02");
        heart3 = GetNode<AnimatedSprite2D>("TopBar/HeartContainers/Heart_03");
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

        int seconds = (int)time;
        int milliseconds = (int)((time - seconds) * 100);
        timeLabel.Text = $"{seconds:00}.{milliseconds:00}";
    }   

    public void UpdateLives(int lives)
    {
        if (lives >= 1) heart1.Play("full");
        else heart1.Play("empty");
        
        if (lives >= 2) heart2.Play("full");
        else heart2.Play("empty");
        
        if (lives >= 3) heart3.Play("full");
        else heart3.Play("empty");
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

    public void UpdateCoins(int coins)
    {
        if (coinsLabel != null)
            coinsLabel.Text = $"💰: {coins}";
    }
}