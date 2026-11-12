using FluentValidation;
using AlertaComApi.Application.DTOs.Ocorrencias;
using AlertaComApi.Application.DTOs.Auth;

namespace AlertaComApi.Application.Validators
{
    public class OcorrenciaCreateValidator : AbstractValidator<OcorrenciaCreateDto>
    {
        public OcorrenciaCreateValidator()
        {
            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("O título da ocorrência é obrigatório.")
                .MinimumLength(5).WithMessage("O título deve ter no mínimo 5 caracteres.")
                .MaximumLength(100).WithMessage("O título não pode exceder 100 caracteres.");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.")
                .MinimumLength(10).WithMessage("A descrição deve ter no mínimo 10 caracteres.");

            RuleFor(x => x.Endereco)
                .NotEmpty().WithMessage("O endereço do ocorrido é obrigatório.");

            RuleFor(x => x.Bairro)
                .NotEmpty().WithMessage("O bairro é obrigatório.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90.0, 90.0).WithMessage("Latitude inválida.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180.0, 180.0).WithMessage("Longitude inválida.");
        }
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("O nome de usuário é obrigatório.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(4).WithMessage("A senha deve ter no mínimo 4 caracteres.");
        }
    }

    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("O nome de usuário é obrigatório.")
                .MinimumLength(3).WithMessage("O usuário deve ter pelo menos 3 caracteres.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome completo é obrigatório.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("E-mail em formato inválido.");

            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("O CPF é obrigatório.");

            RuleFor(x => x.Bairro)
                .NotEmpty().WithMessage("O bairro do cidadão é obrigatório.");
        }
    }
}
