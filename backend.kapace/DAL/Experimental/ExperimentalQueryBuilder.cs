﻿using System.Text;
using System.Threading.RateLimiting;
using Dapper;
using Microsoft.IdentityModel.Tokens;

namespace backend.kapace.DAL.Experimental;

internal sealed class ExperimentalQueryBuilder
{
    private static readonly Random Random = new();
    private static readonly int DefaultTimeoutInSeconds = 5;
    private readonly string _initSql;
    private readonly DynamicParameters _dynamicParameters = new();
    private readonly List<string> _whereSqlFilters = new();
    private string[] _orderByColumns = Array.Empty<string>();
    private int? _limit;
    private int? _offset;

    private bool _canReturnWholeTable = false;

    internal ExperimentalQueryBuilder(string initSql)
    {
        _initSql = initSql;
    }

    internal CommandDefinition Build(CancellationToken token)
    {
        var sql = _initSql;
        
        if (_whereSqlFilters.Count != 0)
        {
            sql = _initSql + $" AND {string.Join(" AND ", _whereSqlFilters)}";
        }
        else
        {
            if (!_canReturnWholeTable)
            {
                throw new ArgumentException("SQL without any filters");
            }
        }

        if (_orderByColumns.Length != 0)
        {
            sql += $" ORDER BY {string.Join(", ", _orderByColumns)} ";
        }
        
        if (_limit > 0)
        {
            sql += " LIMIT @Limit ";
            _dynamicParameters.Add("@Limit", _limit);
        }

        if (_offset >= 0)
        {
            sql += " OFFSET @Offset ";
            _dynamicParameters.Add("@Offset", _offset);
        }

        return new CommandDefinition(
            sql, 
            _dynamicParameters,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);
    }
    
    internal ExperimentalQueryBuilder Where<T>(string columnName, T? value) where T: struct
    {
        return value is null ? this : Where(columnName, value.Value);
    }
    
    internal ExperimentalQueryBuilder Where<T>(string columnName, T value) where T: struct
    {
        var rndName = GenerateRandomString(10);
        _dynamicParameters.Add(rndName, value);
        _whereSqlFilters.Add($"{columnName} = @{rndName}");
        
        return this;
    }
    
    internal ExperimentalQueryBuilder WhereAny<T>(string columnName, T[]? value) where T: struct
    {
        if (value.IsNullOrEmpty())
        {
            return this;
        }

        var rndName = GenerateRandomString(10);
        _dynamicParameters.Add(rndName, value);
        _whereSqlFilters.Add($"{columnName} = ANY(@{rndName})");
        
        return this;
    }
    
    internal ExperimentalQueryBuilder WhereAny(string columnName, string[]? value)
    {
        if (value.IsNullOrEmpty())
        {
            return this;
        }

        var rndName = GenerateRandomString(10);
        _dynamicParameters.Add(rndName, value);
        _whereSqlFilters.Add($"{columnName} = ANY(@{rndName})");
        
        return this;
    }

    internal ExperimentalQueryBuilder Custom<T>(string sql, string sqlValueName, T? value)
    {
        if (value == null)
        {
            return this;
        }
        
        _dynamicParameters.Add(sqlValueName, value);
        _whereSqlFilters.Add(sql);

        return this;
    }
    
    public ExperimentalQueryBuilder Like(string columnName, string? searchInput)
    {
        if (string.IsNullOrEmpty(searchInput))
        {
            return this;
        }
        
        var rndName = GenerateRandomString(10);
        _dynamicParameters.Add(rndName, searchInput);
        _whereSqlFilters.Add($"{columnName} ILIKE CONCAT('%',@{rndName},'%')");

        return this;
    }

    public ExperimentalQueryBuilder CanReturnWholeTable(bool value)
    {
        _canReturnWholeTable = value;

        return this;
    }

    public ExperimentalQueryBuilder AddPaging(int? limit, int? offset)
    {
        _limit = limit;
        _offset = offset;
        
        return this;
    }

    public ExperimentalQueryBuilder OrderBy(params string[] columns)
    {
        _orderByColumns = columns.Where(c => c != null).ToArray();

        return this;
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var builder = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            builder.Append(chars[Random.Next(chars.Length)]);
        }

        return builder.ToString();
    }
}