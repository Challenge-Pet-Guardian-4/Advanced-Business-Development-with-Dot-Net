using System.Text.Json.Serialization;

namespace PetGuardian.Application.Services.Interfaces;

public record ViaCepResponseDto(
    [property: JsonPropertyName("cep")] string? Cep,
    [property: JsonPropertyName("logradouro")] string? Logradouro,
    [property: JsonPropertyName("complemento")] string? Complemento,
    [property: JsonPropertyName("bairro")] string? Bairro,
    [property: JsonPropertyName("localidade")] string? Localidade,
    [property: JsonPropertyName("uf")] string? Uf,
    [property: JsonPropertyName("estado")] string? Estado,
    [property: JsonPropertyName("erro")] object? Erro
)
{
    public bool PossuiErro => Erro is bool b && b || Erro is string s && s.Equals("true", StringComparison.OrdinalIgnoreCase);
}

public interface IViaCepService
{
    Task<ViaCepResponseDto?> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default);
}
