using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.App.Maui.Exceptions;
using MarkBartha.HarvestDemo.Domain.Models;
using static MarkBartha.HarvestDemo.App.Maui.Constants.JsonOptions;

namespace MarkBartha.HarvestDemo.App.Maui.Services.Todos;

public class TodoService : ITodoService
{
    private readonly AuthenticatedHttpClient _authenticatedHttpClient;

    public TodoService(AuthenticatedHttpClient authenticatedHttpClient) =>
        _authenticatedHttpClient = authenticatedHttpClient;

    public async Task<IReadOnlyList<TodoItem>> GetTodosAsync(CancellationToken cancellationToken = default)
    {
        var client = await _authenticatedHttpClient.GetClientAsync();
        using var response = await client.GetAsync("todos", cancellationToken);
        await response.EnsureSuccessAsync(CreateServiceException, cancellationToken);

        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>(SerializerOptions, cancellationToken);
        return todos ?? [];
    }

    public async Task<TodoItem> AddTodoAsync(string title, CancellationToken cancellationToken = default)
    {
        var payload = new { title };
        var client = await _authenticatedHttpClient.GetClientAsync();
        using var response = await client.PostAsJsonAsync("todos", payload, SerializerOptions, cancellationToken);
        await response.EnsureSuccessAsync(CreateServiceException, cancellationToken);

        var created = await response.Content.ReadFromJsonAsync<TodoItem>(SerializerOptions, cancellationToken);
        return created ?? throw new TodoServiceException("The server returned an empty response while creating a todo item.");
    }

    public async Task<TodoItem?> SetCompletionAsync(
        string id,
        bool isCompleted,
        CancellationToken cancellationToken = default)
    {
        var payload = new { isCompleted };
        var client = await _authenticatedHttpClient.GetClientAsync();
        var encodedId = Uri.EscapeDataString(id);

        using var response = await client.PatchAsync(
            $"todos/{encodedId}/completion",
            JsonContent.Create(payload, options: SerializerOptions), cancellationToken);
        await response.EnsureSuccessAsync(CreateServiceException, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TodoItem>(SerializerOptions, cancellationToken);
    }

    public async Task DeleteTodoAsync(string id, CancellationToken cancellationToken = default)
    {
        var client = await _authenticatedHttpClient.GetClientAsync();
        var encodedId = Uri.EscapeDataString(id);

        using var response = await client.DeleteAsync($"todos/{encodedId}", cancellationToken);
        await response.EnsureSuccessAsync(CreateServiceException, cancellationToken);
    }

    private static Exception CreateServiceException(HttpResponseMessage response, string details) =>
        new TodoServiceException($"The server returned status {(int)response.StatusCode}: {details}");
}
