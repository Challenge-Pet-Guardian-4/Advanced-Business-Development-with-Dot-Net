using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Gerencia o conteúdo didático das aulas no MongoDB. A existência da aula é validada no banco relacional.
/// </summary>
public sealed class ConteudoAulaService(
    IAulaRepository aulaRepository,
    IConteudoAulaRepository conteudoRepository,
    ILogger<ConteudoAulaService> logger) : IConteudoAulaService
{
    public Task<ConteudoAulaResponse?> GetByAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default) =>
        conteudoRepository.GetByAulaIdAsync(aulaId, cancellationToken);

    public async Task<ConteudoAulaResponse> CreateAsync(
        Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default)
    {
        ValidarAulaExistente(aulaId);

        if (await conteudoRepository.GetByAulaIdAsync(aulaId, cancellationToken) is not null)
            throw new InvalidOperationException("A aula já possui conteúdo cadastrado.");

        logger.LogInformation("Cadastrando conteúdo da aula {AulaId} no MongoDB.", aulaId);
        return await conteudoRepository.InserirAsync(aulaId, request, cancellationToken);
    }

    public Task<ConteudoAulaResponse?> UpdateAsync(
        Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default)
    {
        ValidarAulaExistente(aulaId);
        return conteudoRepository.AtualizarAsync(aulaId, request, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid aulaId, CancellationToken cancellationToken = default)
    {
        ValidarAulaExistente(aulaId);
        return conteudoRepository.ExcluirPorAulaIdAsync(aulaId, cancellationToken);
    }

    private void ValidarAulaExistente(Guid aulaId)
    {
        if (!aulaRepository.ExistsById(aulaId))
            throw new InvalidOperationException("Aula não encontrada.");
    }
}
