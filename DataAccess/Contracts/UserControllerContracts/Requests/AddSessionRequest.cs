namespace DataAccess.Contracts.UserControllerContracts.Requests;

public record AddSessionRequest(Guid? User_Id, DateTime? RefreshTokenExpiration, Guid? RefreshTokenJti);
