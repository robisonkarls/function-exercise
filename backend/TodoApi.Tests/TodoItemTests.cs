using Xunit;
using TodoApi.Models;

public sealed class TodoItemTests
{
    [Fact]
    public void Create_ShouldHavePendingStatus()
    {
        var item = TodoItem.Create("Buy milk");

        Assert.Equal(TodoStatus.Pending, item.Status);
    }

    [Fact]
    public void Create_ShouldStoreTitle()
    {
        var item = TodoItem.Create("Buy milk");

        Assert.Equal("Buy milk", item.Title);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtUtcToApproximatelyNow()
    {
        var before = DateTime.UtcNow;
        var item = TodoItem.Create("Buy milk");
        var after = DateTime.UtcNow;

        Assert.InRange(item.CreatedAtUtc, before, after);
    }

    [Fact]
    public void Create_ShouldDefaultIsArchivedToFalse()
    {
        var item = TodoItem.Create("Buy milk");

        Assert.False(item.IsArchived);
    }

    [Fact]
    public void Complete_ShouldChangeStatusToCompleted()
    {
        var item = TodoItem.Create("Buy milk");

        item.Complete();

        Assert.Equal(TodoStatus.Completed, item.Status);
    }

    [Fact]
    public void Complete_ShouldSetUpdatedAtUtc()
    {
        var item = TodoItem.Create("Buy milk");
        var before = DateTime.UtcNow;

        item.Complete();

        var after = DateTime.UtcNow;
        Assert.NotNull(item.UpdatedAtUtc);
        Assert.InRange(item.UpdatedAtUtc!.Value, before, after);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        var item = TodoItem.Create("Buy milk");
        item.Complete();

        Assert.Throws<InvalidOperationException>(() => item.Complete());
    }

    [Fact]
    public void Complete_WhenArchived_ShouldThrow()
    {
        var item = TodoItem.Create("Buy milk");
        item.Archive();

        Assert.Throws<InvalidOperationException>(() => item.Complete());
    }

    [Fact]
    public void UpdateTitle_ShouldReplaceTitle()
    {
        var item = TodoItem.Create("Buy milk");

        item.UpdateTitle("Buy oat milk");

        Assert.Equal("Buy oat milk", item.Title);
    }

    [Fact]
    public void UpdateTitle_ShouldSetUpdatedAtUtc()
    {
        var item = TodoItem.Create("Buy milk");
        var before = DateTime.UtcNow;

        item.UpdateTitle("Buy oat milk");

        var after = DateTime.UtcNow;
        Assert.NotNull(item.UpdatedAtUtc);
        Assert.InRange(item.UpdatedAtUtc!.Value, before, after);
    }

    [Fact]
    public void UpdateTitle_WhenCompleted_ShouldThrow()
    {
        var item = TodoItem.Create("Buy milk");
        item.Complete();

        Assert.Throws<InvalidOperationException>(() => item.UpdateTitle("Buy oat milk"));
    }

    [Fact]
    public void UpdateTitle_WhenArchived_ShouldThrow()
    {
        var item = TodoItem.Create("Buy milk");
        item.Archive();

        Assert.Throws<InvalidOperationException>(() => item.UpdateTitle("Buy oat milk"));
    }

    [Fact]
    public void Archive_ShouldSetIsArchivedToTrue()
    {
        var item = TodoItem.Create("Buy milk");

        item.Archive();

        Assert.True(item.IsArchived);
    }

    [Fact]
    public void Archive_ShouldSetUpdatedAtUtc()
    {
        var item = TodoItem.Create("Buy milk");
        var before = DateTime.UtcNow;

        item.Archive();

        var after = DateTime.UtcNow;
        Assert.NotNull(item.UpdatedAtUtc);
        Assert.InRange(item.UpdatedAtUtc!.Value, before, after);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldThrow()
    {
        var item = TodoItem.Create("Buy milk");
        item.Archive();

        Assert.Throws<InvalidOperationException>(() => item.Archive());
    }
}
