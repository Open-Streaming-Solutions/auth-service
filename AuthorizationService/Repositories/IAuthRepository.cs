using Dapper;
using AuthorizationService.Database;
using AuthorizationService.Model;

namespace AuthorizationService.Repositories;

public interface IAuthRepository
{
	Task<Authorization?> GetAuthByIdAsync(Guid id, CancellationToken token = default);
	Task AddAuthAsync(Authorization authData, CancellationToken token = default);
}

public class AuthRepository(IDbConnectionFactory dbConnectionFactory) : IAuthRepository
{
	private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

	public async Task<Authorization?> GetAuthByIdAsync(Guid id, CancellationToken token = default)
	{
		using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
		
		return await connection.QuerySingleOrDefaultAsync<Authorization>(new CommandDefinition("""
			select * from authorizations
			where id = @id
			""", new { id }, cancellationToken: token));
	}

	public async Task AddAuthAsync(Authorization authData, CancellationToken token = default)
	{
		using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
		
		await connection.ExecuteAsync(new CommandDefinition("""
			insert into authorizations
			values (@Id, @Password, @Salt)
			""", authData, cancellationToken: token));
	}
}
