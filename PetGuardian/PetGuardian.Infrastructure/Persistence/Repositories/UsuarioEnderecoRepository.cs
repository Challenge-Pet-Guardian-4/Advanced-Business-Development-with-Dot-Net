using Microsoft.EntityFrameworkCore;
using PetGuardian.Application.Repositories;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Infrastructure.Persistence.Repositories;

public sealed class UsuarioEnderecoRepository(PetGuardianContext context) : IUsuarioEnderecoRepository
{
    public IReadOnlyList<UsuarioEndereco> GetAll() =>
        context.UsuarioEnderecos.AsNoTracking().ToList();

    public IReadOnlyList<UsuarioEndereco> GetByUsuarioId(Guid usuarioId) =>
        context.UsuarioEnderecos.AsNoTracking()
            .Where(ue => ue.UsuarioId == usuarioId).ToList();

    public IReadOnlyList<UsuarioEndereco> GetByEnderecoId(Guid enderecoId) =>
        context.UsuarioEnderecos.AsNoTracking()
            .Where(ue => ue.EnderecoId == enderecoId).ToList();

    public UsuarioEndereco? GetByUsuarioAndEndereco(Guid usuarioId, Guid enderecoId) =>
        context.UsuarioEnderecos
            .FirstOrDefault(ue => ue.UsuarioId == usuarioId && ue.EnderecoId == enderecoId);

    public UsuarioEndereco Add(UsuarioEndereco entity)
    {
        context.UsuarioEnderecos.Add(entity);
        context.SaveChanges();
        return entity;
    }

    public bool Delete(Guid usuarioId, Guid enderecoId)
    {
        var entity = GetByUsuarioAndEndereco(usuarioId, enderecoId);
        if (entity is null) return false;
        context.UsuarioEnderecos.Remove(entity);
        context.SaveChanges();
        return true;
    }

    public bool Exists(Guid usuarioId, Guid enderecoId) =>
        context.UsuarioEnderecos
            .Any(ue => ue.UsuarioId == usuarioId && ue.EnderecoId == enderecoId);
}