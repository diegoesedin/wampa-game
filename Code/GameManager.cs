using Godot;
using System.Collections.Generic;

public partial class GameManager : Node2D
{
    public static GameManager Instance;

    [Export] public Player player { get; set; }
    [Export] public HUD hud { get; set; }
    [Export] private string currentLevelPath = "";

    private float elapsedTime = 0f;
    private int enemiesKilled = 0;
    private int totalEnemies = 0;
    private bool maskCollected = false;
    private int coinCount = 0;
    private bool stopTimer = false;

    public override void _Ready()
    {
        Instance = this;

        if (player == null)
        {
            GD.PrintErr("GameManager: no player.");
            return;
        }

        if (hud == null)
        {
            GD.PrintErr("GameManager: no HUD.");
            return;
        }

        // CONECTA AL JUGADOR
        player.Connect(nameof(Player.LivesChanged), new Callable(this, nameof(OnLivesChanged)));
        player.Connect(nameof(Player.PlayerDied), new Callable(this, nameof(OnPlayerDied)));
        player.Connect(nameof(Player.MaskCollected), new Callable(this, nameof(OnMaskCollected)));
        player.Connect(nameof(Player.LevelCompleted), new Callable(this, nameof(OnLevelCompleted)));

        // CONECTA LAS MONEDAS
        foreach (Node node in GetTree().GetNodesInGroup("Coins"))
        {
            if (node is Coin coin)
            {
                coin.Connect(nameof(Coin.CoinCollected), new Callable(this, nameof(OnCoinCollected)));
            }
        }
        
        // CONECTA LOS ENEMIGOS
        var enemies = GetTree().GetNodesInGroup("Enemy");
        totalEnemies = enemies.Count;
        enemiesKilled = 0;

        foreach (Node enemy in enemies)
        {
            if (enemy.HasSignal("EnemyDied"))
            {
                enemy.Connect("EnemyDied", new Callable(this, nameof(OnEnemyDied)));
            }
        }

        hud.CallDeferred(nameof(HUD.InitializeHUD), player.MaxLives, enemiesKilled, totalEnemies, maskCollected);
    }

    public override void _Process(double delta)
    {
        if (!stopTimer)
        {
            elapsedTime += (float)delta;
        }

        hud.UpdateTime(elapsedTime);
    }

    // EVENTOS DE JUGADOR
    private void OnLivesChanged(int newLives)
    {
        hud.UpdateLives(newLives);
    }

    private void OnPlayerDied()
    {
        GD.Print("[GameManager] Player murió - NO se guardan stats");
        
        // Esperar 2 segundos y volver al menu
        GetTree().CreateTimer(2.0f).Timeout += () => 
        {
            GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
        };
    }

    private void OnMaskCollected()
    {
        maskCollected = true;
        hud.SetMaskCollected(maskCollected);
        
        AudioManager.Instance.PlayMusic("mask_pickup");
    }

    private void OnLevelCompleted()
    { 
    stopTimer = true;
    
    AudioManager.Instance.PlayMusic("level_complete");
    
    // GUARDAR STATS EN SESSION MANAGER
    if (!string.IsNullOrEmpty(currentLevelPath))
    {
        SessionManager.Instance.SaveLevelStats(
            currentLevelPath,
            elapsedTime,
            coinCount,
            enemiesKilled,
            totalEnemies,
            maskCollected,
            player.CurrentLives
        );
        
        GD.Print($"[GameManager] Nivel completado en {elapsedTime:F2}s");

        // Chequea si ganó (agarrar todas las medallas)
        if (SessionManager.Instance.HasPlayerWon())
        {
            GD.Print("[GameManager] Todas las medallas obtenidas");
            GetTree().CallDeferred("change_scene_to_file", "res://Scenes/VictoryScreen.tscn");
            return;
        }

        // Si NO ganó el juego -> volver al menú normal
        GetTree().CallDeferred("change_scene_to_file", "res://Scenes/main_menu.tscn");
    }
    else
    {
        GD.PrintErr("[GameManager] currentLevelPath no configurado!");
    }
    }


    // EVENTOS DE MONEDAS
    private void OnCoinCollected()
    {
        coinCount++;
        AudioManager.Instance.PlaySFX("coin_pickup");
        hud.UpdateCoins(coinCount);
    }

    // EVENTOS DE ENEMIGOS
    private void OnEnemyDied()
    {
        enemiesKilled++;
        hud.UpdateEnemies(enemiesKilled, totalEnemies);
    }
}