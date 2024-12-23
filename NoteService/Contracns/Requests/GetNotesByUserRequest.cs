namespace NoteService.Contracns.Requests;

public record GetNotesByUserRequest(Guid? UserId);