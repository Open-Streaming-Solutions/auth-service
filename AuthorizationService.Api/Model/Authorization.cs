namespace AuthorizationService.Api.Model;

public record Authorization(Guid Id, string Secret, string Salt);
