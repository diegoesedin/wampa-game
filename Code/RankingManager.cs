using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

public class RankingManager
{
    private const string SAVE_PATH = "user://ranking.json";
    public List<RankingEntry> RankingEntries { get; private set; } = new List<RankingEntry>();
    
    public RankingManager()
    {
        LoadRanking();
    }

    public bool TryAddEntry(string name, int score)
    {
        var newEntry = new RankingEntry { Name = name, Score = score };
        RankingEntries.Add(newEntry);
        RankingEntries = RankingEntries
            .OrderByDescending(entry => entry.Score)
            .ToList();

        bool isHighscore = RankingEntries.IndexOf(newEntry) < 5;

        if (RankingEntries.Count > 5)
        {
            RankingEntries = RankingEntries.Take(5).ToList();
        }

        if (isHighscore)
        {
            SaveRanking();
        }

        return isHighscore;
    }

    public void SaveRanking()
    {
        string json = JsonSerializer.Serialize(RankingEntries, new JsonSerializerOptions { WriteIndented = true });
        GD.Print($"Saving JSON {json}");
        
        using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(json);
            GD.Print("[RankingManager] Ranking guardado exitosamente.");
        }
        else
        {
            GD.PushError($"Error al intentar guardar el ranking en {SAVE_PATH}");
        }
    }
    
    public void LoadRanking()
    {
        if (!FileAccess.FileExists(SAVE_PATH))
        {
            RankingEntries = new List<RankingEntry>();
            GD.Print("No se encontró archivo de ranking, iniciando uno nuevo.");
            return;
        }

        using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
        if (file != null)
        {
            string json = file.GetAsText();
            try 
            {
                RankingEntries = JsonSerializer.Deserialize<List<RankingEntry>>(json);
            }
            catch (Exception e)
            {
                GD.PushError($"Error al leer el JSON del ranking: {e.Message}");
                RankingEntries = new List<RankingEntry>(); // Fallback
            }
        }
    }
}

[Serializable]
public class RankingEntry
{
    public string Name { get; set; }
    public int Score { get; set; }
}