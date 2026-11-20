using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Services;
using QuakeNote.Shared.Web.Auth;

namespace QuakeNote.Earthquakes.API.Controllers;

/// <summary>Управление событиями и показаниями (только роль admin).</summary>
[ApiController]
[Authorize(Policy = AuthPolicies.Admin)]
[Route("api/admin/earthquakes")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class AdminEarthquakesController(
    EarthquakeService earthquakes,
    StationReadingService readings,
    EventAssessmentService assessments) : ControllerBase
{
    /// <summary>Создать событие. Публикует earthquake.created для Verification.</summary>
    [HttpPost]
    [ProducesResponseType<EarthquakeDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EarthquakeDetailDto>> Create(CreateEarthquakeDto dto, CancellationToken ct)
    {
        var created = await earthquakes.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(EarthquakesController.Get), "Earthquakes", new { uid = created.Uid }, created);
    }

    /// <summary>Изменить событие (полная замена полей).</summary>
    [HttpPut("{uid:guid}")]
    [ProducesResponseType<EarthquakeDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EarthquakeDetailDto>> Update(Guid uid, UpdateEarthquakeDto dto, CancellationToken ct)
        => Ok(await earthquakes.UpdateAsync(uid, dto, ct));

    /// <summary>Удалить событие; показания и оценка удаляются каскадно.</summary>
    [HttpDelete("{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid uid, CancellationToken ct)
    {
        await earthquakes.DeleteAsync(uid, ct);
        return NoContent();
    }

    /// <summary>Загрузить показания станций: существующие записи обновляются, новые добавляются.</summary>
    /// <remarks>Расстояние до эпицентра можно не передавать: оно считается по координатам.</remarks>
    [HttpPost("{uid:guid}/readings")]
    [ProducesResponseType<List<StationReadingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<StationReadingDto>>> UpsertReadings(
        Guid uid, UpsertReadingsDto dto, CancellationToken ct)
        => Ok(await readings.UpsertAsync(uid, dto, ct));

    /// <summary>Пересчитать расстояния до станций и оценку события, заново запустить приборную ветку.</summary>
    [HttpPost("{uid:guid}/recompute")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Recompute(Guid uid, CancellationToken ct)
    {
        await assessments.RequestRecomputeAsync(uid, ct);
        return Accepted();
    }
}