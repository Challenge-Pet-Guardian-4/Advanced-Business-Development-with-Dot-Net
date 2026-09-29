# 🤖 AGENT.md — Diretrizes Arquiteturais, Boas Práticas & Engenharia de Software (.NET 10 / C# 14)

> **Projeto de Referência:** *Recommenda* (Prof. Thiago Keller — FIAP — 2TDSPG / Advanced Business Development with .NET).  
> **Escopo:** Documento normativo, técnico e prático contendo todas as decisões arquiteturais, padrões de design, convenções de código, Clean Architecture, DDD, SOLID, DRY, Clean Code, Observabilidade e suíte de testes AAA utilizados no ecossistema **PetGuardian**.

---

## 🏛️ 1. Visão Geral da Arquitetura (Clean Architecture & DDD)

A arquitetura organiza a aplicação em 4 camadas desacopladas com fluxo unidirecional e inversão de dependência (DIP):

```
┌─────────────────────────────────────────────────────────┐
│                     PetGuardian.API                     │ ──> Controladores REST, Middlewares, Health Checks, Swagger, DI
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                 PetGuardian.Application                 │ ──> Casos de Uso, DTOs, Contratos de Repositório, Serviços, Factories
└──────────────┬───────────────────────────┬──────────────┘
               │                           │
┌──────────────▼──────────────┐ ┌──────────▼──────────────┐
│     PetGuardian.Domain      │ │ PetGuardian.Infrastructure │
│ (POCOs, Invariantes, Enums) │ │ (EF Core, Repositórios) │
└─────────────────────────────┘ └─────────────────────────┘
```

### Regras de Dependência (DIP):
1. **`Domain` (Núcleo Agnóstico):** POCOs puros, regras de negócio e validações de invariantes com `DomainException`. Totalmente agnóstico de frameworks (sem referências a EF Core, ASP.NET Core ou bibliotecas de banco de dados).
2. **`Application` (Contratos & Orquestração):** DTOs (Records imutáveis com DataAnnotations e conversores `FromDomain`/`ToDomain`), contratos de repositório (`IRepository<T>` genérico e contratos específicos) e serviços com Primary Constructors.
3. **`Infrastructure` (Persistência & EF Core):** `DbContext`, mapeamentos isolados via Fluent API (`IEntityTypeConfiguration<T>`) e implementações de repositório com `AsNoTracking()`.
4. **`API` (Hosting & Apresentação REST):** `Program.cs` limpo com injeção de dependência modularizada em métodos de extensão (`Extensions/`), `GlobalExceptionHandler` (RFC 7807), rastreabilidade de diagnósticos (`X-Correlation-ID`) e observabilidade (Health Checks com `HealthCheckResponseWriter`, OpenTelemetry, Serilog).

---

## 🎯 2. Padrões por Camada (Implementação Prática)

### 2.1 Camada de Domínio (`.Domain`)

* **`BaseEntity`:**
  * Define o identificador único `Guid Id { get; private set; } = Guid.NewGuid();`.
* **Segurança de Senhas (`HashHelper` com BCrypt & Salt):**
  * Senhas nunca são persistidas em texto puro.
  * O `HashHelper` utiliza `BCrypt.Net-Next` combinando a senha com um salt criptográfico exclusivo gerado por usuário (`Guid.NewGuid().ToString("N")`).
  * O método `VerifyPassword(rawPassword)` compara a senha fornecida com o hash armazenado.
* **Invariantes, Encapsulamento & DRY nos Construtores:**
  * Propriedades com `private set` ou `protected set`.
  * Construtores privados/protegidos sem parâmetros (`private Entidade() { }`) exclusivos para hidratação do EF Core.
  * Construtores públicos reutilizam diretamente os métodos de mutação (`Atualizar(...)`) para evitar duplicação de regras de validação (DRY).
  * Métodos de negócio semânticos (`Atualizar(...)`, `Concluir()`, `Castrar()`, `AtualizarResponsabilidade(...)`) validam as regras e lançam `DomainException`.

