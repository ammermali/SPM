using InteractiveItinerary.DTO;
using InteractiveItinerary.Security;
using InteractiveItinerary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteractiveItinerary.Controllers
{
    [ApiController]
    [Route("api/itineraries")]
    public class ItinerariesController : ControllerBase
    {
        private readonly IItineraryService _itineraryService;

        public ItinerariesController(IItineraryService itineraryService)
        {
            _itineraryService = itineraryService;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ItineraryDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ItineraryDTO>> GetById(int id, CancellationToken cancellationToken)
        {
            var itinerary = await _itineraryService.GetByIdAsync(id, cancellationToken);
            if (itinerary is null)
            {
                return NotFound(new ApiErrorResponse(
                    StatusCodes.Status404NotFound,
                    "NotFound",
                    $"Itinerary with id {id} was not found."));
            }

            return Ok(itinerary);
        }

        [HttpPost("create")]
        [Authorize(Policy = AuthorizationPolicies.CanCreateItinerary)]
        [ProducesResponseType(typeof(ItineraryDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ItineraryDTO>> Create(
            [FromBody] CreateItineraryDTO? dto,
            CancellationToken cancellationToken)
        {
            // Explicit check so direct controller calls (outside the auth middleware) also enforce the role.
            if (!User.IsInRole(AppRoles.Editor))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    $"You do not have the required role ({AppRoles.Editor}) to create an itinerary."));
            }

            if (dto is null)
            {
                return BadRequest(new ApiErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "ValidationError",
                    "Request body is required."));
            }

            try
            {
                var created = await _itineraryService.CreateDraftAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
            {
                return BadRequest(new ApiErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "ValidationError",
                    ex.Message));
            }
        }

        [HttpGet("{id:int}/stops")]
        [ProducesResponseType(typeof(IReadOnlyList<StopDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<StopDTO>>> GetStops(int id, CancellationToken cancellationToken)
        {
            var stops = await _itineraryService.GetStopsAsync(id, cancellationToken);
            if (stops is null)
            {
                return NotFound(new ApiErrorResponse(
                    StatusCodes.Status404NotFound,
                    "NotFound",
                    $"Itinerary with id {id} was not found."));
            }

            return Ok(stops);
        }

        [HttpPost("{id:int}/stops")]
        [Authorize(Policy = AuthorizationPolicies.CanCreateItinerary)]
        [ProducesResponseType(typeof(StopDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StopDTO>> AddStop(
            int id,
            [FromBody] StopRequestDTO? dto,
            CancellationToken cancellationToken)
        {
            if (!User.IsInRole(AppRoles.Editor))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    $"You do not have the required role ({AppRoles.Editor}) to modify an itinerary's stops."));
            }

            if (dto is null)
            {
                return BadRequest(new ApiErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "ValidationError",
                    "Request body is required."));
            }

            try
            {
                var created = await _itineraryService.AddStopAsync(id, dto, cancellationToken);
                return CreatedAtAction(nameof(GetStops), new { id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(StatusCodes.Status404NotFound, "NotFound", ex.Message));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(new ApiErrorResponse(StatusCodes.Status400BadRequest, "ValidationError", ex.Message));
            }
        }

        [HttpPut("{id:int}/stops/{stopId:int}")]
        [Authorize(Policy = AuthorizationPolicies.CanCreateItinerary)]
        [ProducesResponseType(typeof(StopDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StopDTO>> UpdateStop(
            int id,
            int stopId,
            [FromBody] StopRequestDTO? dto,
            CancellationToken cancellationToken)
        {
            if (!User.IsInRole(AppRoles.Editor))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    $"You do not have the required role ({AppRoles.Editor}) to modify an itinerary's stops."));
            }

            if (dto is null)
            {
                return BadRequest(new ApiErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "ValidationError",
                    "Request body is required."));
            }

            try
            {
                var updated = await _itineraryService.UpdateStopAsync(id, stopId, dto, cancellationToken);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(StatusCodes.Status404NotFound, "NotFound", ex.Message));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(new ApiErrorResponse(StatusCodes.Status400BadRequest, "ValidationError", ex.Message));
            }
        }
    }
}
