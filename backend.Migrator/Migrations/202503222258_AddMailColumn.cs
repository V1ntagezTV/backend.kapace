using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202503222258)]
public class AddMailColumn : Migration {
    public override void Up()
    {
        const string initSql = """
                               DROP TYPE IF EXISTS verification_code_type;
                               CREATE TYPE verification_code_type AS (
                                   id BIGINT,
                                   user_id BIGINT,
                                   code_hash TEXT,
                                   created_at TIMESTAMP WITH TIME ZONE,
                                   expires_at TIMESTAMP WITH TIME ZONE,
                                   is_used BOOLEAN,
                                   attempts INT,
                                   type INT,
                                   email TEXT
                               );
                               ALTER TABLE verification_codes ADD COLUMN email varchar DEFAULT null;
                               CREATE INDEX IF NOT EXISTS idx_verification_codes_email ON verification_codes (email);
                               """;
        Execute.Sql(initSql);
    }

    public override void Down()
    {
        const string initSql = """
                               ALTER TABLE verification_codes DROP COLUMN email;
                               DROP INDEX idx_verification_codes_email;
                               """;
        
        Execute.Sql(initSql);
    }
}