namespace AuthenticationService.Contracts;

public record GetSessionRequest(Guid? RefreshTokenJti);

