using Microsoft.Extensions.Logging;
using PetGuardian.Application.Common;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de aplicação responsável pela gestão da rede de cuidado colaborativo e vínculos N:N entre usuários e pets.
/// </summary>
public sealed class UsuarioPetService(
    IUsuarioPetRepository        usuarioPetRepository,
    IUsuarioRepository           usuarioRepository,
    IPetRepository               petRepository,
    ITarefaRepository            tarefaRepository,
    IHistoricoRepository         historicoRepository,
    ILogger<UsuarioPetService>   logger) : IUsuarioPetService
{
    public IReadOnlyList<UsuarioPetResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os vínculos da rede de cuidado.");
        return usuarioPetRepository.GetAll().Select(UsuarioPetResponse.FromDomain).ToList();
    }

    public IReadOnlyList<UsuarioPetResponse> GetByUsuarioId(Guid usuarioId)
    {
        logger.LogInformation("Buscando pets vinculados ao usuário: {UsuarioId}", usuarioId);
        return usuarioPetRepository.GetByUsuarioId(usuarioId).Select(UsuarioPetResponse.FromDomain).ToList();
    }

    public IReadOnlyList<UsuarioPetResponse> GetByPetId(Guid petId)
    {
        logger.LogInformation("Buscando cuidadores vinculados ao pet: {PetId}", petId);
        return usuarioPetRepository.GetByPetId(petId).Select(UsuarioPetResponse.FromDomain).ToList();
    }

    public UsuarioPetResponse Create(UsuarioPetRequest request)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("UsuarioPetService.Create");
        activity?.SetTag("usuarioPet.usuarioId", request.UsuarioId.ToString());
        activity?.SetTag("usuarioPet.petId", request.PetId.ToString());

        logger.LogInformation("Iniciando vínculo entre Usuário {UsuarioId} e Pet {PetId}, Responsável Principal: {ResponPrinc}",
            request.UsuarioId, request.PetId, request.ResponPrinc);

        ValidarUsuarioEPet(request.UsuarioId, request.PetId);
        ValidarRegrasResponsavelPrincipalParaNovoVinculo(request.PetId, request.ResponPrinc);

        if (usuarioPetRepository.Exists(request.UsuarioId, request.PetId))
        {
            logger.LogWarning("Tentativa de criar vínculo duplicado entre Usuário {UsuarioId} e Pet {PetId}", request.UsuarioId, request.PetId);
            throw new InvalidOperationException("Este usuário já está vinculado a este pet.");
        }

        var vinculo = request.ToDomain();
        usuarioPetRepository.Add(vinculo);
        logger.LogInformation("Vínculo criado com sucesso entre Usuário {UsuarioId} e Pet {PetId}.", request.UsuarioId, request.PetId);
        return UsuarioPetResponse.FromDomain(vinculo);
    }

    public UsuarioPetResponse InviteByUsuario(UsuarioPetInviteByUsuarioRequest request)
    {
        logger.LogInformation("Admin {AdminId} enviando convite para Usuário {ConvidadoId} no Pet {PetId}",
            request.AdminUsuarioId, request.UsuarioConvidadoId, request.PetId);

        ValidarAdministradorPrincipal(request.AdminUsuarioId, request.PetId);

        if (!usuarioRepository.ExistsById(request.UsuarioConvidadoId))
        {
            logger.LogWarning("Tentativa de convidar usuário inexistente: {UsuarioId}", request.UsuarioConvidadoId);
            throw new InvalidOperationException("Usuário convidado não encontrado.");
        }

        if (usuarioPetRepository.Exists(request.UsuarioConvidadoId, request.PetId))
        {
            logger.LogWarning("Usuário convidado {UsuarioId} já vinculado ao pet {PetId}", request.UsuarioConvidadoId, request.PetId);
            throw new InvalidOperationException("Usuário convidado já está vinculado a este pet.");
        }

        var vinculo = new UsuarioPet(
            request.UsuarioConvidadoId,
            request.PetId,
            responPrinc: false);

        usuarioPetRepository.Add(vinculo);
        logger.LogInformation("Co-cuidador {UsuarioId} adicionado com sucesso ao Pet {PetId}.", request.UsuarioConvidadoId, request.PetId);
        return UsuarioPetResponse.FromDomain(vinculo);
    }

    public UsuarioPetResponse InviteByEmail(UsuarioPetInviteByEmailRequest request)
    {
        logger.LogInformation("Admin {AdminId} enviando convite por e-mail {Email} no Pet {PetId}",
            request.AdminUsuarioId, request.Email, request.PetId);

        ValidarAdministradorPrincipal(request.AdminUsuarioId, request.PetId);

        var usuarioConvidado = usuarioRepository.GetByEmail(request.Email);
        if (usuarioConvidado is null)
        {
            logger.LogWarning("Nenhum usuário encontrado com o e-mail: {Email}", request.Email);
            throw new InvalidOperationException("Usuário convidado não encontrado para o e-mail informado.");
        }

        if (usuarioPetRepository.Exists(usuarioConvidado.Id, request.PetId))
        {
            logger.LogWarning("Usuário {Email} ({UsuarioId}) já vinculado ao pet {PetId}", request.Email, usuarioConvidado.Id, request.PetId);
            throw new InvalidOperationException("Usuário convidado já está vinculado a este pet.");
        }

        var vinculo = new UsuarioPet(
            usuarioConvidado.Id,
            request.PetId,
            responPrinc: false);

        usuarioPetRepository.Add(vinculo);
        logger.LogInformation("Co-cuidador {Email} ({UsuarioId}) vinculado com sucesso ao Pet {PetId}.", request.Email, usuarioConvidado.Id, request.PetId);
        return UsuarioPetResponse.FromDomain(vinculo);
    }

    public UsuarioPetResponse? Update(Guid usuarioId, Guid petId, UsuarioPetUpdateRequest request)
    {
        logger.LogInformation("Atualizando responsabilidade do vínculo Usuário {UsuarioId} e Pet {PetId} para Principal: {ResponPrinc}",
            usuarioId, petId, request.ResponPrinc);

        var vinculo = usuarioPetRepository.GetByUsuarioAndPet(usuarioId, petId);
        if (vinculo is null)
        {
            logger.LogWarning("Vínculo entre Usuário {UsuarioId} e Pet {PetId} não encontrado para atualização.", usuarioId, petId);
            return null;
        }

        if (!request.ResponPrinc && vinculo.ResponPrinc)
        {
            var totalResponsaveis = usuarioPetRepository.GetByPetId(petId).Count(v => v.ResponPrinc);
            if (totalResponsaveis <= 1)
            {
                logger.LogWarning("Tentativa de despromover o único responsável principal do pet {PetId}", petId);
                throw new InvalidOperationException(
                    "Não é permitido remover o único responsável principal do pet. Promova outro cuidador antes.");
            }
        }

        if (request.ResponPrinc && !vinculo.ResponPrinc)
        {
            var jaTemPrincipal = usuarioPetRepository.GetByPetId(petId).Any(v => v.ResponPrinc);
            if (jaTemPrincipal)
            {
                logger.LogWarning("Pet {PetId} já possui um responsável principal.", petId);
                throw new InvalidOperationException("Este pet já possui um responsável principal.");
            }
        }

        vinculo.AtualizarResponsabilidade(request.ResponPrinc);
        usuarioPetRepository.Update(vinculo);
        logger.LogInformation("Vínculo entre Usuário {UsuarioId} e Pet {PetId} atualizado com sucesso.", usuarioId, petId);
        return UsuarioPetResponse.FromDomain(vinculo);
    }

    public RedeCuidadoResponse GetRedeCuidadoByUsuarioId(Guid usuarioId)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("UsuarioPetService.GetRedeCuidado");
        activity?.SetTag("redeCuidado.usuarioId", usuarioId.ToString());

        logger.LogInformation("Montando árvore completa da rede de cuidado para o usuário: {UsuarioId}", usuarioId);
        if (!usuarioRepository.ExistsById(usuarioId))
        {
            logger.LogWarning("Tentativa de obter rede de cuidado para usuário inexistente: {UsuarioId}", usuarioId);
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        var vinculosDoUsuario = usuarioPetRepository.GetByUsuarioId(usuarioId);
        var petIds = vinculosDoUsuario.Select(v => v.PetId).Distinct().ToList();

        var pets = petRepository.Find(p => petIds.Contains(p.Id));

        var tarefasLookup = tarefaRepository.Find(t => petIds.Contains(t.PetId))
            .ToLookup(t => t.PetId);

        var historicosLookup = historicoRepository.Find(h => petIds.Contains(h.PetId))
            .ToLookup(h => h.PetId);

        var vinculosPetsLookup = usuarioPetRepository.GetByPetIds(petIds)
            .ToLookup(v => v.PetId);

        var petsDaRede = new List<RedeCuidadoPetResponse>();
        var coCuidadorIds = new HashSet<Guid>();

        foreach (var pet in pets)
        {
            var tarefas = tarefasLookup[pet.Id]
                .Select(t => new RedeCuidadoTarefaResponse(
                    t.Id,
                    t.Titulo,
                    t.Prazo,
                    t.Conclusao,
                    t.UsuarioId,
                    t.StatusId,
                    t.PontosTarefa))
                .ToList();

            var historico = historicosLookup[pet.Id]
                .Select(h => new RedeCuidadoHistoricoResponse(h.Id, h.TipoHist, h.DataHist))
                .ToList();

            petsDaRede.Add(new RedeCuidadoPetResponse(
                pet.Id,
                pet.Nome,
                tarefas,
                historico));

            foreach (var vinculo in vinculosPetsLookup[pet.Id])
            {
                if (vinculo.UsuarioId != usuarioId)
                    coCuidadorIds.Add(vinculo.UsuarioId);
            }
        }

        var coCuidadores = usuarioRepository
            .Find(u => coCuidadorIds.Contains(u.Id))
            .Select(u => new RedeCuidadoCoCuidadorResponse(u.Id, u.Nome, u.Email))
            .OrderBy(u => u.Nome)
            .ToList();

        logger.LogInformation("Rede de cuidado montada com sucesso para Usuário {UsuarioId}: {TotalPets} pets e {TotalCoCuidadores} co-cuidadores.",
            usuarioId, petsDaRede.Count, coCuidadores.Count);

        return new RedeCuidadoResponse(usuarioId, petsDaRede, coCuidadores);
    }

    public bool Delete(Guid usuarioId, Guid petId)
    {
        logger.LogInformation("Iniciando desvinculação entre Usuário {UsuarioId} e Pet {PetId}", usuarioId, petId);
        var vinculo = usuarioPetRepository.GetByUsuarioAndPet(usuarioId, petId);
        if (vinculo is null)
        {
            logger.LogWarning("Vínculo entre Usuário {UsuarioId} e Pet {PetId} não encontrado para exclusão.", usuarioId, petId);
            return false;
        }

        if (vinculo.ResponPrinc)
        {
            var totalResponsaveis = usuarioPetRepository.GetByPetId(petId)
                .Count(v => v.ResponPrinc);

            if (totalResponsaveis <= 1)
            {
                logger.LogWarning("Tentativa de desvincular único responsável principal do Pet {PetId}", petId);
                throw new InvalidOperationException(
                    "Não é permitido desvincular o responsável principal quando ele é o único administrador do pet.");
            }
        }

        var removido = usuarioPetRepository.Delete(usuarioId, petId);
        if (removido)
            logger.LogInformation("Vínculo entre Usuário {UsuarioId} e Pet {PetId} desfeito com sucesso.", usuarioId, petId);

        return removido;
    }

    private void ValidarUsuarioEPet(Guid usuarioId, Guid petId)
    {
        if (!usuarioRepository.ExistsById(usuarioId))
        {
            logger.LogWarning("Validação falhou: Usuário {UsuarioId} não encontrado.", usuarioId);
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        if (!petRepository.ExistsById(petId))
        {
            logger.LogWarning("Validação falhou: Pet {PetId} não encontrado.", petId);
            throw new InvalidOperationException("Pet não encontrado.");
        }
    }

    private void ValidarAdministradorPrincipal(Guid adminUsuarioId, Guid petId)
    {
        ValidarUsuarioEPet(adminUsuarioId, petId);

        var vinculoAdmin = usuarioPetRepository.GetByUsuarioAndPet(adminUsuarioId, petId);
        if (vinculoAdmin is null)
        {
            logger.LogWarning("Validação falhou: Administrador {AdminId} não vinculado ao pet {PetId}.", adminUsuarioId, petId);
            throw new InvalidOperationException("Administrador não está vinculado ao pet.");
        }

        if (!vinculoAdmin.ResponPrinc)
        {
            logger.LogWarning("Validação falhou: Usuário {AdminId} não é responsável principal do pet {PetId}.", adminUsuarioId, petId);
            throw new InvalidOperationException(
                "Apenas o responsável principal do pet pode convidar novos co-cuidadores.");
        }
    }

    private void ValidarRegrasResponsavelPrincipalParaNovoVinculo(Guid petId, bool novoVinculoEhPrincipal)
    {
        var vinculosDoPet = usuarioPetRepository.GetByPetId(petId);
        var possuiPrincipal = vinculosDoPet.Any(v => v.ResponPrinc);

        if (!possuiPrincipal && !novoVinculoEhPrincipal)
        {
            logger.LogWarning("Validação falhou: Pet {PetId} não tem principal e novo vínculo não é principal.", petId);
            throw new InvalidOperationException("Todo pet precisa ter um responsável principal.");
        }

        if (novoVinculoEhPrincipal && possuiPrincipal)
        {
            logger.LogWarning("Validação falhou: Pet {PetId} já possui responsável principal.", petId);
            throw new InvalidOperationException("Este pet já possui responsável principal.");
        }
    }
}