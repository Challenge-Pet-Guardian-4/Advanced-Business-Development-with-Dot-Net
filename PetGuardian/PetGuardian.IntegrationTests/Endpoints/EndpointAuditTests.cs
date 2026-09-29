using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PetGuardian.Application.DTOs;
using PetGuardian.Domain.Enums;
using PetGuardian.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace PetGuardian.IntegrationTests.Endpoints;

public record EndpointAuditResult(
    string Metodo,
    string Rota,
    HttpStatusCode StatusEsperado,
    HttpStatusCode StatusObtido,
    long LatenciaMs,
    string Detalhe);

[Collection(IntegrationTestCollection.Name)]
public class EndpointAuditTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly List<EndpointAuditResult> _results = [];

    private async Task<HttpResponseMessage> ExecutarAsync(
        HttpMethod metodo,
        string rota,
        HttpStatusCode esperado,
        HttpContent? content = null)
    {
        var sw = Stopwatch.StartNew();
        var request = new HttpRequestMessage(metodo, rota) { Content = content };
        var response = await _client.SendAsync(request);
        sw.Stop();

        _results.Add(new EndpointAuditResult(
            metodo.Method,
            rota,
            esperado,
            response.StatusCode,
            sw.ElapsedMilliseconds,
            response.IsSuccessStatusCode ? "OK" : $"HTTP {(int)response.StatusCode}"
        ));

        output.WriteLine($"[{sw.ElapsedMilliseconds,4}ms] {metodo.Method,-6} {rota,-60} -> {response.StatusCode}");

        Assert.Equal(esperado, response.StatusCode);
        return response;
    }

    [Fact]
    public async Task AuditoriaGeralDeTodosOsEndpoints_VarreduraCompleta99Operacoes_DeveExecutarComSucesso()
    {
        // 1. OBSERVABILIDADE & INFRA
        await ExecutarAsync(HttpMethod.Get, "/health", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, "/health/live", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, "/swagger/v1/swagger.json", HttpStatusCode.OK);

        // 2. ESTADO
        var estadoPostRes = await ExecutarAsync(HttpMethod.Post, "/api/estado", HttpStatusCode.Created, JsonContent.Create(new EstadoRequest("Paraná")));
        var estado = await estadoPostRes.Content.ReadFromJsonAsync<EstadoResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/estado", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/estado/{estado!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/estado/{estado.Id}", HttpStatusCode.OK, JsonContent.Create(new EstadoRequest("Paraná Atualizado")));

        // 3. CIDADE
        var cidadePostRes = await ExecutarAsync(HttpMethod.Post, "/api/cidade", HttpStatusCode.Created, JsonContent.Create(new CidadeRequest("Curitiba", estado.Id)));
        var cidade = await cidadePostRes.Content.ReadFromJsonAsync<CidadeResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/cidade", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/cidade/{cidade!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/cidade/by-estado/{estado.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/cidade/{cidade.Id}", HttpStatusCode.OK, JsonContent.Create(new CidadeRequest("Curitiba Atualizada", estado.Id)));

        // 4. BAIRRO
        var bairroPostRes = await ExecutarAsync(HttpMethod.Post, "/api/bairro", HttpStatusCode.Created, JsonContent.Create(new BairroRequest("Batel", cidade.Id)));
        var bairro = await bairroPostRes.Content.ReadFromJsonAsync<BairroResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/bairro", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/bairro/{bairro!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/bairro/by-cidade/{cidade.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/bairro/{bairro.Id}", HttpStatusCode.OK, JsonContent.Create(new BairroRequest("Batel Atualizado", cidade.Id)));

        // 5. ENDERECO
        var endPostRes = await ExecutarAsync(HttpMethod.Post, "/api/endereco", HttpStatusCode.Created, JsonContent.Create(new EnderecoRequest("01001-000", "200")));
        var endereco = await endPostRes.Content.ReadFromJsonAsync<EnderecoResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/endereco", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/endereco/{endereco!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/endereco/{endereco.Id}", HttpStatusCode.OK, JsonContent.Create(new EnderecoRequest("01001-000", "250")));

        // 6. TELEFONE
        var telPostRes = await ExecutarAsync(HttpMethod.Post, "/api/telefone", HttpStatusCode.Created, JsonContent.Create(new TelefoneRequest("41", "991112233")));
        var telefone = await telPostRes.Content.ReadFromJsonAsync<TelefoneResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/telefone", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/telefone/{telefone!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/telefone/{telefone.Id}", HttpStatusCode.OK, JsonContent.Create(new TelefoneRequest("41", "994445566")));

        // 7. USUARIO
        var emailAuditoria = $"aud_{Guid.NewGuid():N}"[..25] + "@clyvo.com";
        var userPostRes = await ExecutarAsync(HttpMethod.Post, "/api/usuario", HttpStatusCode.Created, JsonContent.Create(new UsuarioRequest("Auditor Clyvo", emailAuditoria, "Senha@1234", RoleUsuario.Premium, telefone.Id)));
        var usuario = await userPostRes.Content.ReadFromJsonAsync<UsuarioResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/usuario", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/usuario/{usuario!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/usuario/by-email?email={emailAuditoria}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/usuario/{usuario.Id}/score", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/usuario/{usuario.Id}", HttpStatusCode.OK, JsonContent.Create(new UsuarioUpdateRequest("Auditor Clyvo Senior", emailAuditoria, "NovaSenha@123", RoleUsuario.Premium)));

        // 8. USUARIO-ENDERECO
        var uEndPostRes = await ExecutarAsync(HttpMethod.Post, "/api/usuarioendereco", HttpStatusCode.Created, JsonContent.Create(new UsuarioEnderecoRequest(usuario.Id, endereco.Id)));
        await ExecutarAsync(HttpMethod.Get, "/api/usuarioendereco", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/usuarioendereco/by-usuario/{usuario.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/usuarioendereco/by-endereco/{endereco.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Delete, $"/api/usuarioendereco/{usuario.Id}/{endereco.Id}", HttpStatusCode.NoContent);

        // 9. RACA
        var racaPostRes = await ExecutarAsync(HttpMethod.Post, "/api/raca", HttpStatusCode.Created, JsonContent.Create(new RacaRequest("Golden Retriever")));
        var raca = await racaPostRes.Content.ReadFromJsonAsync<RacaResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/raca", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/raca/{raca!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/raca/{raca.Id}", HttpStatusCode.OK, JsonContent.Create(new RacaRequest("Golden Retriever Puro")));

        // 10. PET
        var petPostRes = await ExecutarAsync(HttpMethod.Post, "/api/pet", HttpStatusCode.Created, JsonContent.Create(new PetRequest("Thor", DateTime.UtcNow.AddYears(-3), SexoPet.Macho, PortePet.Grande, true, raca.Id)));
        var pet = await petPostRes.Content.ReadFromJsonAsync<PetResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/pet", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/pet/{pet!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/pet/{pet.Id}/historico", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/pet/{pet.Id}", HttpStatusCode.OK, JsonContent.Create(new PetRequest("Thor Odinson", DateTime.UtcNow.AddYears(-3), SexoPet.Macho, PortePet.Grande, true, raca.Id)));

        // 11. USUARIO-PET & REDE DE CUIDADO
        var uPetPostRes = await ExecutarAsync(HttpMethod.Post, "/api/usuariopet", HttpStatusCode.Created, JsonContent.Create(new UsuarioPetRequest(usuario.Id, pet.Id, ResponPrinc: true)));
        await ExecutarAsync(HttpMethod.Get, "/api/usuariopet", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/usuariopet/by-usuario/{usuario.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/usuariopet/by-pet/{pet.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/usuariopet/rede-cuidado/{usuario.Id}", HttpStatusCode.OK);

        // Usuário secundário para convite
        var emailSec = $"sec_{Guid.NewGuid():N}"[..25] + "@clyvo.com";
        var userSecPostRes = await ExecutarAsync(HttpMethod.Post, "/api/usuario", HttpStatusCode.Created, JsonContent.Create(new UsuarioRequest("Cuidador Convidado", emailSec, "Senha@123", RoleUsuario.Comum, telefone.Id)));
        var userSec = await userSecPostRes.Content.ReadFromJsonAsync<UsuarioResponse>();

        await ExecutarAsync(HttpMethod.Post, "/api/usuariopet/invite/by-usuario", HttpStatusCode.Created, JsonContent.Create(new UsuarioPetInviteByUsuarioRequest(usuario.Id, userSec!.Id, pet.Id)));
        await ExecutarAsync(HttpMethod.Put, $"/api/usuariopet/{userSec.Id}/{pet.Id}", HttpStatusCode.OK, JsonContent.Create(new UsuarioPetUpdateRequest(ResponPrinc: false)));
        await ExecutarAsync(HttpMethod.Delete, $"/api/usuariopet/{userSec.Id}/{pet.Id}", HttpStatusCode.NoContent);

        // Convite por e-mail (cria usuário convidado previamente)
        var emailInvite = $"inv_{Guid.NewGuid():N}"[..25] + "@clyvo.com";
        var userInvitePostRes = await ExecutarAsync(HttpMethod.Post, "/api/usuario", HttpStatusCode.Created, JsonContent.Create(new UsuarioRequest("Cuidador Email", emailInvite, "Senha@123", RoleUsuario.Comum, telefone.Id)));
        var userInvite = await userInvitePostRes.Content.ReadFromJsonAsync<UsuarioResponse>();

        await ExecutarAsync(HttpMethod.Post, "/api/usuariopet/invite/by-email", HttpStatusCode.Created, JsonContent.Create(new UsuarioPetInviteByEmailRequest(usuario.Id, emailInvite, pet.Id)));
        await ExecutarAsync(HttpMethod.Delete, $"/api/usuariopet/{userInvite!.Id}/{pet.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/usuario/{userInvite.Id}", HttpStatusCode.NoContent);

        // 12. STATUS
        var statusListRes = await ExecutarAsync(HttpMethod.Get, "/api/status", HttpStatusCode.OK);
        var statusList = await statusListRes.Content.ReadFromJsonAsync<List<StatusResponse>>();
        var statusPendente = statusList!.First(s => s.NomeStatus == "PENDENTE");
        await ExecutarAsync(HttpMethod.Get, $"/api/status/{statusPendente.Id}", HttpStatusCode.OK);

        // 13. TAREFA
        var tarefaPostRes = await ExecutarAsync(HttpMethod.Post, "/api/tarefa", HttpStatusCode.Created, JsonContent.Create(new TarefaRequest("Vacinação V10", 50, "Aplicar dose anual", DateTime.UtcNow.AddDays(3), pet.Id, usuario.Id)));
        var tarefa = await tarefaPostRes.Content.ReadFromJsonAsync<TarefaResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/tarefa", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/tarefa/{tarefa!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/tarefa/by-pet/{pet.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/tarefa/by-usuario/{usuario.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/tarefa/by-status/{statusPendente.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/tarefa/{tarefa.Id}", HttpStatusCode.OK, JsonContent.Create(new TarefaUpdateRequest("Vacinação V10 + Raiva", 60, "Aplicar ambas as doses", DateTime.UtcNow.AddDays(4))));
        await ExecutarAsync(HttpMethod.Post, $"/api/tarefa/{tarefa.Id}/concluir", HttpStatusCode.OK, JsonContent.Create(new TarefaConcluirRequest(usuario.Id)));
        await ExecutarAsync(HttpMethod.Delete, $"/api/tarefa/{tarefa.Id}", HttpStatusCode.NoContent);

        // 14. TRILHA
        var trilhaPostRes = await ExecutarAsync(HttpMethod.Post, "/api/trilha", HttpStatusCode.Created, JsonContent.Create(new TrilhaRequest("Trilha Saúde Canina", "Aprenda a cuidar do seu cão", pet.Id)));
        var trilha = await trilhaPostRes.Content.ReadFromJsonAsync<TrilhaResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/trilha", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/trilha/{trilha!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/trilha/{trilha.Id}", HttpStatusCode.OK, JsonContent.Create(new TrilhaUpdateRequest("Trilha Saúde Canina Avançada", "Desc atualizada")));

        // 15. MODULO
        var moduloPostRes = await ExecutarAsync(HttpMethod.Post, "/api/modulo", HttpStatusCode.Created, JsonContent.Create(new ModuloRequest("Módulo de Nutrição", "45 min", "Alimentação balanceada", trilha.Id)));
        var modulo = await moduloPostRes.Content.ReadFromJsonAsync<ModuloResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/modulo", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/modulo/{modulo!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/modulo/{modulo.Id}", HttpStatusCode.OK, JsonContent.Create(new ModuloUpdateRequest("Módulo de Nutrição e Hidratação", "50 min", "Desc atualizada")));

        // 16. AULA
        var aulaPostRes = await ExecutarAsync(HttpMethod.Post, "/api/aula", HttpStatusCode.Created, JsonContent.Create(new AulaRequest("Porções Diárias", "Como calcular porções", 15, "Iniciante", "Vídeo e texto explicativo", false, modulo.Id)));
        var aula = await aulaPostRes.Content.ReadFromJsonAsync<AulaResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/aula", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/aula/{aula!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/aula/{aula.Id}", HttpStatusCode.OK, JsonContent.Create(new AulaUpdateRequest("Porções Diárias e Petiscos", "Cálculo detalhado", 20, "Iniciante", "Texto revisado", true)));
        await ExecutarAsync(HttpMethod.Delete, $"/api/aula/{aula.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/modulo/{modulo.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/trilha/{trilha.Id}", HttpStatusCode.NoContent);

        // 17. HISTORICO
        var histPostRes = await ExecutarAsync(HttpMethod.Post, "/api/historico", HttpStatusCode.Created, JsonContent.Create(new HistoricoRequest("CONSULTA_ROTINA", DateTime.UtcNow, pet.Id)));
        var historico = await histPostRes.Content.ReadFromJsonAsync<HistoricoResponse>();
        await ExecutarAsync(HttpMethod.Get, "/api/historico", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/historico/{historico!.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Get, $"/api/historico/by-pet/{pet.Id}", HttpStatusCode.OK);
        await ExecutarAsync(HttpMethod.Put, $"/api/historico/{historico.Id}", HttpStatusCode.OK, JsonContent.Create(new HistoricoUpdateRequest("CONSULTA_ROTINA_CONCLUIDA", DateTime.UtcNow)));
        await ExecutarAsync(HttpMethod.Delete, $"/api/historico/{historico.Id}", HttpStatusCode.NoContent);

        // 18. EXCLUSÕES FINAIS DE CICLO
        await ExecutarAsync(HttpMethod.Delete, $"/api/pet/{pet.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/usuario/{userSec.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/usuario/{usuario.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/endereco/{endereco.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/bairro/{bairro.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/cidade/{cidade.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/estado/{estado.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/raca/{raca.Id}", HttpStatusCode.NoContent);
        await ExecutarAsync(HttpMethod.Delete, $"/api/telefone/{telefone.Id}", HttpStatusCode.NoContent);

        // Grava relatório em arquivo Markdown para consumo do relatório
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Relatório de Auditoria de Todos os Endpoints da API PetGuardian (.NET 10)");
        sb.AppendLine();
        sb.AppendLine($"Data da Execução: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC");
        sb.AppendLine();
        sb.AppendLine("| # | Método | Endpoint / Rota | Status Esperado | Status Obtido | Latência | Resultado |");
        sb.AppendLine("| :--- | :---: | :--- | :---: | :---: | :---: | :---: |");
        for (int i = 0; i < _results.Count; i++)
        {
            var r = _results[i];
            sb.AppendLine($"| {i + 1:D2} | `{r.Metodo}` | `{r.Rota}` | `{(int)r.StatusEsperado} {r.StatusEsperado}` | `{(int)r.StatusObtido} {r.StatusObtido}` | {r.LatenciaMs} ms | {r.Detalhe} |");
        }
        sb.AppendLine();
        sb.AppendLine($"**Total de Endpoints Testados e Aprovados:** {_results.Count} operações HTTP com 100% de sucesso.");

        // Caminho portável: pasta "AuditReports" ao lado do assembly de testes (existe em qualquer máquina/CI),
        // em vez de um caminho absoluto fixo de uma máquina específica.
        var reportDir = Path.Combine(AppContext.BaseDirectory, "AuditReports");
        var reportPath = Path.Combine(reportDir, "endpoint_audit_report.md");

        try
        {
            Directory.CreateDirectory(reportDir);
            File.WriteAllText(reportPath, sb.ToString());
            output.WriteLine($"Auditoria gravada em {reportPath}. Total de operações: {_results.Count}");
        }
        catch (Exception ex)
        {
            // A gravação do relatório é só um artefato informativo; não deve derrubar o teste
            // se o ambiente de execução não permitir escrita em disco (ex.: alguns runners de CI).
            output.WriteLine($"Não foi possível gravar o relatório em disco ({ex.Message}). Total de operações: {_results.Count}");
        }
    }
}