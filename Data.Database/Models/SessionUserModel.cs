namespace Database.Data.Models;

public class SessionUserModel
{
    public Guid? UserId { get; set; }
    public DateTime BeginSessionDate { get; set; }
    public SessionUserModel(UserModel user)
    {
        UserId = user.Id;
        BeginSessionDate = DateTime.UtcNow;
    }
}
