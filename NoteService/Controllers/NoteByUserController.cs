namespace NoteService.Controllers;


[ApiController]
[Route("api/[controller]")]
public class NoteByUserController : ControllerBase
{
    private readonly INoteByUserRepository NoteByUserRepository;

    public NoteByUserController(
        INoteByUserRepository noteByUserRepository)
    {

        NoteByUserRepository = noteByUserRepository;
    }

    [HttpPost("CreateNoteByUser")]
    public async Task<IActionResult> CreateNoteByUser([FromBody] CreateNoteByUserRequest createNoteByUserRequest, CancellationToken token)
    {
        if (createNoteByUserRequest is null)
            return BadRequest($"{Constants.NullReferenceMessage} {nameof(createNoteByUserRequest)}");

        var statusOfCreationNoteByUser = await NoteByUserRepository.CreateNoteByUserAsync(createNoteByUserRequest, token);

        return statusOfCreationNoteByUser ?
            Ok() :
            BadRequest(Constants.CreateNoteMessageError);
    }
    [HttpGet("GetNotesByUser")]
    public async Task<IActionResult> GetNotesByUser([FromQuery] GetNotesByUserRequest getNotesByUserRequest, CancellationToken token)
    {
        if (getNotesByUserRequest is null)
            return BadRequest($"{Constants.NullReferenceMessage} {nameof(getNotesByUserRequest)}");

        var getNotesByUserResponse = await NoteByUserRepository.GetNotesByUserAsync(getNotesByUserRequest, token);

        if (!getNotesByUserResponse.Any() || getNotesByUserResponse is null)
            return BadRequest(Constants.GetNotesByUserMessageError);

        return Ok(new GetNotesByUserResponse(getNotesByUserResponse));
    }

    [HttpDelete("DeleteNoteByUser")]
    public async Task<IActionResult> DeleteNoteByUser([FromBody] List<DeleteNoteByUserRequest> deleteNoteByUserRequest, CancellationToken token)
    {
        if (deleteNoteByUserRequest is null)
            return BadRequest($"{Constants.NullReferenceMessage} {nameof(deleteNoteByUserRequest)}");

        var statusofDeleteNoteByUser = await NoteByUserRepository.DeleteNoteByUserAsync(deleteNoteByUserRequest, token);

        return statusofDeleteNoteByUser ?
            Ok() :
            BadRequest(Constants.DeleteNoteByUserMessageError);
    }
}
