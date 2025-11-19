using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }
    
    // -------------------- PLAYERS --------------------
    private AudioStreamPlayer musicPlayer;
    private List<AudioStreamPlayer> sfxPlayers = new List<AudioStreamPlayer>();
    private const int SFX_POOL_SIZE = 16; // Número de canales simultáneos
    
    // -------------------- VOLUMEN --------------------
    [Export] private float masterVolume = 0.5f;
    [Export] private float musicVolume = 0.3f;
    [Export] private float sfxVolume = 0.5f;
    
    // -------------------- CACHE --------------------
    private Dictionary<string, AudioStream> audioCache = new Dictionary<string, AudioStream>();
    
    // ==========================================================
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        
        // Crear player de música
        musicPlayer = new AudioStreamPlayer();
        musicPlayer.Name = "MusicPlayer";
        musicPlayer.Bus = "Music";
        AddChild(musicPlayer);
        
        // Crear pool de players de SFX
        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            var sfxPlayer = new AudioStreamPlayer();
            sfxPlayer.Name = $"SFXPlayer_{i}";
            sfxPlayer.Bus = "SFX";
            AddChild(sfxPlayer);
            sfxPlayers.Add(sfxPlayer);
        }
        
        UpdateVolumes();
    }
    
    // ==========================================================
    // SFX
    // ==========================================================
    public void PlaySFX(string soundName)
    {
        var stream = LoadSound(soundName);
        if (stream == null)
        {
            GD.PrintErr($"[AudioManager] No se encontró: {soundName}");
            return;
        }
        
        // Buscar un player disponible
        AudioStreamPlayer availablePlayer = null;
        foreach (var player in sfxPlayers)
        {
            if (!player.Playing)
            {
                availablePlayer = player;
                break;
            }
        }
        
        // Si todos están ocupados, usar el primero (se interrumpe)
        if (availablePlayer == null)
        {
            availablePlayer = sfxPlayers[0];
        }
        
        availablePlayer.Stream = stream;
        availablePlayer.Play();
    }
    
    public void PlaySFXAt(string soundName, Vector2 position, Node2D parent = null)
    {
        var stream = LoadSound(soundName);
        if (stream == null) return;
        
        // Crear AudioStreamPlayer2D temporal para sonido posicional
        var player = new AudioStreamPlayer2D();
        player.Stream = stream;
        player.GlobalPosition = position;
        player.Bus = "SFX";
        
        if (parent != null)
            parent.AddChild(player);
        else
            GetTree().Root.AddChild(player);
        
        player.Finished += () => player.QueueFree();
        player.Play();
    }
    
    // ==========================================================
    // MÚSICA
    // ==========================================================
    public void PlayMusic(string musicName, bool loop = true)
    {
        var stream = LoadSound(musicName);
        if (stream == null) return;
        
        // Configurar loop si es AudioStreamOggVorbis o AudioStreamMP3
        if (stream is AudioStreamOggVorbis oggStream)
        {
            oggStream.Loop = loop;
        }
        
        musicPlayer.Stream = stream;
        musicPlayer.Play();
    }
    
    public void StopMusic()
    {
        musicPlayer.Stop();
    }
    
    public void FadeOutMusic(float duration = 1.0f)
    {
        var tween = CreateTween();
        tween.TweenProperty(musicPlayer, "volume_db", -80.0f, duration);
        tween.TweenCallback(Callable.From(() => musicPlayer.Stop()));
    }
    
    // ==========================================================
    // CARGA DE SONIDOS
    // ==========================================================
    private AudioStream LoadSound(string soundName)
    {
        // Revisar cache primero
        if (audioCache.ContainsKey(soundName))
        {
            return audioCache[soundName];
        }
        
        // Intentar cargar desde diferentes formatos
        string[] extensions = { ".wav", ".ogg", ".mp3" };
        
        foreach (var ext in extensions)
        {
            string path = $"res://Media/Audio/{soundName}{ext}";
            
            if (ResourceLoader.Exists(path))
            {
                var stream = GD.Load<AudioStream>(path);
                if (stream != null)
                {
                    audioCache[soundName] = stream;
                    return stream;
                }
            }
        }
        
        return null;
    }
    
    // ==========================================================
    // VOLUMEN
    // ==========================================================
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp(volume, 0f, 1f);
        UpdateVolumes();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp(volume, 0f, 1f);
        UpdateVolumes();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp(volume, 0f, 1f);
        UpdateVolumes();
    }
    
    private void UpdateVolumes()
    {
        // Convertir a decibeles (escala logarítmica)
        float masterDB = LinearToDb(masterVolume);
        float musicDB = LinearToDb(musicVolume);
        float sfxDB = LinearToDb(sfxVolume);
        
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), masterDB);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), musicDB);
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), sfxDB);
    }
    
    private float LinearToDb(float linear)
    {
        if (linear <= 0f)
            return -80f; // Silencio
        
        return Mathf.LinearToDb(linear);
    }
    
    // ==========================================================
    // UTILIDADES
    // ==========================================================
    public void StopAllSFX()
    {
        foreach (var player in sfxPlayers)
        {
            player.Stop();
        }
    }
    
    public void PauseAll()
    {
        musicPlayer.StreamPaused = true;
        foreach (var player in sfxPlayers)
        {
            player.StreamPaused = true;
        }
    }
    
    public void ResumeAll()
    {
        musicPlayer.StreamPaused = false;
        foreach (var player in sfxPlayers)
        {
            player.StreamPaused = false;
        }
    }
}