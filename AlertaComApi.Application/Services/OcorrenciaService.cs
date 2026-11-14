using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Application.Mappers;
using AlertaComApi.Domain.Common;
using AlertaComApi.Domain.Entities;
using AlertaComApi.Domain.Enums;
using AlertaComApi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AlertaComApi.Application.Services
{
    public class OcorrenciaService : IOcorrenciaService
    {
        private readonly IOcorrenciaRepository _ocorrenciaRepository;
        private readonly IHateoasService _hateoasService;
        private readonly IOcorrenciaNoSqlRepository? _noSqlRepository;
        private readonly ILogger<OcorrenciaService> _logger;

        public OcorrenciaService(
            IOcorrenciaRepository ocorrenciaRepository,
            IHateoasService hateoasService,
            ILogger<OcorrenciaService> logger,
            IOcorrenciaNoSqlRepository? noSqlRepository = null)
        {
            _ocorrenciaRepository = ocorrenciaRepository;
            _hateoasService = hateoasService;
            _logger = logger;
            _noSqlRepository = noSqlRepository;
        }

        public async Task<OperationResult<OcorrenciaResponseDto>> GetByIdAsync(int id)
        {
            var ocorrencia = await _ocorrenciaRepository.GetByIdAsync(id);
            if (ocorrencia == null)
            {
                return OperationResult<OcorrenciaResponseDto>.Fail($"Ocorrência com ID {id} não encontrada.");
            }

            var dto = OcorrenciaMapper.ToDto(ocorrencia);
            _hateoasService.BuildOcorrenciaLinks(dto);

            return OperationResult<OcorrenciaResponseDto>.Ok(dto);
        }

        public async Task<OperationResult<OcorrenciaResponseDto>> GetByProtocoloAsync(string protocolo)
        {
            var ocorrencia = await _ocorrenciaRepository.GetByProtocoloAsync(protocolo);
            if (ocorrencia == null)
            {
                return OperationResult<OcorrenciaResponseDto>.Fail($"Ocorrência com protocolo '{protocolo}' não encontrada.");
            }

            var dto = OcorrenciaMapper.ToDto(ocorrencia);
            _hateoasService.BuildOcorrenciaLinks(dto);

            return OperationResult<OcorrenciaResponseDto>.Ok(dto);
        }

        public async Task<OperationResult<PageResult<OcorrenciaResponseDto>>> GetPagedAsync(OcorrenciaFilterDto filter, string requestPath)
        {
            var pageIndex = filter.PageIndex ?? 1;
            var pageSize = filter.PageSize ?? 10;

            var (items, totalCount) = await _ocorrenciaRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                filter.Status,
                filter.TipoRisco,
                filter.Bairro);

            var dtoList = items.Select(OcorrenciaMapper.ToDto).ToList();
            var pageResult = new PageResult<OcorrenciaResponseDto>(dtoList, totalCount, pageIndex, pageSize);
            _hateoasService.BuildPagedOcorrenciaLinks(pageResult, requestPath);

            return OperationResult<PageResult<OcorrenciaResponseDto>>.Ok(pageResult);
        }

        public async Task<OperationResult<IEnumerable<OcorrenciaResponseDto>>> GetByUserIdAsync(int userId)
        {
            var items = await _ocorrenciaRepository.GetByUserIdAsync(userId);
            var dtoList = items.Select(o =>
            {
                var dto = OcorrenciaMapper.ToDto(o);
                return _hateoasService.BuildOcorrenciaLinks(dto);
            }).ToList();

            return OperationResult<IEnumerable<OcorrenciaResponseDto>>.Ok(dtoList);
        }

        public async Task<OperationResult<OcorrenciaResponseDto>> CreateAsync(
            OcorrenciaCreateDto dto, 
            int userId, 
            string? idempotencyKey = null)
        {
            // Verificação de Idempotência: caso já tenha sido processada com esta chave
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existing = await _ocorrenciaRepository.GetByIdempotencyKeyAsync(idempotencyKey);
                if (existing != null)
                {
                    _logger.LogInformation("Requisição com Idempotency-Key '{Key}' já processada anteriormente (ID: {Id}). Retornando registro existente.", idempotencyKey, existing.Id);
                    var existingDto = OcorrenciaMapper.ToDto(existing);
                    _hateoasService.BuildOcorrenciaLinks(existingDto);
                    return OperationResult<OcorrenciaResponseDto>.Ok(existingDto, "Registro recuperado por idempotência.");
                }
            }

            var ocorrencia = OcorrenciaMapper.ToEntity(dto, userId, idempotencyKey);
            var created = await _ocorrenciaRepository.AddAsync(ocorrencia);

            _logger.LogInformation("Nova ocorrência registrada com sucesso: {Protocolo} (ID: {Id}) por Usuário {UserId}", created.Protocolo, created.Id, userId);

            // Espelhamento assíncrono/opcional em background no MongoDB NoSQL sem bloquear a resposta HTTP
            if (_noSqlRepository != null)
            {
                var docNoSql = new OcorrenciaNoSql
                {
                    OcorrenciaIdRelacional = created.Id,
                    Protocolo = created.Protocolo,
                    TipoRisco = created.TipoRisco.ToString(),
                    Severidade = created.Severidade.ToString(),
                    Status = created.Status.ToString(),
                    Endereco = created.Endereco,
                    Bairro = created.Bairro,
                    Localizacao = new GeoJsonPointLocalizacao
                    {
                        Coordinates = new[] { created.Longitude, created.Latitude }
                    },
                    DataCriacao = created.DataCriacao
                };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _noSqlRepository.AddAsync(docNoSql);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Não foi possível sincronizar documento NoSQL: {Message}", ex.Message);
                    }
                });
            }

            var responseDto = OcorrenciaMapper.ToDto(created);
            _hateoasService.BuildOcorrenciaLinks(responseDto);

            return OperationResult<OcorrenciaResponseDto>.Ok(responseDto, "Ocorrência criada com sucesso.");
        }

        public async Task<OperationResult<OcorrenciaResponseDto>> UpdateStatusAsync(int id, OcorrenciaUpdateStatusDto dto, string usuario)
        {
            var ocorrencia = await _ocorrenciaRepository.GetByIdAsync(id);
            if (ocorrencia == null)
            {
                return OperationResult<OcorrenciaResponseDto>.Fail($"Ocorrência com ID {id} não encontrada.");
            }

            ocorrencia.Status = dto.NovoStatus;
            ocorrencia.ParecerTecnico = dto.ParecerTecnico ?? ocorrencia.ParecerTecnico;
            ocorrencia.DataAtualizacao = DateTime.UtcNow;

            await _ocorrenciaRepository.UpdateAsync(ocorrencia);
            _logger.LogInformation("Status da ocorrência {Id} atualizado para {Status} pelo usuário {Usuario}", id, dto.NovoStatus, usuario);

            var responseDto = OcorrenciaMapper.ToDto(ocorrencia);
            _hateoasService.BuildOcorrenciaLinks(responseDto);

            return OperationResult<OcorrenciaResponseDto>.Ok(responseDto, "Status atualizado com sucesso.");
        }

        public async Task<OperationResult> DeleteAsync(int id, int userId, string userRole)
        {
            var ocorrencia = await _ocorrenciaRepository.GetByIdAsync(id);
            if (ocorrencia == null)
            {
                return OperationResult.Fail($"Ocorrência com ID {id} não encontrada.");
            }

            // Apenas o próprio autor ou DefesaCivil/Admin pode cancelar/deletar
            if (ocorrencia.UserId != userId && userRole != PerfilUsuario.Admin && userRole != PerfilUsuario.DefesaCivil)
            {
                return OperationResult.Fail("Você não tem permissão para cancelar ou excluir esta ocorrência.");
            }

            await _ocorrenciaRepository.DeleteAsync(ocorrencia);
            _logger.LogInformation("Ocorrência {Id} removida/cancelada por Usuário {UserId}", id, userId);

            return OperationResult.Ok("Ocorrência removida com sucesso.");
        }
    }
}
