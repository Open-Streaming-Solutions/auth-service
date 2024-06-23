namespace AuthorizationService.Model;

public class Authorization
{
	public Guid Id { get; set; }
	public string Password { get; set; }
	public string Salt { get; set; }
}
