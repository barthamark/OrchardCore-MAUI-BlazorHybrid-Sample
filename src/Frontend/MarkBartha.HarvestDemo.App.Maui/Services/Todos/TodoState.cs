using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarkBartha.HarvestDemo.Domain.Models;

namespace MarkBartha.HarvestDemo.App.Maui.Services.Todos;

public class TodoState : IDisposable
{
    private readonly ITodoService _todoService;
    private readonly List<TodoItem> _items = new();
    private IReadOnlyList<TodoItem> _snapshot = Array.Empty<TodoItem>();
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _isInitialized;
    private bool _isLoading;

    public TodoState(ITodoService todoService)
    {
        _todoService = todoService;
    }

    public IReadOnlyList<TodoItem> Items => _snapshot;

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                NotifyStateChanged();
            }
        }
    }

    public event Action? StateChanged;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        await ReloadAsync(cancellationToken);
        _isInitialized = true;
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            IsLoading = true;
            var todos = await _todoService.GetTodosAsync(cancellationToken);
            _items.Clear();
            _items.AddRange(todos.OrderByDescending(t => t.CreatedAt));
            UpdateSnapshotUnsafe();
        }
        finally
        {
            IsLoading = false;
            _gate.Release();
            NotifyStateChanged();
        }
    }

    public async Task AddAsync(string title, CancellationToken cancellationToken = default)
    {
        var trimmed = title.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var created = await _todoService.AddTodoAsync(trimmed, cancellationToken);
            _items.Insert(0, created);
            UpdateSnapshotUnsafe();
        }
        finally
        {
            _gate.Release();
            NotifyStateChanged();
        }
    }

    public async Task ToggleAsync(string id, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var existing = _items.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                return;
            }

            var newValue = !existing.IsCompleted;
            var updated = await _todoService.SetCompletionAsync(id, newValue, cancellationToken) ?? existing.WithCompletion(newValue);
            var index = _items.IndexOf(existing);
            if (index >= 0)
            {
                _items[index] = updated;
                UpdateSnapshotUnsafe();
            }
        }
        finally
        {
            _gate.Release();
            NotifyStateChanged();
        }
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var removed = _items.RemoveAll(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase)) > 0;

            if (removed)
            {
                await _todoService.DeleteTodoAsync(id, cancellationToken);
                UpdateSnapshotUnsafe();
            }
        }
        finally
        {
            _gate.Release();
            NotifyStateChanged();
        }
    }

    private void UpdateSnapshotUnsafe()
    {
        _snapshot = _items
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public void Dispose()
    {
        _gate.Dispose();
    }
}
