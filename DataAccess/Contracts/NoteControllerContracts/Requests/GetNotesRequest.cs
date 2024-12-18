namespace DataAccess.Contracts.NoteControllerContracts.Requests;

public record GetNotesRequest(string? Search, string? SortItem, string? SortOrder);


