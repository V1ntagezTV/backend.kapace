using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202308220121)]
public class CreateImagesTable : Migration {
    public override void Up()
    {
        Execute.Sql(
            @"CREATE TABLE IF NOT EXISTS static_files(
                id BIGSERIAL PRIMARY KEY, 
                file_name VARCHAR, 
                link_type INTEGER, 
                link_id BIGINT, 
                created_at TIMESTAMP WITH TIME ZONE);
            ALTER TABLE content DROP COLUMN image, ADD COLUMN image_id BIGINT;"
        );
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS static_files;");
    }
}