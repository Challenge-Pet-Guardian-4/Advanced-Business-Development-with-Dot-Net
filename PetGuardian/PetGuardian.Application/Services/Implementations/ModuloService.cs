using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pelo gerenciamento de módulos de aprendizagem vinculados a trilhas.
/// </summary>
public sealed class ModuloService(
    IModuloRepository        moduloRepository,
    ITrilhaRepository        trilhaRepository,
    ILogger<ModuloService>   logger) : IModuloService
{
    public IReadOnlyList<ModuloResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os módulos cadastrados.");
        return moduloRepository.GetAll().Select(ModuloResponse.FromDomain).ToList();
    }

    public ModuloResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando módulo por ID: {ModuloId}", id);
        var modulo = moduloRepository.GetById(id);
        if (modulo is null)
            logger.LogWarning("Módulo com ID {ModuloId} não encontrado.", id);

        return modulo is null ? null : ModuloResponse.FromDomain(modulo);
    }

    public IReadOnlyList<ModuloResponse> GetByTrilhaId(Guid trilhaId)
    {
        logger.LogInformation("Buscando módulos da trilha: {TrilhaId}", trilhaId);
        return moduloRepository.GetByTrilhaId(trilhaId).Select(ModuloResponse.FromDomain).ToList();
    }

    public ModuloResponse Create(ModuloRequest request)
    {
        logger.LogInformation("Iniciando cadastro de módulo '{Nome}' para a trilha {TrilhaId}", request.Nome, request.TrilhaId);
        if (!trilhaRepository.ExistsById(request.TrilhaId))
        {
            logger.LogWarning("Tentativa de criar módulo para trilha inexistente: {TrilhaId}", request.TrilhaId);
            throw new InvalidOperationException("Trilha não encontrada.");
        }

        var modulo = request.ToDomain();
        moduloRepository.Add(modulo);
        logger.LogInformation("Módulo {ModuloId} ('{Nome}') cadastrado com sucesso.", modulo.Id, modulo.Nome);
        return ModuloResponse.FromDomain(modulo);
    }

    public ModuloResponse? Update(Guid id, ModuloUpdateRequest request)
    {
        logger.LogInformation("Iniciando atualização do módulo: {ModuloId}", id);
        var modulo = moduloRepository.GetById(id);
        if (modulo is null)
        {
            logger.LogWarning("Tentativa de atualizar módulo inexistente: {ModuloId}", id);
            return null;
        }

        modulo.Atualizar(request.Nome, request.TempoConclusao, request.Descricao);
        moduloRepository.Update(modulo);
        logger.LogInformation("Módulo {ModuloId} atualizado com sucesso.", id);
        return ModuloResponse.FromDomain(modulo);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão do módulo: {ModuloId}", id);
        var removido = moduloRepository.Delete(id);
        if (removido)
            logger.LogInformation("Módulo {ModuloId} removido com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de módulo inexistente: {ModuloId}", id);

        return removido;
    }
}