using System.Text.Json;
using HackerNews.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNews.Services;

public class HackerNewsServiceMemoryCache
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HackerNewsServiceMemoryCache> _logger;
    private readonly int _cacheInMinutes;

    public HackerNewsServiceMemoryCache(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration,
        ILogger<HackerNewsServiceMemoryCache> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _cacheInMinutes = configuration.GetSection("CacheOptions").GetValue<int>("AbsoluteExpirationMinutes");

        _httpClient.BaseAddress = new Uri("https://hacker-news.firebaseio.com");
    }

    public async Task<IEnumerable<HackerNewsStory>> GetBestStoriesAsync(int n = 10)
    {
        var stories = new List<HackerNewsStory>();

        const string cacheKey = "BestStoryIds";

        if (!_cache.TryGetValue(cacheKey, out List<int>? storyIds) || storyIds == null)
        {
            storyIds = (await _httpClient.GetFromJsonAsync<List<int>>("/v0/beststories.json") ?? []).ToList();

            _cache.Set(cacheKey, storyIds, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheInMinutes)
            });
        }

        foreach (var storyId in storyIds.Take(n))
        {
            var story = await GetStoryDetailsAsync(storyId);
            if (story != null)
            {
                stories.Add(story);
            }
        }

        return stories.OrderByDescending(x => x.Score);
    }

    public async Task<HackerNewsStory?> GetStoryDetailsAsync(int storyId)
    {
        try
        {
            var cacheKey = $"StoryDetails_{storyId.ToString()}";
            if (_cache.TryGetValue(cacheKey, out HackerNewsStory? cachedStory) && cachedStory != null)
            {
                return cachedStory;
            }

            var response =
                await _httpClient.GetFromJsonAsync<IDictionary<string, JsonElement>>(
                    $"/v0/item/{storyId.ToString()}.json");
            if (response == null)
                return null;

            var story = HackerNewsStory.FromJson(storyId, response);

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheInMinutes)
            };

            _cache.Set(cacheKey, story, cacheEntryOptions);
            return story;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
    }
}