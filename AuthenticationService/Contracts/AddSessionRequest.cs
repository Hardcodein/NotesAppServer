namespace AuthenticationService.Contracts;

public record AddSessionRequest(Guid? User_Id, DateTime? RefreshTokenExpiration, Guid? RefreshTokenJti);
