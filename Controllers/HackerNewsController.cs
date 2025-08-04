using HackerAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace HackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HackerNewsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public HackerNewsController(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClient = httpClientFactory.CreateClient();
            _cache = cache;
        }

        [HttpGet("newstories")]
        public async Task<IActionResult> GetNewStories(int page = 1, int pageSize = 10, string? filter = null)
        {
            const string cacheKey = "hackernews-newstories";
            List<HackerNews>? allStories;

            // Try to get cached stories
            if (!_cache.TryGetValue(cacheKey, out allStories))
            {
                var idsResponse = await _httpClient.GetStringAsync("https://hacker-news.firebaseio.com/v0/newstories.json");
                var ids = JsonSerializer.Deserialize<List<int>>(idsResponse);

                var topIds = ids?.Take(200).ToList();

                if (topIds == null || topIds.Count == 0)
                    return Ok(new APIResponseModel<HackerNews>());

                var tasks = topIds.Select(async id =>
                {
                    var itemJson = await _httpClient.GetStringAsync($"https://hacker-news.firebaseio.com/v0/item/{id}.json");
                    return JsonSerializer.Deserialize<HackerNews>(itemJson);
                });

                allStories = (await Task.WhenAll(tasks)).Where(x => x != null).ToList()!;

                _cache.Set(cacheKey, allStories, TimeSpan.FromMinutes(5));
            }

            // Filter
            if (!string.IsNullOrWhiteSpace(filter))
            {
                allStories = allStories
                    .Where(story =>
                        (!string.IsNullOrEmpty(story.title) && story.title.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(story.by) && story.by.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            var totalItems = allStories.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedStories = allStories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new APIResponseModel<HackerNews>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Data = pagedStories
            };

            return Ok(result);
        }
    }
}
