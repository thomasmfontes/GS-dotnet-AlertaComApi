using System.Security.Claims;
using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Diagnostics;
using AlertaComApi.Domain.Common;
using AlertaComApi.Domain.Enums;
using AlertaComApi.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlertaComApi.Endpoints
{
    public static class OcorrenciaEndpoints
    {
        public static RouteGroupBuilder MapOcorrenciaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/ocorrencias")
                .WithTags("Ocorrências Cidadãs")
                .RequireRateLimiting("fixed"); // Rate Limiting aplicado aos endpoints de ocorrência

            group.MapGet("/", async (
                [AsParameters] OcorrenciaFilterDto filter,
                HttpContext httpContext,
                IOcorrenciaService service) =>
            {
                var basePath = httpContext.Request.Path.Value ?? "/api/ocorrencias";
                var result = await service.GetPagedAsync(filter, basePath);
                if (result.Data == null || result.Data.TotalCount == 0 || !result.Data.Items.Any())
                {
                    return Results.NotFound(new { message = "Nenhuma ocorrência encontrada com os critérios informados." });
                }
                return Results.Ok(result.Data);
            })
            .AllowAnonymous()
            .WithName("GetOcorrenciasPaged")
            .WithSummary("Lista as ocorrências registradas com suporte a paginação, filtros e HATEOAS")
            .Produces<PageResult<OcorrenciaResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/{id:int}", async (
                int id,
                IOcorrenciaService service) =>
            {
                var result = await service.GetByIdAsync(id);
                return result.Success 
                    ? Results.Ok(result.Data) 
                    : Results.NotFound(new { message = result.Message });
            })
            .AllowAnonymous()
            .WithName("GetOcorrenciaById")
            .WithSummary("Consulta uma ocorrência específica por ID")
            .Produces<OcorrenciaResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/protocolo/{protocolo}", async (
                string protocolo,
                IOcorrenciaService service) =>
            {
                var result = await service.GetByProtocoloAsync(protocolo);
                return result.Success 
                    ? Results.Ok(result.Data) 
                    : Results.NotFound(new { message = result.Message });
            })
            .AllowAnonymous()
            .WithName("GetOcorrenciaByProtocolo")
            .WithSummary("Consulta uma ocorrência pelo código do protocolo cívico")
            .Produces<OcorrenciaResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/minhas", async (
                ClaimsPrincipal user,
                IOcorrenciaService service) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var result = await service.GetByUserIdAsync(userId);
                if (result.Data == null || !result.Data.Any())
                {
                    return Results.NotFound(new { message = "Nenhuma ocorrência encontrada para este cidadão." });
                }

                return Results.Ok(result.Data);
            })
            .RequireAuthorization()
            .WithName("GetMinhasOcorrencias")
            .WithSummary("Lista o histórico de ocorrências abertas pelo cidadão autenticado")
            .Produces<IEnumerable<OcorrenciaResponseDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost("/", async (
                [FromBody] OcorrenciaCreateDto dto,
                [FromHeader(Name = IdempotencyMiddleware.IdempotencyHeader)] string? idempotencyKey,
                ClaimsPrincipal user,
                IOcorrenciaService service,
                IValidator<OcorrenciaCreateDto> validator) =>
            {
                var validationResult = await validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var result = await service.CreateAsync(dto, userId, idempotencyKey);
                if (!result.Success)
                {
                    return Results.BadRequest(new { message = result.Message, errors = result.Errors });
                }

                AlertaComMetrics.OcorrenciasCriadasCounter.Add(1);
                return Results.Created($"/api/ocorrencias/{result.Data!.Id}", result.Data);
            })
            .RequireAuthorization()
            .WithName("CreateOcorrencia")
            .WithSummary("Registra uma nova ocorrência urbana (com suporte a chave de Idempotência)")
            .Produces<OcorrenciaResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPatch("/{id:int}/status", async (
                int id,
                [FromBody] OcorrenciaUpdateStatusDto dto,
                ClaimsPrincipal user,
                IOcorrenciaService service) =>
            {
                var username = user.FindFirst(ClaimTypes.Name)?.Value ?? "Gestor";
                var result = await service.UpdateStatusAsync(id, dto, username);

                return result.Success 
                    ? Results.Ok(result.Data) 
                    : Results.NotFound(new { message = result.Message });
            })
            .RequireAuthorization(policy => policy.RequireRole(PerfilUsuario.DefesaCivil, PerfilUsuario.Admin))
            .WithName("UpdateOcorrenciaStatus")
            .WithSummary("Atualiza o status de atendimento da ocorrência (Restrito à Defesa Civil e Gestores)")
            .Produces<OcorrenciaResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapDelete("/{id:int}", async (
                int id,
                ClaimsPrincipal user,
                IOcorrenciaService service) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = user.FindFirst(ClaimTypes.Role)?.Value ?? PerfilUsuario.Cidadao;

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var result = await service.DeleteAsync(id, userId, userRole);
                if (!result.Success)
                {
                    return Results.BadRequest(new { message = result.Message });
                }

                return Results.NoContent();
            })
            .RequireAuthorization()
            .WithName("DeleteOcorrencia")
            .WithSummary("Cancela ou remove uma ocorrência (disponível ao autor ou moderadores)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

            return group;
        }
    }
}
