namespace AuthorizationService.Model;

public class Session
{
	public Guid Id { get; set; }
	public string UserAgent { get; set; }
	public DateTime IssuedAt { get; set; }
	public DateTime LastRefresh { get; set; }
}
