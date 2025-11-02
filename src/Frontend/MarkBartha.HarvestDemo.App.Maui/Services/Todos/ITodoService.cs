using MarkBartha.HarvestDemo.Domain.Models;

namespace MarkBartha.HarvestDemo.App.Maui.Services.Todos;

public interface ITodoService
{
    Task<IReadOnlyList<TodoItem>> GetTodosAsync(CancellationToken cancellationToken = default);
    Task<TodoItem> AddTodoAsync(string title, CancellationToken cancellationToken = default);
    Task<TodoItem?> SetCompletionAsync(Guid id, bool isCompleted, CancellationToken cancellationToken = default);
    Task DeleteTodoAsync(Guid id, CancellationToken cancellationToken = default);
}
