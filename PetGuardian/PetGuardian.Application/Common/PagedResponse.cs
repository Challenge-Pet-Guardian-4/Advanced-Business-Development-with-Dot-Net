using System.Text.Json.Serialization;

namespace PetGuardian.Application.Common;

/// <summary>Envelope paginado com links HATEOAS da coleção.</summary>
public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    [JsonPropertyName("_links")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<Link>? Links { get; init; }

    public static PagedResponse<T> Create(PagedResult<T> result, PageQuery query)
    {
        var q = query.Normalized();
        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)q.PageSize);
        return new PagedResponse<T>(result.Items, q.Page, q.PageSize, result.TotalCount, totalPages);
    }
}