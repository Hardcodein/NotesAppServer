namespace AuthenticationService.Contracts;
public record DeleteSessionRequest(Guid? RefreshTokenJti);
