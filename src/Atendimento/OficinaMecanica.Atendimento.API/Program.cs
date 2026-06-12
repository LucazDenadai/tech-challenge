using System.Text;
using System.Threading.RateLimiting;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OficinaMecanica.Atendimento.API.Extensions;
using OficinaMecanica.Atendimento.API.Filters;
using OficinaMecanica.Atendimento.Application.UseCases.Auth;
using OficinaMecanica.Atendimento.Application.UseCases.Catalogo;
using OficinaMecanica.Atendimento.Application.UseCases.Cliente;
using OficinaMecanica.Atendimento.Application.UseCases.OrdemServico;
using OficinaMecanica.Atendimento.Application.UseCases.Usuario;
using OficinaMecanica.Atendimento.Application.UseCases.Veiculo;
using OficinaMecanica.Atendimento.Domain.Entities;
using OficinaMecanica.Atendimento.Domain.Enums;
using OficinaMecanica.Atendimento.Infrastructure;
using OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Logging estruturado em JSON ────────────────────────────────────────────────
builder.Logging.AddJsonConsole(o =>
{
    o.IncludeScopes = true;
    o.TimestampFormat = "O";
    o.JsonWriterOptions = new System.Text.Json.JsonWriterOptions { Indented = false };
});

// ── OpenTelemetry (traces → Jaeger, métricas → Prometheus) ───────────────────
builder.AddOpenTelemetry("OficinaMecanica.Atendimento");

// ── Infrastructure (repositórios, stubs, DbContext, JWT service) ──────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Use Cases ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<AuthUseCase>();
builder.Services.AddScoped<AbrirOrdemServicoUseCase>();
builder.Services.AddScoped<ConsultarStatusOSUseCase>();
builder.Services.AddScoped<AprovarOrcamentoUseCase>();
builder.Services.AddScoped<ListarOrdensServicoUseCase>();
builder.Services.AddScoped<AtualizarStatusOSUseCase>();
builder.Services.AddScoped<ObterOrdemServicoUseCase>();
builder.Services.AddScoped<AcompanharOSUseCase>();
builder.Services.AddScoped<AdicionarItemOSUseCase>();
builder.Services.AddScoped<CancelarItemOSUseCase>();
builder.Services.AddScoped<ObterTempoExecucaoUseCase>();
builder.Services.AddScoped<GerenciarClienteUseCase>();
builder.Services.AddScoped<GerenciarVeiculoUseCase>();
builder.Services.AddScoped<GerenciarCatalogoUseCase>();
builder.Services.AddScoped<GerenciarUsuarioUseCase>();

// ── JWT Authentication ─────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new InvalidOperationException("JWT_KEY não configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ── Rate Limiting (10 req/min por IP no login) ─────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── CORS ───────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
        if (origins is { Length: > 0 })
            policy.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader();
        else if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Test"))
            policy.WithOrigins("http://localhost", "https://localhost").AllowAnyMethod().AllowAnyHeader();
        else
            throw new InvalidOperationException("Cors:Origins não configurado. Defina ao menos uma origem permitida.");
    });
});

// ── Controllers + Filters ──────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var erros = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var resultado = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Dados inválidos",
            Detail = "Um ou mais campos não passaram na validação.",
        };
        resultado.Extensions["erros"] = erros;

        return new BadRequestObjectResult(resultado);
    };
});

// ── Health Checks ──────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ── Swagger ────────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Oficina Mecânica — Atendimento API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no campo abaixo."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Migration automática + seed inicial ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var connString = db.Database.GetConnectionString()!;
    var builder2 = new NpgsqlConnectionStringBuilder(connString);
    var dbName = builder2.Database;
    builder2.Database = "postgres";
    await using (var conn = new NpgsqlConnection(builder2.ConnectionString))
    {
        await conn.OpenAsync();
        var exists = (long)(await new NpgsqlCommand($"SELECT COUNT(*) FROM pg_database WHERE datname = '{dbName}'", conn).ExecuteScalarAsync())! > 0;
        if (!exists)
            await new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", conn).ExecuteNonQueryAsync();
    }

    await db.Database.MigrateAsync();

    if (!await db.Usuarios.AnyAsync())
    {
        var senhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        db.Usuarios.Add(new Usuario("Administrador", "admin@oficina.com", senhaHash, PerfilUsuario.Admin));
        await db.SaveChangesAsync();
    }
}

// ── Middleware pipeline ────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Atendimento API v1"));

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint();
app.MapControllers();

await app.RunAsync();

public partial class Program { protected Program() { } }
