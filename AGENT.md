# 🤖 AGENT.md — Diretrizes Arquiteturais, Boas Práticas & Engenharia de Software (.NET 10 / C# 14)

> **Projeto de Referência:** *Recommenda* (Prof. Thiago Keller — FIAP — 2TDSPG / Advanced Business Development with .NET).  
> **Escopo:** Documento normativo, técnico e prático contendo todas as decisões arquiteturais, padrões de design, convenções de código, Clean Architecture, DDD, SOLID, KISS, YAGNI, DRY, Clean Code, Observabilidade e suíte de testes AAA utilizados no ecossistema **PetGuardian**.

---

## 🏛️ 1. Visão Geral da Arquitetura (Clean Architecture & DDD)

A arquitetura organiza a aplicação em 4 camadas desacopladas com fluxo unidirecional e inversão de dependência (DIP):

```text
┌─────────────────────────────────────────────────────────┐
│                     PetGuardian.API                     │ ──> Controladores REST, Auth JWT, HATEOAS, Middlewares, Health Checks, Swagger
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                 PetGuardian.Application                 │ ──> Casos de Uso, DTOs, Filtros, Paginação, Contratos de Repositório e Serviços
└──────────────┬───────────────────────────┬──────────────┘
               │                           │
┌──────────────▼──────────────┐ ┌──────────▼──────────────┐
│     PetGuardian.Domain      │ │ PetGuardian.Infrastructure │
│ (POCOs, Invariantes, Enums) │ │ (EF Core Oracle + MongoDB) │
└─────────────────────────────┘ └─────────────────────────┘
```

### Regras de Dependência (DIP):
1. **`Domain` (Núcleo Agnóstico):** POCOs puros, regras de negócio e validações de invariantes com `DomainException`. Totalmente agnóstico de frameworks (sem referências a EF Core, ASP.NET Core ou bibliotecas de banco de dados).
2. **`Application` (Contratos & Orquestração):** DTOs (Records imutáveis com DataAnnotations, `_links` HATEOAS e conversores `FromDomain`/`ToDomain`), Paginação (`PageQuery`, `PagedResult<T>`, `PagedResponse<T>`), contratos de repositório (`IRepository<T>` genérico e contratos específicos) e 16 serviços coesos de negócio (1 por agregado, seguindo KISS/YAGNI como no *Recommenda*).
3. **`Infrastructure` (Persistência Poliglota — EF Core Oracle + MongoDB Atlas):** `PetGuardianContext`, mapeamentos isolados via Fluent API (`IEntityTypeConfiguration<T>`), implementações de repositório com `AsNoTracking()` e paginação segura no banco (`GetPaged`), além da coleção `conteudos_aula` no MongoDB Atlas (`ConteudoAulaRepository` + `MongoIndexesInitializer`).
4. **`API` (Hosting & Apresentação REST):** `Program.cs` limpo com injeção de dependência modularizada em métodos de extensão (`Extensions/`), `GlobalExceptionHandler` (RFC 7807), autenticação/autorização JWT Bearer (`BearerSecurityOperationFilter`, `RequireAdminForWritesConvention`), rastreabilidade de diagnósticos (`X-Correlation-ID`) e observabilidade (Health Checks com `HealthCheckResponseWriter`, OpenTelemetry, Serilog).

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
  * Métodos de negócio semânticos (`Atualizar(...)`, `Concluir()`, `Desmarcar()`, `AtualizarResponsabilidade(...)`) validam as regras e lançam `DomainException`.

---

### 2.2 Camada de Aplicação (`.Application`)

