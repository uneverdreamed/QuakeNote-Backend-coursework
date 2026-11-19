using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuakeNote.Reports.Core.Dtos;
using QuakeNote.Reports.Core.Services;
using QuakeNote.Shared.Web.Auth;

namespace QuakeNote.Reports.API.Controllers;

/// <summary>Служебные методы для других сервисов. Требуют заголовок X-Service-Secret.</summary>
[ApiController]
[Route("internal/reports")]
[Authorize(Policy = AuthPolicies.ServiceToService)]
[Produces("application/json")]
public class InternalReportsController(ReportService reports) : ControllerBase
{
    /// <summary>Применить результат верификации к отчёту (вызывает Verification).</summary>
    [HttpPatch("{uid:guid}/match")]
    [ProducesResponseType<ReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportDto>> Match(Guid uid, MatchReportDto dto, CancellationToken ct)
        => Ok(await reports.ApplyMatchAsync(uid, dto, ct));

    /// <summary>Отчёты без привязки к событию за период (для обратного сопоставления).</summary>
    /// <param name="from">Начало периода (UTC).</param>
    /// <param name="to">Конец периода (UTC), не позже 7 суток от начала.</param>
    [HttpGet("unmatched")]
    [ProducesResponseType<List<UnmatchedReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<UnmatchedReportDto>>> Unmatched(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        => Ok(await reports.GetUnmatchedAsync(from, to, ct));
}