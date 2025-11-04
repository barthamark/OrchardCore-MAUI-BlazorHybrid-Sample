using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.Domain.Models;

namespace MarkBartha.HarvestDemo.App.Maui.Services.Todos;

/// <summary>
/// Provides an abstraction over the todo API endpoints exposed by the Orchard backend.
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// Retrieves the current todo items for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A read-only list containing the todo items, ordered as defined by the service.</returns>
    Task<IReadOnlyList<TodoItem>> GetTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new todo item with the supplied title.
    /// </summary>
    /// <param name="title">The title to assign to the todo item.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The newly created todo item as returned by the backend.</returns>
    Task<TodoItem> AddTodoAsync(string title, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the completion status of the specified todo item.
    /// </summary>
    /// <param name="id">The identifier of the todo item to update.</param>
    /// <param name="isCompleted">A value indicating whether the item should be marked complete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>The updated todo item when available; otherwise <c>null</c> if the backend omits a body.</returns>
    Task<TodoItem?> SetCompletionAsync(string id, bool isCompleted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified todo item.
    /// </summary>
    /// <param name="id">The identifier of the todo item to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task DeleteTodoAsync(string id, CancellationToken cancellationToken = default);
}
