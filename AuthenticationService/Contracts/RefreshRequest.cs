namespace AuthenticationService.Contracts;

public record RefreshRequest(Guid? UserId, string? RefreshToken);

