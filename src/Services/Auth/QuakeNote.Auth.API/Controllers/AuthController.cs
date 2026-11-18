using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuakeNote.Auth.Core.Dtos;
using QuakeNote.Auth.Core.Services;
using QuakeNote.Shared.Contracts.Errors;
using QuakeNote.Shared.Web.Auth;

namespace QuakeNote.Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(AuthService authService) : ControllerBase
{
    /// <summary>Запросить одноразовый код на email (регистрация и вход в одном сценарии).</summary>
    /// <remarks>Ответ одинаков для любого адреса, поэтому по нему нельзя узнать, зарегистрирован ли email.</remarks>
    [HttpPost("code/request")]
    [AllowAnonymous]
    [ProducesResponseType<RequestCodeResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<RequestCodeResponseDto>> RequestCode(
        RequestCodeDto dto, CancellationToken ct)
        => Ok(await authService.RequestCodeAsync(dto, ct));

    /// <summary>Подтвердить код и получить JWT.</summary>
    /// <remarks>
    /// Для нового пользователя обязательны birthDate и acceptTerms = true.
    /// Для существующего эти поля игнорируются.
    /// </remarks>
    [HttpPost("code/verify")]
    [AllowAnonymous]
    [ProducesResponseType<VerifyCodeResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status410Gone)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<VerifyCodeResponseDto>> VerifyCode(
        VerifyCodeDto dto, CancellationToken ct)
        => Ok(await authService.VerifyCodeAsync(dto, ct));

    /// <summary>Выйти: текущий токен отзывается (попадает в чёрный список Redis).</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var jti = User.GetJti();
        var expiresAt = User.GetExpiresAt();
        if (jti is null || expiresAt is null)
            throw new UnauthorizedException("Недействительный токен.", "invalid_token");

        await authService.LogoutAsync(jti, expiresAt.Value);
        return NoContent();
    }

    /// <summary>Текущий пользователь (определяется по токену).</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<MeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeDto>> Me(CancellationToken ct)
        => Ok(await authService.GetMeAsync(User.GetRequiredUserUid(), ct));
}