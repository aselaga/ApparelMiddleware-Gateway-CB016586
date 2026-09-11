using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ApparelMiddlewareGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IConfiguration config,
            IWebHostEnvironment environment,
            ILogger<AuthController> logger)
        {
            _config = config;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// True in the Development environment, or when 'Auth:EnableTestTokenEndpoint' is
        /// explicitly set. A real deployment leaves the flag off and issues operator tokens
        /// from an enterprise OIDC provider instead.
        /// </summary>
        private bool TestTokenEndpointEnabled =>
            _environment.IsDevelopment()
            || _config.GetValue<bool>("Auth:EnableTestTokenEndpoint");

        /// <summary>
        /// Test-only helper that mints a valid operator token so the LCNC frontend, Postman
        /// and Swagger can be exercised without a real identity provider. Gated: it is a
        /// complete authentication bypass and must never be reachable in a real deployment.
        /// Optional query parameters let a tester mint a token for a specific operator/line
        /// (e.g. a legitimate Spreader_02 token for the positive-case demonstration).
        /// </summary>
        [HttpGet("generate-test-token")]
        public IActionResult GenerateToken(
            [FromQuery(Name = "operator")] string? operatorId,
            [FromQuery] string? line)
        {
            if (!TestTokenEndpointEnabled)
            {
                return NotFound();
            }

            operatorId = string.IsNullOrWhiteSpace(operatorId) ? "OP-777" : operatorId.Trim();
            line = string.IsNullOrWhiteSpace(line) ? "Spreader_01" : line.Trim();

            // 1. Grab the secret key (validated at startup in Program.cs).
            var key = _config["Jwt:Key"]!;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 2. Define the operator's claims (this mimics a real login).
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, operatorId),   // The operator's ID
                new Claim("AssignedLine", line)                     // The line they are authorized to use
            };

            // 3. Construct the JWT (short-lived - this is a bootstrap credential).
            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(30),
                signingCredentials: credentials);

            _logger.LogWarning(
                "Test token issued for operator {OperatorId} on line {AssignedLine} (environment: {Environment})",
                operatorId, line, _environment.EnvironmentName);

            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
