using InteractiveItinerary.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace InteractiveItinerary.Security
{
    /// <summary>
    /// Ensures HTTP 403 Forbidden responses from <c>[Authorize]</c> return a structured <see cref="ApiErrorResponse"/>.
    /// </summary>
    public class ApiAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ApiErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have the required role to perform this action."));
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
