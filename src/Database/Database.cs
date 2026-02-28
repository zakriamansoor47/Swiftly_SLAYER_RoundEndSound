using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.Plugins;
using Dapper;
using SwiftlyS2.Shared.Players;
using Dommel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLAYER_RoundEndSound;

public partial class SLAYER_RoundEndSound : BasePlugin
{
    [Table("SLAYER_RoundEndSound")]
    public sealed class PlayerSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long steamid { get; set; } = 0;
        public string name { get; set; } = "";
        public bool enabled { get; set; } = true;
        public float volume { get; set; } = 1.0f;
    }

    private void InitializeDatabase()
	{
		Task.Run(async () =>
		{
            try
            {
                // Run FluentMigrator migrations
                using var connection = Core.Database.GetConnection(Config.RES_DatabaseConnection);
                MigrationRunner.RunMigrations(connection);
            }
            catch (Exception ex)
            {
                Core.Logger.LogError(ex, "Failed to initialize database");
            }
		});
	}

    private void LoadPlayerSettings(IPlayer player)
    {
        var steamId = (long)player.SteamID;
        var playerName = player.Name;

        Task.Run(async () =>
        {
            try
            {
                var settings = await LoadPlayerSettingsAsync(steamId, playerName);
                if (settings != null)
                    PlayerOption[player] = settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SLAYER_RoundEndSound] Error on PlayerConnectFull while retrieving player data: {ex.Message}");
                Core.Logger.LogError($"[SLAYER_RoundEndSound] Error on PlayerConnectFull while retrieving player data: {ex.Message}");
            }
        });
    }
    public async Task<PlayerSettings?> LoadPlayerSettingsAsync(long SteamId, string Name)
    {
        try
        {
            using var connection = Core.Database.GetConnection(Config.RES_DatabaseConnection);
            connection.Open();

            var settings = await connection.GetAsync<PlayerSettings>(SteamId);
            if (settings != null)
            {
                if (settings.name != Name)
                {
                    settings.name = Name;
                    await connection.UpdateAsync(settings);
                }
                return settings;
            }
            else
            {
                var newSettings = new PlayerSettings
                {
                    steamid = SteamId,
                    name = Name,
                    enabled = true,
                    volume = Config.RES_DefaultVolume
                };
                await connection.InsertAsync(newSettings);
                return newSettings;
            }
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, $"Failed to load player settings for {SteamId}");
            return null;
        }
    }
    public async Task UpdatePlayerSettingsAsync(PlayerSettings settings)
    {
        try
        {
            using var connection = Core.Database.GetConnection(Config.RES_DatabaseConnection);
            connection.Open();
            await connection.UpdateAsync(settings);
        }
        catch (Exception ex)
        {
            Core.Logger.LogError(ex, $"Failed to update player settings for {settings.steamid}");
        }
    }
    private PlayerSettings? GetPlayerStats(IPlayer player)
    {
        if (!player.IsValid || !PlayerOption.ContainsKey(player))
            return null;

        return PlayerOption[player];
    }
    
}