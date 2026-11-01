using PetGuardian.Domain.Entities;
using PetGuardian.Domain.Exceptions;
using PetGuardian.UnitTests.Fixtures;
using Xunit;

namespace PetGuardian.UnitTests.Domain;

[Collection(UnitTestCollection.Name)]
public class TarefaTests(TestFixture fixture)
{
    [Fact]
    public void Construtor_DadosValidos_DeveInstanciarTarefa()
    {
        // Arrange
        var titulo = "Dar Medicamento";
        var pontos = 10;
        var descricao = "Antibiótico pós-cirúrgico";
        var prazo = DateTime.UtcNow.AddDays(2);
        var petId = Guid.NewGuid();
        var statusId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var tarefa = new Tarefa(titulo, pontos, descricao, prazo, petId, statusId, usuarioId);

        // Assert
        Assert.NotEqual(Guid.Empty, tarefa.Id);
        Assert.Equal(titulo, tarefa.Titulo);
        Assert.Equal(pontos, tarefa.PontosTarefa);
        Assert.Equal(descricao, tarefa.Descricao);
        Assert.Equal(prazo, tarefa.Prazo);
        Assert.Equal(petId, tarefa.PetId);
        Assert.Equal(statusId, tarefa.StatusId);
        Assert.Equal(usuarioId, tarefa.UsuarioId);
        Assert.Null(tarefa.Conclusao);
    }

    [Fact]
    public void Construtor_PrazoNoPassado_DeveLancarDomainException()
    {
        // Arrange
        var prazoPassado = DateTime.UtcNow.AddHours(-1);

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Tarefa("Banho", 10, "Banho no pet", prazoPassado, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        Assert.Equal("O prazo deve ser uma data/hora futura.", ex.Message);
    }

    [Fact]
    public void Atualizar_TarefaPendente_DeveAtualizarCamposEditaveis()
    {
        // Arrange
        var tarefa = fixture.CriarTarefaValida();
        var novoTitulo = "Passeio Noturno";
        var novosPontos = 8;
        var novaDescricao = "Volta de 40 min";
        var novoPrazo = DateTime.UtcNow.AddDays(3);

        // Act
        tarefa.Atualizar(novoTitulo, novosPontos, novaDescricao, novoPrazo);

        // Assert
        Assert.Equal(novoTitulo, tarefa.Titulo);
        Assert.Equal(novosPontos, tarefa.PontosTarefa);
        Assert.Equal(novaDescricao, tarefa.Descricao);
        Assert.Equal(novoPrazo, tarefa.Prazo);
    }

    [Fact]
    public void Concluir_TarefaNaoConcluida_DeveDefinirDataConclusao()
    {
        // Arrange
        var tarefa = fixture.CriarTarefaValida();

        // Act
        tarefa.Concluir();

        // Assert
        Assert.NotNull(tarefa.Conclusao);
    }

    [Fact]
    public void Atualizar_TarefaJaConcluida_DeveLancarDomainException()
    {
        // Arrange
        var tarefa = fixture.CriarTarefaValida();
        tarefa.Concluir();

        // Act
        var ex = Assert.Throws<DomainException>(() =>
            tarefa.Atualizar("Novo Titulo", 10, "Nova Desc", DateTime.UtcNow.AddDays(1)));

        // Assert
        Assert.Equal("Não é possível editar uma tarefa já concluída.", ex.Message);
    }

    [Fact]
    public void Construtor_PontosMaiorQueDez_DeveLancarDomainException()
    {
        // Act
        var ex = Assert.Throws<DomainException>(() =>
            new Tarefa("Banho", 15, "Banho no pet", DateTime.UtcNow.AddDays(1), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        Assert.Equal("Os pontos da tarefa devem ser no máximo 10.", ex.Message);
    }

}
