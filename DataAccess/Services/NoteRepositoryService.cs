namespace DataAccess.Services;

public class NoteRepositoryService
{
    private readonly DbContextService _dbContext;
    public NoteRepositoryService(DbContextService dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CreateNote(CreateNoteRequest request, CancellationToken token)
    {
        if (request is null)
        {
            return false;
        }
        try
        {
            await _dbContext.AddAsync(new NoteModel()
            {
               Title = request.Title,
               Description = request.Description
            }, token);
            await _dbContext.SaveChangesAsync(token);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        return true;
    }
    public async Task<List<NoteDto>> GetNote(GetNotesRequest getNotesRequest, CancellationToken token)
    {
        var notesDtos = new List<NoteDto>();

        if (getNotesRequest is null)
            return new List<NoteDto>();

        try
        {
            var notesQuery = _dbContext.Notes
            .Where(x => string.IsNullOrWhiteSpace(getNotesRequest.Search)
                || x.Title!.ToLower().Contains(getNotesRequest.Search.ToLower()));

            Expression<Func<NoteModel, object>> selectorKey = getNotesRequest.SortItem?.ToLower() switch
            {
                "date" => note => note.CreatedAt,
                "title" => note => note.Title!,
                _ => note => note.Id,
            };

            var NoteRequest = getNotesRequest.SortOrder == "desc" ?
                notesQuery.OrderByDescending(selectorKey) :
                notesQuery.OrderBy(selectorKey);

            notesDtos = await notesQuery.Select(n => new NoteDto(n.Id, n.Title!, n.Description!, n.CreatedAt)).ToListAsync(cancellationToken: token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }

        return notesDtos;

    }
    public async Task<bool> DeleteNotes(Dictionary<Guid, DeleteNoteRequest> deletedNotes,CancellationToken token)
    {
        if (deletedNotes is null || !deletedNotes.Any())
            return false;

        try
        {

            var notesQuery =  await _dbContext.Notes
                .Where(x => deletedNotes.Keys.Contains(x.Id))
                    .ToListAsync(cancellationToken:token);

            await Task.Run(() => { 
               _dbContext.Notes.RemoveRange(notesQuery);
            }, token);

            await _dbContext.SaveChangesAsync(token);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }


}
