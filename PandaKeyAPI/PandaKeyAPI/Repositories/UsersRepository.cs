using Microsoft.Data.SqlClient;
using PandaKey.Api.Data;
using PandaKey.Api.Models;

namespace PandaKey.Api.Repositories;

public sealed class UsersRepository
{
    private readonly SqlConnectionFactory _factory;

    public UsersRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<UserDto>> GetTopAsync(int top, CancellationToken ct)
    {
        var list = new List<UserDto>(top);

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT TOP (@top)
  UserId, DepartmentId, FullName, Email, Phone, IsActive, CreatedAt
FROM Users
ORDER BY UserId;
";
        cmd.Parameters.Add(new SqlParameter("@top", top));

        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new UserDto
            {
                UserId = rd.GetInt32(0),
                DepartmentId = rd.IsDBNull(1) ? null : rd.GetInt32(1),
                FullName = rd.GetString(2),
                Email = rd.GetString(3),
                Phone = rd.IsDBNull(4) ? null : rd.GetString(4),
                IsActive = rd.GetBoolean(5),
                CreatedAt = rd.GetDateTime(6)
            });
        }

        return list;
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT UserId, DepartmentId, FullName, Email, Phone, IsActive, CreatedAt
FROM Users
WHERE UserId = @id;
";
        cmd.Parameters.Add(new SqlParameter("@id", id));

        await using var rd = await cmd.ExecuteReaderAsync(ct);
        if (!await rd.ReadAsync(ct)) return null;

        return new UserDto
        {
            UserId = rd.GetInt32(0),
            DepartmentId = rd.IsDBNull(1) ? null : rd.GetInt32(1),
            FullName = rd.GetString(2),
            Email = rd.GetString(3),
            Phone = rd.IsDBNull(4) ? null : rd.GetString(4),
            IsActive = rd.GetBoolean(5),
            CreatedAt = rd.GetDateTime(6)
        };
    }

    public async Task<int> CreateAsync(CreateUserRequest req, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Users(DepartmentId, FullName, Email, Phone, PasswordHash, IsActive)
OUTPUT INSERTED.UserId
VALUES(@depId, @fullName, @email, @phone, @pwdHash, 1);
";

        cmd.Parameters.Add(new SqlParameter("@depId", (object?)req.DepartmentId ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@fullName", req.FullName));
        cmd.Parameters.Add(new SqlParameter("@email", req.Email));
        cmd.Parameters.Add(new SqlParameter("@phone", (object?)req.Phone ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@pwdHash", req.PasswordHash));

        var newIdObj = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(newIdObj);
    }
}
