using Microsoft.EntityFrameworkCore;
using TodoApi.Contracts;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Endpoints;

internal static class TodoEndpoints
{
    private const int MaxTitleLength = 200;

    internal static void MapTodoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/todos");

        group.MapPost("/", CreateTodoAsync);
        group.MapGet("/", GetTodosAsync);
        group.MapPut("/{id:int}/complete", CompleteTodoAsync);
        group.MapPut("/{id:int}/title", UpdateTodoTitleAsync);
        group.MapDelete("/{id:int}", ArchiveTodoAsync);
    }

    private static async Task<IResult> CreateTodoAsync(CreateTodoRequest request, TodoDbContext db)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ValidationFailed("Title cannot be empty.");
        }

        if (request.Title.Length > MaxTitleLength)
        {
            return ValidationFailed($"Title cannot exceed {MaxTitleLength} characters.");
        }

        var todo = TodoItem.Create(request.Title.Trim());
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        return Results.Created($"/api/todos/{todo.Id}", TodoItemResponse.From(todo));
    }

    private static async Task<IResult> GetTodosAsync(string? status, TodoDbContext db)
    {
        TodoStatus? filter = null;

        if (status is not null)
        {
            if (!Enum.TryParse<TodoStatus>(status, ignoreCase: true, out var parsed))
            {
                return ValidationFailed($"'{status}' is not a valid status. Use Pending or Completed.");
            }

            filter = parsed;
        }

        var items = await db.Todos
            .Where(t => !t.IsArchived && (filter == null || t.Status == filter))
            .OrderBy(t => t.CreatedAtUtc)
            .ToListAsync();

        return Results.Ok(items.Select(TodoItemResponse.From).ToList());
    }

    private static async Task<IResult> CompleteTodoAsync(int id, TodoDbContext db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null)
        {
            return TodoNotFound(id);
        }

        try
        {
            todo.Complete();
        }
        catch (InvalidOperationException ex)
        {
            return DomainConflict(ex.Message);
        }

        await db.SaveChangesAsync();

        return Results.Ok(TodoItemResponse.From(todo));
    }

    private static async Task<IResult> UpdateTodoTitleAsync(int id, UpdateTitleRequest request, TodoDbContext db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null)
        {
            return TodoNotFound(id);
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ValidationFailed("Title cannot be empty.");
        }

        if (request.Title.Length > MaxTitleLength)
        {
            return ValidationFailed($"Title cannot exceed {MaxTitleLength} characters.");
        }

        try
        {
            todo.UpdateTitle(request.Title.Trim());
        }
        catch (InvalidOperationException ex)
        {
            return DomainConflict(ex.Message);
        }

        await db.SaveChangesAsync();

        return Results.Ok(TodoItemResponse.From(todo));
    }

    private static async Task<IResult> ArchiveTodoAsync(int id, TodoDbContext db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null)
        {
            return TodoNotFound(id);
        }

        try
        {
            todo.Archive();
        }
        catch (InvalidOperationException ex)
        {
            return DomainConflict(ex.Message);
        }

        await db.SaveChangesAsync();

        return Results.Ok(TodoItemResponse.From(todo));
    }

    private static IResult ValidationFailed(string detail) =>
        Results.Problem(detail, null, 400, "Validation failed");

    private static IResult DomainConflict(string detail) =>
        Results.Problem(detail, null, 409, "Conflict");

    private static IResult TodoNotFound(int id) =>
        Results.Problem($"Todo item {id} was not found.", null, 404, "Not found");
}
