namespace AuthorizationService;

public class ApiEndpointsUrls
{
	private const string ApiBase = "api/v1";

	public const string RegisterBase = $"{ApiBase}/register";
	public const string InitRegistration = $"{RegisterBase}/init";
	
	public const string LoginBase = $"{ApiBase}/login";
	public const string InitLogin = $"{LoginBase}/init";
}
