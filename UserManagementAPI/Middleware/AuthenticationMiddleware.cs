using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace UserManagementAPI.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private const string HardcodedToken = "ValidToken"; // Replaceable in the request file

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader) ||
            !authorizationHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Authorization header missing or malformed.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        var token = authorizationHeader.ToString().Substring("Bearer ".Length).Trim();

        // !: Hardcoded token validation for simplicity
        if (token == HardcodedToken)
        {
            _logger.LogInformation("Hardcoded token accepted.");
            await _next(context);
            return;
        }

        // !: Fallback to JWT validation for future flexibility
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String("YourBase64EncodedSecretKey"); // Optional if you add real JWT later

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = false, // Ignored for hardcoded token
                ValidateIssuer = false,
                ValidateAudience = false
            }, out _);

            _logger.LogInformation("JWT token validation succeeded.");
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
        }
    }
}