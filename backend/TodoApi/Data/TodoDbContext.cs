using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public sealed class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{

    internal DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("todo_items");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(t => t.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
            entity.Property(t => t.Status).HasColumnName("status");
            entity.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.Property(t => t.UpdatedAtUtc).HasColumnName("updated_at_utc");
            entity.Property(t => t.IsArchived).HasColumnName("is_archived").IsRequired().HasDefaultValue(false);
        });
    }
}
