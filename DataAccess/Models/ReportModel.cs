namespace DataAccess.Models;

public class ReportModel
{
    public Guid? UserId { get; set; }
    public string Description { get; set; }
    public ReportModel(UserModel user, string description)
    {
        UserId = user.Id;
        Description = description;
    }
}

