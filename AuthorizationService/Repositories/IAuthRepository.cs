using Dapper;
using AuthorizationService.Database;
using AuthorizationService.Model;

namespace AuthorizationService.Repositories;

public interface IAuthRepository
{
	Task AddAuthAsync(Authorization authData, CancellationToken token = default);
	Task<Authorization?> GetAuthByIdAsync(Guid id, CancellationToken token = default);
}

public class AuthRepository(IDbConnectionFactory dbConnectionFactory) : IAuthRepository
{
	public async Task AddAuthAsync(Authorization authData, CancellationToken token = default)
	{
		using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
		
		await connection.ExecuteAsync(new CommandDefinition("""
			insert into authorizations
		    values (@Id, @Secret, @Salt)
		""", authData, cancellationToken: token));
	}
	
	public async Task<Authorization?> GetAuthByIdAsync(Guid id, CancellationToken token = default)
	{
		using var connection = await dbConnectionFactory.CreateConnectionAsync(token);
		
		return await connection.QuerySingleOrDefaultAsync<Authorization>(new CommandDefinition("""
			select * from authorizations
			where id = @Id
		""", new { Id = id }, cancellationToken: token));
	}
}
