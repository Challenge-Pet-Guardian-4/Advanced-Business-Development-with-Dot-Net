using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

public sealed class UsuarioQueryService(IUsuarioRepository usuarioRepository, ILogger<UsuarioQueryService> logger) : IUsuarioQueryService
{
    public PagedResponse<UsuarioResponse> Search(PageQuery query, UsuarioFilter filter)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("UsuarioQueryService.Search");
        var q = query.Normalized();
        logger.LogInformation("Pesquisando usuários: página {Page}, tamanho {PageSize}, ordenação {SortBy} {SortDir}.",
            q.Page, q.PageSize, q.SortBy ?? nameof(Usuario.Nome), q.SortDir);

        var nome = filter.Nome?.Trim().ToLowerInvariant();
        var email = filter.Email?.Trim().ToLowerInvariant();
        var filtrarRole = filter.Role.HasValue; var role = filter.Role.GetValueOrDefault();

        Expression<Func<Usuario, bool>> predicate = u =>
            (nome == null || u.Nome.ToLower().Contains(nome))
            && (email == null || u.Email.ToLower().Contains(email))
            && (!filtrarRole || u.Role == role);

        var resultado = usuarioRepository.GetPaged(predicate, q.SortBy ?? nameof(Usuario.Nome), q.Descending, q.Page, q.PageSize);
        return PagedResponse<UsuarioResponse>.Create(resultado.Map(UsuarioResponse.FromDomain), q);
    }
}