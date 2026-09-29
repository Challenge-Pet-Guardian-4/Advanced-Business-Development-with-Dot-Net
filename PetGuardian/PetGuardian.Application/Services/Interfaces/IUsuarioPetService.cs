using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IUsuarioPetService
{
    IReadOnlyList<UsuarioPetResponse> GetAll();
    IReadOnlyList<UsuarioPetResponse> GetByUsuarioId(Guid usuarioId);
    IReadOnlyList<UsuarioPetResponse> GetByPetId(Guid petId);
    UsuarioPetResponse Create(UsuarioPetRequest request);
    UsuarioPetResponse InviteByUsuario(UsuarioPetInviteByUsuarioRequest request);
    UsuarioPetResponse InviteByEmail(UsuarioPetInviteByEmailRequest request);
    /// <summary>Alterna o responsável principal do vínculo.</summary>
    UsuarioPetResponse? Update(Guid usuarioId, Guid petId, UsuarioPetUpdateRequest request);
    RedeCuidadoResponse GetRedeCuidadoByUsuarioId(Guid usuarioId);
    bool Delete(Guid usuarioId, Guid petId);
}