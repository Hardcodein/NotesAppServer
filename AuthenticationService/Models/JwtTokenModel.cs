namespace AuthenticationService.Models;

public class JwtTokenModel
{
    public Guid Id { get; set; }
    public Guid Session_Id { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
    public Guid? RefreshTokenJti { get; set; }

}
