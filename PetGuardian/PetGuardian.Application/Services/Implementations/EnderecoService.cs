using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Common;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de orquestração de endereços com resolução automática via IViaCepService.
/// </summary>
public sealed class EnderecoService(
    IRepository<Endereco>    enderecoRepository,
    IRepository<Bairro>      bairroRepository,
    IRepository<Cidade>      cidadeRepository,
    IRepository<Estado>      estadoRepository,
    IViaCepService           viaCepService,
    ILogger<EnderecoService> logger,
    IUnitOfWork?             unitOfWork = null) : IEnderecoService
{
    public IReadOnlyList<EnderecoResponse> GetAll() =>
        enderecoRepository.GetAll().Select(EnderecoResponse.FromDomain).ToList();

    public EnderecoResponse? GetById(Guid id) =>
        enderecoRepository.GetById(id) is { } e ? EnderecoResponse.FromDomain(e) : null;

    public EnderecoResponse Create(EnderecoRequest request)
    {
        var cepLimpo = LimparCep(request.Cep);
        var cepInfo = ConsultarCep(cepLimpo);

        return unitOfWork.RunInTransaction(() =>
        {
            var (rua, bairro) = PersistirHierarquia(cepInfo);
            var numeroLimpo = request.Numero.Trim();
            var endereco = ObterOuCriar(
                enderecoRepository,
                e => e.Cep == cepLimpo && e.Numero == numeroLimpo && e.BairroId == bairro.Id,
                () => new Endereco(cepLimpo, rua, numeroLimpo, bairro.Id));

            logger.LogInformation("Endereço {EnderecoId} resolvido para o CEP {Cep}.", endereco.Id, cepLimpo);
            return EnderecoResponse.FromDomain(endereco);
        });
    }

    public EnderecoResponse? Update(Guid id, EnderecoRequest request)
    {
        var endereco = enderecoRepository.GetById(id);
        if (endereco is null)
            return null;

        var cepLimpo = LimparCep(request.Cep);
        var cepInfo = ConsultarCep(cepLimpo);

        return unitOfWork.RunInTransaction(() =>
        {
            var (rua, bairro) = PersistirHierarquia(cepInfo);
            endereco.Atualizar(cepLimpo, rua, request.Numero.Trim(), bairro.Id);
            enderecoRepository.Update(endereco);
            return EnderecoResponse.FromDomain(endereco);
        });
    }

    public bool Delete(Guid id) => enderecoRepository.Delete(id);

    private static string LimparCep(string cep) => cep.Trim().Replace("-", "");

    private ViaCepResponseDto ConsultarCep(string cepLimpo) =>
        Task.Run(() => viaCepService.ConsultarCepAsync(cepLimpo)).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException($"CEP {cepLimpo} não encontrado.");

    private (string Rua, Bairro Bairro) PersistirHierarquia(ViaCepResponseDto cepInfo)
    {
        var estadoNome = (cepInfo.Estado ?? cepInfo.Uf ?? throw new InvalidOperationException("Estado não informado na resposta do CEP.")).Trim();
        var cidadeNome = (cepInfo.Localidade ?? throw new InvalidOperationException("Cidade não informada na resposta do CEP.")).Trim();
        var bairroNome = (cepInfo.Bairro ?? throw new InvalidOperationException("Bairro não informado na resposta do CEP.")).Trim();
        var ruaNome    = cepInfo.Logradouro ?? string.Empty;

        var estadoLower = estadoNome.ToLowerInvariant();
        var estado = ObterOuCriar(estadoRepository, e => e.NomeEstado.ToLower() == estadoLower, () => new Estado(estadoNome));

        var cidadeLower = cidadeNome.ToLowerInvariant();
        var cidade = ObterOuCriar(cidadeRepository, c => c.NomeCidade.ToLower() == cidadeLower && c.EstadoId == estado.Id, () => new Cidade(cidadeNome, estado.Id));

        var bairroLower = bairroNome.ToLowerInvariant();
        var bairro = ObterOuCriar(bairroRepository, b => b.NomeBairro.ToLower() == bairroLower && b.CidadeId == cidade.Id, () => new Bairro(bairroNome, cidade.Id));

        return (ruaNome, bairro);
    }

    private static T ObterOuCriar<T>(
        IRepository<T> repo,
        Expression<Func<T, bool>> filtro,
        Func<T> fabrica) where T : BaseEntity
    {
        var existente = repo.FirstOrDefault(filtro);
        if (existente is not null)
            return existente;

        var novo = fabrica();
        repo.Add(novo);
        return novo;
    }
}
