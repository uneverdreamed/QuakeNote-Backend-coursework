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
public class SettingsController(SettingsService settingsService) : ControllerBase
{
    /// <summary>Настройки текущего пользователя.</summary>
    [HttpGet]
    [ProducesResponseType<SettingsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SettingsDto>> Get(CancellationToken ct)
        => Ok(await settingsService.GetAsync(User.GetRequiredUserUid(), ct));

    /// <summary>Обновить настройки текущего пользователя (полная замена).</summary>
    [HttpPut]
    [ProducesResponseType<SettingsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SettingsDto>> Update(UpdateSettingsDto dto, CancellationToken ct)
        => Ok(await settingsService.UpdateAsync(User.GetRequiredUserUid(), dto, ct));
}