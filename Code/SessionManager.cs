using Godot;
using System.Collections.Generic;

public partial class SessionManager : Node
{
    public static SessionManager Instance { get; private set; }
    [Export] private string currentLevelPath = "";
    
    // -------------------- DATOS DE NIVELES --------------------
    private Dictionary<string, LevelData> levelsData = new Dictionary<string, LevelData>();
    
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
    // GUARDAR/CARGAR PROGRESO
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
                BestTime = (float)levelDict["BestTime"],
                Coins = (int)levelDict["Coins"],
                EnemiesKilled = (int)levelDict["EnemiesKilled"],
                TotalEnemies = (int)levelDict["TotalEnemies"],
                MaskCollected = (bool)levelDict["MaskCollected"],
                HeartsRemaining = (int)levelDict["HeartsRemaining"],
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
                { "BestTime", kvp.Value.BestTime },
                { "Coins", kvp.Value.Coins },
                { "EnemiesKilled", kvp.Value.EnemiesKilled },
                { "TotalEnemies", kvp.Value.TotalEnemies },
                { "MaskCollected", kvp.Value.MaskCollected },
                { "HeartsRemaining", kvp.Value.HeartsRemaining },
                { "Completed", kvp.Value.Completed }
            };
            data[kvp.Key] = levelDict;
        }
        
        string json = Json.Stringify(data);
        using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
        file.StoreString(json);
        
        GD.Print("[SessionManager] Progreso guardado");
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
        
        // Solo guardar si es mejor tiempo o primera vez
        if (!level.Completed || time < level.BestTime)
        {
            level.BestTime = time;
            level.Coins = coins;
            level.EnemiesKilled = enemiesKilled;
            level.TotalEnemies = totalEnemies;
            level.MaskCollected = maskCollected;
            level.HeartsRemaining = heartsRemaining;
            level.Completed = true;
            
            SaveProgress();
            
            GD.Print($"[SessionManager] Stats guardadas para: {levelPath}");
        }
    }
    
    public LevelData GetLevelData(string levelPath)
    {
        if (levelsData.ContainsKey(levelPath))
        {
            return levelsData[levelPath];
        }
        
        return new LevelData(); // Datos vacíos si nunca jugó
    }
    
    public bool IsLevelCompleted(string levelPath)
    {
        return levelsData.ContainsKey(levelPath) && levelsData[levelPath].Completed;
    }
    
    public bool IsLevelUnlocked(string levelPath)
    {
        // Lógica de desbloqueo - ajustar según tu estructura de niveles
        if (levelPath.Contains("lvl_01")) return true;
        if (levelPath.Contains("lvl_02")) return IsLevelCompleted("res://Scenes/Game Levels/lvl_01.tscn");
        if (levelPath.Contains("lvl_03")) return IsLevelCompleted("res://Scenes/Game Levels/lvl_02.tscn");
        
        // Por defecto desbloqueado (para testing)
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
        
        return new LevelMedals
        {
            HasHeartsMedal = data.HeartsRemaining == 3,           // Sin perder vida
            HasTimeMedal = data.BestTime > 0 && data.BestTime < 15f, // Menos de 15 (ajustable)
            HasEnemiesMedal = data.EnemiesKilled == data.TotalEnemies && data.TotalEnemies > 0, // Todos los enemigos
            HasMaskMedal = data.MaskCollected                     // Máscara conseguida
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
    public float BestTime { get; set; } = 0f;
    public int Coins { get; set; } = 0;
    public int EnemiesKilled { get; set; } = 0;
    public int TotalEnemies { get; set; } = 0;
    public bool MaskCollected { get; set; } = false;
    public int HeartsRemaining { get; set; } = 0;
    public bool Completed { get; set; } = false;
}