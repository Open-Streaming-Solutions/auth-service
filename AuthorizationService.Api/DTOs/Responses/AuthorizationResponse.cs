namespace AuthorizationService.Api.DTOs.Responses;

public record AuthorizationResponse(string Salt, string Challenge);
