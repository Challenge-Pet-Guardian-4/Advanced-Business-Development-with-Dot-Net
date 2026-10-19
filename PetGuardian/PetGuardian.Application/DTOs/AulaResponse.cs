using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record AulaResponse(
    Guid Id, string Nome, string Descricao, int PontosAula, string Dificuldade,
    bool Concluida, Guid ModuloId, string? Conteudo = null)
{
    public static AulaResponse FromDomain(Aula a, string? conteudo = null) =>
        new(a.Id, a.Nome, a.Descricao, a.PontosAula, a.Dificuldade, a.Concluida, a.ModuloId, conteudo);
}