using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core de <see cref="IUnitOfWork"/> com suporte a transações aninhadas e provedor InMemory em testes.
/// </summary>
public sealed class UnitOfWork(PetGuardianContext context) : IUnitOfWork
{
    public T ExecuteInTransaction<T>(Func<T> action)
    {
        if (!context.Database.IsRelational() || context.Database.CurrentTransaction is not null)
            return action();

        using var transaction = context.Database.BeginTransaction();
        var result = action();
        transaction.Commit();
        return result;
    }
}
