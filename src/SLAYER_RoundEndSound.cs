using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.Translation;
using AudioApi;

namespace SLAYER_RoundEndSound;

#pragma warning disable CS9107
public class SLAYER_RoundEndSoundConfig
{
    public string RES_DatabaseConnection { get; set; } = "default"; // name from database.jsonc
    public bool RES_PlayInRandomOrder { get; set; } = true;
    public bool RES_EnableSoundNotification { get; set; } = true;
    public float RES_DefaultVolume { get; set; } = 1.0f;
    public List<RoundEndSounds> RES_Sounds { get; set; } = new List<RoundEndSounds>{ new RoundEndSounds() };
}
public class RoundEndSounds
{
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
}
[PluginMetadata
(
    Id = "SLAYER_RoundEndSound", 
    Version = "1.0", 
    Name = "SLAYER_RoundEndSound", 
    Author = "SLAYER", 
    Description = "Plays a sound at the end of each round"
)]
public partial class SLAYER_RoundEndSound(ISwiftlyCore core) : BasePlugin(core)
{
    public static new ISwiftlyCore Core { get; private set; } = null!;
    private ServiceProvider? _provider;
    private SLAYER_RoundEndSoundConfig Config { get; set; } = new();
    private ILocalizer Localizer => core.Localizer;
    public static IAudioApi? _audioApi;
    Dictionary<IPlayer, PlayerSettings> PlayerOption = new Dictionary<IPlayer, PlayerSettings>();
    public int _currentSoundIndex = 0;
    Random random = new Random();

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        if (!interfaceManager.HasSharedInterface("audio"))
        {
            Core.Logger.LogWarning("[SLAYER_RoundEndSound] Audio shared interface not found. Install/enable the 'Audio' plugin.");
            _audioApi = null;
            return;
        }

        var audioApi = interfaceManager.GetSharedInterface<IAudioApi>("audio");
        _audioApi = audioApi;
    }

    public override void Load(bool hotReload) 
    {
        // Ensure static Core is initialized before any usage.
        // Swiftly injects the instance core via the primary constructor parameter `core`.
        Core = core;

        // Initialize configuration
        Core.Configuration.InitializeJsonWithModel<SLAYER_RoundEndSoundConfig>("SLAYER_RoundEndSound.jsonc", "Main")
        .Configure(builder =>
        {
            builder.AddJsonFile("SLAYER_RoundEndSound.jsonc", optional: false, reloadOnChange: true);
        });

        // Register configuration with dependency injection
        ServiceCollection services = new();
        services.AddSwiftly(Core).AddOptionsWithValidateOnStart<SLAYER_RoundEndSoundConfig>().BindConfiguration("Main");

        _provider = services.BuildServiceProvider();

        Config = _provider.GetRequiredService<IOptions<SLAYER_RoundEndSoundConfig>>().Value;

        // database connection
        InitializeDatabase();
        // Load player settings for currently connected players on hot reload
        if(hotReload)
        {
            foreach (var player in Core.PlayerManager.GetAllValidPlayers())
            {
                if (!player.IsFakeClient) LoadPlayerSettings(player);
            }
        }

        // Events
        Core.GameEvent.HookPost<EventPlayerConnectFull>((@event) =>
        {
            var player = @event.UserIdPlayer;
            if(player == null || !player.IsValid) return HookResult.Continue;
            
            LoadPlayerSettings(player);
            return HookResult.Continue;
        });
        Core.GameEvent.HookPost<EventPlayerDisconnect>((@event) =>
        {
            var player = @event.UserIdPlayer;
            if(player == null || !player.IsValid) return HookResult.Continue;
            
            PlayerOption.Remove(player);
            return HookResult.Continue;
        });
        Core.GameEvent.HookPre<EventRoundMvp>((@event) =>
        {
            @event.DontBroadcast = true; // Prevent default MVP sound from playing
            @event.NoMusic = 1; // Prevent default MVP music from playing
            @event.MusickItID = -1; // Clear any custom MVP music
            @event.MusickItMvps = -1; // Clear any custom MVP music
            return HookResult.Continue;
        });
        Core.GameEvent.HookPre<EventRoundStart>((@event) =>
        {
            if (_audioApi == null) return HookResult.Continue;
            var controller = _audioApi.UseChannel("slayer_roundendsound");
            if (controller != null)
            {
                controller.StopAll();
            }
            return HookResult.Continue;
        });
        Core.GameEvent.HookPre<EventRoundEnd>((@event) =>
        {
            var sounds = Config.RES_Sounds;
            if (sounds.Count == 0) return HookResult.Continue;

            RoundEndSounds soundToPlay;
            if (Config.RES_PlayInRandomOrder)
            {
                soundToPlay = sounds[random.Next(sounds.Count)];
            }
            else
            {
                soundToPlay = sounds[_currentSoundIndex % sounds.Count];
                _currentSoundIndex++;
            }

            if (string.IsNullOrEmpty(soundToPlay.FilePath))
            {
                Core.Logger.LogError($"[SLAYER_RoundEndSound] Sound file path is empty for sound '{soundToPlay.Name}'. Please check your configuration.");
                return HookResult.Continue;
            }
            // else if (!File.Exists(Path.Combine(core.PluginDataDirectory, soundToPlay.FilePath)))
            // {
            //     Core.Logger.LogError($"[SLAYER_RoundEndSound] Sound file not found: '{Path.Combine(core.PluginDataDirectory, soundToPlay.FilePath)}'. Please check your configuration.");
            //     return HookResult.Continue;
            // }

            foreach (var player in Core.PlayerManager.GetAllValidPlayers())
            {
                if(player == null || !player.IsValid) continue;

                var settings = GetPlayerStats(player);
                if (settings == null || !settings.enabled) continue;

                PlayMP3SoundOnPlayer(player, soundToPlay.FilePath, settings.volume);

                player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.RES_PlayingSound", soundToPlay.Name]}");
                if (Config.RES_EnableSoundNotification)
                {
                    player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.RES_Notification"]}");
                }
            }
            return HookResult.Continue;
        });
    }

    private void PlayMP3SoundOnPlayer(IPlayer player, string soundPath, float volume = 1.0f)
    {
        if(!player.IsValid)return;

        if (_audioApi == null)
        {
            Core.Logger.LogError($"[SLAYER_RoundEndSound] Audio API is not initialized. Cannot play sound for player {player.Name}");
            return;
        }
        var controller = _audioApi.UseChannel("slayer_roundendsound");
        if (controller == null)
        {
            Core.Logger.LogError($"[SLAYER_RoundEndSound] Failed to get audio channel for player {player.Name}");
            return;
        }
        IAudioSource source = _audioApi.DecodeFromFile(Path.Combine(core.PluginDataDirectory, soundPath));
        controller.SetSource(source);

        controller.SetVolume(player.PlayerID, volume);
        controller.Play(player.PlayerID);
    }

    public override void Unload()
    {
    
    }
} 