using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace HackerNews.Models;

[Table("Stories")]
[Index("Id")]
public class HackerNewsStory(
    int id,
    string title,
    string uri,
    string postedBy,
    string time,
    int score,
    int commentCount)
{
    [JsonIgnore] public int Id { get; set; } = id;
    public string Title { get; set; } = title;
    public string Uri { get; set; } = uri;
    public string PostedBy { get; set; } = postedBy;
    public string Time { get; set; } = time;
    public int Score { get; set; } = score;
    public int CommentCount { get; set; } = commentCount;

    public static HackerNewsStory FromJson(int id, IDictionary<string, JsonElement> response)
    {
        return new HackerNewsStory(
            id,
            response.TryGetValue("title", out var title) ? title.GetString()! : "",
            response.TryGetValue("url", out var url) ? url.GetString()! : "",
            response.TryGetValue("by", out var by) ? by.GetString()! : "",
            DateTime.UnixEpoch.AddSeconds(response.TryGetValue("time", out var time) ? time.GetInt64() : 0)
                .ToUniversalTime()
                .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
            response.TryGetValue("score", out var score) ? score.GetInt32() : 0,
            response.TryGetValue("kids", out var kids) ? kids.EnumerateArray().Count() : 0
        );
    }
}