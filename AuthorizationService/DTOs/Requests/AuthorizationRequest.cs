namespace AuthorizationService.DTOs.Requests;

/// <summary>
/// Authorization request DTO
/// </summary>
/// <param name="Username">User login</param>
/// <param name="Secret">Hash string calculated by the formula
/// SHA256(scrypt(PASSWORD, SERVER_SALT), SERVER_CHALLENGE, CLIENT_CHALLENGE)</param>
/// <param name="Challenge">Client-generated challenge</param>
public record AuthorizationRequest(string Username, string? Secret, string? Challenge);
