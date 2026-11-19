using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuakeNote.Reports.Core.Dtos;
using QuakeNote.Reports.Core.Services;
using QuakeNote.Shared.Contracts.Dtos;
using QuakeNote.Shared.Web.Auth;

namespace QuakeNote.Reports.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController(ReportService reports, DetailedReportService details) : ControllerBase
{
    /// <summary>Отправить отчёт «я ощутил землетрясение» (анонимно или с токеном).</summary>
    /// <remarks>
    /// Отчёт сохраняется сразу, а верификация идёт асинхронно: статус сначала pending.
    /// Если передан токен, отчёт привязывается к пользователю и попадает в его историю.
    /// </remarks>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<ReportDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportDto>> Create(CreateReportDto dto, CancellationToken ct)
    {
        var report = await reports.CreateAsync(dto, User.GetUserUid(), ct);
        return CreatedAtAction(nameof(GetOne), new { uid = report.Uid }, report);
    }

    /// <summary>Изменить свой отчёт (в течение 15 минут после создания).</summary>
    /// <remarks>После изменения результат верификации сбрасывается, и отчёт верифицируется заново.</remarks>
    [HttpPut("{uid:guid}")]
    [Authorize]
    [ProducesResponseType<ReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReportDto>> Update(Guid uid, UpdateReportDto dto, CancellationToken ct)
        => Ok(await reports.UpdateAsync(uid, dto, User.GetRequiredUserUid(), ct));

    /// <summary>Приложить расширенную анкету MSK-64 (один раз на отчёт).</summary>
    [HttpPost("{uid:guid}/details")]
    [Authorize]
    [ProducesResponseType<DetailedReportDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DetailedReportDto>> AddDetails(
        Guid uid, DetailedReportDto dto, CancellationToken ct)
    {
        var result = await details.CreateAsync(uid, dto, User.GetRequiredUserUid(), ct);
        return CreatedAtAction(nameof(GetOne), new { uid }, result);
    }

    /// <summary>Изменить расширенную анкету (в течение 15 минут после создания отчёта).</summary>
    [HttpPut("{uid:guid}/details")]
    [Authorize]
    [ProducesResponseType<DetailedReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DetailedReportDto>> UpdateDetails(
        Guid uid, DetailedReportDto dto, CancellationToken ct)
        => Ok(await details.UpdateAsync(uid, dto, User.GetRequiredUserUid(), ct));

    /// <summary>История моих отчётов (новые сверху).</summary>
    /// <param name="page">Номер страницы, с 1.</param>
    /// <param name="pageSize">Размер страницы, 1..100 (по умолчанию 20).</param>
    [HttpGet("mine")]
    [Authorize]
    [ProducesResponseType<PagedResultDto<ReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<ReportDto>>> Mine(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await reports.GetMineAsync(User.GetRequiredUserUid(), page, pageSize, ct));

    /// <summary>Один отчёт вместе с расширенной анкетой (владелец или администратор).</summary>
    [HttpGet("{uid:guid}")]
    [Authorize]
    [ProducesResponseType<ReportWithDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportWithDetailsDto>> GetOne(Guid uid, CancellationToken ct)
        => Ok(await reports.GetWithDetailsAsync(uid, User.GetRequiredUserUid(), User.IsAdmin(), ct));

    /// <summary>Отчёты по землетрясению (публично, без данных об авторах).</summary>
    /// <remarks>Для неизвестного события вернёт пустой список. Не больше 1000 отчётов.</remarks>
    [HttpGet("/api/earthquakes/{earthquakeUid:guid}/reports")]
    [AllowAnonymous]
    [ProducesResponseType<List<AnonymizedReportDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AnonymizedReportDto>>> ByEarthquake(Guid earthquakeUid, CancellationToken ct)
        => Ok(await reports.GetByEarthquakeAsync(earthquakeUid, ct));
}