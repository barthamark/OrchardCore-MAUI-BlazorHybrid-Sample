using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.App.Maui.GraphQL;
using MarkBartha.HarvestDemo.Domain.Models;

namespace MarkBartha.HarvestDemo.App.Maui.Services.Todos;

public class TodoApiService : ITodoService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public TodoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<TodoItem>> GetTodosAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/todos", cancellationToken);
        await EnsureSuccessStatusCode(response, cancellationToken);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>(SerializerOptions, cancellationToken)
                    ?? new List<TodoItem>();
        return todos;
    }

    public async Task<TodoItem> AddTodoAsync(string title, CancellationToken cancellationToken = default)
    {
        var payload = new { title };
        var response = await _httpClient.PostAsJsonAsync("api/todos", payload, SerializerOptions, cancellationToken);
        await EnsureSuccessStatusCode(response, cancellationToken);
        var created = await response.Content.ReadFromJsonAsync<TodoItem>(SerializerOptions, cancellationToken);
        if (created is null)
        {
            throw new TodoServiceException("The server returned an empty response while creating a todo item.");
        }

        return created;
    }

    public async Task<TodoItem?> SetCompletionAsync(Guid id, bool isCompleted, CancellationToken cancellationToken = default)
    {
        var payload = new { isCompleted };
        var response = await _httpClient.PatchAsync($"api/todos/{id}/completion", JsonContent.Create(payload, options: SerializerOptions), cancellationToken);
        await EnsureSuccessStatusCode(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<TodoItem>(SerializerOptions, cancellationToken);
    }

    public async Task DeleteTodoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/todos/{id}", cancellationToken);
        await EnsureSuccessStatusCode(response, cancellationToken);
    }

    private static async Task EnsureSuccessStatusCode(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new TodoServiceException($"The server returned status {(int)response.StatusCode}: {message}");
    }
}
