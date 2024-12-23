using NoteService.Contracns.Requests;

namespace NoteService.Services;

public class NoteByUserRepository : INoteByUserRepository
{
    private readonly HttpClient _httpClient;
    public NoteByUserRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<bool> CreateNoteByUserAsync(CreateNoteByUserRequest createNoteByUserRequest, CancellationToken token)
    {
        if (createNoteByUserRequest.UserId.Equals(Guid.Empty)
            || string.IsNullOrWhiteSpace(createNoteByUserRequest.Title)
            || string.IsNullOrWhiteSpace(createNoteByUserRequest.Description))
            throw new ArgumentOutOfRangeException(Constants.EmptyFieldInObjectMessage);

        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var jsonContentCreateNoteByUserRequest = new StringContent(JsonConvert.SerializeObject(createNoteByUserRequest, jsonSettings), Encoding.UTF8, "application/json");

        var httpResponseCreationNoteByUser = await _httpClient.PostAsync($"{Links.DataAccessDockerPrivateConnectionUriString}/api/Note/CreateNote", jsonContentCreateNoteByUserRequest);

        return httpResponseCreationNoteByUser.IsSuccessStatusCode;
    }
    public async Task<List<NoteDto>> GetNotesByUserAsync(GetNotesByUserRequest getNotesByUserRequest, CancellationToken token)
    {
        if (getNotesByUserRequest.UserId.Equals(Guid.Empty))
            throw new ArgumentOutOfRangeException(Constants.EmptyFieldInObjectMessage);

        var getNotesByUserResponse = await _httpClient.GetAsync($"{Links.DataAccessDockerPrivateConnectionUriString}/api/Note/GetNotes", token);

        if (!getNotesByUserResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"{Constants.BadCodeHttpOperation} {nameof(getNotesByUserRequest)}");

        var responsestring = await getNotesByUserResponse.Content.ReadAsStringAsync();

        var deserializedGetNotesByUserResponse = JsonConvert.DeserializeObject<GetNotesByUserResponse>(responsestring);

        if (deserializedGetNotesByUserResponse is null || !deserializedGetNotesByUserResponse.notes.Any())
            throw new InvalidOperationException($"{Constants.EmptyCollectionMessage} {nameof(getNotesByUserResponse)}");

        var userNotes = deserializedGetNotesByUserResponse.notes.Where(x => x.UserId == getNotesByUserRequest.UserId).ToList();

        return userNotes.Any()
            ? userNotes
            : throw new InvalidOperationException($"{Constants.EmptyCollectionMessage} {nameof(userNotes)}");
    }
    public async Task<bool> DeleteNoteByUserAsync(List<DeleteNoteByUserRequest> deleteNoteByUserRequest, CancellationToken token)
    {
        if (!deleteNoteByUserRequest.Any())
            throw new InvalidOperationException($"{Constants.EmptyCollectionMessage} {nameof(deleteNoteByUserRequest)}");

        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var jsonContentofDeletedNotesByUserRequest = new StringContent(JsonConvert.SerializeObject(deleteNoteByUserRequest, jsonSettings), Encoding.UTF8, "application/json");

        var httpRequestofDeletedNotesByUser = new HttpRequestMessage { Method = HttpMethod.Delete, RequestUri = new Uri($"{Links.DataAccessDockerPrivateConnectionUriString}/api/Note/DeleteNote"), Content = jsonContentofDeletedNotesByUserRequest };

        var httpResponsetofDeletedNotesByUser = await _httpClient.SendAsync(httpRequestofDeletedNotesByUser);

        return httpResponsetofDeletedNotesByUser.IsSuccessStatusCode;
    }


}
