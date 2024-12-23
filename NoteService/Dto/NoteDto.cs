namespace NoteService.Dto;

public record NoteDto(Guid Id, Guid UserId, string Title, string Description, DateTime CreatedAt);