* **DTOs com Records Imutáveis e HATEOAS:**
  * Modelados com `public record ...Request(...)` e `public record ...Response(...)` utilizando DataAnnotations (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Range]`) e propriedade opcional `_links` (`IReadOnlyList<Link>? Links = null`).
  * Métodos utilitários: `FromDomain(Entity entity)` estático e `ToDomain()` de instância.
* **Repositório Genérico vs. Repositórios Específicos:**
  * `IRepository<T>` define operações fundamentais: `GetAll()`, `GetById(id)`, `Find(predicate)`, `FirstOrDefault(predicate)`, `Add(entity)`, `Update(entity)`, `Delete(id)`, `ExistsById(id)`, `Exists(predicate)` e `GetPaged(filter, sortBy, descending, page, pageSize)`.
  * Repositórios específicos (`IPetRepository`, `IUsuarioRepository`, `ITarefaRepository`, `IUsuarioPetRepository`, `ITrilhaRepository`, `IModuloRepository`, `IAulaRepository`, `IHistoricoRepository`, `IConteudoAulaRepository`) adicionam consultas de domínio indexadas e otimizadas.
* **Serviços de Aplicação Coesos (KISS / YAGNI):**
  * Cada agregado possui um único serviço `public sealed class ...Service(...) : I...Service` utilizando **Primary Constructors**, concentrando comandos (`Create`, `Update`, `Delete`) e consultas paginadas (`Search(PageQuery, Filter)`).
  * Injeção de `ILogger<TService>` em todos os serviços de aplicação com registro semântico de mutações (`LogInformation`) e avisos de validação de negócio (`LogWarning`).
  * Sanitização rigorosa: senhas brutas, hashes e dados sensíveis nunca são impressos nos logs nem expostos em ordenação (`CamposNaoOrdenaveis = { "Senha", "Salt" }`).

---

### 2.3 Camada de Infraestrutura (`.Infrastructure`)

* **Configurações Fluent API Segregadas:**
  * Cada entidade possui sua configuração isolada em `Persistence/Configurations/{Entidade}Configuration.cs` implementando `IEntityTypeConfiguration<T>`.
  * O `DbContext` carrega todos os mapeamentos automaticamente via `ApplyConfigurationsFromAssembly`.
* **Performance com `AsNoTracking()` e Paginação em Banco:**
  * Todas as leituras e paginações (`Skip`/`Take`) em `Repository<T>` utilizam `.AsNoTracking()`.
* **Persistência NoSQL (MongoDB Atlas):**
  * `ConteudoAulaRepository` gerencia documentos na coleção `conteudos_aula` com busca unitária (`GetByAulaIdAsync`) e em lote (`GetByAulaIdsAsync`).

---

### 2.4 Camada de Apresentação (`.API`)

* **`Program.cs` Enxuto & Modular:**
  * Delega configurações de DI para métodos de extensão coesos:
    - `AddPetGuardianDbContext(configuration)`
    - `AddPetGuardianMongo(configuration)`
    - `AddPetGuardianRepositories()`
    - `AddPetGuardianApplicationServices()`
    - `AddPetGuardianJwtAuthentication(configuration)`
    - `AddPetGuardianObservability(configuration)`
* **Tratamento Global de Exceções (`GlobalExceptionHandler` / RFC 7807):**
  * Implementa `IExceptionHandler` devolvendo `ProblemDetails` padronizado com `traceId` e `correlationId`.
* **Health Checks Estruturados com `HealthCheckResponseWriter`:**
  * Serialização customizada em JSON exibindo status global, duração e detalhamento de cada probe (`oracle-database`, `mongodb`, `external-service-viacep`).
* **Swagger na Raiz (`RoutePrefix = string.Empty`) + `BearerSecurityOperationFilter`:**
  * Exibe o cadeado de segurança JWT apenas nos endpoints protegidos (omitindo nos marcados com `[AllowAnonymous]`).

---

## 🧪 3. Padrão de Testes Automatizados (AAA & xUnit/Moq)

### 3.1 Convenção de Nomenclatura
```text
MetodoTestado_Cenario_ResultadoEsperado
```
*Exemplos:*
- `Create_RacaInexistente_DeveLancarInvalidOperationException`
- `Search_ComFiltros_DeveGerarPredicadoQueFiltraPorNomeEPorte`
- `Desmarcar_AulaConcluida_DeveMarcarComoNaoConcluida`
- `EscritaTrilhaModuloAula_UsuarioPremium_DeveRetornar403Forbidden`

### 3.2 Estrutura dos Testes
* **Testes Unitários (`.UnitTests` — 212 testes):** 16 classes de Domínio + 17 classes de Aplicação com `Moq` e `TestFixture`.
* **Testes de Integração (`.IntegrationTests` — 146 testes):** Pipeline HTTP completo via `CustomWebApplicationFactory` cobrindo CRUD, Paginação, Ordenação, Filtros, HATEOAS, Autenticação/Autorização RBAC, Observabilidade e Auditoria de 101 operações HTTP (`EndpointAuditTests`).

---

## ✅ 4. Checklist de Conformidade da Solução

- [x] **Domínio:** Entidades herdam de `BaseEntity` com `Guid Id` e construtores privados para EF Core?
- [x] **Segurança:** Senhas utilizam `HashHelper` com BCrypt e Salt criptográfico exclusivo?
- [x] **DRY & KISS:** Construtores de entidades reutilizam métodos `Atualizar(...)` e cada agregado possui 1 único Service coeso?
- [x] **Mutação Completa:** Operações `PUT`/`PATCH` implementadas e validadas em todos os 16 recursos?
- [x] **DTOs & HATEOAS:** Records imutáveis para Request, UpdateRequest, Filter e Response com `_links` e DataAnnotations?
- [x] **Paginação, Ordenação e Filtros:** `PageQuery` e `PagedResponse<T>` ativos nos 7 agregados principais (`Pet`, `Tarefa`, `Usuario`, `Historico`, `Trilha`, `Modulo`, `Aula`)?
- [x] **Persistência Híbrida:** Oracle Database (15 tabelas EF Core) + MongoDB Atlas (`conteudos_aula`) integrados com Health Checks?
- [x] **Observabilidade:** `GlobalExceptionHandler` RFC 7807, `HealthCheckResponseWriter`, Serilog e OpenTelemetry ativos?
- [x] **Apresentação:** Documentação OpenAPI/Swagger ativa na rota raiz (`RoutePrefix = string.Empty`) com `BearerSecurityOperationFilter` e contrato exportado em `docs/swagger.json`?
- [x] **Testes AAA:** Suíte com 100% de sucesso em `dotnet test` (358 testes passando: 212 unitários + 146 de integração)?
