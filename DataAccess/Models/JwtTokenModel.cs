namespace DataAccess.Models;

public class JwtTokenModel
{
    public Guid Id { get; set; }
    public Guid Session_Id { get; set; }
    public DateTime? RefreshTokenExpiration { get; set; }
    public Guid? RefreshTokenJti { get; set; }
    public virtual SessionUserModel? SessionUser { get; set; }
}
