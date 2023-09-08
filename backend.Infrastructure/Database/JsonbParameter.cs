using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace backend.Infrastructure.Database;

public sealed class JsonbParameter : SqlMapper.ICustomQueryParameter
{
    private readonly string _json;

    public JsonbParameter(string json)
    {
        _json = json;
    }
        
    public void AddParameter(IDbCommand command, string name)
    {
        var parameter = new NpgsqlParameter(name, NpgsqlDbType.Json)
        {
            Value = _json
        };
        
        command.Parameters.Add(parameter);
    }
}