using System.Data;
using Npgsql;

namespace AuthorizationService.Api.Database;

public interface IDbConnectionFactory
{
	Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default);
}

public class NpgsqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
	public async Task<IDbConnection> CreateConnectionAsync(CancellationToken token)
	{
		var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync(token);
		return connection;
	}
}
