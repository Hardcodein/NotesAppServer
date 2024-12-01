namespace DataAccess.Models;

public class NoteModel
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public DateTime CreatedAt { get; init; }

    public NoteModel(string title, string description)
    {
        Title = title;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }
}
