using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace InteractiveItinerary.Security
{
    /// <summary>
    /// PLACEHOLDER authentication handler for development/testing until real auth (e.g., JWT) is wired up.
    /// Reads the <c>X-User-Role</c> request header (e.g., <c>Editor</c>, <c>Viewer</c>) to build a <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public class HeaderRoleAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "HeaderRole";
        public const string RoleHeaderName = "X-User-Role";

        public HeaderRoleAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(RoleHeaderName, out var roles) || string.IsNullOrWhiteSpace(roles))
                return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new List<Claim> { new(ClaimTypes.Name, "dev-user") };
            foreach (var role in roles.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
