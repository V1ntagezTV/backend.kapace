using FluentMigrator;

namespace backend.Migrator.Migrations;

[Migration(202304091334)]
public class CreateEpisodeTable : Migration {
    public override void Up()
    {
        Execute.Sql(@"
CREATE TABLE IF NOT EXISTS episode(
    id BIGINT PRIMARY KEY,
    content_id BIGINT NOT NULL,
    title VARCHAR,
    image VARCHAR,
    number INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS genre(
    id BIGINT PRIMARY KEY,
    name VARCHAR NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE,
    created_by bigint
);

CREATE TABLE IF NOT EXISTS content_genre(
    content_id BIGINT NOT NULL,
    genre_id BIGINT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE,
    created_by bigint,
    PRIMARY KEY (content_id, genre_id)
);
");
    }

    public override void Down()
    {
        Execute.Sql(@"
DROP TABLE IF EXISTS episode;
DROP TABLE IF EXISTS genre;
DROP TABLE IF EXISTS content_genre;");
    }
}