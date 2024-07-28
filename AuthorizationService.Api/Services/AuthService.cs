using System.Security.Cryptography;
using System.Text;
using AuthorizationService.Api.DTOs.Requests;
using AuthorizationService.Api.DTOs.Responses;
using AuthorizationService.Api.Model;
using AuthorizationService.Api.Repositories;
using CryptSharp;
using CryptSharp.Utility;

namespace AuthorizationService.Api.Services;

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

	public async Task<bool> Register(RegistrationRequest authData)
	{
		var userId = await userService.GetUserIdByUsername(authData.Username);
		
		if (userId.HasValue)
		{
			return false;
		}
		
		var guid = Guid.NewGuid();
		var salt = Crypter.Sha512.GenerateSalt();
		
		var passBytes = Encoding.UTF8.GetBytes(authData.Password);
		var saltBytes = Encoding.UTF8.GetBytes(salt);
		
		const int cost = 16384, blockSize = 8, parallel = 1, maxThreads = 1, keyLength = 255;
		
		var scryptedPass = SCrypt.ComputeDerivedKey(
            passBytes,
            saltBytes,
            cost,
            blockSize,
            parallel,
            maxThreads,
            keyLength
		);
		
		await authRepository.AddAuthAsync(new Authorization(
			guid, GetSha256StringFromString(Convert.ToBase64String(scryptedPass)), salt));
		await userService.CreateUser(guid, authData.Username);
		
		return true;
	}
	
	public async Task<AuthorizationResponse?> InitLogin(string username)
	{
		var userId = await userService.GetUserIdByUsername(username);
		
		if (!userId.HasValue)
		{
			return null;
		}
		
		var authData = await authRepository.GetAuthByIdAsync(userId.Value);
		
		if (authData is null)
		{
			return null;
		}
		
		var authResponse = new AuthorizationResponse(authData.Salt, Crypter.Sha512.GenerateSalt());
		
		await cache.SetRecordAsync($"LOGIN_DATA_{username}", authResponse, TimeSpan.FromMinutes(30));
		
		return authResponse;
	}

	public async Task<AuthorizationResponse?> Login(AuthorizationRequest authData)
	{
		// TODO: Remove this call. Try invariant logic
		var userId = await userService.GetUserIdByUsername(authData.Username);
		
		if (!userId.HasValue)
		{
			return null;
		}
		
		var cachedAuthData = await cache.GetRecordAsync<AuthorizationResponse>($"LOGIN_DATA_{authData.Username}");

		if (cachedAuthData is null)
		{
			return null;
		}

		var storedAuth = await authRepository.GetAuthByIdAsync(userId.Value);
		
		if (storedAuth is null)
		{
			return null;
		}

		var serverData = $"{storedAuth.Secret}{cachedAuthData.Challenge}{authData.Challenge}";
		var serverHash = GetSha256StringFromString(serverData);
		
		if (authData.Secret != serverHash)
		{
			return null;
		}
		
		// TODO: Create new session
		
		throw new NotImplementedException();
	}

	private static string GetSha256StringFromString(string value)
    {
        var builder = new StringBuilder();
        var result = SHA256.HashData(Encoding.UTF8.GetBytes(value));

        foreach (var b in result)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}
