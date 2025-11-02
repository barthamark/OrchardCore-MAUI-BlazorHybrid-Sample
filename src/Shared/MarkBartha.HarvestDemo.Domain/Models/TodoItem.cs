namespace MarkBartha.HarvestDemo.Domain.Models;

public record TodoItem
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public static TodoItem Create(string title) => new()
    {
        Id = Guid.NewGuid(),
        Title = title,
        IsCompleted = false,
        CreatedAt = DateTimeOffset.UtcNow,
    };

    public TodoItem WithCompletion(bool isCompleted) => this with { IsCompleted = isCompleted };
    public TodoItem WithTitle(string title) => this with { Title = title };
}
