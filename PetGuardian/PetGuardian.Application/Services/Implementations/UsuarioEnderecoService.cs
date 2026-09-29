using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de vínculos N:N entre usuários e endereços.
/// </summary>
public sealed class UsuarioEnderecoService(
    IUsuarioEnderecoRepository        usuarioEnderecoRepository,
    IUsuarioRepository                usuarioRepository,
    IRepository<Endereco>             enderecoRepository,
    ILogger<UsuarioEnderecoService>   logger) : IUsuarioEnderecoService
{
    public IReadOnlyList<UsuarioEnderecoResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os vínculos de endereços de usuários.");
        return usuarioEnderecoRepository.GetAll().Select(UsuarioEnderecoResponse.FromDomain).ToList();
    }

    public IReadOnlyList<UsuarioEnderecoResponse> GetByUsuarioId(Guid usuarioId)
    {
        logger.LogInformation("Buscando endereços do usuário: {UsuarioId}", usuarioId);
        return usuarioEnderecoRepository.GetByUsuarioId(usuarioId)
            .Select(UsuarioEnderecoResponse.FromDomain).ToList();
    }

    public IReadOnlyList<UsuarioEnderecoResponse> GetByEnderecoId(Guid enderecoId)
    {
        logger.LogInformation("Buscando usuários residentes no endereço: {EnderecoId}", enderecoId);
        return usuarioEnderecoRepository.GetByEnderecoId(enderecoId)
            .Select(UsuarioEnderecoResponse.FromDomain).ToList();
    }

    public UsuarioEnderecoResponse Create(UsuarioEnderecoRequest request)
    {
        logger.LogInformation("Iniciando vínculo entre Usuário {UsuarioId} e Endereço {EnderecoId}", request.UsuarioId, request.EnderecoId);
        if (!usuarioRepository.ExistsById(request.UsuarioId))
        {
            logger.LogWarning("Tentativa de vincular endereço a usuário inexistente: {UsuarioId}", request.UsuarioId);
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        if (!enderecoRepository.ExistsById(request.EnderecoId))
        {
            logger.LogWarning("Tentativa de vincular endereço inexistente: {EnderecoId}", request.EnderecoId);
            throw new InvalidOperationException("Endereço não encontrado.");
        }

        if (usuarioEnderecoRepository.Exists(request.UsuarioId, request.EnderecoId))
        {
            logger.LogWarning("Vínculo já existente entre Usuário {UsuarioId} e Endereço {EnderecoId}", request.UsuarioId, request.EnderecoId);
            throw new InvalidOperationException("Este endereço já está vinculado a este usuário.");
        }

        var vinculo = request.ToDomain();
        usuarioEnderecoRepository.Add(vinculo);
        logger.LogInformation("Vínculo criado com sucesso entre Usuário {UsuarioId} e Endereço {EnderecoId}.", request.UsuarioId, request.EnderecoId);
        return UsuarioEnderecoResponse.FromDomain(vinculo);
    }

    public bool Delete(Guid usuarioId, Guid enderecoId)
    {
        logger.LogInformation("Iniciando desvinculação entre Usuário {UsuarioId} e Endereço {EnderecoId}", usuarioId, enderecoId);
        var removido = usuarioEnderecoRepository.Delete(usuarioId, enderecoId);
        if (removido)
            logger.LogInformation("Vínculo entre Usuário {UsuarioId} e Endereço {EnderecoId} desfeito com sucesso.", usuarioId, enderecoId);
        else
            logger.LogWarning("Tentativa de desvincular relação inexistente entre Usuário {UsuarioId} e Endereço {EnderecoId}", usuarioId, enderecoId);

        return removido;
    }
}