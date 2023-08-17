using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202307222348)]
public class CreateChangesHistory : Migration
{
    public override void Up()
    {
        const string sql = $@"
            CREATE TABLE IF NOT EXISTS changes_history(
                id BIGSERIAL,
                target_id BIGINT,
                history_type INTEGER,
                text jsonb,
                created_by BIGINT,
                created_at TIMESTAMP WITH TIME ZONE,
                approved_by BIGINT,
                approved_at TIMESTAMP WITH TIME ZONE,
                PRIMARY KEY(id, target_id, history_type)
            );";

        Execute.Sql(sql);
    }

    public override void Down()
    {
        Execute.Sql($"DROP TABLE IF EXISTS changes_history;");
    }
}