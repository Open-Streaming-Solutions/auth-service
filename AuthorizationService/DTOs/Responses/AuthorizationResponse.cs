namespace AuthorizationService.DTOs.Responses;

public record AuthorizationResponse(string Salt, string Challenge);
