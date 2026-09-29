using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Common;
using PetGuardian.Infrastructure.Persistence;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação genérica EF Core de <see cref="IRepository{T}"/>.
/// </summary>
public class Repository<T>(PetGuardianContext context) : IRepository<T> where T : BaseEntity
{
    protected PetGuardianContext Context { get; } = context;

    private readonly DbSet<T> _set = context.Set<T>();

    // Nome da propriedade de denominação principal usada em todas as entidades do projeto.
    private const string PropriedadeNome = "Nome";

    /// <inheritdoc />
    public IReadOnlyList<T> GetAll()
    {
        return _set
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<T> Find(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _set
            .AsNoTracking()
            .Where(predicate)
            .ToList();
    }

    /// <inheritdoc />
    public T? FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _set.FirstOrDefault(predicate);
    }

    /// <inheritdoc />
    public T? GetById(Guid id)
    {
        return _set.Find(id);
    }

    /// <inheritdoc />
    public T Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _set.Add(entity);
        Context.SaveChanges();

        return entity;
    }

    /// <inheritdoc />
    public T Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _set.Update(entity);
        Context.SaveChanges();

        return entity;
    }

    /// <inheritdoc />
    public bool Delete(Guid id)
    {
        var entity = GetById(id);
        if (entity is null)
            return false;

        _set.Remove(entity);
        Context.SaveChanges();

        return true;
    }

    /// <inheritdoc />
    public bool ExistsById(Guid id)
    {
        return _set.Any(e => e.Id == id);
    }

    /// <inheritdoc />
    public bool Exists(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _set.Any(predicate);
    }

    /// <inheritdoc />
    public bool ExistsByNome(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return false;

        ValidarPropriedadeNome();

        var normalizado = valor.Trim().ToLowerInvariant();

        return _set.Any(e =>
            EF.Property<string>(e, PropriedadeNome)
              .ToLower() == normalizado);
    }

    private void ValidarPropriedadeNome()
    {
        var entityType = Context.Model.FindEntityType(typeof(T));

        if (entityType is null)
            throw new InvalidOperationException(
                $"ExistsByNome: o tipo '{typeof(T).Name}' não está registrado no modelo EF Core.");

        var prop = entityType.FindProperty(PropriedadeNome);

        if (prop is null || prop.ClrType != typeof(string))
            throw new InvalidOperationException(
                $"ExistsByNome: a entidade '{typeof(T).Name}' não possui a propriedade " +
                $"'{PropriedadeNome}' (string) mapeada. " +
                "Esse método só se aplica a entidades com campo de denominação 'Nome'.");
    }
}
