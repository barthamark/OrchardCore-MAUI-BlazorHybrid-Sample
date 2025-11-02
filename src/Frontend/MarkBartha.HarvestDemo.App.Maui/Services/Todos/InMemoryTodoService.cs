using MarkBartha.HarvestDemo.Domain.Models;

namespace MarkBartha.HarvestDemo.App.Maui.Services.Todos;

public class InMemoryTodoService : ITodoService
{
    private readonly List<TodoItem> _items;
    private readonly object _lock = new();

    public InMemoryTodoService()
    {
        _items = new List<TodoItem>
        {
            TodoItem.Create("Design new landing page"),
            TodoItem.Create("Update documentation").WithCompletion(true),
            TodoItem.Create("Plan onboarding session"),
        };

        // Adjust creation dates for ordering
        _items = _items
            .Select((item, index) => item with { CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-index * 15) })
            .ToList();
    }

    public Task<IReadOnlyList<TodoItem>> GetTodosAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<TodoItem>>(_items
                .OrderByDescending(x => x.CreatedAt)
                .ToList());
        }
    }

    public Task<TodoItem> AddTodoAsync(string title, CancellationToken cancellationToken = default)
    {
        var item = TodoItem.Create(title);
        lock (_lock)
        {
            _items.Insert(0, item);
        }

        return Task.FromResult(item);
    }

    public Task<TodoItem?> SetCompletionAsync(Guid id, bool isCompleted, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var existing = _items.FirstOrDefault(x => x.Id == id);
            if (existing is null)
            {
                return Task.FromResult<TodoItem?>(null);
            }

            var updated = existing.WithCompletion(isCompleted);
            var index = _items.IndexOf(existing);
            _items[index] = updated;
            return Task.FromResult<TodoItem?>(updated);
        }
    }

    public Task DeleteTodoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _items.RemoveAll(x => x.Id == id);
        }

        return Task.CompletedTask;
    }
}
