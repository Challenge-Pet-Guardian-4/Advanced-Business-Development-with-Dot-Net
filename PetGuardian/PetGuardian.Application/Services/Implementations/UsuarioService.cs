using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de usuários, autenticação segura (BCrypt) e score de gamificação.
/// </summary>
public sealed class UsuarioService(
    IUsuarioRepository      usuarioRepository,
    IRepository<Telefone>   telefoneRepository,
    IEnderecoService        enderecoService,
    ITarefaRepository       tarefaRepository,
    ILogger<UsuarioService> logger,
    IUnitOfWork?            unitOfWork = null) : IUsuarioService
{
    public PagedResponse<UsuarioResponse> Search(PageQuery query, UsuarioFilter filter)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("UsuarioService.Search");
        var q = query.Normalized();

        var nome = filter.Nome?.Trim().ToLowerInvariant();
        var email = filter.Email?.Trim().ToLowerInvariant();
        Expression<Func<Usuario, bool>> predicate = u =>
            (nome == null || u.Nome.ToLower().Contains(nome))
            && (email == null || u.Email.ToLower().Contains(email))
            && (filter.Role == null || u.Role == filter.Role);

        var resultado = usuarioRepository.GetPaged(predicate, q.SortBy ?? nameof(Usuario.Nome), q.Descending, q.Page, q.PageSize);
        return PagedResponse<UsuarioResponse>.Create(resultado.Map(UsuarioResponse.FromDomain), q);
    }

    public UsuarioResponse? GetById(Guid id) =>
        usuarioRepository.GetById(id) is { } u ? UsuarioResponse.FromDomain(u) : null;

    public UsuarioResponse? GetByEmail(string email) =>
        usuarioRepository.GetByEmail(email) is { } u ? UsuarioResponse.FromDomain(u) : null;

    public UsuarioScoreResponse GetScore(Guid usuarioId)
    {
        if (!usuarioRepository.ExistsById(usuarioId))
            throw new InvalidOperationException("Usuário não encontrado.");

        var pontosTotais = tarefaRepository.GetByUsuarioId(usuarioId)
            .Where(t => t.Conclusao.HasValue)
            .Sum(t => t.PontosTarefa);

        return new UsuarioScoreResponse(usuarioId, pontosTotais);
    }

    public UsuarioResponse Create(UsuarioRequest request)
    {
        if (usuarioRepository.ExistsByEmail(request.Email))
            throw new InvalidOperationException("Já existe um usuário com este e-mail.");

        return unitOfWork.RunInTransaction(() =>
        {
            var endereco = enderecoService.Create(request.Endereco);
            var telefone = telefoneRepository.Add(request.ToTelefone());
            var usuario = usuarioRepository.Add(request.ToDomain(telefone.Id, endereco.Id));

            logger.LogInformation("Usuário {UsuarioId} ({Email}) cadastrado com sucesso.", usuario.Id, usuario.Email);
            return UsuarioResponse.FromDomain(usuario);
        });
    }

    public UsuarioResponse? Update(Guid id, UsuarioUpdateRequest request)
    {
        var usuario = usuarioRepository.GetById(id);
        if (usuario is null)
            return null;

        var usuarioComEsteEmail = usuarioRepository.GetByEmail(request.Email);
        if (usuarioComEsteEmail is not null && usuarioComEsteEmail.Id != id)
            throw new InvalidOperationException("Já existe um usuário com este e-mail.");

        return unitOfWork.RunInTransaction(() =>
        {
            usuario.AtualizarNome(request.Nome);
            usuario.AtualizarEmail(request.Email);
            usuario.AtualizarSenha(request.Senha);
            usuario.AtualizarRole(request.Role);

            if (!string.IsNullOrWhiteSpace(request.Ddd) && !string.IsNullOrWhiteSpace(request.NumeroTelefone))
            {
                var telefone = telefoneRepository.GetById(usuario.TelefoneId);
                if (telefone is not null)
                {
                    telefone.Atualizar(request.Ddd, request.NumeroTelefone);
                    telefoneRepository.Update(telefone);
                }
            }

            if (request.Endereco is not null)
            {
                var endereco = enderecoService.Create(request.Endereco);
                usuario.AtualizarEndereco(endereco.Id);
            }

            usuarioRepository.Update(usuario);
            logger.LogInformation("Usuário {UsuarioId} atualizado com sucesso.", id);
            return UsuarioResponse.FromDomain(usuario);
        });
    }

    public UsuarioResponse UpgradePremium(Guid id)
    {
        var usuario = usuarioRepository.GetById(id)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        usuario.AtualizarRole(RoleUsuario.Premium);
        usuarioRepository.Update(usuario);
        logger.LogInformation("Usuário {UsuarioId} atualizado para Premium.", id);
        return UsuarioResponse.FromDomain(usuario);
    }

    public bool Delete(Guid id) => usuarioRepository.Delete(id);
}
