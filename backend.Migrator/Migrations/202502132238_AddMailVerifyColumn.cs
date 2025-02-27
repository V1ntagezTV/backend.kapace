using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202502132238)]
public class AddMailVerifyColumn : Migration {
    public override void Up()
    {
        const string initSql = @"
ALTER TABLE users 
    ADD COLUMN is_mail_verified BOOLEAN DEFAULT FALSE;
";
        Execute.Sql(initSql);
    }

    public override void Down()
    {
        const string initSql = @"ALTER TABLE users DROP COLUMN is_mail_verified";
        Execute.Sql(initSql);
    }
}