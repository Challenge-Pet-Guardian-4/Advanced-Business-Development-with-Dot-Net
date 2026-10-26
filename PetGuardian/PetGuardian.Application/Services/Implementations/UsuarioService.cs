using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de usuários, autenticação segura (BCrypt) e score de gamificação.
/// </summary>
public sealed class UsuarioService(
    IUsuarioRepository         usuarioRepository,
    IRepository<Telefone>      telefoneRepository,
    IEnderecoService           enderecoService,
    ITarefaRepository          tarefaRepository,
    ILogger<UsuarioService>    logger) : IUsuarioService
{
    public PagedResponse<UsuarioResponse> Search(PageQuery query, UsuarioFilter filter)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("UsuarioService.Search");
        var q = query.Normalized();
        logger.LogInformation("Pesquisando usuários: página {Page}, tamanho {PageSize}, ordenação {SortBy} {SortDir}.",
            q.Page, q.PageSize, q.SortBy ?? nameof(Usuario.Nome), q.SortDir);

        var nome = filter.Nome?.Trim().ToLowerInvariant();
        var email = filter.Email?.Trim().ToLowerInvariant();
        Expression<Func<Usuario, bool>> predicate = u =>
            (nome == null || u.Nome.ToLower().Contains(nome))
            && (email == null || u.Email.ToLower().Contains(email))
            && (filter.Role == null || u.Role == filter.Role);

        var resultado = usuarioRepository.GetPaged(predicate, q.SortBy ?? nameof(Usuario.Nome), q.Descending, q.Page, q.PageSize);
        return PagedResponse<UsuarioResponse>.Create(resultado.Map(UsuarioResponse.FromDomain), q);
    }

    public UsuarioResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando usuário por ID: {UsuarioId}", id);
        var u = usuarioRepository.GetById(id);
        if (u is null)
            logger.LogWarning("Usuário com ID {UsuarioId} não foi encontrado.", id);

        return u is null ? null : UsuarioResponse.FromDomain(u);
    }

    public UsuarioResponse? GetByEmail(string email)
    {
        logger.LogInformation("Buscando usuário por e-mail: {Email}", email);
        var u = usuarioRepository.GetByEmail(email);
        if (u is null)
            logger.LogWarning("Usuário com e-mail {Email} não foi encontrado.", email);

        return u is null ? null : UsuarioResponse.FromDomain(u);
    }

    public UsuarioScoreResponse GetScore(Guid usuarioId)
    {
        logger.LogInformation("Calculando score do usuário: {UsuarioId}", usuarioId);
        if (!usuarioRepository.ExistsById(usuarioId))
        {
            logger.LogWarning("Tentativa de calcular score para usuário inexistente: {UsuarioId}", usuarioId);
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        var pontosTotais = tarefaRepository.GetByUsuarioId(usuarioId)
            .Where(t => t.Conclusao.HasValue)
            .Sum(t => t.PontosTarefa);

        logger.LogInformation("Score calculado para usuário {UsuarioId}: {PontosTotais} pontos.", usuarioId, pontosTotais);
        return new UsuarioScoreResponse(usuarioId, pontosTotais);
    }

    public UsuarioResponse Create(UsuarioRequest request)
    {
        logger.LogInformation("Iniciando criação de novo usuário com e-mail: {Email}, Role: {Role}", request.Email, request.Role);
        if (usuarioRepository.ExistsByEmail(request.Email))
        {
            logger.LogWarning("Tentativa de cadastro com e-mail já existente: {Email}", request.Email);
            throw new InvalidOperationException("Já existe um usuário com este e-mail.");
        }

        var endereco = enderecoService.Create(request.Endereco);
        var telefone = telefoneRepository.Add(request.ToTelefone());
        var usuario = request.ToDomain(telefone.Id, endereco.Id);
        usuarioRepository.Add(usuario);
        logger.LogInformation("Usuário {UsuarioId} ({Nome}) cadastrado com sucesso com senha protegida por Salt e BCrypt.", usuario.Id, usuario.Nome);
        return UsuarioResponse.FromDomain(usuario);
    }

    public UsuarioResponse? Update(Guid id, UsuarioUpdateRequest request)
    {
        logger.LogInformation("Iniciando atualização do usuário: {UsuarioId}", id);
        var usuario = usuarioRepository.GetById(id);
        if (usuario is null)
        {
            logger.LogWarning("Tentativa de atualizar usuário inexistente: {UsuarioId}", id);
            return null;
        }

        var usuarioComEsteEmail = usuarioRepository.GetByEmail(request.Email);
        if (usuarioComEsteEmail is not null && usuarioComEsteEmail.Id != id)
        {
            logger.LogWarning("Tentativa de alteração para e-mail já pertencente a outro usuário: {Email}", request.Email);
            throw new InvalidOperationException("Já existe um usuário com este e-mail.");
        }

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

        logger.LogInformation("Usuário {UsuarioId} atualizado com sucesso. Senha re-hasheada e atualizada.", id);
        return UsuarioResponse.FromDomain(usuario);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão de usuário: {UsuarioId}", id);
        var removido = usuarioRepository.Delete(id);
        if (removido)
            logger.LogInformation("Usuário {UsuarioId} removido com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de usuário inexistente: {UsuarioId}", id);

        return removido;
    }
}
