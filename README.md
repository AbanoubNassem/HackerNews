# Hacker News Service with SQLite and Memory Cache

This ASP.NET Core service interacts with the Hacker News API to retrieve and cache best stories using SQLite for storage and memory caching. The service is designed to provide efficient access to Hacker News stories with the option to store story details persistently and optimize performance through in-memory caching.

## Features

- Retrieves best Hacker News stories asynchronously.
- Stores story details in an SQLite database for persistent storage.
- Utilizes memory caching to improve performance by reducing redundant API requests.

## Prerequisites

- .NET Core 8 SDK (Minimal API)

## Installation

   ```bash
   git clone https://github.com/AbanoubNassem/HackerNews.git
   cd HackerNews
   dotnet build
   dotnet run
```

# Usage (InMemoryCache)

## Best Stories from Memory Cache

### Endpoint: `/best-stories-memory`

Retrieves the best Hacker News stories from memory cache.

#### Parameters

- `n` (optional): Number of stories to retrieve. Defaults to 10 if not specified.

#### Example

```curl
GET /best-stories-memory?n=5
```
#### Response

Returns a JSON array containing details of the best Hacker News stories.

## Story Details from Memory Cache

### Endpoint: `/story-memory/{id}`

Retrieves details for a specific Hacker News story from memory cache.

#### Parameters

- `id`: ID of the story to retrieve.

#### Example
```curl
GET /story-memory/123
```
#### Response

Returns JSON containing details of the specified Hacker News story.

# Usage (Persistent with SQLite)


## Best Stories from SQLite Database

### Endpoint: `/best-stories-sqlite`

Retrieves the best Hacker News stories from the SQLite database.

#### Parameters

- `n` (optional): Number of stories to retrieve. Defaults to 10 if not specified.

#### Example

```curl
GET /best-stories-sqlite?n=5
```
#### Response

Returns a JSON array containing details of the best Hacker News stories.

## Story Details from SQLite Database

### Endpoint: `/story-sqlite/{id}`

Retrieves details for a specific Hacker News story from the SQLite database.

#### Parameters

- `id`: ID of the story to retrieve.

#### Example
```curl
GET /story-memory/123
```
#### Response

Returns JSON containing details of the specified Hacker News story.