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
    IRepository<Bairro>     bairroRepository,
    IRepository<Cidade>     cidadeRepository,
    ILogger<BairroService>  logger) : IBairroService
{
    public IReadOnlyList<BairroResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os bairros cadastrados.");
        return bairroRepository.GetAll().Select(BairroResponse.FromDomain).ToList();
    }

    public BairroResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando bairro por ID: {BairroId}", id);
        var bairro = bairroRepository.GetById(id);
        if (bairro is null)
            logger.LogWarning("Bairro com ID {BairroId} não encontrado.", id);

        return bairro is null ? null : BairroResponse.FromDomain(bairro);
    }

    public IReadOnlyList<BairroResponse> GetByCidadeId(Guid cidadeId)
    {
        logger.LogInformation("Buscando bairros da cidade: {CidadeId}", cidadeId);
        return bairroRepository.Find(b => b.CidadeId == cidadeId)
            .Select(BairroResponse.FromDomain)
            .ToList();
    }

    public BairroResponse Create(BairroRequest request)
    {
        logger.LogInformation("Iniciando cadastro de bairro '{NomeBairro}' na cidade {CidadeId}", request.NomeBairro, request.CidadeId);
        if (!cidadeRepository.ExistsById(request.CidadeId))
        {
            logger.LogWarning("Tentativa de criar bairro para cidade inexistente: {CidadeId}", request.CidadeId);
            throw new InvalidOperationException("Cidade não encontrada.");
        }

        var bairro = request.ToDomain();
        bairroRepository.Add(bairro);
        logger.LogInformation("Bairro {BairroId} ('{NomeBairro}') cadastrado com sucesso.", bairro.Id, bairro.NomeBairro);
        return BairroResponse.FromDomain(bairro);
    }
    
    public BairroResponse? Update(Guid id, BairroRequest request)
    {
        logger.LogInformation("Iniciando atualização do bairro: {BairroId}", id);
        var bairro = bairroRepository.GetById(id);
        if (bairro is null)
        {
            logger.LogWarning("Tentativa de atualizar bairro inexistente: {BairroId}", id);
            return null;
        }

        if (!cidadeRepository.ExistsById(request.CidadeId))
        {
            logger.LogWarning("Tentativa de atualizar bairro com cidade inexistente: {CidadeId}", request.CidadeId);
            throw new InvalidOperationException("Cidade não encontrada.");
        }

        bairro.Atualizar(request.NomeBairro, request.CidadeId);
        bairroRepository.Update(bairro);
        logger.LogInformation("Bairro {BairroId} atualizado com sucesso.", id);
        return BairroResponse.FromDomain(bairro);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão do bairro: {BairroId}", id);
        var removido = bairroRepository.Delete(id);
        if (removido)
            logger.LogInformation("Bairro {BairroId} removido com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de bairro inexistente: {BairroId}", id);

        return removido;
    }
}