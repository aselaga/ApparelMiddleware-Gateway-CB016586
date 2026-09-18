using ApparelMiddlewareGateway.Data;
using ApparelMiddlewareGateway.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Azure.Security.KeyVault.Secrets;

// Define an explicit alias at the end of the using block to completely resolve ambiguity
using DefaultCredential = Azure.Identity.DefaultAzureCredential;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    // In production, connect to Key Vault using Managed Identity
    var kvUriString = builder.Configuration["KeyVault:VaultUri"];

    if (!string.IsNullOrWhiteSpace(kvUriString))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(kvUriString), new DefaultCredential());
    }
}

// 0. Fail fast on missing/weak configuration.
// In production these values must be supplied by Key Vault; a misconfigured gateway should refuse to start rather than run with no/weak security.
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT configuration is incomplete. 'Jwt:Key', 'Jwt:Issuer' and 'Jwt:Audience' must all be provided " +
        "(via Azure Key Vault in production).");
}

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "'Jwt:Key' must be at least 32 bytes (256 bits) to sign tokens with HMAC-SHA256.");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
}

// 1. Configure CORS (Cross-Origin Resource Sharing)
// This allows the LCNC React/Blazor frontend to communicate with this API.
// Origins come from configuration so they are not baked into the binary.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLCNCApp",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 2. Configure Entity Framework Core (Database Context)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Configure JWT Authentication (Zero-Trust Pipeline)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// 4. Per-caller rate limiting (partitioned by operator id, falling back to remote IP).
//    Limits are configurable so a load test (JMeter) can raise them without a code change.
var permitLimit = builder.Configuration.GetValue<int?>("RateLimiting:PermitLimit") ?? 100;
var windowSeconds = builder.Configuration.GetValue<int?>("RateLimiting:WindowSeconds") ?? 60;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? httpContext.Connection.RemoteIpAddress?.ToString()
                          ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0
            }));
});

// 5. Centralised problem-details responses + global exception handling.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 6. Configure Swagger to accept JWTs for testing
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Loudly flag the unauthenticated test-token endpoint if it has been enabled outside
// Development - it is an authentication bypass and should never be on in a real deployment.
if (!app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("Auth:EnableTestTokenEndpoint"))
{
    app.Logger.LogWarning(
        "SECURITY: 'Auth:EnableTestTokenEndpoint' is enabled in the {Environment} environment. " +
        "The /api/Auth/generate-test-token endpoint mints valid operator tokens without authentication.",
        app.Environment.EnvironmentName);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages();

// Swagger is a development aid only; it is not exposed in production.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANT: Middleware Pipeline Order Matters!
app.UseCors("AllowLCNCApp");      
app.UseAuthentication();         
app.UseAuthorization();           
app.UseRateLimiter();             

app.MapControllers();
app.Run();
