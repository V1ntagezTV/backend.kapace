using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202502280053)]
public class AddVerificationRepository : Migration
{
    public override void Up()
    {
        const string sql = 
            """
            CREATE TYPE verification_code_type AS (
                id BIGINT,
                user_id BIGINT,
                code_hash TEXT,
                created_at TIMESTAMP WITH TIME ZONE,
                expires_at TIMESTAMP WITH TIME ZONE,
                is_used BOOLEAN,
                attempts INT,
                type INT
            );
            
            CREATE TABLE IF NOT EXISTS verification_codes (
                id BIGSERIAL PRIMARY KEY,
                user_id BIGINT NOT NULL,
                code_hash TEXT NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
                is_used BOOLEAN NOT NULL DEFAULT FALSE,
                attempts INT NOT NULL,
                type INT NOT NULL
            );
            
            CREATE INDEX IF NOT EXISTS idx_verification_codes_user_id ON verification_codes (user_id);
            CREATE INDEX IF NOT EXISTS idx_verification_codes_user_id_type ON verification_codes (user_id, type);
            CREATE INDEX IF NOT EXISTS idx_verification_codes_created_at ON verification_codes (created_at);
            """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = 
            """
            DROP TABLE verification_codes;
            DROP TYPE verification_code_type;
            """;
        Execute.Sql(sql);
    }
}