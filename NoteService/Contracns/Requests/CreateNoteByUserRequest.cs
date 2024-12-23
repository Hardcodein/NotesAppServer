namespace NoteService.Contracns.Requests;

public record CreateNoteByUserRequest(Guid? UserId, string? Title, string? Description);
