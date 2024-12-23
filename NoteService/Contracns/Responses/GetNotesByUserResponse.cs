namespace NoteService.Contracns.Responses;

public record GetNotesByUserResponse(List<NoteDto> notes);
