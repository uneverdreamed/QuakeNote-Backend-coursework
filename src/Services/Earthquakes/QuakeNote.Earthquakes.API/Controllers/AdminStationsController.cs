using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Services;
using QuakeNote.Shared.Web.Auth;

namespace QuakeNote.Earthquakes.API.Controllers;

/// <summary>Управление станциями (только роль admin).</summary>
[ApiController]
[Authorize(Policy = AuthPolicies.Admin)]
[Route("api/admin/stations")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class AdminStationsController(StationService stations) : ControllerBase
{
    /// <summary>Создать станцию. Код уникален.</summary>
    [HttpPost]
    [ProducesResponseType<StationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StationDto>> Create(CreateStationDto dto, CancellationToken ct)
    {
        var created = await stations.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(StationsController.Get), "Stations", new { uid = created.Uid }, created);
    }

    /// <summary>Изменить станцию. Расстояния в уже загруженных показаниях пересчитываются.</summary>
    [HttpPut("{uid:guid}")]
    [ProducesResponseType<StationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StationDto>> Update(Guid uid, UpdateStationDto dto, CancellationToken ct)
        => Ok(await stations.UpdateAsync(uid, dto, ct));

    /// <summary>Удалить станцию. Если у неё есть показания, вернётся 409.</summary>
    [HttpDelete("{uid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid uid, CancellationToken ct)
    {
        await stations.DeleteAsync(uid, ct);
        return NoContent();
    }
}