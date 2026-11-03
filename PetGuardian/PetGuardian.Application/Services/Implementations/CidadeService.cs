using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de cidades vinculadas a estados.
/// </summary>
public sealed class CidadeService(
    IRepository<Cidade>    cidadeRepository,
    IRepository<Estado>    estadoRepository,
    ILogger<CidadeService> logger) : ICidadeService
{
    public IReadOnlyList<CidadeResponse> GetAll() =>
        cidadeRepository.GetAll().Select(CidadeResponse.FromDomain).ToList();

    public CidadeResponse? GetById(Guid id) =>
        cidadeRepository.GetById(id) is { } cidade ? CidadeResponse.FromDomain(cidade) : null;

    public IReadOnlyList<CidadeResponse> GetByEstadoId(Guid estadoId) =>
        cidadeRepository.Find(c => c.EstadoId == estadoId).Select(CidadeResponse.FromDomain).ToList();

    public CidadeResponse Create(CidadeRequest request)
    {
        if (!estadoRepository.ExistsById(request.EstadoId))
            throw new InvalidOperationException("Estado não encontrado.");

        var cidade = cidadeRepository.Add(request.ToDomain());
        logger.LogInformation("Cidade {CidadeId} ('{NomeCidade}') cadastrada.", cidade.Id, cidade.NomeCidade);
        return CidadeResponse.FromDomain(cidade);
    }

    public CidadeResponse? Update(Guid id, CidadeRequest request)
    {
        var cidade = cidadeRepository.GetById(id);
        if (cidade is null)
            return null;

        if (!estadoRepository.ExistsById(request.EstadoId))
            throw new InvalidOperationException("Estado não encontrado.");

        cidade.Atualizar(request.NomeCidade, request.EstadoId);
        cidadeRepository.Update(cidade);
        return CidadeResponse.FromDomain(cidade);
    }

    public bool Delete(Guid id) => cidadeRepository.Delete(id);
}
