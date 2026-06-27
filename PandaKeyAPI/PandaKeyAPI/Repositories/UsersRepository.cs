using Microsoft.Data.SqlClient;
using PandaKey.Api.Data;
using PandaKey.Api.Models;

namespace PandaKey.Api.Repositories;

public sealed class UsersRepository
{
    private readonly SqlConnectionFactory _factory;

    public UsersRepository(SqlConnectionFactory factory) => _factory = factory;

    private static UserDto Map(SqlDataReader rd) => new UserDto
    {
        UserId = rd.GetInt32(0),
        DepartmentId = rd.IsDBNull(1) ? null : rd.GetInt32(1),
        FullName = rd.GetString(2),
        Email = rd.GetString(3),
        Phone = rd.IsDBNull(4) ? null : rd.GetString(4),
        IsActive = rd.GetBoolean(5),
        Role = rd.IsDBNull(6) ? "user" : rd.GetString(6),
        CreatedAt = rd.GetDateTime(7)
    };

    public async Task<IReadOnlyList<UserDto>> GetTopAsync(int top, CancellationToken ct)
    {
        var list = new List<UserDto>(top);

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT TOP (@top)
  UserId, DepartmentId, FullName, Email, Phone, IsActive, Role, CreatedAt
FROM Users
ORDER BY UserId;
";
        cmd.Parameters.Add(new SqlParameter("@top", top));

        await using var rd = (SqlDataReader)await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
            list.Add(Map(rd));

        return list;
    }

    /// <summary>Returns every user (admin listing, no TOP clamp).</summary>
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct)
    {
        var list = new List<UserDto>();

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT UserId, DepartmentId, FullName, Email, Phone, IsActive, Role, CreatedAt
FROM Users
ORDER BY UserId;
";

        await using var rd = (SqlDataReader)await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
            list.Add(Map(rd));

        return list;
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT UserId, DepartmentId, FullName, Email, Phone, IsActive, Role, CreatedAt
FROM Users
WHERE UserId = @id;
";
        cmd.Parameters.Add(new SqlParameter("@id", id));

        await using var rd = (SqlDataReader)await cmd.ExecuteReaderAsync(ct);
        return await rd.ReadAsync(ct) ? Map(rd) : null;
    }

    /// <summary>
    /// Looks a user up by e-mail and also returns the stored password hash.
    /// Used by the login flow to verify credentials.
    /// </summary>
    public async Task<(UserDto User, string PasswordHash)?> GetByEmailWithHashAsync(string email, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT UserId, DepartmentId, FullName, Email, Phone, IsActive, Role, CreatedAt, PasswordHash
FROM Users
WHERE Email = @email;
";
        cmd.Parameters.Add(new SqlParameter("@email", email));

        await using var rd = (SqlDataReader)await cmd.ExecuteReaderAsync(ct);
        if (!await rd.ReadAsync(ct)) return null;

        var user = Map(rd);
        var hash = rd.IsDBNull(8) ? "" : rd.GetString(8);
        return (user, hash);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE Email = @email;";
        cmd.Parameters.Add(new SqlParameter("@email", email));

        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        return count > 0;
    }

    public async Task<int> CreateAsync(CreateUserRequest req, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Users(DepartmentId, FullName, Email, Phone, PasswordHash, IsActive, Role)
OUTPUT INSERTED.UserId
VALUES(@depId, @fullName, @email, @phone, @pwdHash, 1, 'user');
";

        cmd.Parameters.Add(new SqlParameter("@depId", (object?)req.DepartmentId ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@fullName", req.FullName));
        cmd.Parameters.Add(new SqlParameter("@email", req.Email));
        cmd.Parameters.Add(new SqlParameter("@phone", (object?)req.Phone ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@pwdHash", req.PasswordHash));

        var newIdObj = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(newIdObj);
    }

    /// <summary>Creates a user during registration with a pre-computed BCrypt hash.</summary>
    public async Task<int> RegisterAsync(RegisterRequest req, string passwordHash, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Users(DepartmentId, FullName, Email, Phone, PasswordHash, IsActive, Role)
OUTPUT INSERTED.UserId
VALUES(NULL, @fullName, @email, @phone, @pwdHash, 1, 'user');
";
        cmd.Parameters.Add(new SqlParameter("@fullName", req.FullName));
        cmd.Parameters.Add(new SqlParameter("@email", req.Email));
        cmd.Parameters.Add(new SqlParameter("@phone", (object?)req.Phone ?? DBNull.Value));
        cmd.Parameters.Add(new SqlParameter("@pwdHash", passwordHash));

        var newIdObj = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(newIdObj);
    }

    public async Task<bool> UpdateRoleAsync(int userId, string role, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Users SET Role = @role WHERE UserId = @id;";
        cmd.Parameters.Add(new SqlParameter("@role", role));
        cmd.Parameters.Add(new SqlParameter("@id", userId));

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int userId, CancellationToken ct)
    {
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE UserId = @id;";
        cmd.Parameters.Add(new SqlParameter("@id", userId));

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }
}
