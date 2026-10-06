using System.Linq.Expressions;
using System.Reflection;

namespace PetGuardian.Infrastructure.Persistence.Extensions;

internal static class QueryableOrderingExtensions
{
    /// <summary>OrderBy por nome de propriedade (já validada contra o modelo EF pelo repositório).</summary>
    public static IOrderedQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string propertyName, bool descending)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
                       ?? throw new ArgumentException($"Propriedade '{propertyName}' inexistente em {typeof(T).Name}.", nameof(propertyName));

        var parametro = Expression.Parameter(typeof(T), "e");
        var seletor = Expression.Lambda(Expression.Property(parametro, property), parametro);
        var metodo = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var chamada = Expression.Call(
            typeof(Queryable), metodo, [typeof(T), property.PropertyType],
            source.Expression, Expression.Quote(seletor));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(chamada);
    }
}