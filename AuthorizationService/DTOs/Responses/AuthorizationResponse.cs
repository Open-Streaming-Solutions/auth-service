namespace AuthorizationService.DTOs.Responses;

public class AuthorizationResponse
{
	public string Salt { get; init; }
	public string? Challenge { get; init; }
}
