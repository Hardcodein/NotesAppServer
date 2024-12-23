namespace DataAccess.Contracts.NoteControllerContracts.Requests;


public record CreateNoteRequest(Guid? UserId, string? Title, string? Description);


