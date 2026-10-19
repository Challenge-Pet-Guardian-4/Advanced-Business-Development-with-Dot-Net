using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

/// <summary>Corpo do POST. O conteúdo didático é gerenciado em <c>/api/aula/{id}/conteudo</c> (MongoDB).</summary>
public record AulaRequest(
    [Required][StringLength(50, MinimumLength = 2)] string Nome,
    [Required][StringLength(100, MinimumLength = 2)] string Descricao,
    [Range(0, 99999)] int PontosAula,
    [Required][StringLength(20, MinimumLength = 2)] string Dificuldade,
    bool Concluida,
    [Required] Guid ModuloId)
{
    public Aula ToDomain() => new(Nome, Descricao, PontosAula, Dificuldade, Concluida, ModuloId);
}

/// <summary>Corpo do PUT. O módulo vinculado não é reatribuível por aqui.</summary>
public record AulaUpdateRequest(
    [Required][StringLength(50, MinimumLength = 2)] string Nome,
    [Required][StringLength(100, MinimumLength = 2)] string Descricao,
    [Range(0, 99999)] int PontosAula,
    [Required][StringLength(20, MinimumLength = 2)] string Dificuldade,
    bool Concluida
);