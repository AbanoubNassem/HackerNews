using HackerNews.Models;
using Microsoft.EntityFrameworkCore;

namespace HackerNews;

public class SqLiteDbContext(DbContextOptions<SqLiteDbContext> options) : DbContext(options)
{
    public required DbSet<HackerNewsStory> Stories { get; set; }
}