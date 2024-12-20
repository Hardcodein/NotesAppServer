namespace AuthenticationService.Dto;

public record SessionDto(DateTime? RefreshTokenExpiration, Guid? RefreshTokenJti);

