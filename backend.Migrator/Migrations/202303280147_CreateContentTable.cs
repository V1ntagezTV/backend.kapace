using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202303280147)]
public class CreateContentTable : Migration {
    public override void Up()
    {
        const string sql = @"
CREATE TABLE IF NOT EXISTS content(
    id BIGINT PRIMARY KEY,
    title varchar not null,
    description VARCHAR NOT NULL,
    type INTEGER NOT NULL,
    status INTEGER NOT NULL,
    image VARCHAR NOT NULL,
    import_stars decimal default 0,
    out_series INTEGER default 0,
    planned_series INTEGER default 0,
    views INTEGER default 0,
    country INTEGER not null,
    released_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE,
    last_update_at TIMESTAMP WITH TIME ZONE,
    min_age_limit integer default 0,
    duration integer default null
);";

        Execute.Sql(sql);
    }

    public override void Down()
    {
        Execute.Sql(@"DROP TABLE IF EXISTS content;");
    }
}