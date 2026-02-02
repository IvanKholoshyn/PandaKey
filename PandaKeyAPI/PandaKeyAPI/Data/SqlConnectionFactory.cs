using System.Data;
using Microsoft.Data.SqlClient;

namespace PandaKey.Api.Data;

public sealed class SqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");
    }

    public SqlConnection Create() => new SqlConnection(_connectionString);
}
