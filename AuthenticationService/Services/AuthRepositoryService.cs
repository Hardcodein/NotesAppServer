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


    public async Task<UserModel?> AuthenticationUserAsync(string? userLogin, string? userPassword)
    {
        if (string.IsNullOrWhiteSpace(userLogin) || string.IsNullOrWhiteSpace(userPassword))
        {
            return null;
        }

        var GetUserResponse = await _httpClient.GetAsync($"{Links.DbProvider}/api/User/GetUser?login={userLogin}");

        if (GetUserResponse.IsSuccessStatusCode)
        {
            var user = await GetUserResponse.Content.ReadFromJsonAsync<UserModel>();

            if (user != null)
            {
                // Хэширование пароля
                string? hashedPassword = Cryptography.HashPassword(userPassword);
                string? hashedPasswordSalt = Cryptography.ApplySalt(hashedPassword, user.SaltPassword);

                if (user.Password == hashedPasswordSalt)
                {
                    // Генерация JWT токенов
                    JwtTokenModel tokens = user.JwtTokens;

                    tokens.AccessToken = GenerateAccessToken(user.Uid.ToString());

                    (tokens.RefreshToken, tokens.RefreshTokenExpiration) =
                        GenerateRefreshToken(user.Uid.ToString());

                    tokens.RefreshTokenJti = GetJtiFromToken(tokens.RefreshToken);

                    if (!await AddSessionUser(user))
                    {
                        return null;
                    }
                    return user;
                }
            }
        }

        return null;
    }

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
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsModel.Secret));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            DateTime expiration = DateTime.Now.AddDays(_jwtSettingsModel.RefreshTokenExpirationDays);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtSettingsModel.Issuer,
                audience: _jwtSettingsModel.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds);
            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }

        return (null, DateTime.MinValue);
    }

    public async Task<bool> ValidateRefreshToken(Guid userUid, Guid? refreshTokenJti)
    {
        if (Guid.Empty.Equals(userUid) || Guid.Empty.Equals(refreshTokenJti))
        {
            return false;
        }
        var GetSessionuri = $"{Links.DbProvider}/api/User/GetSessionUser?refreshTokenJti={refreshTokenJti}";

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
                    await DeleteSessionUser(userUid,jwtTokenModel.RefreshTokenJti);
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    public async Task<bool> AddSessionUser(UserModel userModel)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(userModel, jsonSettings), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{Links.DbProvider}/api/User/AddSessionUser", jsonContent);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateSessionUser(UserModel? userModel, Guid? oldRefreshTokenJti)
    {
        if (userModel == null)
            throw new ArgumentNullException(nameof(userModel));

        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        var payload = new
        {
            User = userModel,
            OldRefreshTokenJti = oldRefreshTokenJti
        };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(payload, jsonSettings), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{Links.DbProvider}/api/User/UpdateSessionUser", jsonContent);
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> DeleteSessionUser(Guid uidUser,Guid? refreshTokenJti)
    {
        var query = $"?userId={uidUser}&sessionId={refreshTokenJti}";
        var response = await _httpClient.DeleteAsync($"{Links.DbProvider}/api/User/DeleteSessionUser{query}");
        return response.IsSuccessStatusCode;
    }

    public UserModel? ExtractClaimsFromRefreshToken(string? refreshToken)
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
                    UserModel userModel = new UserModel()
                    {
                        Uid = Guid.Parse(userUid),
                        JwtTokens =
                        {
                            RefreshTokenJti = Guid.Parse(jti),
                            RefreshTokenExpiration = expiration
                        }
                    };
                    return userModel;
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
}