```csharp
// Exemplo Canônico de Entidade Rica com Hashing Seguro e DRY
public sealed class Usuario : BaseEntity
{
    public const int MinimumPasswordLength = 6;

    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Senha { get; private set; } = string.Empty;
    public string Salt { get; private set; } = string.Empty;
    public RoleUsuario Role { get; private set; }

    public Guid TelefoneId { get; private set; }
    public Telefone? Telefone { get; private set; }

    private Usuario() { }

    public Usuario(string nome, string email, string senha, RoleUsuario role, Guid telefoneId)
    {
        AtualizarNome(nome);
        AtualizarEmail(email);
        AtualizarSenha(senha);
        Role = role;
        if (telefoneId == Guid.Empty)
            throw new DomainException("O usuário deve ter um telefone válido.");
        TelefoneId = telefoneId;
    }

    public void AtualizarNome(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new DomainException("O nome não pode ser vazio.");
        Nome = novoNome.Trim();
    }

    public void AtualizarEmail(string novoEmail)
    {
        if (string.IsNullOrWhiteSpace(novoEmail) || !novoEmail.Contains('@'))
            throw new DomainException("O e-mail informado é inválido.");
        Email = novoEmail.Trim();
    }

    public void AtualizarSenha(string novaSenhaRaw)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaRaw) || novaSenhaRaw.Length < MinimumPasswordLength)
            throw new DomainException($"A senha deve ter pelo menos {MinimumPasswordLength} caracteres.");

        Salt = Guid.NewGuid().ToString("N");
        Senha = HashHelper.Hash(novaSenhaRaw, Salt);
    }

    public bool VerifyPassword(string rawPassword) =>
        !string.IsNullOrWhiteSpace(rawPassword) && HashHelper.Verify(rawPassword, Salt, Senha);
}
```

---

### 2.2 Camada de Aplicação (`.Application`)

