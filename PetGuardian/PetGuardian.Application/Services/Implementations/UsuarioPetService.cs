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
    IUsuarioPetRepository      usuarioPetRepository,
    IUsuarioRepository         usuarioRepository,
    IPetRepository             petRepository,
    ITarefaRepository          tarefaRepository,
    IHistoricoRepository       historicoRepository,
    ILogger<UsuarioPetService> logger,
    IUnitOfWork?               unitOfWork = null) : IUsuarioPetService
{
    public IReadOnlyList<UsuarioPetResponse> GetAll() =>
        usuarioPetRepository.GetAll().Select(UsuarioPetResponse.FromDomain).ToList();

    public IReadOnlyList<UsuarioPetResponse> GetByUsuarioId(Guid usuarioId) =>
        usuarioPetRepository.GetByUsuarioId(usuarioId).Select(UsuarioPetResponse.FromDomain).ToList();

    public IReadOnlyList<UsuarioPetResponse> GetByPetId(Guid petId) =>
        usuarioPetRepository.GetByPetId(petId).Select(UsuarioPetResponse.FromDomain).ToList();

    public UsuarioPetResponse Create(UsuarioPetRequest request)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("UsuarioPetService.Create");
        activity?.SetTag("usuarioPet.usuarioId", request.UsuarioId.ToString());
        activity?.SetTag("usuarioPet.petId", request.PetId.ToString());

        ValidarUsuarioEPet(request.UsuarioId, request.PetId);
        ValidarRegrasResponsavelPrincipalParaNovoVinculo(request.PetId, request.ResponPrinc);

        if (usuarioPetRepository.Exists(request.UsuarioId, request.PetId))
            throw new InvalidOperationException("Este usuário já está vinculado a este pet.");

        var vinculo = usuarioPetRepository.Add(request.ToDomain());
        logger.LogInformation("Vínculo criado entre Usuário {UsuarioId} e Pet {PetId} (Principal: {ResponPrinc}).",
            request.UsuarioId, request.PetId, request.ResponPrinc);
        return UsuarioPetResponse.FromDomain(vinculo);
    }

    public UsuarioPetResponse InviteByUsuario(UsuarioPetInviteByUsuarioRequest request)
    {
        if (!usuarioRepository.ExistsById(request.UsuarioConvidadoId))
            throw new InvalidOperationException("Usuário convidado não encontrado.");

        return ConvidarCoCuidador(request.AdminUsuarioId, request.UsuarioConvidadoId, request.PetId);
    }

    public UsuarioPetResponse InviteByEmail(UsuarioPetInviteByEmailRequest request)
    {
        var convidado = usuarioRepository.GetByEmail(request.Email)
            ?? throw new InvalidOperationException("Usuário convidado não encontrado para o e-mail informado.");

        return ConvidarCoCuidador(request.AdminUsuarioId, convidado.Id, request.PetId);
    }

    public UsuarioPetResponse? Update(Guid usuarioId, Guid petId, UsuarioPetUpdateRequest request)
    {
        var vinculo = usuarioPetRepository.GetByUsuarioAndPet(usuarioId, petId);
        if (vinculo is null)
            return null;

        if (!request.ResponPrinc && vinculo.ResponPrinc)
        {
            GarantirQueNaoEhUnicoPrincipal(petId,
                "Não é permitido remover o único responsável principal do pet. Promova outro cuidador antes.");
        }

        return unitOfWork.RunInTransaction(() =>
        {
            if (request.ResponPrinc && !vinculo.ResponPrinc)
            {
                var principalAtual = usuarioPetRepository.GetByPetId(petId).FirstOrDefault(v => v.ResponPrinc);
                if (principalAtual is not null && principalAtual.UsuarioId != usuarioId)
                {
                    principalAtual.AtualizarResponsabilidade(false);
                    usuarioPetRepository.Update(principalAtual);
                }
            }

            vinculo.AtualizarResponsabilidade(request.ResponPrinc);
            usuarioPetRepository.Update(vinculo);
            logger.LogInformation("Vínculo entre Usuário {UsuarioId} e Pet {PetId} atualizado (Principal: {ResponPrinc}).",
                usuarioId, petId, request.ResponPrinc);
            return UsuarioPetResponse.FromDomain(vinculo);
        });
    }

    public RedeCuidadoResponse GetRedeCuidadoByUsuarioId(Guid usuarioId)
    {
        using var activity = PetGuardianActivitySource.Source.StartActivity("UsuarioPetService.GetRedeCuidado");
        activity?.SetTag("redeCuidado.usuarioId", usuarioId.ToString());

        if (!usuarioRepository.ExistsById(usuarioId))
            throw new InvalidOperationException("Usuário não encontrado.");

        var petIds = usuarioPetRepository.GetByUsuarioId(usuarioId).Select(v => v.PetId).Distinct().ToList();
        var pets = petRepository.Find(p => petIds.Contains(p.Id));
        var tarefasLookup = tarefaRepository.Find(t => petIds.Contains(t.PetId)).ToLookup(t => t.PetId);
        var historicosLookup = historicoRepository.Find(h => petIds.Contains(h.PetId)).ToLookup(h => h.PetId);
        var vinculosPetsLookup = usuarioPetRepository.GetByPetIds(petIds).ToLookup(v => v.PetId);

        var petsDaRede = new List<RedeCuidadoPetResponse>(pets.Count);
        var coCuidadorIds = new HashSet<Guid>();

        foreach (var pet in pets)
        {
            var tarefas = tarefasLookup[pet.Id]
                .Select(t => new RedeCuidadoTarefaResponse(
                    t.Id, t.Titulo, t.Prazo, t.Conclusao, t.UsuarioId, t.StatusId, t.PontosTarefa))
                .ToList();

            var historico = historicosLookup[pet.Id]
                .Select(h => new RedeCuidadoHistoricoResponse(h.Id, h.TipoHist, h.DataHist))
                .ToList();

            petsDaRede.Add(new RedeCuidadoPetResponse(pet.Id, pet.Nome, tarefas, historico));

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

        return new RedeCuidadoResponse(usuarioId, petsDaRede, coCuidadores);
    }

    public bool Delete(Guid usuarioId, Guid petId)
    {
        var vinculo = usuarioPetRepository.GetByUsuarioAndPet(usuarioId, petId);
        if (vinculo is null)
            return false;

        if (vinculo.ResponPrinc)
        {
            GarantirQueNaoEhUnicoPrincipal(petId,
                "Não é permitido desvincular o responsável principal quando ele é o único administrador do pet.");
        }

        var removido = usuarioPetRepository.Delete(usuarioId, petId);
        if (removido)
            logger.LogInformation("Vínculo entre Usuário {UsuarioId} e Pet {PetId} removido.", usuarioId, petId);

        return removido;
    }

    private UsuarioPetResponse ConvidarCoCuidador(Guid adminUsuarioId, Guid convidadoId, Guid petId)
    {
        ValidarAdministradorPrincipal(adminUsuarioId, petId);

        if (usuarioPetRepository.Exists(convidadoId, petId))
            throw new InvalidOperationException("Usuário convidado já está vinculado a este pet.");

        var vinculo = usuarioPetRepository.Add(new UsuarioPet(convidadoId, petId, responPrinc: false));
        logger.LogInformation("Co-cuidador {ConvidadoId} adicionado ao Pet {PetId} pelo Admin {AdminId}.",
            convidadoId, petId, adminUsuarioId);
        return UsuarioPetResponse.FromDomain(vinculo);
    }

    private void GarantirQueNaoEhUnicoPrincipal(Guid petId, string mensagemErro)
    {
        if (usuarioPetRepository.GetByPetId(petId).Count(v => v.ResponPrinc) <= 1)
            throw new InvalidOperationException(mensagemErro);
    }

    private void ValidarUsuarioEPet(Guid usuarioId, Guid petId)
    {
        if (!usuarioRepository.ExistsById(usuarioId))
            throw new InvalidOperationException("Usuário não encontrado.");

        if (!petRepository.ExistsById(petId))
            throw new InvalidOperationException("Pet não encontrado.");
    }

    private void ValidarAdministradorPrincipal(Guid adminUsuarioId, Guid petId)
    {
        ValidarUsuarioEPet(adminUsuarioId, petId);

        var vinculoAdmin = usuarioPetRepository.GetByUsuarioAndPet(adminUsuarioId, petId)
            ?? throw new InvalidOperationException("Administrador não está vinculado ao pet.");

        if (!vinculoAdmin.ResponPrinc)
            throw new InvalidOperationException("Apenas o responsável principal do pet pode convidar novos co-cuidadores.");
    }

    private void ValidarRegrasResponsavelPrincipalParaNovoVinculo(Guid petId, bool novoVinculoEhPrincipal)
    {
        var possuiPrincipal = usuarioPetRepository.GetByPetId(petId).Any(v => v.ResponPrinc);

        if (!possuiPrincipal && !novoVinculoEhPrincipal)
            throw new InvalidOperationException("Todo pet precisa ter um responsável principal.");

        if (novoVinculoEhPrincipal && possuiPrincipal)
            throw new InvalidOperationException("Este pet já possui responsável principal.");
    }
}
