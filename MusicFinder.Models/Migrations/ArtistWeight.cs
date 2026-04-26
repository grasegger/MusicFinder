using System;
using FluentMigrator;

namespace MusicFinder.Models.Migrations;

[Migration(2)]
public class ArtistWeight : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("ArtistWeights")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Artist").AsString().NotNullable().Unique()
            .WithColumn("Weight").AsDecimal().NotNullable().WithDefaultValue(0);

    }
}
