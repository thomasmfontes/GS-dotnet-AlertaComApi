using AlertaComApi.Application.DTOs.Alertas;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Diagnostics;
using AlertaComApi.Domain.Common;
using AlertaComApi.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AlertaComApi.Endpoints
{
    public static class AlertaEndpoints
    {
        public static RouteGroupBuilder MapAlertaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/alertas")
                .WithTags("Alertas Climáticos & Defesa Civil");

            group.MapGet("/", async (
                [FromQuery] int? pageIndex,
                [FromQuery] int? pageSize,
                HttpContext httpContext,
                IAlertaService service) =>
            {
                var result = await service.GetAlertasPaginadosAsync(
                    pageIndex ?? 1,
                    pageSize ?? 10,
                    httpContext.Request.Path.Value ?? "/api/alertas");

                if (result.Data == null || !result.Data.Items.Any())
                {
                    return Results.NotFound(new { message = "Nenhum alerta climático ativo no momento." });
                }

                return Results.Ok(result.Data);
            })
            .AllowAnonymous()
            .WithName("GetAlertasAtivos")
            .WithSummary("Lista todos os alertas climáticos ativos com paginação e HATEOAS")
            .Produces<PageResult<AlertaClimaticoDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/{id:int}", async (int id, IAlertaService service) =>
            {
                var result = await service.GetByIdAsync(id);
                return result.Success 
                    ? Results.Ok(result.Data) 
                    : Results.NotFound(new { message = result.Message });
            })
            .AllowAnonymous()
            .WithName("GetAlertaById")
            .WithSummary("Consulta um alerta climático específico por ID")
            .Produces<AlertaClimaticoDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/", async (
                [FromBody] AlertaCreateDto dto,
                IAlertaService service) =>
            {
                var result = await service.CreateAsync(dto);
                if (!result.Success)
                {
                    return Results.BadRequest(new { message = result.Message });
                }

                AlertaComMetrics.AlertasEmitidosCounter.Add(1);
                return Results.Created($"/api/alertas/{result.Data!.Id}", result.Data);
            })
            .RequireAuthorization(policy => policy.RequireRole(PerfilUsuario.DefesaCivil, PerfilUsuario.Admin))
            .WithName("CreateAlerta")
            .WithSummary("Emite um novo alerta de risco climático (Restrito à Defesa Civil e Administradores)")
            .Produces<AlertaClimaticoDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

            group.MapPatch("/{id:int}/desativar", async (int id, IAlertaService service) =>
            {
                var result = await service.DesativarAsync(id);
                return result.Success 
                    ? Results.Ok(new { message = result.Message }) 
                    : Results.NotFound(new { message = result.Message });
            })
            .RequireAuthorization(policy => policy.RequireRole(PerfilUsuario.DefesaCivil, PerfilUsuario.Admin))
            .WithName("DesativarAlerta")
            .WithSummary("Desativa um alerta de risco que expirou ou foi controlado")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
