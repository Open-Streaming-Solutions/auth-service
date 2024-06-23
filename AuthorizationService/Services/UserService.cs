namespace AuthorizationService.Services;

public interface IUserService
{
	public Task<Guid?> GetUserIdByUsername(string username);
	public Task CreateUser(Guid id, string username);
}

public class UserServiceMock : IUserService
{
	public async Task<Guid?> GetUserIdByUsername(string username)
	{
		await Task.Delay(2000);
		
		return username == "admin" ? Guid.Parse("fee86a4e-8e26-47b8-bd84-1c2a19216d74") : null;
	}
	
	public async Task CreateUser(Guid id, string username)
	{
		await Task.Delay(2000);
	}
}
