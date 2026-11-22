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
    }
}
