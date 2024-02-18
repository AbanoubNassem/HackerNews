using HackerNews;
using HackerNews.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<HackerNewsServiceMemoryCache>();
builder.Services.AddDbContext<SqLiteDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));
builder.Services.AddHttpClient<HackerNewsServiceSqlite>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/best-stories-memory",
        async ([FromQuery] int n, HackerNewsServiceMemoryCache service) =>
        Results.Ok(await service.GetBestStoriesAsync(n)))
    .WithName("BestStoriesFromMemory")
    .WithOpenApi();

app.MapGet("/story-memory/{id:int}",
        async ([FromRoute] int id, HackerNewsServiceMemoryCache service) =>
        {
            var story = await service.GetStoryDetailsAsync(id);
            return story == null ? Results.NotFound() : Results.Ok(story);
        })
    .WithName("StoryFromMemory")
    .WithOpenApi();


app.MapGet("/best-stories-sqlite",
        async ([FromQuery] int n, HackerNewsServiceSqlite service) =>
        Results.Ok(await service.GetBestStoriesAsync(n)))
    .WithName("BestStoriesFromSqlite")
    .WithOpenApi();

app.MapGet("/story-sqlite/{id:int}",
        async ([FromRoute] int id, HackerNewsServiceSqlite service) =>
        {
            var story = await service.GetStoryDetailsAsync(id);
            return story == null ? Results.NotFound() : Results.Ok(story);
        })
    .WithName("StoryFromSqlite")
    .WithOpenApi();


app.Run();