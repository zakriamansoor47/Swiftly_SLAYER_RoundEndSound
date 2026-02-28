using FluentMigrator;

namespace SLAYER_RoundEndSound.Database.Migrations;

[Migration(2026022800, "Initialize SLAYER_RoundEndSound table")]
public class InitTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("SLAYER_RoundEndSound").Exists())
        {
            Create.Table("SLAYER_RoundEndSound")
                .WithColumn("steamid").AsInt64().PrimaryKey().NotNullable()
                .WithColumn("name").AsString().NotNullable().WithDefaultValue("Unknown")
                .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(1)
                .WithColumn("volume").AsFloat().NotNullable().WithDefaultValue(1.0);
        }
    }

    public override void Down()
    {
        if (Schema.Table("SLAYER_RoundEndSound").Exists())
            Delete.Table("SLAYER_RoundEndSound");
    }
}