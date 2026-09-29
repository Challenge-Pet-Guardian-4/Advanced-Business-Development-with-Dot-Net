using PetGuardian.Domain.Common;
using PetGuardian.Domain.Exceptions;

namespace PetGuardian.Domain.Entities;

/// <summary>Estado da federação. Topo da hierarquia de endereço.</summary>
public sealed class Estado : BaseEntity
{
    public string NomeEstado { get; private set; } = string.Empty;

    public List<Cidade> Cidades { get; private set; } = [];

    private Estado() { }

    public Estado(string nomeEstado)
    {
        NomeEstado = Validar(nomeEstado);
    }

    /// <summary>Atualiza o nome do estado (usado pelo PUT).</summary>
    public void Atualizar(string nomeEstado) => NomeEstado = Validar(nomeEstado);

    private static string Validar(string nomeEstado)
    {
        if (string.IsNullOrWhiteSpace(nomeEstado))
            throw new DomainException("O nome do estado não pode ser vazio.");
        nomeEstado = nomeEstado.Trim();
        if (nomeEstado.Length > 30)
            throw new DomainException("O nome do estado deve ter no máximo 30 caracteres.");
        return nomeEstado;
    }
}