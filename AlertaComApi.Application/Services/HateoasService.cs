using AlertaComApi.Application.DTOs.Alertas;
using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Domain.Common;

namespace AlertaComApi.Application.Services
{
    public class HateoasService : IHateoasService
    {
        public OcorrenciaResponseDto BuildOcorrenciaLinks(OcorrenciaResponseDto dto)
        {
            dto.Links.Clear();
            dto.Links.Add(new LinkDto($"/api/ocorrencias/{dto.Id}", "self", "GET"));
            dto.Links.Add(new LinkDto($"/api/ocorrencias/{dto.Id}/status", "update_status", "PATCH"));
            dto.Links.Add(new LinkDto($"/api/ocorrencias/{dto.Id}", "delete", "DELETE"));
            dto.Links.Add(new LinkDto("/api/ocorrencias", "collection", "GET"));
            return dto;
        }

        public PageResult<OcorrenciaResponseDto> BuildPagedOcorrenciaLinks(PageResult<OcorrenciaResponseDto> pageResult, string basePath)
        {
            foreach (var item in pageResult.Items)
            {
                BuildOcorrenciaLinks(item);
            }

            PopulatePageLinks(pageResult, basePath);
            return pageResult;
        }

        public AlertaClimaticoDto BuildAlertaLinks(AlertaClimaticoDto dto)
        {
            dto.Links.Clear();
            dto.Links.Add(new LinkDto($"/api/alertas/{dto.Id}", "self", "GET"));
            dto.Links.Add(new LinkDto($"/api/alertas/{dto.Id}/desativar", "deactivate", "PATCH"));
            dto.Links.Add(new LinkDto("/api/alertas", "collection", "GET"));
            return dto;
        }

        public PageResult<AlertaClimaticoDto> BuildPagedAlertaLinks(PageResult<AlertaClimaticoDto> pageResult, string basePath)
        {
            foreach (var item in pageResult.Items)
            {
                BuildAlertaLinks(item);
            }

            PopulatePageLinks(pageResult, basePath);
            return pageResult;
        }

        public UserDto BuildUserLinks(UserDto dto)
        {
            var links = new List<LinkDto>
            {
                new("/api/auth/profile", "self", "GET"),
                new("/api/ocorrencias/minhas", "my_incidents", "GET"),
                new("/api/alertas", "active_alerts", "GET")
            };

            return dto with { Links = links };
        }

        private static void PopulatePageLinks<T>(PageResult<T> pageResult, string basePath)
        {
            pageResult.Links.Clear();
            pageResult.Links.Add(new LinkDto($"{basePath}?pageIndex={pageResult.PageIndex}&pageSize={pageResult.PageSize}", "self", "GET"));

            if (pageResult.HasPreviousPage)
            {
                pageResult.Links.Add(new LinkDto($"{basePath}?pageIndex={pageResult.PageIndex - 1}&pageSize={pageResult.PageSize}", "previous_page", "GET"));
            }

            if (pageResult.HasNextPage)
            {
                pageResult.Links.Add(new LinkDto($"{basePath}?pageIndex={pageResult.PageIndex + 1}&pageSize={pageResult.PageSize}", "next_page", "GET"));
            }
        }
    }
}
