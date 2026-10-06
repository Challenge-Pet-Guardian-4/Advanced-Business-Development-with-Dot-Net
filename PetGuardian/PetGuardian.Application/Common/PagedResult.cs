namespace PetGuardian.Application.Common;

/// <summary>Resultado bruto de uma consulta paginada nos repositórios.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount)
{
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new(Items.Select(selector).ToList(), TotalCount);
}