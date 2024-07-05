namespace AuthorizationService;

public class ApiEndpointsUrls
{
	private const string ApiBase = "api/v1";

	public const string Register = $"{ApiBase}/register";
	
	public const string LoginBase = $"{ApiBase}/login";
	public const string InitLogin = $"{LoginBase}/init";
}
