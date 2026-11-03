namespace PetGuardian.Application.Repositories;

/// <summary>
/// Coordena transações atômicas para casos de uso que realizam múltiplas escritas relacionais.
/// </summary>
public interface IUnitOfWork
{
    T ExecuteInTransaction<T>(Func<T> action);
}

public static class UnitOfWorkExtensions
{
    public static T RunInTransaction<T>(this IUnitOfWork? unitOfWork, Func<T> action) =>
        unitOfWork is null ? action() : unitOfWork.ExecuteInTransaction(action);
}
