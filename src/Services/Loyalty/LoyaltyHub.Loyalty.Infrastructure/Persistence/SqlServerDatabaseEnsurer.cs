using Microsoft.Data.SqlClient;

namespace LoyaltyHub.Loyalty.Infrastructure.Persistence;

internal static class SqlServerDatabaseEnsurer
{
    public static async Task EnsureDatabaseExistsAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Connection string must specify an Initial Catalog (Database).");
        }

        builder.InitialCatalog = "master";

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = @name)
                CREATE DATABASE [{databaseName.Replace("]", "]]")}];
            """;
        command.Parameters.AddWithValue("@name", databaseName);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
