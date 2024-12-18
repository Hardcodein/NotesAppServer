using DataAccess.Contracts.UserControllerContracts.Responses;

namespace DataAccess.Services;

public class UserRepositoryService
{
    private readonly DbContextService _dbContext;
    public UserRepositoryService(DbContextService contextService)
    {
        _dbContext = contextService;
    }

    public UserModel? GetUser(string? login)
    {
        UserModel? user;

        try
        {
            if (login == null)
            {
                return null;
            }
            var notesQuery = _dbContext.Users
                .Where(x => !string.IsNullOrWhiteSpace(login)
                    && x.Login.ToLower().Contains(login.ToLower()));

            user = notesQuery.FirstOrDefault();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return user;
    }

    public bool AddSessionUser(AddSessionRequest addSessionRequest, CancellationToken token)
    {
        try
        {

            var existsUser = _dbContext.Users.FirstOrDefault(x => x.Id == addSessionRequest!.User_Id);


            var newSession = new SessionUserModel()
            {

                Id = new Guid(),
                User_Id = existsUser!.Id,
                Token_Id = new Guid(),
            };

            _dbContext.SessionUsers.Add(newSession);

            var jwtToken = new JwtTokenModel()
            {
                Id = newSession.Token_Id,
                Session_Id = newSession.Id,
                RefreshTokenJti = addSessionRequest.RefreshTokenJti,
                RefreshTokenExpiration = addSessionRequest.RefreshTokenExpiration,

            };
            _dbContext.JwtTokenModels.Add(jwtToken);

            _dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new InvalidDataException(e.Message);
        }


        return true;
    }

    public bool UpdateSessionUser(UpdateSessionRequest updateSessionRequest, CancellationToken token)
    {
        try
        {

            var localToken = _dbContext.JwtTokenModels.FirstOrDefault(u => u.RefreshTokenJti == updateSessionRequest.OldRefreshTokenJti);

            if (localToken is not null)
            {
                localToken.RefreshTokenJti = updateSessionRequest.RefreshTokenJti;
                localToken.RefreshTokenExpiration = updateSessionRequest.RefreshTokenExpiration;
                _dbContext.JwtTokenModels.Update(localToken);
                _dbContext.SaveChanges();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    public bool DeleteSessionUser(DeleteSessionRequest deleteSessionRequest, CancellationToken token)
    {
        try
        {
            var loadJwtToken = _dbContext.JwtTokenModels.FirstOrDefault(x => x.RefreshTokenJti == deleteSessionRequest.RefreshTokenJti);

            if (loadJwtToken is null)
                return false;

            _dbContext.JwtTokenModels.Remove(loadJwtToken);
            _dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    public GetSessionResponse? GetSessionUser(GetSessionRequest getSessionRequest)
    {
        SessionDto session;
        try
        {

            var jwtToken = _dbContext.JwtTokenModels.FirstOrDefault(x => x.RefreshTokenJti == getSessionRequest.RefreshTokenJti);

            if (jwtToken is null)
                return null;

            session = new SessionDto(jwtToken.RefreshTokenExpiration, jwtToken.RefreshTokenJti);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return session is not null ? new GetSessionResponse(session) : null;
    }
}