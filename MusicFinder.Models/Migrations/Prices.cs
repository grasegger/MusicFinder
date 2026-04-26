using System;
using FluentMigrator;

namespace MusicFinder.Models.Migrations;

[Migration(1)]
public class Prices : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Prices")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("AlbumId").AsInt64().NotNullable().ForeignKey("Albums", "Id").OnDeleteOrUpdate(System.Data.Rule.Cascade)
            .WithColumn("Provider").AsString(255).NotNullable()
            .WithColumn("Price").AsDecimal(18, 2).NotNullable()
            .WithColumn("LastUpdated").AsDateTime().NotNullable();


        Create.UniqueConstraint("UQ_Prices_AlbumId_Provider").OnTable("Prices").Columns("AlbumId", "Provider");
    }

}
