using TodoApi.Models;

namespace TodoApi.Contracts;

internal sealed record TodoItemResponse(
    int Id,
    string Title,
    string Status,
    bool IsArchived,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc)
{
    internal static TodoItemResponse From(TodoItem item)
    {
        return new TodoItemResponse(
            item.Id,
            item.Title,
            item.Status.ToString(),
            item.IsArchived,
            item.CreatedAtUtc,
            item.UpdatedAtUtc);
    }
}
