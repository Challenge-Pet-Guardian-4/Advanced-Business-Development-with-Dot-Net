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
    public Task<ConteudoAulaResponse?> GetByAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Buscando conteúdo da aula {AulaId}.", aulaId);
        return conteudoRepository.GetByAulaIdAsync(aulaId, cancellationToken);
    }

    public async Task<ConteudoAulaResponse> CreateAsync(
        Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cadastrando conteúdo da aula {AulaId}.", aulaId);
        ValidarAulaExistente(aulaId);

        if (await conteudoRepository.GetByAulaIdAsync(aulaId, cancellationToken) is not null)
        {
            logger.LogWarning("A aula {AulaId} já possui conteúdo cadastrado.", aulaId);
            throw new InvalidOperationException("A aula já possui conteúdo cadastrado.");
        }

        return await conteudoRepository.InserirAsync(aulaId, request, cancellationToken);
    }

    public async Task<ConteudoAulaResponse?> UpdateAsync(
        Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Atualizando conteúdo da aula {AulaId}.", aulaId);
        ValidarAulaExistente(aulaId);

        var atualizado = await conteudoRepository.AtualizarAsync(aulaId, request, cancellationToken);
        if (atualizado is null)
            logger.LogWarning("Conteúdo da aula {AulaId} não encontrado para atualização.", aulaId);

        return atualizado;
    }

    public async Task<bool> DeleteAsync(Guid aulaId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Excluindo conteúdo da aula {AulaId}.", aulaId);
        ValidarAulaExistente(aulaId);

        var removido = await conteudoRepository.ExcluirPorAulaIdAsync(aulaId, cancellationToken);
        if (!removido)
            logger.LogWarning("Conteúdo da aula {AulaId} não encontrado para exclusão.", aulaId);

        return removido;
    }

    private void ValidarAulaExistente(Guid aulaId)
    {
        if (!aulaRepository.ExistsById(aulaId))
        {
            logger.LogWarning("Aula inexistente para operação de conteúdo: {AulaId}", aulaId);
            throw new InvalidOperationException("Aula não encontrada.");
        }
    }
}
