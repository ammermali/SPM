using System.Security.Claims;
using System.Text.Json;
using InteractiveItinerary.Controllers;
using InteractiveItinerary.DTO;
using InteractiveItinerary.Entities;
using InteractiveItinerary.Repositories;
using InteractiveItinerary.Security;
using InteractiveItinerary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InteractiveItinerary.Tests
{
    public class ItinerariesControllerTests
    {
        private static ItinerariesController CreateController(TestSqliteDb db, params string[] roles)
        {
            var service = new ItineraryService(new ItineraryRepository(db.CreateContext()));
            var controller = new ItinerariesController(service);

            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            var identity = claims.Count > 0
                ? new ClaimsIdentity(claims, HeaderRoleAuthHandler.SchemeName)
                : new ClaimsIdentity();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            return controller;
        }

        [Fact]
        public async Task Create_WithEditorRole_Returns201CreatedWithRouteAndDraftBody()
        {
            using var db = new TestSqliteDb();
            var controller = CreateController(db, AppRoles.Editor);

            var input = new CreateItineraryDTO
            {
                Title = "Medieval Florence",
                Theme = "History",
                Description = "Towers and bridges.",
                GeographicArea = "Florence"
            };

            var actionResult = await controller.Create(input, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
            Assert.Equal(nameof(ItinerariesController.GetById), created.ActionName);

            var body = Assert.IsType<ItineraryDTO>(created.Value);
            Assert.True(body.Id > 0);
            Assert.Equal(body.Id, created.RouteValues?["id"]);
            Assert.Equal(ItineraryStatus.DRAFT, body.Status);
            Assert.Equal("Medieval Florence", body.Title);
        }

        [Fact]
        public async Task Create_WithUnauthorizedRole_Returns403ForbiddenWithStructuredError()
        {
            using var db = new TestSqliteDb();
            var controller = CreateController(db, AppRoles.Viewer);

            var input = new CreateItineraryDTO { Title = "Blocked Draft" };
            var actionResult = await controller.Create(input, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);

            var error = Assert.IsType<ApiErrorResponse>(objectResult.Value);
            Assert.Equal(StatusCodes.Status403Forbidden, error.Status);
            Assert.Equal("Forbidden", error.Error);

            using var verifyCtx = db.CreateContext();
            Assert.Empty(verifyCtx.Itineraries);
        }

        [Fact]
        public async Task Create_WithNullBody_Returns400BadRequestWithStructuredError()
        {
            using var db = new TestSqliteDb();
            var controller = CreateController(db, AppRoles.Editor);

            var actionResult = await controller.Create(null, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

            var error = Assert.IsType<ApiErrorResponse>(badRequest.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, error.Status);
            Assert.Equal("ValidationError", error.Error);
        }

        [Fact]
        public async Task GetById_ReturnsOkWhenFoundAndNotFoundWhenMissing()
        {
            using var db = new TestSqliteDb();
            var controller = CreateController(db, AppRoles.Editor);

            var createResult = await controller.Create(new CreateItineraryDTO { Title = "Saved" }, CancellationToken.None);
            var created = Assert.IsType<ItineraryDTO>(Assert.IsType<CreatedAtActionResult>(createResult.Result).Value);

            var okResult = await controller.GetById(created.Id, CancellationToken.None);
            var ok = Assert.IsType<OkObjectResult>(okResult.Result);
            var fetched = Assert.IsType<ItineraryDTO>(ok.Value);
            Assert.Equal(created.Id, fetched.Id);

            var missingResult = await controller.GetById(99999, CancellationToken.None);
            var notFound = Assert.IsType<NotFoundObjectResult>(missingResult.Result);
            var error = Assert.IsType<ApiErrorResponse>(notFound.Value);
            Assert.Equal(StatusCodes.Status404NotFound, error.Status);
        }

        [Fact]
        public async Task AuthorizationMiddlewareResultHandler_Forbidden_Writes403ApiErrorResponse()
        {
            var handler = new ApiAuthorizationMiddlewareResultHandler();
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var policy = new AuthorizationPolicyBuilder().RequireRole(AppRoles.Editor).Build();

            await handler.HandleAsync(
                _ => Task.CompletedTask,
                httpContext,
                policy,
                PolicyAuthorizationResult.Forbid());

            Assert.Equal(StatusCodes.Status403Forbidden, httpContext.Response.StatusCode);
            httpContext.Response.Body.Position = 0;
            var error = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(
                httpContext.Response.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(error);
            Assert.Equal(StatusCodes.Status403Forbidden, error.Status);
            Assert.Equal("Forbidden", error.Error);
        }

        [Fact]
        public async Task AddAndUpdateStop_WithUnauthorizedRole_Returns403AndDoesNotModify_AC05()
        {
            using var db = new TestSqliteDb();
            var editor = CreateController(db, AppRoles.Editor);
            var created = Assert.IsType<ItineraryDTO>(
                Assert.IsType<CreatedAtActionResult>((await editor.Create(new CreateItineraryDTO { Title = "Draft" }, CancellationToken.None)).Result).Value);

            var stopResult = await editor.AddStop(created.Id, new StopRequestDTO
            {
                Name = "Initial",
                Description = "Desc",
                Latitude = 41.9,
                Longitude = 12.5,
                Address = "Addr"
            }, CancellationToken.None);
            var addedStop = Assert.IsType<StopDTO>(Assert.IsType<CreatedAtActionResult>(stopResult.Result).Value);

            var viewer = CreateController(db, AppRoles.Viewer);
            var payload = new StopRequestDTO { Name = "Tampered", Description = "d", Latitude = 0, Longitude = 0, Address = "a" };

            var addForbidden = Assert.IsType<ObjectResult>((await viewer.AddStop(created.Id, payload, CancellationToken.None)).Result);
            Assert.Equal(StatusCodes.Status403Forbidden, addForbidden.StatusCode);
            Assert.Equal("Forbidden", Assert.IsType<ApiErrorResponse>(addForbidden.Value).Error);

            var updateForbidden = Assert.IsType<ObjectResult>((await viewer.UpdateStop(created.Id, addedStop.Id, payload, CancellationToken.None)).Result);
            Assert.Equal(StatusCodes.Status403Forbidden, updateForbidden.StatusCode);
            Assert.Equal("Forbidden", Assert.IsType<ApiErrorResponse>(updateForbidden.Value).Error);

            var stopsResult = Assert.IsType<OkObjectResult>((await editor.GetStops(created.Id, CancellationToken.None)).Result);
            var stops = Assert.IsAssignableFrom<IReadOnlyList<StopDTO>>(stopsResult.Value);
            Assert.Single(stops);
            Assert.Equal("Initial", stops[0].Name);
        }

        [Fact]
        public async Task AddStop_OutOfRangeCoordinates_Returns400BadRequest_AC04()
        {
            using var db = new TestSqliteDb();
            var editor = CreateController(db, AppRoles.Editor);
            var created = Assert.IsType<ItineraryDTO>(
                Assert.IsType<CreatedAtActionResult>((await editor.Create(new CreateItineraryDTO { Title = "Draft" }, CancellationToken.None)).Result).Value);

            var bad = new StopRequestDTO { Name = "Bad", Description = "d", Latitude = 91, Longitude = 0, Address = "Addr" };
            var badResult = Assert.IsType<BadRequestObjectResult>((await editor.AddStop(created.Id, bad, CancellationToken.None)).Result);
            var error = Assert.IsType<ApiErrorResponse>(badResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, error.Status);
            Assert.Equal("ValidationError", error.Error);
        }
    }
}
