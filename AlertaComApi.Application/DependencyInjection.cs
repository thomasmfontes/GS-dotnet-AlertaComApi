using AlertaComApi.Application.DTOs.Auth;
using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Application.Services;
using AlertaComApi.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AlertaComApi.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Serviços de Aplicação
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IOcorrenciaService, OcorrenciaService>();
            services.AddScoped<IAlertaService, AlertaService>();
            services.AddScoped<IHateoasService, HateoasService>();

            // Validadores FluentValidation
            services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();
            services.AddScoped<IValidator<RegisterRequestDto>, RegisterRequestValidator>();
            services.AddScoped<IValidator<OcorrenciaCreateDto>, OcorrenciaCreateValidator>();

            return services;
        }
    }
}
