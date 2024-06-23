using Dapper;

namespace AuthorizationService.Database;

public class DbInitializer(IDbConnectionFactory dbConnectionFactory)
{
	private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

	public async Task InitializeAsync()
	{
		using var connection = await _dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync("""
		                                  create table if not exists authorizations (
		                                  id uuid primary key,
		                                  password varchar not null,
		                                  salt varchar not null);
		                              """);
	}
}
