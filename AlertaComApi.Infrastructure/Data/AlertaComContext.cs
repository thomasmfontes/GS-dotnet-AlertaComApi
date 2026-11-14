using AlertaComApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AlertaComApi.Infrastructure.Data
{
    public class AlertaComContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
        public DbSet<AlertaClimatico> Alertas => Set<AlertaClimatico>();
        public DbSet<AuditoriaOcorrencia> Auditorias => Set<AuditoriaOcorrencia>();

        public AlertaComContext(DbContextOptions<AlertaComContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
