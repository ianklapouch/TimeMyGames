using System.Text.Json.Serialization;

namespace TimeMyGames.Models;

public class SteamOwnedGamesResponse
{
    [JsonPropertyName("response")]
    public SteamOwnedGamesInner Response { get; set; }
}

public class SteamOwnedGamesInner
{
    [JsonPropertyName("games")]
    public List<SteamGameRaw> Games { get; set; }
}

public class SteamGameRaw
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }
    [JsonPropertyName("playtime_forever")]
    public int PlaytimeForever { get; set; }
}
