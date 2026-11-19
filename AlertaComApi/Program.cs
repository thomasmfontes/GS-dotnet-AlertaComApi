using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AlertaComApi.Application;
using AlertaComApi.Diagnostics;
using AlertaComApi.Endpoints;
using AlertaComApi.HealthChecks;
using AlertaComApi.Infrastructure;
using AlertaComApi.Infrastructure.Data;
using AlertaComApi.Infrastructure.NoSql;
using AlertaComApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Logging Estruturado (Serilog)
// ==========================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "AlertaComApi")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/alertacom-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================
// 2. Configurações de JSON
// ==========================================
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ==========================================
// 3. Injeção de Dependências por Camada (Clean Architecture)
// ==========================================
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ==========================================
// 6. Rate Limiting Funcional com HTTP 429 (Requisito 04: 15 pts)
// ==========================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        var problem = new
        {
            type = "https://tools.ietf.org/html/rfc6585#section-4",
            title = "Limite de requisições excedido",
            status = StatusCodes.Status429TooManyRequests,
            detail = "Você enviou muitas requisições em um curto período. Por favor, aguarde antes de tentar novamente.",
            instance = context.HttpContext.Request.Path
        };
        await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken: token);
    };

    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

// ==========================================
// 7. Autenticação e Autorização JWT Bearer (Requisito 02: 20 pts)
// ==========================================
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "ChaveMestraSuperSeguraAlertaComGlobalSolutionFIAP2026_JWT_Token_Key_987654321";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AlertaComApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AlertaComMobile";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DefesaCivilOrAdmin", policy => policy.RequireRole("Admin", "DefesaCivil"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

// ==========================================
// 8. Health Checks (Requisito 06: 10 pts)
// ==========================================
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API operacional e responsiva"), tags: new[] { "live" })
    .AddDbContextCheck<AlertaComContext>("oracle-database", tags: new[] { "ready", "db" })
    .AddCheck<MongoHealthCheck>("mongodb", tags: new[] { "ready", "nosql" })
    .AddCheck<ExternalServicesHealthCheck>("external-services", tags: new[] { "ready", "external" });

// ==========================================
// 9. OpenTelemetry (Tracing & Metrics)
// ==========================================
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("AlertaComApi")
            .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter(AlertaComMetrics.MeterName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

// ==========================================
// 10. Swagger / OpenAPI Interativo com JWT (Requisito 01: 20 pts)
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlertaCom API - Plataforma CitizenTech de Prevenção e Zeladoria Urbana",
        Version = "v1",
        Description = "API RESTful corporativa para gestão de ocorrências climáticas, zeladoria urbana colaborativa e alertas da Defesa Civil. Desenvolvida para a disciplina Advanced Business Development with .NET na Global Solution FIAP 2026/2.",
        Contact = new OpenApiContact
        {
            Name = "Equipe AlertaCom - FIAP 2026",
            Email = "contato@alertacom.com.br"
        }
    });

    options.EnableAnnotations();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira apenas o seu token JWT (o Swagger adiciona 'Bearer ' automaticamente no cabeçalho)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });

    options.OperationFilter<AllowAnonymousFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configuração de CORS para permitir acesso do React Native (Expo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "5208";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Inicialização automática e seed de dados iniciais
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AlertaComContext>();
    await context.Database.EnsureCreatedAsync();
    await DbInitializer.SeedAsync(context);
}

// ==========================================
// 11. Pipeline de Middlewares
// ==========================================
app.UseCors("AllowAll"); // CORS deve ser o primeiro middleware para responder aos preflights OPTIONS
app.UseStaticFiles(); // Permite servir as imagens das ocorrências em wwwroot/images

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>(); // Idempotência funcional (Requisito 03)

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AlertaCom API v1");
    c.DocumentTitle = "AlertaCom API - Documentação Interativa";
});

// HTTPS é gerenciado pelo proxy reverso em nuvem (Render) ou Kestrel local em HTTP na porta configurada

app.UseRateLimiter(); // Rate Limiting (Requisito 04)

app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// 12. Endpoints de Health Check (Requisito 06)
// ==========================================
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

// ==========================================
// 13. Mapeamento das Minimal APIs (Requisito 01)
// ==========================================
// Redireciona a rota raiz ("/") diretamente para o Swagger interativo
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapAuthEndpoints();
app.MapOcorrenciaEndpoints();
app.MapAlertaEndpoints();

app.Run();

// Necessário para testes de integração com WebApplicationFactory<Program>
public partial class Program { }

public class AllowAnonymousFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor?.EndpointMetadata;
        var hasAllowAnonymous = (metadata != null && metadata.OfType<IAllowAnonymous>().Any())
            || (context.MethodInfo != null && (
                (context.MethodInfo.DeclaringType != null && context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any())
                || context.MethodInfo.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any()));

        if (hasAllowAnonymous)
        {
            operation.Security = new List<OpenApiSecurityRequirement>();
        }
    }
}
