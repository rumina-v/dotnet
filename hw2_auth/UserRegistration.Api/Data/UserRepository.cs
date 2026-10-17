using Npgsql;
using UserRegistration.Api.Models;

namespace UserRegistration.Api.Data;

public class UserRepository(NpgsqlDataSource dataSource)
{
    private const string Columns =
        "id, login, password_hash, first_name, last_name, created_date, updated_date";

    public async Task<User?> AddAsync(User user, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand($"""
            INSERT INTO app_users (login, password_hash, first_name, last_name, created_date)
            VALUES ($1, $2, $3, $4, $5)
            ON CONFLICT DO NOTHING
            RETURNING {Columns}
            """);
        command.Parameters.AddWithValue(user.Login);
        command.Parameters.AddWithValue(user.PasswordHash);
        command.Parameters.AddWithValue(user.FirstName);
        command.Parameters.AddWithValue(user.LastName);
        command.Parameters.AddWithValue(user.CreatedDate);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadUser(reader) : null;
    }

    public async Task<User?> FindByLoginAsync(string login, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            $"SELECT {Columns} FROM app_users WHERE lower(login) = lower($1)");
        command.Parameters.AddWithValue(login);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadUser(reader) : null;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            $"SELECT {Columns} FROM app_users WHERE id = $1");
        command.Parameters.AddWithValue(id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadUser(reader) : null;
    }

    public async Task<List<User>> GetAllAsync(UsersQuery query, CancellationToken cancellationToken)
    {
        // Имя столбца выбирается только из двух констант, значения передаются параметрами.
        var dateColumn = query.DateField.Equals("updatedDate", StringComparison.OrdinalIgnoreCase)
            ? "updated_date"
            : "created_date";
        await using var command = dataSource.CreateCommand();
        var conditions = new List<string>();

        if (query.From.HasValue)
        {
            command.Parameters.AddWithValue(query.From.Value.UtcDateTime);
            conditions.Add($"{dateColumn} >= ${command.Parameters.Count}");
        }
        if (query.To.HasValue)
        {
            command.Parameters.AddWithValue(query.To.Value.UtcDateTime);
            conditions.Add($"{dateColumn} <= ${command.Parameters.Count}");
        }

        command.CommandText = $"SELECT {Columns} FROM app_users";
        if (conditions.Count > 0)
        {
            command.CommandText += " WHERE " + string.Join(" AND ", conditions);
        }
        command.CommandText += " ORDER BY id";

        var users = new List<User>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(ReadUser(reader));
        }
        return users;
    }

    public async Task<bool> UpdateAsync(
        int id, string firstName, string lastName, DateTime updatedDate,
        CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("""
            UPDATE app_users
            SET first_name = $1, last_name = $2, updated_date = $3
            WHERE id = $4
            """);
        command.Parameters.AddWithValue(firstName);
        command.Parameters.AddWithValue(lastName);
        command.Parameters.AddWithValue(updatedDate);
        command.Parameters.AddWithValue(id);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("DELETE FROM app_users WHERE id = $1");
        command.Parameters.AddWithValue(id);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static User ReadUser(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Login = reader.GetString(1),
        PasswordHash = reader.GetString(2),
        FirstName = reader.GetString(3),
        LastName = reader.GetString(4),
        CreatedDate = reader.GetDateTime(5),
        UpdatedDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
    };
}
