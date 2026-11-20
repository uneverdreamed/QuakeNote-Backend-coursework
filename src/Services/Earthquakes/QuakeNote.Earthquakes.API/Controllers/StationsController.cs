using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Services;

namespace QuakeNote.Earthquakes.API.Controllers;

/// <summary>Сейсмические станции: публичный просмотр.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/stations")]
public class StationsController(StationService stations) : ControllerBase
{
    /// <summary>Список станций; можно ограничить областью (все четыре значения сразу).</summary>
    [HttpGet]
    [ProducesResponseType<List<StationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<StationDto>>> List([FromQuery] StationsFilterDto filter, CancellationToken ct)
        => Ok(await stations.ListAsync(filter, ct));

    /// <summary>Станция по идентификатору.</summary>
    [HttpGet("{uid:guid}")]
    [ProducesResponseType<StationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StationDto>> Get(Guid uid, CancellationToken ct)
        => Ok(await stations.GetAsync(uid, ct));
}