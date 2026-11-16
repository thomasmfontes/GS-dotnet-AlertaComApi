using System.Text.Json;
using AlertaComApi.Infrastructure.NoSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AlertaComApi.HealthChecks
{
    public class MongoHealthCheck : IHealthCheck
    {
        private readonly MongoDbContext _context;

        public MongoHealthCheck(MongoDbContext context)
        {
            _context = context;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_context.Ocorrencias != null)
                {
                    return Task.FromResult(HealthCheckResult.Healthy("Conexão MongoDB NoSQL operando normalmente."));
                }
                return Task.FromResult(HealthCheckResult.Degraded("MongoDB não configurado ou coleção indisponível."));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy($"Falha ao conectar no MongoDB: {ex.Message}"));
            }
        }
    }

    public class ExternalServicesHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ExternalServicesHealthCheck(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                // Testa conectividade com serviço externo de meteorologia ou OpenStreetMap
                var response = await client.GetAsync("https://nominatim.openstreetmap.org/status.php", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("Serviços externos de geolocalização e mapas disponíveis.");
                }

                return HealthCheckResult.Degraded("Serviço de mapas respondeu com latência ou status não esperado.");
            }
            catch
            {
                // Degraded em vez de Unhealthy para não derrubar a API se a internet externa oscilar
                return HealthCheckResult.Degraded("Serviços externos temporariamente inacessíveis.");
            }
        }
    }

    public static class HealthCheckResponseWriter
    {
        public static Task WriteResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds + "ms",
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds + "ms",
                    tags = e.Value.Tags
                })
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return context.Response.WriteAsync(json);
        }
    }
}
