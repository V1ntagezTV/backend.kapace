using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202411150153)]
public class AddUsers : Migration {
    public override void Up()
    {
        const string initSql = @"
CREATE TABLE IF NOT EXISTS users(
    id BIGSERIAL PRIMARY KEY,
    email VARCHAR NOT NULL,
    nickname VARCHAR,
    password_hash VARCHAR,
    created_at TIMESTAMP WITH TIME ZONE NULL);

CREATE TABLE IF NOT EXISTS users_permissions(
    user_id BIGINT,
    permission_id BIGINT,
    gived_by BIGINT,
    created_at TIMESTAMP WITH TIME ZONE NULL,
    PRIMARY KEY (user_id, permission_id));

CREATE TABLE IF NOT EXISTS permissions(
    id BIGSERIAL PRIMARY KEY,
    alias VARCHAR,
    description VARCHAR,
    created_by BIGINT,
    created_at TIMESTAMP WITH TIME ZONE NULL);
";
        Execute.Sql(initSql);
    }

    public override void Down()
    {
        const string initSql = @"
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS users_permissions;
DROP TABLE IF EXISTS permissions;
";
        Execute.Sql(initSql);
    }
}