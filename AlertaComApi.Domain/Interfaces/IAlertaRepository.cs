using AlertaComApi.Domain.Entities;

namespace AlertaComApi.Domain.Interfaces
{
    public interface IAlertaRepository
    {
        Task<IEnumerable<AlertaClimatico>> GetAtivosAsync();
        Task<(IEnumerable<AlertaClimatico> Items, int TotalCount)> GetPagedAtivosAsync(int pageIndex, int pageSize);
        Task<AlertaClimatico?> GetByIdAsync(int id);
        Task<AlertaClimatico> AddAsync(AlertaClimatico alerta);
        Task UpdateAsync(AlertaClimatico alerta);
    }
}
