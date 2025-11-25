using Godot;
using System.Collections.Generic;

public partial class SessionManager : Node
{
    public static SessionManager Instance { get; private set; }
    
    // -------------------- DATOS DE NIVELES --------------------
    private Dictionary<string, LevelData> levelsData = new Dictionary<string, LevelData>();
    
    private readonly string[] levelPaths =
    [
        "res://Scenes/Game Levels/tutorial.tscn",
        "res://Scenes/Game Levels/lvl_01.tscn",
        "res://Scenes/Game Levels/lvl_02.tscn",
        "res://Scenes/Game Levels/lvl_03.tscn"
    ];
    
    // ==========================================================
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        
        LoadProgress();
    }
    
    // ==========================================================
    // GUARDAR/CARGAR PROGRESO + VERIFICAR SI GANO
    // ==========================================================
    private const string SAVE_PATH = "user://progress.json";
    
    private void LoadProgress()
    {
        if (!FileAccess.FileExists(SAVE_PATH))
        {
            GD.Print("[SessionManager] No hay progreso guardado. Creando nuevo.");
            return;
        }
        
        using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();
        
        var data = Json.ParseString(json).AsGodotDictionary();
        
        foreach (var key in data.Keys)
        {
            var levelDict = data[key].AsGodotDictionary();
            
            levelsData[key.ToString()] = new LevelData
            {
                LastTime = (float)levelDict["LastTime"],
                LastCoins = (int)levelDict["LastCoins"],
                LastEnemiesKilled = (int)levelDict["LastEnemiesKilled"],
                LastTotalEnemies = (int)levelDict["LastTotalEnemies"],
                LastMaskCollected = (bool)levelDict["LastMaskCollected"],
                LastHeartsRemaining = (int)levelDict["LastHeartsRemaining"],
                BestTime = (float)levelDict["BestTime"],
                HeartsMedalUnlocked = (bool)levelDict["HeartsMedalUnlocked"],
                TimeMedalUnlocked = (bool)levelDict["TimeMedalUnlocked"],
                EnemiesMedalUnlocked = (bool)levelDict["EnemiesMedalUnlocked"],
                MaskMedalUnlocked = (bool)levelDict["MaskMedalUnlocked"],
                Completed = (bool)levelDict["Completed"]
            };
        }
        
        GD.Print($"[SessionManager] Progreso cargado: {levelsData.Count} niveles");
    }
    
    private void SaveProgress()
    {
        var data = new Godot.Collections.Dictionary();
        
        foreach (var kvp in levelsData)
        {
            var levelDict = new Godot.Collections.Dictionary
            {
                { "LastTime", kvp.Value.LastTime },
                { "LastCoins", kvp.Value.LastCoins },
                { "LastEnemiesKilled", kvp.Value.LastEnemiesKilled },
                { "LastTotalEnemies", kvp.Value.LastTotalEnemies },
                { "LastMaskCollected", kvp.Value.LastMaskCollected },
                { "LastHeartsRemaining", kvp.Value.LastHeartsRemaining },
                { "BestTime", kvp.Value.BestTime },
                { "HeartsMedalUnlocked", kvp.Value.HeartsMedalUnlocked },
                { "TimeMedalUnlocked", kvp.Value.TimeMedalUnlocked },
                { "EnemiesMedalUnlocked", kvp.Value.EnemiesMedalUnlocked },
                { "MaskMedalUnlocked", kvp.Value.MaskMedalUnlocked },
                { "Completed", kvp.Value.Completed }
            };
            data[kvp.Key] = levelDict;
        }
        
        string json = Json.Stringify(data);
        using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
        file.StoreString(json);
        
        GD.Print("[SessionManager] Progreso guardado");
    }

    public bool HasPlayerWon()
    {
        foreach (string path in levelPaths)
        {
            if (!levelsData.ContainsKey(path))
                return false;

            var d = levelsData[path];

            bool allMedals =
                d.HeartsMedalUnlocked &&
                d.TimeMedalUnlocked &&
                d.EnemiesMedalUnlocked &&
                d.MaskMedalUnlocked;

            if (!allMedals)
                return false; // faltan medallas -> false
        }

        return true; // tiene todas las medallas de todos los niveles
    }

    public bool HasPlayerAllLevelsCompleted()
    {
        foreach (string path in levelPaths)
        {
            if (!levelsData.ContainsKey(path))
                return false;

            var d = levelsData[path];

            if (!d.Completed)
                return false;
        }

        return true;
    }

    public int GetCalculatedScore()
    {
        int score = 0;
        foreach (var level in levelsData.Values)
        {
            score += level.LastCoins;
            score += level.LastEnemiesKilled;
            score += level.LastHeartsRemaining;
            
            score -= (int)level.BestTime;

            if (level.EnemiesMedalUnlocked)
                score += 100;
            
            if (level.HeartsMedalUnlocked)
                score += 100;
            
            if (level.MaskMedalUnlocked)
                score += 100;
            
            if (level.TimeMedalUnlocked)
                score += 100;
        }
        return score;
    }
    
    // ==========================================================
    // GESTIÓN DE NIVELES
    // ==========================================================
    public void SaveLevelStats(string levelPath, float time, int coins, int enemiesKilled, int totalEnemies, bool maskCollected, int heartsRemaining)
    {
        if (!levelsData.ContainsKey(levelPath))
        {
            levelsData[levelPath] = new LevelData();
        }
        
        var level = levelsData[levelPath];
        
        // SIEMPRE guardar stats de la última partida
        level.LastTime = time;
        level.LastCoins = coins;
        level.LastEnemiesKilled = enemiesKilled;
        level.LastTotalEnemies = totalEnemies;
        level.LastMaskCollected = maskCollected;
        level.LastHeartsRemaining = heartsRemaining;
        level.Completed = true;
        
        // Actualizar mejor tiempo
        if (level.BestTime == 0 || time < level.BestTime)
        {
            level.BestTime = time;
        }
        
        // MEDALLAS PERMANENTES 
        if (heartsRemaining == 3)
            level.HeartsMedalUnlocked = true;
        
        if (time < 15f) 
            level.TimeMedalUnlocked = true;
        
        if (enemiesKilled == totalEnemies && totalEnemies > 0)
            level.EnemiesMedalUnlocked = true;
        
        if (maskCollected)
            level.MaskMedalUnlocked = true;
        
        SaveProgress();
        
        GD.Print($"[SessionManager] Stats guardadas para: {levelPath}");

    }
    
    public LevelData GetLevelData(string levelPath)
    {
        if (levelsData.ContainsKey(levelPath))
        {
            return levelsData[levelPath];
        }
        
        return new LevelData(); 
    }
    
    public bool IsLevelCompleted(string levelPath)
    {
        return levelsData.ContainsKey(levelPath) && levelsData[levelPath].Completed;
    }
    
    public bool IsLevelUnlocked(string levelPath)
    {
        // Lógica de desbloqueo 
        if (levelPath.Contains("tutorial")) return true;
        if (levelPath.Contains("lvl_01")) return IsLevelCompleted("res://Scenes/Game Levels/tutorial.tscn");
        if (levelPath.Contains("lvl_02")) return IsLevelCompleted("res://Scenes/Game Levels/lvl_01.tscn");
        if (levelPath.Contains("lvl_03")) return IsLevelCompleted("res://Scenes/Game Levels/lvl_02.tscn");
        

        return true;
    }
    
    // ==========================================================
    // MEDALLAS
    // ==========================================================
    public LevelMedals GetLevelMedals(string levelPath)
    {
        if (!levelsData.ContainsKey(levelPath))
        {
            return new LevelMedals();
        }
        
        var data = levelsData[levelPath];
        
        // Devolver las medallas PERMANENTES guardadas
        return new LevelMedals
        {
            HasHeartsMedal = data.HeartsMedalUnlocked,
            HasTimeMedal = data.TimeMedalUnlocked,
            HasEnemiesMedal = data.EnemiesMedalUnlocked,
            HasMaskMedal = data.MaskMedalUnlocked
        };
    }
    
    // ==========================================================
    // RESET 
    // ==========================================================
    public void ResetProgress()
    {
        levelsData.Clear();
        
        if (FileAccess.FileExists(SAVE_PATH))
        {
            DirAccess.RemoveAbsolute(SAVE_PATH);
        }
        
        GD.Print("[SessionManager] Progreso reseteado");
    }
}

// ==========================================================
// CLASE DE DATOS
// ==========================================================
public class LevelData
{
    // Stats de la ÚLTIMA partida (siempre se sobrescriben)
    public float LastTime { get; set; } = 0f;
    public int LastCoins { get; set; } = 0;
    public int LastEnemiesKilled { get; set; } = 0;
    public int LastTotalEnemies { get; set; } = 0;
    public bool LastMaskCollected { get; set; } = false;
    public int LastHeartsRemaining { get; set; } = 0;
    
    // Mejor tiempo
    public float BestTime { get; set; } = 0f;
    
    // Medallas
    public bool HeartsMedalUnlocked { get; set; } = false;
    public bool TimeMedalUnlocked { get; set; } = false;
    public bool EnemiesMedalUnlocked { get; set; } = false;
    public bool MaskMedalUnlocked { get; set; } = false;
    public bool Completed { get; set; } = false;
}