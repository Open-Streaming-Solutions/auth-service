using Dapper;

namespace AuthorizationService.Api.Database;

public class DbInitializer(IDbConnectionFactory dbConnectionFactory)
{
	public async Task InitializeAsync()
	{
		using var connection = await dbConnectionFactory.CreateConnectionAsync();

		await connection.ExecuteAsync("""
		                                  create table if not exists authorizations (
		                                  id uuid primary key,
		                                  secret varchar not null,
		                                  salt varchar not null);
		                              """);
	}
}
