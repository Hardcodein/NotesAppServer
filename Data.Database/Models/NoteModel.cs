namespace DataAccess.Models;

public class NoteModel
{
    public Guid Id { get; init; }
    public Guid User_Id { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public virtual UserModel? User { get; set; }
}
