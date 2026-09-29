using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Repositories;

public interface IUsuarioEnderecoRepository
{
    IReadOnlyList<UsuarioEndereco> GetAll();
    IReadOnlyList<UsuarioEndereco> GetByUsuarioId(Guid usuarioId);
    IReadOnlyList<UsuarioEndereco> GetByEnderecoId(Guid enderecoId);
    UsuarioEndereco? GetByUsuarioAndEndereco(Guid usuarioId, Guid enderecoId);
    UsuarioEndereco Add(UsuarioEndereco entity);
    bool Delete(Guid usuarioId, Guid enderecoId);
    bool Exists(Guid usuarioId, Guid enderecoId);
}