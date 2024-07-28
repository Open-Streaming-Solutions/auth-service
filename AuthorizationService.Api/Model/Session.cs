namespace AuthorizationService.Api.Model;

public record Session
{
	public required Guid Id { get; init; }
	public required string UserAgent { get; init; }
	public required DateTime IssuedAt { get; init; }
	public required DateTime LastRefresh { get; init; }
}
