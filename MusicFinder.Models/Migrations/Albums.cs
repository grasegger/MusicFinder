using FluentMigrator;

[Migration(0)]
public class Albums : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Albums")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Artist").AsString().NotNullable();

        Create.UniqueConstraint("UQ_Albums_Name_Artist")
            .OnTable("Albums")
            .Columns("Name", "Artist");
    }

}
