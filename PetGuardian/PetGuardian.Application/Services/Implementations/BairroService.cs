using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de bairros vinculados a cidades.
/// </summary>
public sealed class BairroService(
    IRepository<Bairro>    bairroRepository,
    IRepository<Cidade>    cidadeRepository,
    ILogger<BairroService> logger) : IBairroService
{
    public IReadOnlyList<BairroResponse> GetAll() =>
        bairroRepository.GetAll().Select(BairroResponse.FromDomain).ToList();

    public BairroResponse? GetById(Guid id) =>
        bairroRepository.GetById(id) is { } bairro ? BairroResponse.FromDomain(bairro) : null;

    public IReadOnlyList<BairroResponse> GetByCidadeId(Guid cidadeId) =>
        bairroRepository.Find(b => b.CidadeId == cidadeId).Select(BairroResponse.FromDomain).ToList();

    public BairroResponse Create(BairroRequest request)
    {
        if (!cidadeRepository.ExistsById(request.CidadeId))
            throw new InvalidOperationException("Cidade não encontrada.");

        var bairro = bairroRepository.Add(request.ToDomain());
        logger.LogInformation("Bairro {BairroId} ('{NomeBairro}') cadastrado.", bairro.Id, bairro.NomeBairro);
        return BairroResponse.FromDomain(bairro);
    }

    public BairroResponse? Update(Guid id, BairroRequest request)
    {
        var bairro = bairroRepository.GetById(id);
        if (bairro is null)
            return null;

        if (!cidadeRepository.ExistsById(request.CidadeId))
            throw new InvalidOperationException("Cidade não encontrada.");

        bairro.Atualizar(request.NomeBairro, request.CidadeId);
        bairroRepository.Update(bairro);
        return BairroResponse.FromDomain(bairro);
    }

    public bool Delete(Guid id) => bairroRepository.Delete(id);
}
