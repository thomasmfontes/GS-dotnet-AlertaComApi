using AlertaComApi.Application.DTOs.Alertas;
using AlertaComApi.Domain.Common;

namespace AlertaComApi.Application.Interfaces
{
    public interface IAlertaService
    {
        Task<OperationResult<IEnumerable<AlertaClimaticoDto>>> GetAlertasAtivosAsync();
        Task<OperationResult<PageResult<AlertaClimaticoDto>>> GetAlertasPaginadosAsync(int pageIndex, int pageSize, string requestPath);
        Task<OperationResult<AlertaClimaticoDto>> GetByIdAsync(int id);
        Task<OperationResult<AlertaClimaticoDto>> CreateAsync(AlertaCreateDto dto);
        Task<OperationResult> DesativarAsync(int id);
    }
}
