using backend.kapace.DAL.Models;
using Npgsql;

namespace backend.kapace.DAL;

public static class TypeMapper
{
    public static NpgsqlDataSourceBuilder MapComposites(this NpgsqlDataSourceBuilder builder)
    {
        builder.MapComposite<Content>();
        builder.MapComposite<ContentGenreV1>("content_genre_v1");
        return builder;
    }
}