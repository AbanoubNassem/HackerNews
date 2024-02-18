using System.Text.Json;
using HackerNews.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNews.Services;

public class HackerNewsServiceSqlite
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly SqLiteDbContext _dbContext;
    private readonly ILogger<HackerNewsServiceMemoryCache> _logger;
    private readonly int _cacheInMinutes;

    public HackerNewsServiceSqlite(HttpClient httpClient, SqLiteDbContext dbContext, IConfiguration configuration,
        ILogger<HackerNewsServiceMemoryCache> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
        _cacheInMinutes = configuration.GetSection("CacheOptions").GetValue<int>("AbsoluteExpirationMinutes");

        _dbContext.Database.EnsureCreated();

        _httpClient.BaseAddress = new Uri("https://hacker-news.firebaseio.com");
    }

    public async Task<IEnumerable<HackerNewsStory>> GetBestStoriesAsync(int n = 10)
    {
        const string cacheKey = "BestStoryIds";

        if (!_cache.TryGetValue(cacheKey, out List<int>? cachedStoryIds) || cachedStoryIds == null)
        {
            cachedStoryIds = (await _httpClient.GetFromJsonAsync<List<int>>("/v0/beststories.json") ?? []).ToList();

            _cache.Set(cacheKey, cachedStoryIds, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheInMinutes)
            });
        }

        var storyIds = cachedStoryIds.Take(n);

        var stories = _dbContext.Stories
            .Where(x => storyIds.Contains(x.Id))
            .ToList();

        foreach (var storyId in storyIds.Except(stories.Select(x => x.Id)))
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
            var story = await _dbContext.Stories.SingleOrDefaultAsync(x => x.Id == storyId);

            if (story != null) return story;

            var response =
                await _httpClient.GetFromJsonAsync<IDictionary<string, JsonElement>>(
                    $"/v0/item/{storyId.ToString()}.json");
            if (response == null)
                return null;


            await _dbContext.Stories.AddAsync(HackerNewsStory.FromJson(storyId, response));
            await _dbContext.SaveChangesAsync();
            return story;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return null;
        }
    }
}