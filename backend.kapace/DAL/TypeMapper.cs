using backend.kapace.DAL.Models;
using Npgsql;
using Npgsql.NameTranslation;

namespace backend.kapace.DAL;

public static class TypeMapper
{
    private static readonly INpgsqlNameTranslator TranslatorForClasses = new NpgsqlSnakeCaseNameTranslator();
    
    public static NpgsqlDataSourceBuilder MapComposites(this NpgsqlDataSourceBuilder builder, string connectionString)
    {
        Console.WriteLine($"{nameof(TypeMapper)} starts composite types mapping");
        builder.MapComposite<Content>("content", TranslatorForClasses);
        Console.WriteLine($"{nameof(TypeMapper)} end composite types mapping");

        return builder;
    }
}