// PLACEHOLDER host — intentionally incomplete.
// Wires up persistence, service, controller, and placeholder header-based role auth
// until full JWT/Identity auth and remaining endpoints are added.

using InteractiveItinerary.Data;
using InteractiveItinerary.DTO;
using InteractiveItinerary.Repositories;
using InteractiveItinerary.Security;
using InteractiveItinerary.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Resolve a relative SQLite path against the project (content root) folder, so the database
// is always <project>/interactiveitinerary.db no matter which folder the app is started from.
var sqlite = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing."));

if (!Path.IsPathRooted(sqlite.DataSource) && sqlite.DataSource != ":memory:")
    sqlite.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqlite.DataSource);

builder.Services.AddDbContext<ItineraryDbContext>(options =>
    options.UseSqlite(sqlite.ToString()));

builder.Services.AddScoped<IItineraryRepository, ItineraryRepository>();
builder.Services.AddScoped<IItineraryService, ItineraryService>();

builder.Services.AddAuthentication(HeaderRoleAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, HeaderRoleAuthHandler>(HeaderRoleAuthHandler.SchemeName, null);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.CanCreateItinerary, policy =>
        policy.RequireRole(AppRoles.Editor));
});
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationMiddlewareResultHandler>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return new BadRequestObjectResult(new ApiErrorResponse(
                StatusCodes.Status400BadRequest,
                "ValidationError",
                "One or more validation errors occurred.",
                errors));
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(HeaderRoleAuthHandler.SchemeName, new OpenApiSecurityScheme
    {
        Name = HeaderRoleAuthHandler.RoleHeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = $"Placeholder role header. Enter '{AppRoles.Editor}' to create itineraries, or '{AppRoles.Viewer}' to test 403 Forbidden."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = HeaderRoleAuthHandler.SchemeName
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Create the database file (if missing) and apply any pending migrations at startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ItineraryDbContext>();
    db.Database.Migrate();
    app.Logger.LogInformation("SQLite database ready at {Path}", sqlite.DataSource);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
