using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Implementação assíncrona desacoplada para consulta de endereços na API ViaCEP com logging estruturado.
/// </summary>
public sealed class ViaCepService(
    HttpClient                 httpClient,
    ILogger<ViaCepService>     logger) : IViaCepService
{
    public async Task<ViaCepResponseDto?> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cep))
        {
            logger.LogWarning("Tentativa de consulta com CEP nulo ou vazio.");
            return null;
        }

        var cepLimpo = cep.Trim().Replace("-", "").Replace(".", "");
        if (cepLimpo.Length != 8 || !cepLimpo.All(char.IsDigit))
        {
            logger.LogWarning("Formato de CEP inválido para consulta: {CepOriginal}", cep);
            return null;
        }

        var url = $"https://viacep.com.br/ws/{cepLimpo}/json/";
        logger.LogInformation("Consultando serviço externo ViaCEP para CEP: {Cep}", cepLimpo);

        try
        {
            var response = await httpClient.GetFromJsonAsync<ViaCepResponseDto>(url, cancellationToken);
            if (response == null || response.PossuiErro)
            {
                logger.LogWarning("ViaCEP retornou CEP não encontrado ou inexistente para: {Cep}", cepLimpo);
                return null;
            }

            logger.LogInformation("Endereço resolvido com sucesso via ViaCEP: {Logradouro}, {Bairro}, {Localidade}-{Uf}",
                response.Logradouro, response.Bairro, response.Localidade, response.Uf);

            return response;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Falha de comunicação HTTP ao consultar ViaCEP para CEP {Cep}: {Message}", cepLimpo, ex.Message);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "Timeout/Cancelamento ao consultar ViaCEP para CEP {Cep}: {Message}", cepLimpo, ex.Message);
            return null;
        }
    }
}
