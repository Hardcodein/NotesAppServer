namespace AuthenticationService.Contracts;

public record UpdateSessionRequest(DateTime? RefreshTokenExpiration, Guid? RefreshTokenJti, Guid? OldRefreshTokenJti);

