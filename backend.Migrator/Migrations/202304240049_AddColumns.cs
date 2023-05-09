using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202304240049)]
public class AddColumns : Migration
{
    public override void Up()
    {
        const string sql = @"
alter table content_translation add column if not exists content_id bigint not null default 0;
alter table content add column if not exists eng_title varchar null;
alter table content add column if not exists channel varchar null;
alter table content add column if not exists origin_title varchar null;";

        Execute.Sql(sql);
    }
    
    public override void Down()
    {
        const string sql = @"
alter table content_translation drop column if exists content_id;
alter table content drop column if exists eng_name;
alter table content drop column if exists channel;
alter table content drop column if exists origin_title;";

        Execute.Sql(sql);
    }
}