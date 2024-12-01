namespace DataAccess.Models;

public class UserModel
{
    public Guid? Id { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Login { get; set; }

    public UserModel(
        string userName,
        string password,
        string login
        )
    {
        Id = new Guid();
        UserName = userName;
        Password = password;
        Login = login;
    }
}
