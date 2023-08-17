using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202304190010)]
public class CreateTranslationContentTable : Migration
{
    public override void Up()
    {
        const string initSql = @"
        CREATE TABLE content_translation(
            id BIGSERIAL PRIMARY KEY,
            episode_id BIGINT NOT NULL,
            language VARCHAR NOT NULL,
            link VARCHAR NOT NULL,
            translation_type INTEGER NOT NULL, 
            created_at TIMESTAMP WITH TIME ZONE NOT NULL,
            created_by BIGINT NOT NULL
        );";

        Execute.Sql(initSql);
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS content_translation;");
    }
}