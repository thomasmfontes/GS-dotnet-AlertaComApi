using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Interfaces;
using AlertaComApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlertaComApi.Infrastructure.Repositories
{
    public class AlertaRepository : IAlertaRepository
    {
        private readonly AlertaComContext _context;

        public AlertaRepository(AlertaComContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AlertaClimatico>> GetAtivosAsync()
        {
            return await _context.Alertas
                .Where(a => a.Ativo && a.DataExpiracao > DateTime.UtcNow)
                .OrderByDescending(a => a.Nivel)
                .ThenByDescending(a => a.DataEmissao)
                .ToListAsync();
        }

        public async Task<(IEnumerable<AlertaClimatico> Items, int TotalCount)> GetPagedAtivosAsync(int pageIndex, int pageSize)
        {
            var query = _context.Alertas
                .Where(a => a.Ativo && a.DataExpiracao > DateTime.UtcNow)
                .OrderByDescending(a => a.Nivel)
                .ThenByDescending(a => a.DataEmissao);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<AlertaClimatico?> GetByIdAsync(int id)
        {
            return await _context.Alertas.FindAsync(id);
        }

        public async Task<AlertaClimatico> AddAsync(AlertaClimatico alerta)
        {
            await _context.Alertas.AddAsync(alerta);
            await _context.SaveChangesAsync();
            return alerta;
        }

        public async Task UpdateAsync(AlertaClimatico alerta)
        {
            _context.Alertas.Update(alerta);
            await _context.SaveChangesAsync();
        }
    }
}
