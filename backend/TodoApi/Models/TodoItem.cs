namespace TodoApi.Models;

internal sealed class TodoItem
{
    public static TodoItem Create(string title)
    {
        return new TodoItem
        {
            Title = title,
            Status = TodoStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private TodoItem() { }

    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public TodoStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsArchived { get; private set; }

    public void Complete()
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Cannot complete an archived item.");
        }

        if (Status == TodoStatus.Completed)
        {
            throw new InvalidOperationException("Todo item is already completed.");
        }

        Status = TodoStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateTitle(string title)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Cannot update title of an archived item.");
        }

        if (Status == TodoStatus.Completed)
        {
            throw new InvalidOperationException("Cannot update title of a completed item.");
        }

        Title = title;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Todo item is already archived.");
        }

        IsArchived = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
