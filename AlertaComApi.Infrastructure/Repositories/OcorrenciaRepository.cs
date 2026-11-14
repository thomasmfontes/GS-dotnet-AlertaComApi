using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Enums;
using AlertaComApi.Domain.Interfaces;
using AlertaComApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlertaComApi.Infrastructure.Repositories
{
    public class OcorrenciaRepository : IOcorrenciaRepository
    {
        private readonly AlertaComContext _context;

        public OcorrenciaRepository(AlertaComContext context)
        {
            _context = context;
        }

        public async Task<Ocorrencia?> GetByIdAsync(int id)
        {
            return await _context.Ocorrencias
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Ocorrencia?> GetByProtocoloAsync(string protocolo)
        {
            return await _context.Ocorrencias
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Protocolo == protocolo);
        }

        public async Task<Ocorrencia?> GetByIdempotencyKeyAsync(string idempotencyKey)
        {
            return await _context.Ocorrencias
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey);
        }

        public async Task<(IEnumerable<Ocorrencia> Items, int TotalCount)> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            StatusOcorrencia? status = null, 
            TipoRisco? tipoRisco = null, 
            string? bairro = null)
        {
            var query = _context.Ocorrencias
                .Include(o => o.User)
                .AsNoTracking()
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (tipoRisco.HasValue)
            {
                query = query.Where(o => o.TipoRisco == tipoRisco.Value);
            }

            if (!string.IsNullOrWhiteSpace(bairro))
            {
                query = query.Where(o => o.Bairro.ToLower().Contains(bairro.ToLower()));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.DataCriacao)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Ocorrencia>> GetByUserIdAsync(int userId)
        {
            return await _context.Ocorrencias
                .Include(o => o.User)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.DataCriacao)
                .ToListAsync();
        }

        public async Task<Ocorrencia> AddAsync(Ocorrencia ocorrencia)
        {
            await _context.Ocorrencias.AddAsync(ocorrencia);
            await _context.SaveChangesAsync();

            // Grava auditoria de inserção
            var auditoria = new AuditoriaOcorrencia(
                ocorrencia.Id, 
                "INSERT", 
                ocorrencia.User?.Username ?? "Cidadao", 
                null, 
                ocorrencia.Status.ToString(), 
                null, 
                ocorrencia.Severidade.ToString());

            await _context.Auditorias.AddAsync(auditoria);
            await _context.SaveChangesAsync();

            return ocorrencia;
        }

        public async Task UpdateAsync(Ocorrencia ocorrencia)
        {
            _context.Ocorrencias.Update(ocorrencia);

            // Grava auditoria de atualização
            var auditoria = new AuditoriaOcorrencia(
                ocorrencia.Id, 
                "UPDATE", 
                "Sistema/Gestor", 
                null, 
                ocorrencia.Status.ToString(), 
                null, 
                ocorrencia.Severidade.ToString());

            await _context.Auditorias.AddAsync(auditoria);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Ocorrencia ocorrencia)
        {
            var auditoria = new AuditoriaOcorrencia(
                ocorrencia.Id, 
                "DELETE", 
                "Sistema/Cidadao", 
                ocorrencia.Status.ToString(), 
                "CANCELADA", 
                ocorrencia.Severidade.ToString(), 
                null);

            await _context.Auditorias.AddAsync(auditoria);
            _context.Ocorrencias.Remove(ocorrencia);
            await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
