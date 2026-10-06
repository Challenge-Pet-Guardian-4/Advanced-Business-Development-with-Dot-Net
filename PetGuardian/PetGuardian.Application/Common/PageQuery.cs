using System.ComponentModel.DataAnnotations;

namespace PetGuardian.Application.Common;

/// <summary>Paginação/ordenação via query string: ?page=1&amp;pageSize=10&amp;sortBy=Nome&amp;sortDir=asc</summary>
public sealed class PageQuery
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    [Range(1, int.MaxValue, ErrorMessage = "page deve ser maior ou igual a 1.")]
    public int Page { get; set; } = 1;

    [Range(1, MaxPageSize, ErrorMessage = "pageSize deve estar entre 1 e 100.")]
    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>Propriedade de ordenação (ex.: Nome, Prazo). Campo inválido retorna 400.</summary>
    public string? SortBy { get; set; }

    [RegularExpression("(?i)^(asc|desc)$", ErrorMessage = "sortDir deve ser 'asc' ou 'desc'.")]
    public string? SortDir { get; set; } = "asc";

    public bool Descending => string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);

    public PageQuery Normalized() => new()
    {
        Page = Math.Max(1, Page),
        PageSize = Math.Clamp(PageSize, 1, MaxPageSize),
        SortBy = string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim(),
        SortDir = Descending ? "desc" : "asc"
    };
}