using System.Data;
using Npgsql;

namespace AuthorizationService.Database;

public interface IDbConnectionFactory
{
	Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default);
}

public class NpgsqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
	private readonly string _connectionString = connectionString;
	
	public async Task<IDbConnection> CreateConnectionAsync(CancellationToken token)
	{
		var connection = new NpgsqlConnection(_connectionString);
		await connection.OpenAsync(token);
		return connection;
	}
}
