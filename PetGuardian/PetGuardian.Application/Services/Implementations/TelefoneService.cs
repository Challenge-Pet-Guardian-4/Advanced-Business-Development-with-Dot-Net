using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de telefones de contato de usuários.
/// </summary>
public sealed class TelefoneService(
    IRepository<Telefone>    telefoneRepository,
    ILogger<TelefoneService> logger) : ITelefoneService
{
    public IReadOnlyList<TelefoneResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os telefones cadastrados.");
        return telefoneRepository.GetAll().Select(TelefoneResponse.FromDomain).ToList();
    }

    public TelefoneResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando telefone por ID: {TelefoneId}", id);
        var telefone = telefoneRepository.GetById(id);
        if (telefone is null)
            logger.LogWarning("Telefone com ID {TelefoneId} não encontrado.", id);

        return telefone is null ? null : TelefoneResponse.FromDomain(telefone);
    }

    public TelefoneResponse Create(TelefoneRequest request)
    {
        logger.LogInformation("Iniciando cadastro de telefone: ({Ddd}) {Numero}", request.NumDdd, request.NumTel);
        var telefone = request.ToDomain();
        telefoneRepository.Add(telefone);
        logger.LogInformation("Telefone {TelefoneId} cadastrado com sucesso.", telefone.Id);
        return TelefoneResponse.FromDomain(telefone);
    }
    
    public TelefoneResponse? Update(Guid id, TelefoneRequest request)
    {
        logger.LogInformation("Iniciando atualização do telefone: {TelefoneId}", id);
        var telefone = telefoneRepository.GetById(id);
        if (telefone is null)
        {
            logger.LogWarning("Tentativa de atualizar telefone inexistente: {TelefoneId}", id);
            return null;
        }

        telefone.Atualizar(request.NumDdd, request.NumTel);
        telefoneRepository.Update(telefone);
        logger.LogInformation("Telefone {TelefoneId} atualizado com sucesso.", id);
        return TelefoneResponse.FromDomain(telefone);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão do telefone: {TelefoneId}", id);
        var removido = telefoneRepository.Delete(id);
        if (removido)
            logger.LogInformation("Telefone {TelefoneId} removido com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de telefone inexistente: {TelefoneId}", id);

        return removido;
    }
}