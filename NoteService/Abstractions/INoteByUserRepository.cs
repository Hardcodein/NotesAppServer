namespace NoteService.Abstractions;

public interface INoteByUserRepository
{
    public Task<bool> CreateNoteByUserAsync(CreateNoteByUserRequest createNoteByUserRequest, CancellationToken token);
    public Task<List<NoteDto>> GetNotesByUserAsync(GetNotesByUserRequest getNotesByUserRequest,CancellationToken token);
    public Task<bool> DeleteNoteByUserAsync(List<DeleteNoteByUserRequest> deleteNoteByUserRequest, CancellationToken token);
}