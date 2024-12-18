namespace DataAccess.Contracts.UserControllerContracts.Requests;

public record GetSessionRequest(Guid? RefreshTokenJti);

