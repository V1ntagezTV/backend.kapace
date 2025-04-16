using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202504061219)]
public class AddFavoritesColumn : Migration {
    public override void Up()
    {
        const string initSql = """
                               CREATE TABLE IF NOT EXISTS favorites(
                                   id BIGSERIAL PRIMARY KEY,
                                   user_id BIGINT NOT NULL,
                                   content_id BIGINT NOT NULL,
                                   episode_id BIGINT NULL,
                                   status int NULL,
                                   stars int not null,
                                   created_at TIMESTAMP WITH TIME ZONE NOT NULL
                               );
                               """;

        Execute.Sql(initSql);
    }

    public override void Down()
    {
        const string initSql = """
                               DROP TABLE IF EXISTS favorites;
                               """;

        Execute.Sql(initSql);
    }
}