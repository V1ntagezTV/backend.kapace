using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202504150149, TransactionBehavior.None)]
public class AddLogTables : Migration {
    public override void Up()
    {
        const string initSql = """
                               CREATE TABLE IF NOT EXISTS logs(
                                   id BIGSERIAL PRIMARY KEY,
                                   user_id BIGINT,
                                   changes JSONB,
                                   created_at TIMESTAMP WITH TIME ZONE,
                                   metadata TEXT[]
                               );
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