using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace JJBussert.Aspire.Api.Authentication;

public class StaticWebAppsAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

public class StaticWebAppsAuthenticationHandler : AuthenticationHandler<StaticWebAppsAuthenticationSchemeOptions>
{
    public StaticWebAppsAuthenticationHandler(IOptionsMonitor<StaticWebAppsAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for the x-ms-client-principal header (SWA CLI sets this)
        if (!Request.Headers.TryGetValue("x-ms-client-principal", out var principalHeader))
        {
            // For development/testing, create a test user if no header is present
            if (Context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                return Task.FromResult(CreateTestUserResult());
            }
            
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            var principalJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(principalHeader.ToString()));
            var principal = JsonSerializer.Deserialize<ClientPrincipal>(principalJson);

            if (principal == null || string.IsNullOrEmpty(principal.UserId))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid client principal"));
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, principal.UserId),
                new(ClaimTypes.Name, principal.UserDetails ?? "Unknown"),
                new(ClaimTypes.AuthenticationMethod, principal.IdentityProvider ?? "unknown")
            };

            // Add roles
            if (principal.UserRoles != null)
            {
                claims.AddRange(principal.UserRoles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error parsing client principal header");
            return Task.FromResult(AuthenticateResult.Fail("Error parsing client principal"));
        }
    }

    private AuthenticateResult CreateTestUserResult()
    {
        // Create different test users based on request path or query parameter
        var testUserType = Request.Query["testUser"].FirstOrDefault() ?? "basic";
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, testUserType == "admin" ? "admin-test-id" : "basic-test-id"),
            new(ClaimTypes.Name, testUserType == "admin" ? "Test Admin User" : "Test Basic User"),
            new(ClaimTypes.Email, testUserType == "admin" ? "admin@test.com" : "basic@test.com"),
            new(ClaimTypes.AuthenticationMethod, "test"),
            new(ClaimTypes.Role, testUserType == "admin" ? "Admin" : "Basic")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

public class ClientPrincipal
{
    public string? IdentityProvider { get; set; }
    public string? UserId { get; set; }
    public string? UserDetails { get; set; }
    public IEnumerable<string>? UserRoles { get; set; }
}
