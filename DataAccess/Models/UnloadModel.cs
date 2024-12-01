namespace DataAccess.Models;

public class UnloadModel
{
    public Guid? UserId { get; set; }
    public string? DetalInformation { get; set; }

    public UnloadModel(UserModel user, string detailInformation)
    {
        UserId = user.Id;
        DetalInformation = detailInformation;
    }
}
