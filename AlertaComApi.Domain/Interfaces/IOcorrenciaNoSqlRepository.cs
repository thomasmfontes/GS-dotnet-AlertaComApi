using AlertaComApi.Domain.Entities;

namespace AlertaComApi.Domain.Interfaces
{
    public interface IOcorrenciaNoSqlRepository
    {
        Task<IEnumerable<OcorrenciaNoSql>> GetAllAsync();
        Task<OcorrenciaNoSql?> GetByIdAsync(string id);
        Task<OcorrenciaNoSql?> GetByRelacionalIdAsync(int relacionalId);
        Task<OcorrenciaNoSql> AddAsync(OcorrenciaNoSql ocorrenciaNoSql);
        Task<IEnumerable<OcorrenciaNoSql>> GetNearAsync(double longitude, double latitude, double maxDistanceMeters);
    }
}
