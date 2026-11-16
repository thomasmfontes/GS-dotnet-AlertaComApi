using System.Security.Claims;
using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlertaComApi.Endpoints
{
    public static class AuthEndpoints
    {
        public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/auth")
                .WithTags("Autenticação & Usuários");

            group.MapPost("/login", async (
                [FromBody] LoginRequestDto request,
                IAuthService authService,
                IValidator<LoginRequestDto> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await authService.LoginAsync(request);
                if (!result.Success)
                {
                    return Results.Unauthorized();
                }

                AlertaComMetrics.LoginSucessoCounter.Add(1);
                return Results.Ok(result.Data);
            })
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Autentica um cidadão ou agente da Defesa Civil")
            .Produces<LoginResponseDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

            group.MapPost("/register", async (
                [FromBody] RegisterRequestDto request,
                IAuthService authService,
                IValidator<RegisterRequestDto> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await authService.RegisterAsync(request);
                if (!result.Success)
                {
                    return Results.BadRequest(new { message = result.Message, errors = result.Errors });
                }

                return Results.Created($"/api/auth/profile", result.Data);
            })
            .AllowAnonymous()
            .WithName("Register")
            .WithSummary("Cadastra um novo cidadão na plataforma AlertaCom")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

            group.MapGet("/profile", async (
                ClaimsPrincipal user,
                IAuthService authService) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var result = await authService.GetProfileAsync(userId);
                return result.Success ? Results.Ok(result.Data) : Results.NotFound(new { message = result.Message });
            })
            .RequireAuthorization()
            .WithName("GetProfile")
            .WithSummary("Obtém os dados do perfil do cidadão autenticado")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
