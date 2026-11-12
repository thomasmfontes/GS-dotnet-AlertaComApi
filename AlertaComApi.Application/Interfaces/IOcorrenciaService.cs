using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Domain.Common;

namespace AlertaComApi.Application.Interfaces
{
    public interface IOcorrenciaService
    {
        Task<OperationResult<OcorrenciaResponseDto>> GetByIdAsync(int id);
        Task<OperationResult<OcorrenciaResponseDto>> GetByProtocoloAsync(string protocolo);
        Task<OperationResult<PageResult<OcorrenciaResponseDto>>> GetPagedAsync(OcorrenciaFilterDto filter, string requestPath);
        Task<OperationResult<IEnumerable<OcorrenciaResponseDto>>> GetByUserIdAsync(int userId);
        Task<OperationResult<OcorrenciaResponseDto>> CreateAsync(OcorrenciaCreateDto dto, int userId, string? idempotencyKey = null);
        Task<OperationResult<OcorrenciaResponseDto>> UpdateStatusAsync(int id, OcorrenciaUpdateStatusDto dto, string usuario);
        Task<OperationResult> DeleteAsync(int id, int userId, string userRole);
    }
}
