using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202404141521)]
public class AddCreationInfoToEpisode : Migration
{
    public override void Up()
    {
        const string initSql = @"
            ALTER TABLE episode
            ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITH TIME ZONE NOT NULL,
            ADD COLUMN IF NOT EXISTS created_by INTEGER NOT NULL DEFAULT 0;";
        Execute.Sql(initSql);
    }

    public override void Down()
    {
        const string initSql = "ALTER TABLE episode DROP COLUMN created_at, DROP COLUMN created_by;";
        Execute.Sql(initSql);
    }
}