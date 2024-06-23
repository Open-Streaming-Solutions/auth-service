using System.Text;
using AuthorizationService.DTOs.Requests;
using AuthorizationService.DTOs.Responses;
using AuthorizationService.Model;
using AuthorizationService.Repositories;
using CryptSharp;
using CryptSharp.Utility;

namespace AuthorizationService.Services;

public interface IAuthService
{
	public Task<bool> Register(RegistrationRequest authData);
	
	public Task<AuthorizationResponse?> InitLogin(string username);
	public Task<AuthorizationResponse?> Login(AuthorizationRequest authorizationData);
}

public class AuthService(IAuthRepository authRepository, IUserService userService,
	ICachingService cache, ILogger<AuthService> logger) : IAuthService
{
	// TODO: Logging
	private readonly ILogger<AuthService> _logger = logger;
	private readonly ICachingService _cache = cache;
	private readonly IUserService _userService = userService;
	private readonly IAuthRepository _authRepository = authRepository;
	
	public async Task<bool> Register(RegistrationRequest authData)
	{
		var userId = await _userService.GetUserIdByUsername(authData.Username);

		if (userId.HasValue)
		{
			return false;
		}
		
		var guid = Guid.NewGuid();
		var salt = Crypter.Sha512.GenerateSalt();
		
		var passBytes = Encoding.UTF8.GetBytes(authData.Password);
		var saltBytes = Encoding.UTF8.GetBytes(salt);

		const int cost = 32768, blockSize = 8, parallel = 1, maxThreads = 1, keyLength = 255;

		var scryptedPass = SCrypt.ComputeDerivedKey(
            passBytes,
            saltBytes,
            cost,
            blockSize,
            parallel,
            maxThreads,
            keyLength
		);
		
		await _authRepository.AddAuthAsync(new Authorization
		{
			Id = guid,
			Salt = salt,
			Password = Convert.ToBase64String(scryptedPass)
		});
		await _userService.CreateUser(guid, authData.Username);

		return true;
	}

	public async Task<AuthorizationResponse?> InitLogin(string username)
	{
		var userId = await _userService.GetUserIdByUsername(username);

		if (userId == null)
		{
			return null;
		}
		
		var authData = await _authRepository.GetAuthByIdAsync((Guid)userId);

		if (authData == null)
		{
			return null;
		}

		var authResponse = new AuthorizationResponse
		{
			Salt = authData.Salt,
			Challenge = Crypter.Sha512.GenerateSalt()
		};
		
		await _cache.SetRecordAsync($"LOGIN_SALT_{username}", authResponse, TimeSpan.FromMinutes(5));
		
		return authResponse;
	}

	public async Task<AuthorizationResponse?> Login(AuthorizationRequest authData)
	{
		// TODO: Login method
		
		return null;
	}
}
