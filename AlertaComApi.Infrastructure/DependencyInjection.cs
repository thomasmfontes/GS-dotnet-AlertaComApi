using AlertaComApi.Application.Interfaces;
using AlertaComApi.Domain.Interfaces;
using AlertaComApi.Infrastructure.Data;
using AlertaComApi.Infrastructure.NoSql;
using AlertaComApi.Infrastructure.Repositories;
using AlertaComApi.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlertaComApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Banco de Dados Relacional (EF Core / Oracle / In-Memory)
            services.AddDbContext<AlertaComContext>(options =>
            {
                var useInMemory = bool.TryParse(configuration["UseInMemoryDatabase"], out var parsed) && parsed;
                var connectionString = configuration.GetConnectionString("OracleConnection");

                if (useInMemory || string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("SECRETO"))
                {
                    options.UseInMemoryDatabase("AlertaComDb");
                }
                else
                {
                    options.UseOracle(connectionString);
                }
            });

            // Banco de Dados NoSQL (MongoDB)
            services.Configure<MongoDbSettings>(opts =>
            {
                var section = configuration.GetSection("MongoDbSettings");
                if (!string.IsNullOrWhiteSpace(section["ConnectionString"]))
                    opts.ConnectionString = section["ConnectionString"]!;
                if (!string.IsNullOrWhiteSpace(section["DatabaseName"]))
                    opts.DatabaseName = section["DatabaseName"]!;
                if (!string.IsNullOrWhiteSpace(section["OcorrenciasCollectionName"]))
                    opts.OcorrenciasCollectionName = section["OcorrenciasCollectionName"]!;
            });
            services.AddSingleton<MongoDbContext>();

            // Repositórios
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOcorrenciaRepository, OcorrenciaRepository>();
            services.AddScoped<IAlertaRepository, AlertaRepository>();
            services.AddScoped<IOcorrenciaNoSqlRepository, OcorrenciaNoSqlRepository>();

            // Infraestrutura de Segurança (JWT)
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
