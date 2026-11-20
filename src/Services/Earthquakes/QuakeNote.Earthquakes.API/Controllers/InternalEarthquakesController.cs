using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuakeNote.Earthquakes.Core.Dtos;
using QuakeNote.Earthquakes.Core.Services;
using QuakeNote.Shared.Web.Auth;

namespace QuakeNote.Earthquakes.API.Controllers;

/// <summary>Межсервисные вызовы (заголовок X-Service-Secret). Через Gateway не доступны.</summary>
[ApiController]
[Authorize(Policy = AuthPolicies.ServiceToService)]
[Route("internal/earthquakes")]
public class InternalEarthquakesController(EventAssessmentService assessments) : ControllerBase
{
    /// <summary>Принять инструментальную интенсивность от Verification и пересчитать оценку события.</summary>
    [HttpPost("{uid:guid}/assessment/recompute")]
    [ProducesResponseType<EventAssessmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventAssessmentDto>> RecomputeAssessment(
        Guid uid, RecomputeAssessmentRequest request, CancellationToken ct)
        => Ok(await assessments.SetInstrumentalAsync(uid, request.InstrumentalIntensity, ct));
}
