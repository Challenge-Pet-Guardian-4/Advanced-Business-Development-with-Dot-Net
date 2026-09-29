using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Enums;

namespace PetGuardian.UnitTests.Fixtures;

/// <summary>
/// Fixture compartilhada para geração consistente de entidades e DTOs de teste.
/// </summary>
public class TestFixture
{
    public Pet CriarPetValido(string nome = "Rex", Guid? racaId = null)
    {
        return new Pet(
            nome: nome,
            dataNascimento: DateTime.UtcNow.AddYears(-3),
            sexo: SexoPet.Macho,
            porte: PortePet.Medio,
            castrado: false,
            racaId: racaId ?? Guid.NewGuid()
        );
    }

    public Usuario CriarUsuarioValido(string nome = "Carlos Silva", string email = "carlos@email.com", Guid? telefoneId = null)
    {
        return new Usuario(
            nome: nome,
            email: email,
            senha: "senhaSegura123",
            role: RoleUsuario.Comum,
            telefoneId: telefoneId ?? Guid.NewGuid()
        );
    }

    public Tarefa CriarTarefaValida(Guid? petId = null, Guid? usuarioId = null, Guid? statusId = null)
    {
        return new Tarefa(
            titulo: "Passeio Matinal",
            pontosTarefa: 50,
            descricao: "Caminhada de 30 minutos no parque.",
            prazo: DateTime.UtcNow.AddDays(1),
            petId: petId ?? Guid.NewGuid(),
            statusId: statusId ?? Guid.NewGuid(),
            usuarioId: usuarioId ?? Guid.NewGuid()
        );
    }

    public Trilha CriarTrilhaValida(Guid? petId = null)
    {
        return new Trilha(
            nome: "Trilha de Adestramento",
            descricao: "Comandos básicos de obediência e socialização.",
            petId: petId ?? Guid.NewGuid()
        );
    }

    public Modulo CriarModuloValido(Guid? trilhaId = null)
    {
        return new Modulo(
            nome: "Módulo 1 - Comandos Básicos",
            tempoConclusao: "2 horas",
            descricao: "Comandos senta, deita e fica.",
            trilhaId: trilhaId ?? Guid.NewGuid()
        );
    }

    public Aula CriarAulaValida(Guid? moduloId = null)
    {
        return new Aula(
            nome: "Aula 1: Sentar",
            descricao: "Ensinando o comando senta com petisco.",
            pontosAula: 20,
            dificuldade: "Facil",
            conteudo: "Guia passo a passo do treino.",
            concluida: false,
            moduloId: moduloId ?? Guid.NewGuid()
        );
    }

    public Estado CriarEstadoValido(string nome = "São Paulo")
    {
        return new Estado(nome);
    }

    public Cidade CriarCidadeValida(string nome = "Campinas", Guid? estadoId = null)
    {
        return new Cidade(nome, estadoId ?? Guid.NewGuid());
    }

    public Bairro CriarBairroValido(string nome = "Centro", Guid? cidadeId = null)
    {
        return new Bairro(nome, cidadeId ?? Guid.NewGuid());
    }

    public Endereco CriarEnderecoValido(string cep = "01001000", string rua = "Praça da Sé", string numero = "100", Guid? bairroId = null)
    {
        return new Endereco(cep, rua, numero, bairroId ?? Guid.NewGuid());
    }

    public Telefone CriarTelefoneValido(string ddd = "11", string numero = "987654321")
    {
        return new Telefone(ddd, numero);
    }

    public Status CriarStatusValido(string nome = "PENDENTE")
    {
        return new Status(nome);
    }

    public Raca CriarRacaValida(string nome = "Labrador")
    {
        return new Raca(nome);
    }

    public Historico CriarHistoricoValido(string tipo = "CONSULTA_ROTINA", Guid? petId = null)
    {
        return new Historico(tipo, DateTime.UtcNow, petId ?? Guid.NewGuid());
    }

    public UsuarioEndereco CriarUsuarioEnderecoValido(Guid? usuarioId = null, Guid? enderecoId = null)
    {
        return new UsuarioEndereco(usuarioId ?? Guid.NewGuid(), enderecoId ?? Guid.NewGuid());
    }

    public UsuarioPet CriarUsuarioPetValido(Guid? usuarioId = null, Guid? petId = null, bool responPrinc = true)
    {
        return new UsuarioPet(usuarioId ?? Guid.NewGuid(), petId ?? Guid.NewGuid(), responPrinc);
    }
}
