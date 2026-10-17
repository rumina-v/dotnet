using Npgsql;

namespace UserRegistration.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(NpgsqlDataSource dataSource)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS app_users (
                id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                login varchar(100) NOT NULL CHECK (length(btrim(login)) > 0),
                password_hash text NOT NULL,
                first_name varchar(100) NOT NULL,
                last_name varchar(100) NOT NULL,
                created_date timestamptz NOT NULL,
                updated_date timestamptz NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ux_app_users_login
                ON app_users (lower(login));
            """;

        await using var command = dataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync();
    }
}
