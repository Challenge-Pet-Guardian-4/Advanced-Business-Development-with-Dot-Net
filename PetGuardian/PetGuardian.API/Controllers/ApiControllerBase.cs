using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetGuardian.Application.Common;
using PetGuardian.Domain.Enums;

namespace PetGuardian.API.Controllers;

/// <summary>Helpers compartilhados: identidade do usuário logado e construção de links HATEOAS.</summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected bool IsAdmin => User.IsInRole(nameof(RoleUsuario.Admin));

    protected Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>true se o usuário do token é o próprio <paramref name="usuarioId"/> ou é Admin.</summary>
    protected bool IsSelfOrAdmin(Guid usuarioId) => IsAdmin || CurrentUserId == usuarioId;

    protected Link LinkTo(string rel, string method, string action, object? values = null, string? controller = null) =>
        new(Url.ActionLink(action, controller, values) ?? string.Empty, rel, method);

    /// <summary>Adiciona links de navegação (self/first/prev/next/last) à página e links HATEOAS a cada item.</summary>
    protected PagedResponse<T> Paginar<T>(
        PagedResponse<T> pagina, string action, Func<int, object> routeValues, Func<T, T> comLinks)
    {
        var links = new List<Link>
        {
            LinkTo("self", "GET", action, routeValues(pagina.Page)),
            LinkTo("first", "GET", action, routeValues(1))
        };

        if (pagina.Page > 1)
            links.Add(LinkTo("prev", "GET", action, routeValues(pagina.Page - 1)));
        if (pagina.Page < pagina.TotalPages)
            links.Add(LinkTo("next", "GET", action, routeValues(pagina.Page + 1)));
        if (pagina.TotalPages > 0)
            links.Add(LinkTo("last", "GET", action, routeValues(pagina.TotalPages)));

        return pagina with { Items = pagina.Items.Select(comLinks).ToList(), Links = links };
    }
}