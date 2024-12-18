namespace DataAccess.Contracts.UserControllerContracts.Requests;

public record UpdateSessionRequest(DateTime? RefreshTokenExpiration, Guid? RefreshTokenJti, Guid? OldRefreshTokenJti);

