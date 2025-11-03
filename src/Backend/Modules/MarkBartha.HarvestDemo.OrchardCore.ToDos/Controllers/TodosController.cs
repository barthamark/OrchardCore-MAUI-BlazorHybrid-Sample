using System.ComponentModel.DataAnnotations;
using MarkBartha.HarvestDemo.Domain.Models;
using MarkBartha.HarvestDemo.OrchardCore.ToDos.Constants;
using MarkBartha.HarvestDemo.OrchardCore.ToDos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using YesSql;

namespace MarkBartha.HarvestDemo.OrchardCore.ToDos.Controllers;

[ApiController]
[Route("api/todos")]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
public class TodosController : ControllerBase
{
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly IUserService _userService;

    public TodosController(IContentManager contentManager, ISession session, IUserService userService)
    {
        _contentManager = contentManager;
        _session = session;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();

        var toDos = await _session.Query<ContentItem, ContentItemIndex>(index =>
                index.ContentType == ContentTypes.Todo &&
                index.Published &&
                index.Owner == userId)
            .ListAsync();

        return Ok(toDos.Select(CreateTodoItem));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();

        var todo = await _contentManager.GetAsync(id);
        if (todo is null || todo.Owner != userId) return NotFound();

        return Ok(CreateTodoItem(todo));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateTodoRequest request)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();

        if (request is null) return BadRequest();

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var title = request.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError(nameof(CreateTodoRequest.Title), "Title is required.");
            return ValidationProblem(ModelState);
        }

        var todo = await _contentManager.NewAsync(ContentTypes.Todo);
        todo.Owner = userId;
        todo.Author = userId;
        todo.DisplayText = title;
        todo.Alter<Todo>(part => part.Done.Value = false);

        await _contentManager.CreateAsync(todo, VersionOptions.Published);

        return CreatedAtAction(nameof(Get), new { id = todo.ContentItemId }, CreateTodoItem(todo));
    }

    [HttpPatch("{id}/completion")]
    public async Task<IActionResult> SetCompletion(string id, [FromBody] UpdateCompletionRequest request)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();

        if (request is null) return BadRequest();

        var todo = await _contentManager.GetAsync(id);
        if (todo is null || todo.Owner != userId) return NotFound();

        todo.Alter<Todo>(part => part.Done.Value = request.IsCompleted);

        await _contentManager.UpdateAsync(todo);
        await _contentManager.PublishAsync(todo);

        return Ok(CreateTodoItem(todo));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();

        var todo = await _contentManager.GetAsync(id, VersionOptions.Latest);
        if (todo is null || todo.Owner != userId) return NotFound();

        await _contentManager.RemoveAsync(todo);

        return NoContent();
    }

    private static TodoItem CreateTodoItem(ContentItem todo) =>
        new()
        {
            CreatedAt = todo.CreatedUtc!.Value,
            Id = todo.ContentItemId,
            IsCompleted = todo.As<Todo>().Done.Value,
            Title = todo.DisplayText,
        };

    private async Task<string> GetCurrentUserIdAsync()
    {
        var user = await _userService.GetAuthenticatedUserAsync(User) as User;
        return user?.UserId;
    }

    public sealed class CreateTodoRequest
    {
        [Required]
        public string Title { get; init; } = string.Empty;
    }

    public sealed class UpdateCompletionRequest
    {
        public bool IsCompleted { get; init; }
    }
}
