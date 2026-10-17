using Approvals.Api.Models;
using Npgsql;

namespace Approvals.Api.Data;

public class RequestRepository(NpgsqlDataSource dataSource)
{
    private const string Columns = "id, user_id, title, description, status, created_date, decided_date";

    public static async Task InitializeAsync(NpgsqlDataSource dataSource)
    {
        await using var command = dataSource.CreateCommand("""
            CREATE TABLE IF NOT EXISTS approval_requests (
                id uuid PRIMARY KEY,
                user_id integer NOT NULL CHECK (user_id > 0),
                title varchar(200) NOT NULL CHECK (length(btrim(title)) > 0),
                description varchar(2000) NOT NULL CHECK (length(btrim(description)) > 0),
                status varchar(8) NOT NULL CHECK (status IN ('pending', 'approved', 'rejected')),
                created_date timestamptz NOT NULL,
                decided_date timestamptz NULL,
                CHECK ((status = 'pending' AND decided_date IS NULL)
                    OR (status <> 'pending' AND decided_date IS NOT NULL))
            );
            CREATE INDEX IF NOT EXISTS ix_approval_requests_user_id ON approval_requests (user_id);
            """);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<ApprovalRequest> CreateAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand($"""
            INSERT INTO approval_requests (id, user_id, title, description, status, created_date)
            VALUES ($1, $2, $3, $4, 'pending', $5) RETURNING {Columns}
            """);
        command.Parameters.AddWithValue(Guid.NewGuid());
        command.Parameters.AddWithValue(request.UserId);
        command.Parameters.AddWithValue(request.Title.Trim());
        command.Parameters.AddWithValue(request.Description.Trim());
        command.Parameters.AddWithValue(DateTime.UtcNow);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return ReadRequest(reader);
    }

    public async Task<ApprovalRequest?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand($"SELECT {Columns} FROM approval_requests WHERE id = $1");
        command.Parameters.AddWithValue(id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadRequest(reader) : null;
    }

    public async Task<List<ApprovalRequest>> ListAsync(int? userId, CancellationToken cancellationToken)
    {
        var sql = $"SELECT {Columns} FROM approval_requests";
        if (userId.HasValue) { sql += " WHERE user_id = $1"; }
        await using var command = dataSource.CreateCommand(sql + " ORDER BY created_date, id");
        if (userId.HasValue) { command.Parameters.AddWithValue(userId.Value); }
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var requests = new List<ApprovalRequest>();
        while (await reader.ReadAsync(cancellationToken)) { requests.Add(ReadRequest(reader)); }
        return requests;
    }

    public async Task<ApprovalRequest?> DecideAsync(Guid id, string status, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand($"""
            UPDATE approval_requests SET status = $1, decided_date = $2
            WHERE id = $3 AND status = 'pending' RETURNING {Columns}
            """);
        command.Parameters.AddWithValue(status);
        command.Parameters.AddWithValue(DateTime.UtcNow);
        command.Parameters.AddWithValue(id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadRequest(reader) : null;
    }

    private static ApprovalRequest ReadRequest(NpgsqlDataReader reader) => new(
        reader.GetGuid(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3),
        reader.GetString(4), reader.GetDateTime(5), reader.IsDBNull(6) ? null : reader.GetDateTime(6));
}
