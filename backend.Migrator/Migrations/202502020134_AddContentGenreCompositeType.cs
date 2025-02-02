using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202502020134)]
public class AddContentGenreCompositeType : Migration {
    public override void Up()
    {
        const string initSql = @"
CREATE TYPE content_genre_v1 AS (
    content_id BIGINT,
    genre_id BIGINT,
    created_at TIMESTAMP WITH TIME ZONE,
    created_by BIGINT
);
";
        Execute.Sql(initSql);
    }

    public override void Down()
    {
        const string initSql = @"DROP TYPE content_genre_v1";
        Execute.Sql(initSql);
    }
}