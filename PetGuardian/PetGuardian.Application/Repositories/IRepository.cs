using System.Linq.Expressions;
using PetGuardian.Domain.Common;
using PetGuardian.Application.Common;

namespace PetGuardian.Application.Repositories;

/// <summary>
/// Contrato genérico de persistência para entidades que derivam de <see cref="BaseEntity"/>.
/// </summary>
/// <typeparam name="T">Tipo da entidade de domínio.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    IReadOnlyList<T> GetAll();

    IReadOnlyList<T> Find(Expression<Func<T, bool>> predicate);

    T? FirstOrDefault(Expression<Func<T, bool>> predicate);

    T? GetById(Guid id);

    T Add(T entity);
    
    T Update(T entity);

    bool Delete(Guid id);

    bool ExistsById(Guid id);

    bool Exists(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Verifica existência pelo campo <c>Nome</c>. Lança <see cref="InvalidOperationException"/>
    /// se a entidade não possuir essa propriedade mapeada.
    /// </summary>
    bool ExistsByNome(string valor);
    
    /// <summary>
    /// Consulta paginada com filtro e ordenação executados no banco (Skip/Take).
    /// sortBy inválido ou sensível lança ArgumentException (vira 400 no GlobalExceptionHandler).
    /// </summary>
    PagedResult<T> GetPaged(Expression<Func<T, bool>>? filter, string? sortBy, bool descending, int page, int pageSize);
}
