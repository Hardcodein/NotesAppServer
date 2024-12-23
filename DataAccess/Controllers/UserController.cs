
﻿namespace DataAccess.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserRepositoryService _userRepositoryService;
    public UserController(UserRepositoryService userRepositoryService)
    {
        _userRepositoryService = userRepositoryService;
    }

    [HttpGet("GetUser")]
    public ActionResult GetUser([FromQuery] string login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return BadRequest("Login is required.");
        }

        UserModel? user = _userRepositoryService.GetUser(login);

        if (user == null)
        {
            return NotFound("User not found.");
        }
        return Ok(user);
    }

    [HttpGet("GetSessionUser")]
    public ActionResult GetSessionUser([FromQuery] GetSessionRequest getSessionRequest, CancellationToken token)
    {
        if (Guid.Empty.Equals(getSessionRequest.RefreshTokenJti))
        {
            return BadRequest("No valid data.");
        }

        GetSessionResponse? session = _userRepositoryService.GetSessionUser(getSessionRequest);

        if (session == null)
        {
            return NotFound("Session not found.");
        }
        return Ok(session);
    }

    [HttpPost("AddSessionUser")]
    public ActionResult AddSessionUser([FromBody] AddSessionRequest addSessionRequest, CancellationToken token)
    {
        if (addSessionRequest == null)
        {
            return BadRequest("No valid data.");
        }
        return _userRepositoryService.AddSessionUser(addSessionRequest, token)
            ? Ok()
            : BadRequest("Invalid data or failed to add session.");
    }

    [HttpPut("UpdateSessionUser")]
    public ActionResult UpdateSessionUser([FromBody] UpdateSessionRequest updateSessionRequest, CancellationToken token)
    {
        if (updateSessionRequest == null)
        {
            return BadRequest("No valid data.");
        }


        // Передача параметров в сервис
        return _userRepositoryService.UpdateSessionUser(updateSessionRequest, token)
            ? Ok()
            : BadRequest("Invalid data or failed to update session.");
    }

    [HttpDelete("DeleteSessionUser")]
    public ActionResult DeleteSessionUser([FromQuery] DeleteSessionRequest deleteSessionRequest, CancellationToken token)
    {
        if (deleteSessionRequest == null)
            return BadRequest("No valid data.");


        var result = _userRepositoryService.DeleteSessionUser(deleteSessionRequest, token);
        return result ? Ok() : BadRequest("Invalid data or failed to delete session.");
    }

}