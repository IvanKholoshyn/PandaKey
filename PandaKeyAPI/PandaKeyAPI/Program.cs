using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PandaKey.Api.Data;
using PandaKey.Api.Repositories;
using PandaKey.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- CORS: allow the Angular dev server (and configurable extra origins) ----
const string WebCorsPolicy = "PandaKeyWeb";
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(WebCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ---- JWT authentication / role-based authorization --------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "PandaKey",
            ValidAudience = jwtSection["Audience"] ?? "PandaKeyClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ---- Data access + domain services -----------------------------------------
builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<ZonesRepository>();
builder.Services.AddScoped<AccessEventsRepository>();

builder.Services.AddScoped<AccessDecisionRepository>();
builder.Services.AddScoped<PandaKey.Api.Services.AccessDecisionService>();

// New for Lab 3 (web app): auth tokens + backup.
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<BackupService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(WebCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
