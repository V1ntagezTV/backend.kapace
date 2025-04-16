using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202504150201)]
public class AddLogCompositeType : Migration {
    public override void Up()
    {
        const string initSql = @"
CREATE TYPE log_v1 AS (
    id BIGINT,
    user_id BIGINT,
    changes JSONB,
    created_at TIMESTAMP WITH TIME ZONE,
    metadata TEXT[]
);
";
        Execute.Sql(initSql);
    }

    public override void Down()
    {
        const string initSql = @"DROP TYPE log_v1";
        Execute.Sql(initSql);
    }
}