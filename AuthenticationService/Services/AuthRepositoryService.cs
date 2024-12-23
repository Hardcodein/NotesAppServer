namespace AuthenticationService.Services;

public class AuthRepositoryService
{
    private readonly JwtSettingsModel _jwtSettingsModel;

    private readonly HttpClient _httpClient;

    public AuthRepositoryService(IOptions<JwtSettingsModel> jwtSettings, HttpClient httpClient)
    {
        _jwtSettingsModel = jwtSettings.Value;
        _httpClient = httpClient;
    }


    public async Task<JwtTokenModel?> AuthenticationUserAsync(string? userLogin, string? userPassword)
    {
        try
        {

            if (string.IsNullOrWhiteSpace(userLogin)
                || string.IsNullOrWhiteSpace(userPassword))
            {
                return null;
            }

            var getUserResponse = await _httpClient.GetAsync($"{Links.DataAccessDockerPrivateConnectionUriString}/api/User/GetUser?login={userLogin}");

            if (!getUserResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var user = await getUserResponse.Content.ReadFromJsonAsync<UserModel>();

            if (user == null)
            {
                return null;
            }

            string? hashedPassword = Cryptography.HashPassword(userPassword);
            string? hashedPasswordSalt = Cryptography.ApplySalt(hashedPassword, user.SaltPassword);

            if (user.Password == hashedPasswordSalt)
            {
                var token = new JwtTokenModel
                {
                    AccessToken = GenerateAccessToken(user.Id.ToString())
                };

                (token.RefreshToken, token.RefreshTokenExpiration) = GenerateRefreshToken(user.Id.ToString());

                token.RefreshTokenJti = GetJtiFromToken(token.RefreshToken);

                var AccessToken = GenerateAccessToken(user.Id.ToString());

                if (!await AddSessionUser(new AddSessionRequest(user.Id, token.RefreshTokenExpiration, token.RefreshTokenJti)))
                {
                    return null;
                }
                return token;

            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
        return null;
    }

    public async Task<bool> AddSessionUser(AddSessionRequest addSessionRequest)
    {
        ArgumentNullException.ThrowIfNull(addSessionRequest);

        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(addSessionRequest, jsonSettings), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{Links.DataAccessDockerPrivateConnectionUriString}/api/User/AddSessionUser", jsonContent);

        return response.IsSuccessStatusCode;
    } 

    public async Task<bool> UpdateSessionUser(UpdateSessionRequest updateSessionRequest)
    {
        ArgumentNullException.ThrowIfNull(updateSessionRequest);

        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(updateSessionRequest, jsonSettings), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{Links.DataAccessDockerPrivateConnectionUriString}/api/User/UpdateSessionUser", jsonContent);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSessionUser(DeleteSessionRequest deleteSessionRequest)
    {
        ArgumentNullException.ThrowIfNull(deleteSessionRequest);

        var query = $"?RefreshTokenJti={deleteSessionRequest.RefreshTokenJti}";

        var response = await _httpClient.DeleteAsync($"{Links.DataAccessDockerPrivateConnectionUriString}/api/User/DeleteSessionUser{query}");
        return response.IsSuccessStatusCode;
    }

    #region Работа с токенами
    public Guid? GetJtiFromToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        var jwtToken = tokenHandler.ReadJwtToken(token);

        var jtiClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti);

        if (jtiClaim != null && Guid.TryParse(jtiClaim.Value, out var jtiGuid))
        {
            return jtiGuid;
        }

        return null;
    }

    public async Task<bool> ValidateRefreshToken( Guid? refreshTokenJti)
    {
        if (Guid.Empty.Equals(refreshTokenJti))
        {
            return false;
        }
        var GetSessionuri = $"{Links.DataAccessDockerPrivateConnectionUriString}/api/User/GetSessionUser?refreshTokenJti={refreshTokenJti}";

        var GetSessionResponse = await _httpClient.GetAsync(GetSessionuri);

        if (GetSessionResponse.IsSuccessStatusCode)
        {
            var jsonGetSessionResponse = await GetSessionResponse.Content.ReadAsStringAsync();

            var tokenInformation = JsonConvert.DeserializeObject(jsonGetSessionResponse);

            if (tokenInformation != null)
            {
                JwtTokenModel? jwtTokenModel = JsonConvert.DeserializeObject<JwtTokenModel>(
                    JObject.Parse(jsonGetSessionResponse)["session"]?.ToString() ?? string.Empty
                );

                if (jwtTokenModel != null && jwtTokenModel.RefreshTokenExpiration <= DateTime.UtcNow)
                {
                    await DeleteSessionUser(new DeleteSessionRequest(jwtTokenModel.RefreshTokenJti));
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    public string? GenerateAccessToken(string? userUid)
    {
        if (string.IsNullOrWhiteSpace(userUid))
            return null;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userUid),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(_jwtSettingsModel.Secret))
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsModel.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettingsModel.Issuer,
                audience: _jwtSettingsModel.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettingsModel.AccessTokenExpirationMinutes), // Используем UtcNow
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        throw new InvalidOperationException("JWT secret is not configured.");
    }

    public JwtTokenModel? ExtractClaimsFromRefreshToken(string? refreshToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(refreshToken))
            {
                JwtSecurityToken? jwtToken = handler.ReadJwtToken(refreshToken);

                string? userUid = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                string? jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

                DateTime expiration = jwtToken.ValidTo;

                if (userUid != null && jti != null)
                {
                    JwtTokenModel token = new JwtTokenModel()
                    {

                        RefreshTokenJti = Guid.Parse(jti),
                        RefreshTokenExpiration = expiration

                    };
                    return token;
                }
            }
            return null;
        }
        catch
        {
            Console.WriteLine("No valid token");
            return null;
        }
    }

    public (string? Token, DateTime Expiration) GenerateRefreshToken(string? userUid)
    {
        if (string.IsNullOrWhiteSpace(userUid))
            return (null, DateTime.MinValue);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userUid),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (_jwtSettingsModel.Secret != null)
        {
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettingsModel.Secret));

            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            DateTime expiration = DateTime.Now.AddDays(_jwtSettingsModel.RefreshTokenExpirationDays).ToUniversalTime();

            JwtSecurityToken token = new(
                issuer: _jwtSettingsModel.Issuer,
                audience: _jwtSettingsModel.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }

        return (null, DateTime.MinValue);
    }
    #endregion
}