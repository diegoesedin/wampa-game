using Godot;
using System.Collections.Generic;

public partial class GameManager : Node2D
{
    public static GameManager Instance; //REFE GLOBAL

    [Export] public Player player { get; set; }
    [Export] public HUD hud { get; set; }
    [Export] private AudioStreamPlayer sfxPlayer;

    private float elapsedTime = 0f;
    private int enemiesKilled = 0;
    private int totalEnemies = 0;
    private bool maskCollected = false;
    private int coinCount = 0;

    public override void _Ready()
    {
        Instance = this; //REFE GLOBAL (Singleton Manual, supuestamente)

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

        // CONECTA AL JUGADOR ///////////////////////////////////////////////////////
        player.Connect(nameof(Player.LivesChanged), new Callable(this, nameof(OnLivesChanged)));
        player.Connect(nameof(Player.PlayerDied), new Callable(this, nameof(OnPlayerDied)));
        player.Connect(nameof(Player.MaskCollected), new Callable(this, nameof(OnMaskCollected)));


        // CONECTA LAS MONEDAS ///////////////////////////////////////////////////////
        foreach (Node node in GetTree().GetNodesInGroup("Coins"))
        {
            if (node is Coin coin)
            {
                coin.Connect(nameof(Coin.CoinCollected), new Callable(this, nameof(OnCoinCollected)));
            }
        }
        
        // CONECTA LOS ENEMIGOS ///////////////////////////////////////////////////////
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

        hud.CallDeferred(nameof(HUD.InitializeHUD), player.CurrentLives, enemiesKilled, totalEnemies, maskCollected);
        //ejecuta InitializeHUD un frame despues, lo del nameof() en vez del string supuestamente es mas prolijo y seguro
    }

    public override void _Process(double delta)
    {
        elapsedTime += (float)delta;
        hud.UpdateTime(elapsedTime);
    }


    // EVENTOS DE JUGADOR ///////////////////////////////////////////////////////
    private void OnLivesChanged(int newLives)
    {
        hud.UpdateLives(newLives);
    }

    private void OnPlayerDied()
    {
        GD.Print("GAMEOVER");
    }

    private void OnMaskCollected()
    {
        maskCollected = true;
        hud.SetMaskCollected(maskCollected);
    }

    // EVENTOS DE MONEDAS ///////////////////////////////////////////////////////
    private void OnCoinCollected()
    {
        coinCount++;
        PlaySFX("coin_pickup");
        hud.UpdateCoins(coinCount);
    }

    // EVENTOS DE ENEMIGOS ///////////////////////////////////////////////////////
    private void OnEnemyDied()
    {
        enemiesKilled++;
        hud.UpdateEnemies(enemiesKilled, totalEnemies);
    }


    // DISPARADOR DE EFECTOS DE SONIDO ////////////////////////////////////////////
    public void PlaySFX(string soundName)
    {
        var path = $"res://Media/Audio/{soundName}.wav";
        var stream = GD.Load<AudioStream>(path);
        if (stream == null)
        {
            GD.PrintErr($"[GameManager] No se encontró el sonido: {path}");
            return;
        }

        sfxPlayer.Stream = stream;
        sfxPlayer.Play();
    }
}
