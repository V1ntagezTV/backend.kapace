using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202504061942, TransactionBehavior.None)]
public class AddFavoritesUniqueIndex : Migration {
    public override void Up()
    {
        const string initSql = """
                               CREATE UNIQUE INDEX CONCURRENTLY user_id_content_id_idx ON favorites(user_id, content_id);
                               """;

        Execute.Sql(initSql);
    }

    public override void Down()
    {
        const string initSql = """
                               DROP INDEX user_id_content_id_idx;
                               """;

        Execute.Sql(initSql);
    }
}