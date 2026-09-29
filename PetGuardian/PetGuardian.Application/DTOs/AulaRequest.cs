using System.ComponentModel.DataAnnotations;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.DTOs;

/// <summary>Schema 2026-08-29: ganhou Conteudo e Concluida.</summary>
public record AulaRequest(
    [Required][StringLength(50, MinimumLength = 2)] string Nome,
    [Required][StringLength(100, MinimumLength = 2)] string Descricao,
    [Range(0, 99999)] int PontosAula,
    [Required][StringLength(20, MinimumLength = 2)] string Dificuldade,
    [Required][StringLength(1000, MinimumLength = 2)] string Conteudo,
    bool Concluida,
    [Required] Guid ModuloId)
{
    public Aula ToDomain() => new(Nome, Descricao, PontosAula, Dificuldade, Conteudo, Concluida, ModuloId);
}

/// <summary>Corpo do PUT. O módulo vinculado não é reatribuível por aqui.</summary>
public record AulaUpdateRequest(
    [Required][StringLength(50, MinimumLength = 2)] string Nome,
    [Required][StringLength(100, MinimumLength = 2)] string Descricao,
    [Range(0, 99999)] int PontosAula,
    [Required][StringLength(20, MinimumLength = 2)] string Dificuldade,
    [Required][StringLength(1000, MinimumLength = 2)] string Conteudo,
    bool Concluida
);