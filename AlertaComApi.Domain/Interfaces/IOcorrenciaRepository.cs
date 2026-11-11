using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Enums;

namespace AlertaComApi.Domain.Interfaces
{
    public interface IOcorrenciaRepository
    {
        Task<Ocorrencia?> GetByIdAsync(int id);
        Task<Ocorrencia?> GetByProtocoloAsync(string protocolo);
        Task<Ocorrencia?> GetByIdempotencyKeyAsync(string idempotencyKey);
        Task<(IEnumerable<Ocorrencia> Items, int TotalCount)> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            StatusOcorrencia? status = null, 
            TipoRisco? tipoRisco = null, 
            string? bairro = null);
        Task<IEnumerable<Ocorrencia>> GetByUserIdAsync(int userId);
        Task<Ocorrencia> AddAsync(Ocorrencia ocorrencia);
        Task UpdateAsync(Ocorrencia ocorrencia);
        Task DeleteAsync(Ocorrencia ocorrencia);
        Task<int> SaveChangesAsync();
    }
}
