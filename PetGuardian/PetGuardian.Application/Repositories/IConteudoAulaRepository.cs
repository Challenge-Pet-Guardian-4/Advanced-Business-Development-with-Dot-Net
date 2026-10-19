using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Repositories;

/// <summary>Contrato do repositório NoSQL do conteúdo das aulas (a Application não conhece o driver).</summary>
public interface IConteudoAulaRepository
{
    Task<ConteudoAulaResponse?> GetByAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ConteudoAulaResponse>> GetByAulaIdsAsync(
        IReadOnlyCollection<Guid> aulaIds, CancellationToken cancellationToken = default);

    Task<ConteudoAulaResponse> InserirAsync(Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default);

    /// <returns>O documento atualizado, ou <c>null</c> se a aula ainda não possui conteúdo.</returns>
    Task<ConteudoAulaResponse?> AtualizarAsync(Guid aulaId, ConteudoAulaRequest request, CancellationToken cancellationToken = default);

    Task<bool> ExcluirPorAulaIdAsync(Guid aulaId, CancellationToken cancellationToken = default);
}
