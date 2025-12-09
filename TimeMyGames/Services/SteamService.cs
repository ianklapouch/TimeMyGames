using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TimeMyGames.Models;
using TimeMyGames.Options;

namespace TimeMyGames.Services;

public class SteamService
{
    private readonly HttpClient _httpClient;
    private readonly string _steamApiKey;

    public SteamService(HttpClient httpClient, IOptions<SteamOptions> options)
    {
        _httpClient = httpClient;
        _steamApiKey = options.Value.ApiKey;
    }

    public async Task<string?> ResolveVanityUrlAsync(string vanityName)
    {
        var url =
            $"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1/" +
            $"?key={_steamApiKey}&vanityurl={vanityName}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var result = doc
            .RootElement
            .GetProperty("response");

        if (!result.TryGetProperty("steamid", out var steamIdProp))
            return null;

        return steamIdProp.GetString();
    }

    public async Task<string?> GetGameNameAsync(int appId)
    {
        var url = $"https://store.steampowered.com/api/appdetails?appids={appId}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        if (!root.TryGetProperty(appId.ToString(), out var appNode))
            return null;

        if (!appNode.TryGetProperty("success", out var successProp))
            return null;

        if (!successProp.GetBoolean())
            return null;

        return appNode
            .GetProperty("data")
            .GetProperty("name")
            .GetString();
    }

    public async Task<List<MyGameDto>> GetSteamGamesAsync(string name)
    {
        try
        {
            var steamId = await ResolveVanityUrlAsync(name);
            var steamApiUrl =
                $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={_steamApiKey}&steamid={steamId}&format=json";
            var response = await _httpClient.GetAsync(steamApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                return new List<MyGameDto>();
            }

            var json = await response.Content.ReadAsStringAsync();

            var ownedGames =
                JsonSerializer.Deserialize<SteamOwnedGamesResponse>(json);

            var appIds = ownedGames?.Response.Games.Select(g => g.AppId);
            var gameDtos = new ConcurrentBag<MyGameDto>();

            var semaphore = new SemaphoreSlim(10);

            if (ownedGames != null)
            {
                var tasks = ownedGames.Response.Games.Select(async g =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var gameName = await GetGameNameAsync(g.AppId);

                        gameDtos.Add(new MyGameDto()
                        {
                            AppId = g.AppId,
                            Name = gameName ?? "Desconhecido",
                            PlaytimeMinutes = g.PlaytimeForever
                        });
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
            }

            return gameDtos.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new List<MyGameDto>();
        }
    }
}