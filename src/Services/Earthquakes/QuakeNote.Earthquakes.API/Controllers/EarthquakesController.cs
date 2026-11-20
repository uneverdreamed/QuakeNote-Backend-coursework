using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Services;
using QuakeNote.Shared.Contracts.Dtos;

namespace QuakeNote.Earthquakes.API.Controllers;

/// <summary>Сейсмические события: публичный просмотр.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/earthquakes")]
public class EarthquakesController(EarthquakeService earthquakes) : ControllerBase
{
    /// <summary>Список событий с фильтрами (область, период, магнитуда) и пагинацией.</summary>
    /// <remarks>Результат кэшируется в Redis на 5 минут по набору фильтров.</remarks>
    [HttpGet]
    [ProducesResponseType<PagedResultDto<EarthquakeListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<EarthquakeListItemDto>>> List(
        [FromQuery] EarthquakesFilterDto filter, CancellationToken ct)
        => Ok(await earthquakes.ListAsync(filter, ct));

    /// <summary>Событие целиком: параметры, оценка (статус верификации) и показания станций.</summary>
    /// <remarks>Кэшируется в Redis на 15 минут.</remarks>
    [HttpGet("{uid:guid}")]
    [ProducesResponseType<EarthquakeDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EarthquakeDetailDto>> Get(Guid uid, CancellationToken ct)
        => Ok(await earthquakes.GetAsync(uid, ct));
}