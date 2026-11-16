using System.Net;
using System.Text.Json;
using AlertaComApi.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AlertaComApi.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exceção não tratada capturada pelo middleware global: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Instance = context.Request.Path,
                Detail = exception.Message
            };

            switch (exception)
            {
                case BadHttpRequestException badHttpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    problem.Title = "Requisição inválida";
                    problem.Status = (int)HttpStatusCode.BadRequest;
                    problem.Detail = badHttpEx.Message;
                    break;

                case DomainValidationException domainEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    problem.Title = "Erro de validação de dados";
                    problem.Status = (int)HttpStatusCode.BadRequest;
                    problem.Extensions["errors"] = domainEx.Errors;
                    break;

                case EntityNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    problem.Title = "Recurso não encontrado";
                    problem.Status = (int)HttpStatusCode.NotFound;
                    break;

                case BusinessRuleException:
                    context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    problem.Title = "Violação de regra de negócio";
                    problem.Status = (int)HttpStatusCode.UnprocessableEntity;
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    problem.Title = "Acesso não autorizado";
                    problem.Status = (int)HttpStatusCode.Unauthorized;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    problem.Title = "Erro interno no servidor";
                    problem.Status = (int)HttpStatusCode.InternalServerError;
                    problem.Detail = "Ocorreu um erro interno inesperado. Contate o suporte da plataforma AlertaCom.";
                    break;
            }

            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            return context.Response.WriteAsync(json);
        }
    }
}
