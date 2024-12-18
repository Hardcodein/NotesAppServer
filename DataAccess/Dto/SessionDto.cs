namespace DataAccess.Dto;

public record SessionDto (DateTime? RefreshTokenExpiration, Guid? RefreshTokenJti); 

