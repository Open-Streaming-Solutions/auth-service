namespace AuthorizationService.Model;

public record Authorization(Guid Id, string Secret, string Salt);
