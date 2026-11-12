using AlertaComApi.Application.DTOs.Alertas;
using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Domain.Common;

namespace AlertaComApi.Application.Interfaces
{
    public interface IHateoasService
    {
        OcorrenciaResponseDto BuildOcorrenciaLinks(OcorrenciaResponseDto dto);
        PageResult<OcorrenciaResponseDto> BuildPagedOcorrenciaLinks(PageResult<OcorrenciaResponseDto> pageResult, string basePath);
        AlertaClimaticoDto BuildAlertaLinks(AlertaClimaticoDto dto);
        PageResult<AlertaClimaticoDto> BuildPagedAlertaLinks(PageResult<AlertaClimaticoDto> pageResult, string basePath);
        UserDto BuildUserLinks(UserDto dto);
    }
}
