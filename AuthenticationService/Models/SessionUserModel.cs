namespace AuthenticationService.Models;

public class SessionUserModel
{
    public Guid Id { get; set; }
    public Guid User_Id { get; set; }
    public Guid Token_Id { get; set; }
}
