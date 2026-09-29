using PetGuardian.Application.DTOs;

namespace PetGuardian.Application.Services.Interfaces;

public interface IUsuarioEnderecoService
{
    IReadOnlyList<UsuarioEnderecoResponse> GetAll();
    IReadOnlyList<UsuarioEnderecoResponse> GetByUsuarioId(Guid usuarioId);
    IReadOnlyList<UsuarioEnderecoResponse> GetByEnderecoId(Guid enderecoId);
    UsuarioEnderecoResponse Create(UsuarioEnderecoRequest request);
    bool Delete(Guid usuarioId, Guid enderecoId);
}