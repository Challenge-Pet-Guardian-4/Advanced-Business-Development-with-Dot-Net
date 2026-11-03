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
    public IReadOnlyList<TelefoneResponse> GetAll() =>
        telefoneRepository.GetAll().Select(TelefoneResponse.FromDomain).ToList();

    public TelefoneResponse? GetById(Guid id) =>
        telefoneRepository.GetById(id) is { } telefone ? TelefoneResponse.FromDomain(telefone) : null;

    public TelefoneResponse Create(TelefoneRequest request)
    {
        var telefone = telefoneRepository.Add(request.ToDomain());
        logger.LogInformation("Telefone {TelefoneId} cadastrado.", telefone.Id);
        return TelefoneResponse.FromDomain(telefone);
    }

    public TelefoneResponse? Update(Guid id, TelefoneRequest request)
    {
        var telefone = telefoneRepository.GetById(id);
        if (telefone is null)
            return null;

        telefone.Atualizar(request.NumDdd, request.NumTel);
        telefoneRepository.Update(telefone);
        return TelefoneResponse.FromDomain(telefone);
    }

    public bool Delete(Guid id) => telefoneRepository.Delete(id);
}
