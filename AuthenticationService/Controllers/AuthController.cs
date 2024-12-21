using Microsoft.IdentityModel.JsonWebTokens;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthRepositoryService _authService;

    public AuthController(AuthRepositoryService authService)
    {
        _authService = authService;
    }


    [HttpDelete("logout")]
    [SwaggerOperation(
        Summary = "Выход",
        Description = "Этот метод деактивирует сеесию пользователя.")]
    public async Task<IActionResult> Logout()
    {
        string? refreshToken = HttpContext.Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest("Refresh token not found.");
        }

        var jwtToken = _authService.ExtractClaimsFromRefreshToken(refreshToken);

        if (jwtToken is null)
        {
            return BadRequest();

        }

        await _authService.DeleteSessionUser(new DeleteSessionRequest(jwtToken.RefreshTokenJti));

        Response.Cookies.Append("refreshToken", "", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Ok("Logout successful.");
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Логин пользователя",
        Description = "Этот метод авторизует пользователя, создаёт ему JWT токены и сессию в БД.")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null
            || string.IsNullOrWhiteSpace(loginRequest.Login)
            || string.IsNullOrWhiteSpace(loginRequest.Password))
        {
            return BadRequest("Invalid login credentials.");
        }

        var jwtToken = await _authService.AuthenticationUserAsync(loginRequest.Login, loginRequest.Password);

        if (jwtToken is null)
        {
            return Unauthorized();
        }

        SetRefreshTokenCookie(jwtToken);

        return Ok(new
        {
            jwtToken.AccessToken,
        });


    }

    [HttpPost("refreshToken")]
    [SwaggerOperation(
        Summary = "Обновление refresh токен",
        Description = "Этот метод обновляет refresh токен, соверашая проверку по access токену.")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
    {
        if ( refreshRequest is null
            ||string.IsNullOrWhiteSpace(refreshRequest.RefreshToken))
        {
            return BadRequest("Invalid refresh token.");
        }

        var tokenClaims = _authService.ExtractClaimsFromRefreshToken(refreshRequest.RefreshToken);

        if (tokenClaims == null)
        {
            return Unauthorized();
        }

        var jti = tokenClaims.RefreshTokenJti;

        if (!await _authService.ValidateRefreshToken(jti))
        {
            return Unauthorized();
        }

        string? newAccessToken = _authService.GenerateAccessToken(refreshRequest.RefreshToken.ToString());
        var newRefreshToken = _authService.GenerateRefreshToken(refreshRequest.RefreshToken.ToString());

        var jwtToken = _authService.ExtractClaimsFromRefreshToken(newRefreshToken.Token);

        if (!await _authService.UpdateSessionUser(new UpdateSessionRequest(jwtToken.RefreshTokenExpiration,jwtToken.RefreshTokenJti,jti)))
        {
            return Unauthorized();
        }
        JwtTokenModel jwtTokenModel = new JwtTokenModel
        {
            RefreshToken = newRefreshToken.Token,
            RefreshTokenExpiration = newRefreshToken.Expiration
        };
        SetRefreshTokenCookie(jwtTokenModel);
        return Ok(new
        {
            AccessToken = newAccessToken,
        });
        
    }


    // TODO Убрать
    [HttpGet("getRefreshToken")]
    [SwaggerOperation(
        Summary = "Получить refresh токен",
        Description = "Этот метод получает активный refresh токен")]
    public string GetRefreshToken()
    {
        string value = "test";
        if (Request.Cookies["refreshToken"] != null)
        {
            value = Request.Cookies["refreshToken"];
        }
        return value;
    }


    private void SetRefreshTokenCookie(JwtTokenModel token)
    {
        try
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = token.RefreshTokenExpiration
            };
            if (token.RefreshToken != null)
                Response.Cookies.Append("refreshToken", token.RefreshToken, cookieOptions);
        }
        catch
        {
            Console.WriteLine("Invalid token!");
        }
    }
}