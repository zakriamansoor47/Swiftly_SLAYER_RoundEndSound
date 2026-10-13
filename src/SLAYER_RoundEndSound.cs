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
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Sounds;
using SwiftlyS2.Shared.ProtobufDefinitions;

namespace SLAYER_RoundEndSound;

#pragma warning disable CS9107
public class SLAYER_RoundEndSoundConfig
{
    public string RES_DatabaseConnection { get; set; } = "default"; // name from database.jsonc
    public bool RES_UseAudioPlugin { get; set; } = true;
    public string RES_SoundEventFile { get; set; } = "";
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
    Version = "1.1.0",
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
    private const string VolumeFeature = "SLAYER_RoundEndSound";
    private Func<long, string, float>? _getEffectiveVolume;
    private Action<string>? _unregisterVolumeFeature;
    private readonly Dictionary<int, (ulong SteamId, uint Guid)> _addonSounds = new();
    System.Collections.Concurrent.ConcurrentDictionary<IPlayer, PlayerSettings> PlayerOption = new();
    public int _currentSoundIndex = 0;
    Random random = new Random();

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        DisconnectVolumeApi();
        _audioApi = null;
        try
        {
            if (Config.RES_UseAudioPlugin && interfaceManager.HasSharedInterface("audio"))
                _audioApi = interfaceManager.GetSharedInterface<IAudioApi>("audio");
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, "[SLAYER_RoundEndSound] Audio API initialization failed.");
        }

        // Resolve the optional contract only once. No reflection occurs during playback,
        // and the plugin can start without Volume.API or Volume.Contract.dll installed.
        if (interfaceManager.HasSharedInterface("Volume.Player.v1"))
        {
            try
            {
                var contract = Type.GetType("Volume.Contract.IPlayerVolumeAPI, Volume.Contract", throwOnError: true)!;
                var getter = typeof(IInterfaceManager).GetMethod(nameof(IInterfaceManager.GetSharedInterface))!
                    .MakeGenericMethod(contract);
                var api = getter.Invoke(interfaceManager, new object[] { "Volume.Player.v1" })!;
                var register = contract.GetMethod("RegisterFeature")!.CreateDelegate<Action<string, string>>(api);
                var effective = contract.GetMethod("GetEffectiveVolume")!.CreateDelegate<Func<long, string, float>>(api);
                var unregister = contract.GetMethod("UnregisterFeature")!.CreateDelegate<Action<string>>(api);
                register(VolumeFeature, "Round End Sound");
                _getEffectiveVolume = effective;
                _unregisterVolumeFeature = unregister;
            }
            catch (Exception ex)
            {
                Core.Logger.LogError(ex, "[SLAYER_RoundEndSound] Volume API initialization failed; using RES volume settings.");
            }
        }

        Core.Logger.LogInformation("[SLAYER_RoundEndSound] Playback mode: {Mode}. Volume API: {VolumeApi}",
            _audioApi != null ? "Audio" : "Workshop addon", _getEffectiveVolume != null);
        if (Config.RES_UseAudioPlugin && _audioApi == null)
            Core.Logger.LogWarning("[SLAYER_RoundEndSound] Audio plugin unavailable. Addon fallback requires valid sound event names and RES_SoundEventFile.");
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

        Core.Event.OnPrecacheResource += OnPrecacheResource;

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

            _addonSounds.Remove(player.PlayerID);
            PlayerOption.TryRemove(player, out _);
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
            StopSounds();
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
                if(player == null || !player.IsValid || player.IsFakeClient) continue;

                var settings = GetPlayerStats(player);
                if (settings == null || !settings.enabled) continue;

                if (!PlaySoundOnPlayer(player, soundToPlay.FilePath, settings.volume)) continue;

                player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.RES_PlayingSound", soundToPlay.Name]}");
                if (Config.RES_EnableSoundNotification)
                {
                    player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.RES_Notification"]}");
                }
            }
            return HookResult.Continue;
        });
    }

    private static float ClampVolume(float volume) => float.IsFinite(volume) ? Math.Clamp(volume, 0f, 1f) : 0f;

    private float GetPlaybackVolume(IPlayer player, float localVolume)
    {
        // Audio playback uses only the plugin's own !res_vol setting.
        if (_audioApi != null) return ClampVolume(localVolume);

        // Workshop playback uses only Volume API (!volume), never !res_vol.
        if (_getEffectiveVolume == null) return 1f;
        try
        {
            return ClampVolume(_getEffectiveVolume((long)player.SteamID, VolumeFeature));
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, "[SLAYER_RoundEndSound] Volume API failed; using default Workshop volume until reload.");
            _getEffectiveVolume = null;
            return 1f;
        }
    }

    private bool PlaySoundOnPlayer(IPlayer player, string soundPath, float volume)
    {
        if (!player.IsValid || player.IsFakeClient) return false;
        volume = GetPlaybackVolume(player, volume);
        if (volume <= 0f) return false;
        try
        {
            if (_audioApi != null)
            {
                var controller = _audioApi.UseChannel("slayer_roundendsound");
                if (controller == null) return false;
                IAudioSource source = _audioApi.DecodeFromFile(Path.Combine(core.PluginDataDirectory, soundPath));
                controller.SetSource(source);
                controller.SetVolume(player.PlayerID, volume);
                controller.Play(player.PlayerID);
            }
            else
            {
                var name = soundPath.Trim();
                if (name.EndsWith(".vsnd_c", StringComparison.OrdinalIgnoreCase)) name = name[..^7];
                else if (name.EndsWith(".vsnd", StringComparison.OrdinalIgnoreCase)) name = name[..^5];
                using var soundEvent = new SoundEvent
                {
                    Name = name,
                    Volume = volume,
                    SourceEntityIndex = player.Pawn == null ? -1 : (int)player.Pawn.Index
                };
                soundEvent.Recipients.AddRecipient(player.PlayerID);
                _addonSounds[player.PlayerID] = (player.SteamID, soundEvent.Emit());
            }
            return true;
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, "[SLAYER_RoundEndSound] Failed to play {Sound}", soundPath);
            return false;
        }
    }

    private void OnPrecacheResource(IOnPrecacheResourceEvent @event)
    {
        // Also precache when Audio is requested: its absence can select addon fallback.
        if (!string.IsNullOrWhiteSpace(Config.RES_SoundEventFile))
            @event.AddItem(Config.RES_SoundEventFile);
    }

    private void StopSounds()
    {
        try
        {
            _audioApi?.UseChannel("slayer_roundendsound")?.StopAll();
            foreach (var player in Core.PlayerManager.GetAllValidPlayers())
            {
                if (player.IsFakeClient || !_addonSounds.TryGetValue(player.PlayerID, out var sound)
                    || sound.SteamId != player.SteamID) continue;
                using var message = Core.NetMessage.Create<CMsgSosStopSoundEvent>();
                message.SoundeventGuid = unchecked((int)sound.Guid);
                message.Recipients.AddRecipient(player.PlayerID);
                message.Send();
            }
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, "[SLAYER_RoundEndSound] Failed to stop round end sounds.");
        }
        finally
        {
            _addonSounds.Clear();
        }
    }

    private void DisconnectVolumeApi()
    {
        try { _unregisterVolumeFeature?.Invoke(VolumeFeature); }
        catch (Exception ex) { core.Logger.LogWarning(ex, "[SLAYER_RoundEndSound] Could not unregister Volume API feature."); }
        _getEffectiveVolume = null;
        _unregisterVolumeFeature = null;
    }

    public override void Unload()
    {
        StopSounds();
        Core.Event.OnPrecacheResource -= OnPrecacheResource;
        DisconnectVolumeApi();
        _audioApi = null;
        PlayerOption.Clear();
        _provider?.Dispose();
        _provider = null;
    }
}
