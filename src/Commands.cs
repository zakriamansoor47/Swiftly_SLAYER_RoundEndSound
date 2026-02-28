using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.Commands;

namespace SLAYER_RoundEndSound;
public partial class SLAYER_RoundEndSound : BasePlugin
{
    [Command("res")]
	public void PlayerRoundEndSoundSettings(ICommandContext command)
	{
        var player = command.Sender;
        if (player == null || !player.IsValid) return;

        var settings = GetPlayerStats(player);
        if (settings == null) return;

        if (settings.enabled)
        {
            PlayerOption[player].enabled = false;
            player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.RES_Disabled"]}");
            Task.Run(async () => await UpdatePlayerSettingsAsync(PlayerOption[player]));
        }
        else
        {
            
            PlayerOption[player].enabled = true;
            player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.RES_Enabled"]}");
            Task.Run(async () => await UpdatePlayerSettingsAsync(PlayerOption[player]));
        }
    }
    [Command("res_volume")]
    [CommandAlias("res_vol")]
	public void PlayerRoundEndSoundVolume(ICommandContext command)
	{
        var player = command.Sender;
        if (player == null || !player.IsValid) return;

        var settings = GetPlayerStats(player);
        if (settings == null) return;

        if (command.Args.Count() > 0 && float.TryParse(command.Args[0], out float volume))
        {
            if (volume < 0.0f || volume > 1.0f)
            {
                player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.RES_VolumeUsage", PlayerOption[player].volume]}");
            }

            PlayerOption[player].volume = Math.Clamp(volume, 0.0f, 1.0f);
            player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.RES_Volume", PlayerOption[player].volume, PlayerOption[player].volume * 100]}");
            Task.Run(async () => await UpdatePlayerSettingsAsync(PlayerOption[player]));
        }
        else
        {
            player.SendChat($"{Localizer["Chat.Prefix"]} {Localizer["Chat.RES_VolumeUsage", PlayerOption[player].volume]}");
            return;
        }
    }
}