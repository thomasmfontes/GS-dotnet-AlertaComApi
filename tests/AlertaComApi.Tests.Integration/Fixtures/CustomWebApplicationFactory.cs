using AlertaComApi.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlertaComApi.Tests.Integration.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove qualquer DbContextOptions anterior
                var descriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<AlertaComContext>)).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Adiciona banco em memória exclusivo para a suíte de testes de integração
                var dbName = "AlertaComTestDb_" + Guid.NewGuid();
                services.AddDbContext<AlertaComContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                // Garante a criação e o seed
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AlertaComContext>();
                db.Database.EnsureCreated();
                DbInitializer.SeedAsync(db).GetAwaiter().GetResult();
            });

            builder.UseEnvironment("Testing");
        }
    }
}
