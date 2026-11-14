using AlertaComApi.Application.DTOs.Alertas;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Application.Mappers;
using AlertaComApi.Domain.Common;
using AlertaComApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlertaComApi.Application.Services
{
    public class AlertaService : IAlertaService
    {
        private readonly IAlertaRepository _alertaRepository;
        private readonly IHateoasService _hateoasService;
        private readonly ILogger<AlertaService> _logger;

        public AlertaService(
            IAlertaRepository alertaRepository,
            IHateoasService hateoasService,
            ILogger<AlertaService> logger)
        {
            _alertaRepository = alertaRepository;
            _hateoasService = hateoasService;
            _logger = logger;
        }

        public async Task<OperationResult<IEnumerable<AlertaClimaticoDto>>> GetAlertasAtivosAsync()
        {
            var alertas = await _alertaRepository.GetAtivosAsync();
            var dtos = alertas.Select(a => _hateoasService.BuildAlertaLinks(AlertaMapper.ToDto(a))).ToList();
            return OperationResult<IEnumerable<AlertaClimaticoDto>>.Ok(dtos);
        }

        public async Task<OperationResult<PageResult<AlertaClimaticoDto>>> GetAlertasPaginadosAsync(int pageIndex, int pageSize, string requestPath)
        {
            var safePageIndex = pageIndex < 1 ? 1 : pageIndex;
            var safePageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 50);

            var (items, totalCount) = await _alertaRepository.GetPagedAtivosAsync(safePageIndex, safePageSize);
            var dtos = items.Select(AlertaMapper.ToDto).ToList();
            var pageResult = new PageResult<AlertaClimaticoDto>(dtos, totalCount, safePageIndex, safePageSize);
            _hateoasService.BuildPagedAlertaLinks(pageResult, requestPath);

            return OperationResult<PageResult<AlertaClimaticoDto>>.Ok(pageResult);
        }

        public async Task<OperationResult<AlertaClimaticoDto>> GetByIdAsync(int id)
        {
            var alerta = await _alertaRepository.GetByIdAsync(id);
            if (alerta == null)
            {
                return OperationResult<AlertaClimaticoDto>.Fail($"Alerta com ID {id} não encontrado.");
            }

            var dto = _hateoasService.BuildAlertaLinks(AlertaMapper.ToDto(alerta));
            return OperationResult<AlertaClimaticoDto>.Ok(dto);
        }

        public async Task<OperationResult<AlertaClimaticoDto>> CreateAsync(AlertaCreateDto dto)
        {
            var alerta = AlertaMapper.ToEntity(dto);
            var created = await _alertaRepository.AddAsync(alerta);
            _logger.LogInformation("Novo Alerta Climático emitido: {Titulo} ({Bairro})", created.Titulo, created.Bairro);
            var responseDto = _hateoasService.BuildAlertaLinks(AlertaMapper.ToDto(created));
            return OperationResult<AlertaClimaticoDto>.Ok(responseDto, "Alerta climático emitido com sucesso.");
        }

        public async Task<OperationResult> DesativarAsync(int id)
        {
            var alerta = await _alertaRepository.GetByIdAsync(id);
            if (alerta == null)
            {
                return OperationResult.Fail($"Alerta com ID {id} não encontrado.");
            }

            alerta.Ativo = false;
            await _alertaRepository.UpdateAsync(alerta);
            _logger.LogInformation("Alerta {Id} desativado.", id);
            return OperationResult.Ok("Alerta desativado com sucesso.");
        }
    }
}
