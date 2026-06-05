using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TodoApi.Contracts;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Tests;

public sealed class TodoApiTests : IDisposable
{
    private readonly SqliteConnection sqliteConnection;
    private readonly WebApplicationFactory<Program> factory;
    private readonly HttpClient client;

    public TodoApiTests()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        sqliteConnection = connection;

        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TodoDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<TodoDbContext>(options =>
                    options.UseSqlite(sqliteConnection));
            });
        });

        client = factory.CreateClient();
    }

    public void Dispose()
    {
        client.Dispose();
        factory.Dispose();
        sqliteConnection.Close();
        sqliteConnection.Dispose();
    }

    [Fact]
    public async Task CreateTodo_WithValidTitle_Returns201()
    {
        var response = await client.PostAsJsonAsync("/api/todos", new { title = "Buy milk" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateTodo_WithEmptyTitle_Returns400()
    {
        var response = await client.PostAsJsonAsync("/api/todos", new { title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTodo_WithWhitespaceTitle_Returns400()
    {
        var response = await client.PostAsJsonAsync("/api/todos", new { title = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTodo_WithTitleExceeding200Characters_Returns400()
    {
        var title = new string('x', 201);
        var response = await client.PostAsJsonAsync("/api/todos", new { title });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTodo_WithExactly200CharacterTitle_Returns201()
    {
        var title = new string('x', 200);
        var response = await client.PostAsJsonAsync("/api/todos", new { title });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetTodos_WhenEmpty_ReturnsEmptyArray()
    {
        var todos = await client.GetFromJsonAsync<TodoItemResponse[]>("/api/todos");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task GetTodos_WithStatusFilter_ReturnsOnlyMatchingTodos()
    {
        await client.PostAsJsonAsync("/api/todos", new { title = "Stays pending" });
        var createResponse = await client.PostAsJsonAsync("/api/todos", new { title = "To complete" });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItemResponse>();

        await client.PutAsJsonAsync($"/api/todos/{created!.Id}/complete", new { });

        var todos = await client.GetFromJsonAsync<TodoItemResponse[]>("/api/todos?status=Pending");

        Assert.NotNull(todos);
        Assert.All(todos, t => Assert.Equal("Pending", t.Status));
    }

    [Fact]
    public async Task GetTodos_WithInvalidStatus_Returns400()
    {
        var response = await client.GetAsync("/api/todos?status=invalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompleteTodo_WhenPending_Returns200WithCompletedStatus()
    {
        var createResponse = await client.PostAsJsonAsync("/api/todos", new { title = "Buy milk" });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItemResponse>();

        var response = await client.PutAsJsonAsync($"/api/todos/{created!.Id}/complete", new { });
        var updated = await response.Content.ReadFromJsonAsync<TodoItemResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Completed", updated!.Status);
    }

    [Fact]
    public async Task CompleteTodo_WhenNotFound_Returns404()
    {
        var response = await client.PutAsJsonAsync("/api/todos/99999/complete", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompleteTodo_WhenAlreadyCompleted_Returns409()
    {
        var createResponse = await client.PostAsJsonAsync("/api/todos", new { title = "Buy milk" });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItemResponse>();

        await client.PutAsJsonAsync($"/api/todos/{created!.Id}/complete", new { });
        var response = await client.PutAsJsonAsync($"/api/todos/{created.Id}/complete", new { });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTitle_WithValidTitle_Returns200WithNewTitle()
    {
        var createResponse = await client.PostAsJsonAsync("/api/todos", new { title = "Old title" });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItemResponse>();

        var response = await client.PutAsJsonAsync($"/api/todos/{created!.Id}/title", new { title = "New title" });
        var updated = await response.Content.ReadFromJsonAsync<TodoItemResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("New title", updated!.Title);
    }

    [Fact]
    public async Task UpdateTitle_WithEmptyTitle_Returns400()
    {
        var createResponse = await client.PostAsJsonAsync("/api/todos", new { title = "Some todo" });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItemResponse>();

        var response = await client.PutAsJsonAsync($"/api/todos/{created!.Id}/title", new { title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTitle_WhenNotFound_Returns404()
    {
        var response = await client.PutAsJsonAsync("/api/todos/99999/title", new { title = "New title" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTitle_OnCompletedItem_Returns409()
    {
        var createResponse = await client.PostAsJsonAsync("/api/todos", new { title = "Some todo" });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItemResponse>();

        await client.PutAsJsonAsync($"/api/todos/{created!.Id}/complete", new { });
        var response = await client.PutAsJsonAsync($"/api/todos/{created.Id}/title", new { title = "New title" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveTodo_WhenActive_Returns200WithIsArchivedTrue()
    {
        var createResponse = await client.PostAsJsonAsync("/api/todos", new { title = "Buy milk" });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItemResponse>();

        var response = await client.DeleteAsync($"/api/todos/{created!.Id}");
        var archived = await response.Content.ReadFromJsonAsync<TodoItemResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(archived!.IsArchived);
    }

    [Fact]
    public async Task ArchiveTodo_WhenNotFound_Returns404()
    {
        var response = await client.DeleteAsync("/api/todos/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveTodo_WhenAlreadyArchived_Returns409()
    {
        var createResponse = await client.PostAsJsonAsync("/api/todos", new { title = "Buy milk" });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItemResponse>();

        await client.DeleteAsync($"/api/todos/{created!.Id}");
        var response = await client.DeleteAsync($"/api/todos/{created.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
