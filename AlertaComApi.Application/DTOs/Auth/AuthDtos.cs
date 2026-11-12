using AlertaComApi.Domain.Common;

namespace AlertaComApi.Application.DTOs.Auth
{
    public record LoginRequestDto(string Username, string Password);

    public record LoginResponseDto(
        string Token,
        string Username,
        string Nome,
        string Email,
        string Role,
        DateTime ExpiresAt);

    public record RegisterRequestDto(
        string Username,
        string Password,
        string Nome,
        string Email,
        string Cpf,
        string Bairro,
        string? Telefone = null,
        string Role = "Cidadao");

    public record UserDto(
        int Id,
        string Username,
        string Nome,
        string Email,
        string Cpf,
        string Bairro,
        string Role,
        DateTime DataCriacao,
        List<LinkDto>? Links = null);
}
