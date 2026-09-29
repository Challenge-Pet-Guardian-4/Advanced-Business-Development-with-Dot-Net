using Microsoft.Extensions.Logging;
using PetGuardian.Application.DTOs;
using PetGuardian.Application.Repositories;
using PetGuardian.Application.Services.Interfaces;
using PetGuardian.Domain.Entities;

namespace PetGuardian.Application.Services.Implementations;

/// <summary>
/// Serviço de orquestração de endereços com resolução automática via IViaCepService e logging estruturado.
/// </summary>
public sealed class EnderecoService(
    IRepository<Endereco>     enderecoRepository,
    IRepository<Bairro>       bairroRepository,
    IRepository<Cidade>       cidadeRepository,
    IRepository<Estado>       estadoRepository,
    IViaCepService            viaCepService,
    ILogger<EnderecoService>  logger) : IEnderecoService
{
    public IReadOnlyList<EnderecoResponse> GetAll()
    {
        logger.LogInformation("Buscando todos os endereços cadastrados.");
        return enderecoRepository.GetAll().Select(EnderecoResponse.FromDomain).ToList();
    }

    public EnderecoResponse? GetById(Guid id)
    {
        logger.LogInformation("Buscando endereço por ID: {EnderecoId}", id);
        var e = enderecoRepository.GetById(id);
        if (e is null)
            logger.LogWarning("Endereço com ID {EnderecoId} não encontrado.", id);

        return e is null ? null : EnderecoResponse.FromDomain(e);
    }

    public EnderecoResponse Create(EnderecoRequest request)
    {
        logger.LogInformation("Iniciando criação de endereço para CEP: {Cep}, Número: {Numero}", request.Cep, request.Numero);
        var resolved = ResolveAddress(request.Cep);
        var endereco = FindOrCreateByCepAndNumero(request.Cep, request.Numero, resolved.Rua, resolved.Bairro.Id);
        logger.LogInformation("Endereço {EnderecoId} cadastrado/resolvido com sucesso para o CEP: {Cep}", endereco.Id, request.Cep);
        return EnderecoResponse.FromDomain(endereco);
    }

    public EnderecoResponse? Update(Guid id, EnderecoRequest request)
    {
        logger.LogInformation("Iniciando atualização do endereço: {EnderecoId}", id);
        var endereco = enderecoRepository.GetById(id);
        if (endereco is null)
        {
            logger.LogWarning("Tentativa de atualizar endereço inexistente: {EnderecoId}", id);
            return null;
        }

        var resolved = ResolveAddress(request.Cep);
        var cepLimpo = request.Cep.Trim().Replace("-", "");
        endereco.Atualizar(cepLimpo, resolved.Rua, request.Numero.Trim(), resolved.Bairro.Id);
        enderecoRepository.Update(endereco);
        logger.LogInformation("Endereço {EnderecoId} atualizado com sucesso.", id);
        return EnderecoResponse.FromDomain(endereco);
    }

    public bool Delete(Guid id)
    {
        logger.LogInformation("Iniciando exclusão do endereço: {EnderecoId}", id);
        var removido = enderecoRepository.Delete(id);
        if (removido)
            logger.LogInformation("Endereço {EnderecoId} removido com sucesso.", id);
        else
            logger.LogWarning("Tentativa de exclusão de endereço inexistente: {EnderecoId}", id);

        return removido;
    }

    private (string Rua, Bairro Bairro) ResolveAddress(string cep)
    {
        var cepLimpo = cep.Trim().Replace("-", "");
        var cepInfo = viaCepService.ConsultarCepAsync(cepLimpo).GetAwaiter().GetResult();
        if (cepInfo is null)
        {
            logger.LogWarning("Resolução de endereço falhou: CEP {Cep} não encontrado no ViaCEP.", cepLimpo);
            throw new InvalidOperationException($"CEP {cepLimpo} não encontrado.");
        }

        var estadoNome = cepInfo.Estado ?? cepInfo.Uf ?? throw new InvalidOperationException("Estado não informado na resposta do CEP.");
        var cidadeNome = cepInfo.Localidade ?? throw new InvalidOperationException("Cidade não informada na resposta do CEP.");
        var bairroNome = cepInfo.Bairro ?? throw new InvalidOperationException("Bairro não informado na resposta do CEP.");
        var ruaNome = cepInfo.Logradouro ?? string.Empty;

        var estado = FindOrCreateEstado(estadoNome);
        var cidade = FindOrCreateCidade(cidadeNome, estado.Id);
        var bairro = FindOrCreateBairro(bairroNome, cidade.Id);

        return (ruaNome, bairro);
    }

    private Endereco FindOrCreateByCepAndNumero(string cep, string numero, string rua, Guid bairroId)
    {
        var cepLimpo = cep.Trim().Replace("-", "");
        var numeroLimpo = numero.Trim();

        var endereco = enderecoRepository.FirstOrDefault(e =>
            e.Cep == cepLimpo && e.Numero == numeroLimpo && e.BairroId == bairroId);

        if (endereco is null)
        {
            endereco = new Endereco(cepLimpo, rua, numeroLimpo, bairroId);
            enderecoRepository.Add(endereco);
            logger.LogInformation("Novo registro de endereço persistido no banco: {EnderecoId} (CEP: {Cep})", endereco.Id, cepLimpo);
        }

        return endereco;
    }

    private Estado FindOrCreateEstado(string nomeEstado)
    {
        var nomeNormalizado = nomeEstado.Trim();
        var nomeLower = nomeNormalizado.ToLower();
        var estado = estadoRepository.FirstOrDefault(e => e.NomeEstado.ToLower() == nomeLower);

        if (estado is null)
        {
            estado = new Estado(nomeNormalizado);
            estadoRepository.Add(estado);
            logger.LogInformation("Novo Estado persistido: {NomeEstado}", nomeNormalizado);
        }

        return estado;
    }

    private Cidade FindOrCreateCidade(string nomeCidade, Guid estadoId)
    {
        var nomeNormalizado = nomeCidade.Trim();
        var nomeLower = nomeNormalizado.ToLower();
        var cidade = cidadeRepository.FirstOrDefault(c =>
            c.NomeCidade.ToLower() == nomeLower && c.EstadoId == estadoId);

        if (cidade is null)
        {
            cidade = new Cidade(nomeNormalizado, estadoId);
            cidadeRepository.Add(cidade);
            logger.LogInformation("Nova Cidade persistida: {NomeCidade}", nomeNormalizado);
        }

        return cidade;
    }

    private Bairro FindOrCreateBairro(string nomeBairro, Guid cidadeId)
    {
        var nomeNormalizado = nomeBairro.Trim();
        var nomeLower = nomeNormalizado.ToLower();
        var bairro = bairroRepository.FirstOrDefault(b =>
            b.NomeBairro.ToLower() == nomeLower && b.CidadeId == cidadeId);

        if (bairro is null)
        {
            bairro = new Bairro(nomeNormalizado, cidadeId);
            bairroRepository.Add(bairro);
            logger.LogInformation("Novo Bairro persistido: {NomeBairro}", nomeNormalizado);
        }

        return bairro;
    }
}