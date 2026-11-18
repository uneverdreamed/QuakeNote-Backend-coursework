using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuakeNote.Auth.Core.Dtos;
using QuakeNote.Auth.Core.Services;
using QuakeNote.Shared.Web.Auth;

namespace QuakeNote.Auth.API.Controllers;

[ApiController]
[Route("api/auth/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProfileController(ProfileService profileService) : ControllerBase
{
    /// <summary>Профиль текущего пользователя.</summary>
    [HttpGet]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDto>> Get(CancellationToken ct)
        => Ok(await profileService.GetAsync(User.GetRequiredUserUid(), ct));

    /// <summary>Обновить профиль текущего пользователя.</summary>
    [HttpPut]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDto>> Update(UpdateProfileDto dto, CancellationToken ct)
        => Ok(await profileService.UpdateAsync(User.GetRequiredUserUid(), dto, ct));
}