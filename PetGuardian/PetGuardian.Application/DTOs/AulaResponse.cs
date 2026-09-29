using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

public record AulaResponse(
    Guid Id, string Nome, string Descricao, int PontosAula, string Dificuldade,
    string Conteudo, bool Concluida, Guid ModuloId)
{
    public static AulaResponse FromDomain(Aula a) =>
        new(a.Id, a.Nome, a.Descricao, a.PontosAula, a.Dificuldade, a.Conteudo, a.Concluida, a.ModuloId);
}