* **DTOs com Records Imutáveis:**
  * Modelados com `public record ...Request(...)` e `public record ...Response(...)` utilizando DataAnnotations (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Range]`).
  * Métodos utilitários: `FromDomain(Entity entity)` estático e `ToDomain()` de instância.
* **Repositório Genérico vs. Repositórios Específicos:**
  * `IRepository<T>` define operações fundamentais: `GetAll()`, `GetById(id)`, `Find(predicate)`, `FirstOrDefault(predicate)`, `Add(entity)`, `Update(entity)`, `Delete(id)`, `ExistsById(id)`, `Exists(predicate)`.
  * Repositórios específicos (`IPetRepository`, `IUsuarioRepository`, `ITarefaRepository`, `IUsuarioPetRepository`) adicionam consultas de domínio indexadas e otimizadas (`GetByEmail`, `GetByRacaId`, `GetByUsuarioAndPet`).
* **Serviços de Aplicação & Logging Estruturado Semântico:**
  * Classes `public sealed class ...Service(...) : I...Service` utilizando **Primary Constructors** do C# 12+.
  * Injeção de `ILogger<TService>` em todos os serviços de aplicação com registro semântico de mutações (`LogInformation`) e avisos de validação de negócio (`LogWarning`) antes de lançar exceções.
  * Sanitização rigorosa: senhas brutas, hashes e dados sensíveis nunca são impressos nos logs.
  * Pré-validação de integridade referencial antes de invocar persistência (evita exceções de FK não tratadas no banco).
  * Uso de consultas batch no repositório com expressões lambda (ex: `petRepository.Find(p => petIds.Contains(p.Id))`), eliminando roundtrips repetitivos dentro de loops.
* **Padrão Factory Method / Strategy (OCP):**
  * Para criação polimórfica de subtipos sem poluir serviços com `switch-case`, cria-se uma interface `IFactory` contendo `CanHandle(enum)` e `Create(request)` injetada como `IEnumerable<IFactory>`.

---

### 2.3 Camada de Infraestrutura (`.Infrastructure`)

* **Configurações Fluent API Segregadas:**
  * Cada entidade possui sua configuração isolada em `Persistence/Configurations/{Entidade}Configuration.cs` implementando `IEntityTypeConfiguration<T>`.
  * O `DbContext` carrega todos os mapeamentos automaticamente:
    ```csharp
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetGuardianContext).Assembly);
    ```
* **Performance com `AsNoTracking()`:**
  * Todas as leituras em `Repository<T>` utilizam `.AsNoTracking()` para otimizar tempo de resposta e consumo de memória.

---

### 2.4 Camada de Apresentação (`.API`)

* **`Program.cs` Enxuto & Modular:**
  * Delega configurações de DI para métodos de extensão coesos:
    - `AddPetGuardianDbContext(configuration)`
    - `AddPetGuardianRepositories()`
    - `AddPetGuardianApplicationServices()`
    - `AddPetGuardianObservability(configuration)`
* **Registro de Repositório Genérico:**
  ```csharp
  services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
  ```
* **Tratamento Global de Exceções (`GlobalExceptionHandler` / RFC 7807):**
  * Implementa `IExceptionHandler` devolvendo `ProblemDetails` padronizado.
  * Injeta `traceId` e `correlationId` nos dados da resposta para rastreamento de diagnósticos em produção.
* **Health Checks Estruturados com `HealthCheckResponseWriter`:**
  * Serialização customizada em JSON exibindo status global, duração e detalhamento de cada probe:
  ```csharp
  app.MapHealthChecks("/health", new HealthCheckOptions
  {
      ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
  });
  ```
* **Swagger na Raiz (`RoutePrefix = string.Empty`):**
  * Permite abrir a documentação interativa OpenAPI diretamente ao acessar a URL raiz do serviço (`/`).

---

## 🧪 3. Padrão de Testes Automatizados (AAA & xUnit/Moq)

### 3.1 Convenção de Nomenclatura
```
MetodoTestado_Cenario_ResultadoEsperado
```
*Exemplos:*
- `Create_QuandoRacaNaoExiste_DeveLancarExceptionENaoPersistir`
- `GetHistorico_PetComTarefasConcluidas_DeveRetornarLinhaDoTempoOrdenada`
- `Construtor_DadosValidos_DeveInstanciarUsuarioComSenhaHasheada`
- `GetRedeCuidadoByUsuarioId_UsuarioExistente_DeveRetornarEstruturaRedeCuidado`

### 3.2 Estrutura dos Testes Unitários
* **Testes de Domínio (`.UnitTests/Domain`):** Validam regras de negócio, cálculo dinâmico de idades, exceções `DomainException`, criptografia de senhas com salt e integridade das entidades sem dependência de mocks.
* **Testes de Aplicação (`.UnitTests/Application`):** Validam orquestração de serviços com `Moq`, garantindo que repositórios sejam acionados com `Times.Once` no caminho feliz ou `Times.Never` em caso de falha de validação.

```csharp
[Fact]
public void Create_QuandoRacaNaoExiste_DeveLancarExceptionENaoPersistir()
{
    // Arrange
    var service = CreateService();
    var racaId = Guid.NewGuid();
    var request = new PetRequest("Max", DateTime.UtcNow.AddYears(-1), SexoPet.Macho, PortePet.Medio, false, racaId);

    _racaRepoMock.Setup(r => r.ExistsById(racaId)).Returns(false);

    // Act
    var act = () => service.Create(request);

    // Assert
    var ex = Assert.Throws<InvalidOperationException>(act);
    Assert.Equal("Raça não encontrada.", ex.Message);
    _petRepoMock.Verify(r => r.Add(It.IsAny<Pet>()), Times.Never);
}
```

---

## ✅ 4. Checklist de Conformidade da Solução

- [x] **Domínio:** Entidades herdam de `BaseEntity` com `Guid Id` e construtores privados para EF Core?
- [x] **Segurança:** Senhas utilizam `HashHelper` com BCrypt e Salt criptográfico exclusivo?
- [x] **DRY:** Construtores de entidades reutilizam métodos de mutação `Atualizar(...)`?
- [x] **Mutação Completa:** Operações `PUT` implementadas e validadas em todos os 16 agregados?
- [x] **DTOs:** Records imutáveis para Request, UpdateRequest e Response com `FromDomain` e DataAnnotations?
- [x] **Performance EF Core:** Consultas semânticas diretas (`Find`, `FirstOrDefault`, `Exists`) com `.AsNoTracking()`?
- [x] **Serviços:** Classes `sealed`, Primary Constructors e validação prévia de chaves estrangeiras?
- [x] **Observabilidade:** `GlobalExceptionHandler` RFC 7807, `HealthCheckResponseWriter`, Serilog e OpenTelemetry ativos?
- [x] **Apresentação:** Documentação OpenAPI/Swagger ativa na rota raiz (`RoutePrefix = string.Empty`)?
- [x] **Testes AAA:** Suíte com 100% de sucesso em `dotnet test` (65 testes passando)?
