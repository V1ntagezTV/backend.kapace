using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(2023052106260626)]
public class _AddTranslatorsAndColumns: Migration {
    public override void Up()
    {
        const string sql = @"
CREATE TABLE translator(
    id BIGINT PRIMARY KEY NOT NULL,
    name varchar not null,
    link varchar null
);
ALTER TABLE content_translation
    ADD COLUMN quality integer null,
    ADD COLUMN translation_id integer not null default 1;
ALTER TABLE content ALTER COLUMN image DROP NOT NULL;";
        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"
DROP TABLE IF EXISTS translators;
ALTER TABLE content_translation 
    DROP COLUMN quality,
    DROP COLUMN translation_id;";
        Execute.Sql(sql);
    }
}