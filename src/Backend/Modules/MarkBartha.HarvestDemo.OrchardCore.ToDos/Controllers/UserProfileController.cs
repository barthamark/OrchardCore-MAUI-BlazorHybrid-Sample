using MarkBartha.HarvestDemo.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace MarkBartha.HarvestDemo.OrchardCore.ToDos.Controllers;

[ApiController]
[Route("api/user-profile")]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
public class UserProfileController : ControllerBase
{
    private readonly IUserService _userService;

    public UserProfileController(IUserService userService) => _userService = userService;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (await _userService.GetAuthenticatedUserAsync(User) is not User user) return Unauthorized();

        var profile = new UserProfile
        {
            UserId = user.UserId ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
        };

        return Ok(profile);
    }
}
