namespace AuthorizationService.DTOs.Requests;

public class AuthorizationRequest
{
	public string Username { get; init; }
	public string Secret { get; init; }
	public string? Challenge { get; init; }
}
