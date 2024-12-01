namespace DataAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly NoteRepositoryService _noteRepositoryService;
    public NoteController(NoteRepositoryService noteRepositoryService)
    {
        _noteRepositoryService = noteRepositoryService;
    }


    [HttpPost("CreateNote")]
    public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequest noteRequest, CancellationToken token)
    {
        if (noteRequest is null)
            return BadRequest(Constants.NoValidDataMessage);


        var result = await _noteRepositoryService.CreateNote(noteRequest, token);

        return result ?
            Ok() :
            BadRequest(Constants.InvalidDataMessageToCreateNote);
    }

    [HttpGet("GetNote")]
    public async Task<IActionResult> GetNote([FromQuery] GetNotesRequest noteRequest, CancellationToken token)
    {
        if (noteRequest is null)
            return BadRequest(Constants.NoValidDataMessage);

        var notesFromDatabase = await _noteRepositoryService.GetNote(noteRequest, token);
        if (!notesFromDatabase.Any() || notesFromDatabase is null)
            return BadRequest("No items");

        return Ok(new GetNotesResponse(notesFromDatabase));
    }

    [HttpDelete("DeleteNote")]
    public async Task <IActionResult> DeleteNotes([FromBody] List<DeleteNoteRequest> deleteNoteRequest,CancellationToken token)
    {
        if(deleteNoteRequest is null)
            return BadRequest(Constants.NoValidDataMessage);
        
       var result =  await _noteRepositoryService.DeleteNotes(deleteNoteRequest.ToDictionary(x => x.Id!.Value),token);
        
        return result ?
            Ok(): 
            BadRequest(Constants.InvalidDataMessageToDeleteNote);
    }
}

