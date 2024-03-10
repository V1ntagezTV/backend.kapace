using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202403080220)]
public class CreateViewsAndStarsColumns : Migration
{
    public override void Up()
    {
        const string initSql = @"
            ALTER TABLE episode
            ADD COLUMN IF NOT EXISTS views INTEGER NOT NULL DEFAULT 0,
            ADD COLUMN IF NOT EXISTS stars DOUBLE PRECISION NOT NULL DEFAULT 0;";
        Execute.Sql(initSql);
    }

    public override void Down()
    {
        const string initSql = "ALTER TABLE episode DROP COLUMN views, DROP COLUMN stars;";
        Execute.Sql(initSql);
    }
}