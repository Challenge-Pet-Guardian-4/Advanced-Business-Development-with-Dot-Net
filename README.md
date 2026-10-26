# 🐾 PetGuardian — Plataforma de Cuidado Colaborativo e Saúde Animal

> **Advanced Business Development with .NET** — FIAP (2º Ano ADS / 2TDSPG — Challenge 2026 - 2º Semestre)  
> API RESTful corporativa desenvolvida em **.NET 10** fundamentada em **Clean Architecture (DDD)**, princípios **SOLID**, **KISS**, **YAGNI**, **DRY** e **Clean Code**, persistência poliglota (**Oracle Database + EF Core** e **MongoDB Atlas NoSQL**), **Paginação, Ordenação, Filtros e HATEOAS**, camadas completas de **Monitoramento e Observabilidade** (Health Checks com `HealthCheckResponseWriter`, Logging Estruturado Serilog com `X-Correlation-ID`, OpenTelemetry Distributed Tracing e Métricas Prometheus), segurança de senhas com **BCrypt + Salt criptográfico**, **Autenticação e Autorização JWT Bearer (RFC 7519 / HS256)** nativa e suíte de **358 Testes Automatizados (Padrão AAA)** com xUnit, Moq e WebApplicationFactory.

---

## 🛠️ Tecnologias & Badges

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp&logoColor=white&style=for-the-badge)
![Entity Framework](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=nuget&logoColor=white&style=for-the-badge)
![Oracle Database](https://img.shields.io/badge/Oracle-19c%20%2F%2021c-F80000?logo=oracle&logoColor=white&style=for-the-badge)
![MongoDB Atlas](https://img.shields.io/badge/MongoDB-Atlas%20NoSQL-47A248?logo=mongodb&logoColor=white&style=for-the-badge)
![Serilog](https://img.shields.io/badge/Serilog-Structured%20Logs-000000?logo=serilog&logoColor=white&style=for-the-badge)
![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-Tracing%20%26%20Metrics-4A154B?logo=opentelemetry&logoColor=white&style=for-the-badge)
![JWT](https://img.shields.io/badge/JWT-Bearer%20HS256-orange?style=for-the-badge)
![BCrypt](https://img.shields.io/badge/BCrypt-Security%20%26%20Salt-green?style=for-the-badge)
![xUnit](https://img.shields.io/badge/xUnit-358%20Tests%20Passing-brightgreen?style=for-the-badge)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white&style=for-the-badge)

---

## 👥 Integrantes

| Nome | RM | Turma | GitHub | LinkedIn |
| :--- | :---: | :---: | :--- | :--- |
| **Enzo Okuizumi** | **561432** | 2TDSPG | [EnzoOkuizumiFiap](https://github.com/EnzoOkuizumiFiap) | [Enzo Okuizumi](https://www.linkedin.com/in/enzo-okuizumi-b60292256/) |
| **Gustavo Okada** | **563428** | 2TDSPG | [Gdev3356](https://github.com/Gdev3356) | [Gustavo Okada](https://www.linkedin.com/in/gustavo-okada-53a3b8359/) |
| **Lucas Barros Gouveia** | **566422** | 2TDSPG | [LuzBGouveia](https://github.com/LuzBGouveia) | [Lucas Barros Gouveia](https://www.linkedin.com/in/lucas-barros-gouveia-09b147355/) |
| **Luna de Carvalho Guimarães** | **562290** | 2TDSPG | [lunaguima](https://github.com/lunaguima) | [Luna Guimarães](https://www.linkedin.com/in/luna-guimar%C3%A3es-b0ba82309/) |
| **Milton Marcelino** | **564836** | 2TDSPG | [MiltonMarcelino](https://github.com/MiltonMarcelino) | [Milton Marcelino](https://www.linkedin.com/in/milton-marcelino-250298142) |

---

## 💡 Sobre o Projeto & Funcionalidades

O **PetGuardian** foi concebido para resolver o problema da descentralização do cuidado diário de animais domésticos quando múltiplos cuidadores, famílias ou profissionais estão envolvidos. A plataforma centraliza rotinas, registra o histórico de saúde, monitora tarefas e incentiva a realização de cuidados através de um sistema gamificado centrado no animal.

### 🌟 Destaques Arquiteturais & Negócio
1. **Rede de Cuidado Colaborativo (`UsuarioPet`):** Relação N:N entre Cuidadores (`Usuario`) e `Pet`. Suporta definição de **Responsável Principal** (`ResponPrinc`), envio de convites para novos co-cuidadores (por ID ou E-mail) e salvaguarda do animal (impede desvinculação sem um tutor ativo).
2. **Ciclo Completo de Tarefas (`Tarefa`):** Agendamento e execução de cuidados com pontuação gamificada (`PontosTarefa`). Conclusão de tarefas (`POST /api/tarefa/{id}/concluir`) audita a ação e gera automaticamente registro no histórico do pet (`TAREFA_CONCLUIDA`).
3. **Trilhas, Módulos e Aulas com Persistência Poliglota (`Trilha`, `Modulo`, `Aula` + `ConteudoAula` no MongoDB):** Sistema de capacitação dos tutores com aulas interativas (`PATCH /api/aula/{id}/concluir` e `/desmarcar`), pontuação gamificada no Oracle e conteúdo didático rico em Markdown armazenado na coleção `conteudos_aula` no MongoDB Atlas.
4. **Histórico Consolidado do Pet (`Historico` & `GetHistorico`):** Linha do tempo unificada de eventos clínicos, marcos de saúde, vacinas e tarefas realizadas em ordem cronológica decrescente.
5. **Segurança de Senhas no Domínio (`HashHelper` + Salt):** Criptografia com BCrypt e Salt criptográfico exclusivo (`Guid.NewGuid().ToString("N")`) por usuário, prevenindo persistência de senhas em texto puro.
6. **Resolução Inteligente de Endereço via ViaCEP (`IViaCepService`):** Cadastro automatizado de endereços (`Cep`, `Numero`) resolvendo de forma transparente `Bairro`, `Cidade` e `Estado` com reaproveitamento de entidades no banco.
7. **Paginação, Ordenação, Filtros e HATEOAS:** Consultas paginadas (`PageQuery` + `PagedResponse<T>`) executadas no banco (`Skip`/`Take`) para os 7 agregados principais (`Pet`, `Tarefa`, `Usuario`, `Historico`, `Trilha`, `Modulo` e `Aula`) e hipermídia `_links` em todos os recursos da API.
8. **Eliminação de N+1 Queries & Otimização DRY (Consultas Batch & ToLookup):** Consultas do EF Core executadas com expressões semânticas (`Find`, `FirstOrDefault`) e `.AsNoTracking()`. Na montagem da Rede de Cuidado (`UsuarioPetService.GetRedeCuidadoByUsuarioId`) e na resolução de conteúdos de aulas no MongoDB (`GetByAulaIdsAsync`), os dados são carregados em lote, reduzindo o overhead de $O(N)$ para $O(1)$ roundtrips de banco de dados.

---

## 🏛️ Diagrama de Arquitetura

```text
┌────────────────────┐
│     App Mobile     │
│  (React Native /   │
│   Clientes HTTP)   │
└─────────┬──────────┘
          │ HTTPS + JWT Bearer
          ▼
┌───────────────────────────────────────────────────────────────────────────────┐
│                              Solução .NET 10                                  │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────────┐  │
│  │ PetGuardian.API                                                         │  │
│  │ Controllers REST · Auth JWT · HATEOAS · Middlewares · Health Checks     ├──┼───► [/metrics] ───► Prometheus
│  └──────────────────┬───────────────────────────────────┬──────────────────┘  │
│                     │ chama                             │ DI / Configuração   │
│                     ▼                                   ▼                     │
│  ┌─────────────────────────────────────┐   ┌───────────────────────────────┐  │
│  │ PetGuardian.Application             │◄──┤ PetGuardian.Infrastructure    │  │
│  │ Services · DTOs · Interfaces        │   │ EF Core (Oracle)              │  │
│  │ Regras de Orquestração              │   │ MongoDB.Driver (NoSQL)        │  │
│  └─────────┬───────────────────┬───────┘   │ Repositórios · Fluent API     │  │
│            │ usa               │           └────────┬──────────────┬───────┘  │
│            ▼                   │                    │              │          │
│  ┌─────────────────────────┐   │                    │              │          │
│  │ PetGuardian.Domain      │   │                    │              │          │
│  │ Entidades · Enums       │   │                    │              │          │
│  │ Invariantes · BCrypt    │   │                    │              │          │
│  └─────────────────────────┘   │                    │              │          │
└────────────────────────────────┼────────────────────┼──────────────┼──────────┘
                                 │                    │              │
                  HttpClient     │       EF Core      │              │ MongoDB.Driver
                 (Consulta CEP)  │      (15 Tabelas)  │              │ (conteudos_aula)
                                 ▼                    ▼              ▼
                        ┌─────────────────┐  ┌──────────────┐ ┌────────────────┐
                        │   API ViaCEP    │  │    Oracle    │ │ MongoDB Atlas  │
                        │ (Serviço Ext.)  │  │   Database   │ │ (NoSQL Cloud)  │
                        └─────────────────┘  └──────────────┘ └────────────────┘
```

---

## 🗄️ Modelagem Lógica e Relacional do Banco de Dados

A base é composta por **15 tabelas relacionais normalizadas no Oracle + 1 coleção NoSQL no MongoDB Atlas**:

### Modelo Lógico
![Modelo Lógico](docs/Logical.png)

### Modelo Relacional
![Modelo Relacional](docs/Relational.png)

### 📋 Mapeamento das 15 Tabelas Relacionais Oficiais (Oracle) + Coleção NoSQL (MongoDB Atlas)

| Tabela / Coleção | Entidade / Documento C# (.NET) | Descrição & Propósito no Ecossistema |
| :--- | :--- | :--- |
| `AULA` | `Aula` | Aulas de capacitação dos tutores com pontuação educativa |
| `conteudos_aula` *(MongoDB)* | `ConteudoAulaDocument` | Conteúdo didático rico (Markdown e links de recursos) vinculado 1:1 à Aula |
| `BAIRRO` | `Bairro` | Bairros associados a endereços (resolvidos via CEP) |
| `CIDADE` | `Cidade` | Cidades federadas vinculadas a estados |
| `ENDERECO` | `Endereco` | Endereços cadastrados com resolução automática ViaCEP |
| `ESTADO` | `Estado` | Unidades Federativas (UF) brasileiras |
| `HISTORICO` | `Historico` | Linha do tempo unificada de eventos clínicos e cuidados do pet |
| `MODULO` | `Modulo` | Módulos organizacionais dentro de uma trilha de aprendizado |
| `PET` | `Pet` | Entidade nuclear da plataforma: dados vitais, porte, raça e score |
| `RACA` | `Raca` | Raças caninas e felinas |
| `STATUS` | `Status` | Ciclo de vida de tarefas (`PENDENTE`, `CONCLUIDO`, `EXPIRADO`) |
| `TAREFA` | `Tarefa` | Rotinas diárias de cuidado com pontuação gamificada de bem-estar |
| `TELEFONE` | `Telefone` | Telefones de contato dos usuários |
| `TRILHA` | `Trilha` | Trilhas temáticas de adestramento e saúde preventiva |
| `USUARIO` | `Usuario` | Tutores e cuidadores vinculados a telefone (1:1) e endereço (N:1) |
| `USUARIO_PET` | `UsuarioPet` | Care Circle: Rede N:N de cuidadores com eleição do tutor principal |

---

### 📸 Evidências das Tabelas Populadas no Oracle Database (SQL Developer)

Abaixo constam as capturas de tela das tabelas físicas populadas e estruturadas no banco de dados Oracle oficial da FIAP (`oracle.fiap.com.br:1521/orcl` — Esquema `RM561432`):

#### 1. Usuários, Contatos e Care Circle
* **Tabela `USUARIO`:** Tutores e cuidadores com senhas protegidas via BCrypt e Salt exclusivo:
  ![Tabela USUARIO](docs/usuarios.png)
* **Tabela `TELEFONE`:** Contatos telefônicos associados aos usuários:
  ![Tabela TELEFONE](docs/telefones.png)
* **Tabela `USUARIO_PET`:** Vínculo N:N da rede de cuidado (Care Circle) e eleição de tutor principal:
  ![Tabela USUARIO_PET](docs/usuario_pet.png)

#### 2. Animais, Raças e Rotinas de Cuidado
* **Tabela `PET`:** Animais domésticos, porte, dados vitais e score acumulado:
  ![Tabela PET](docs/pets.png)
* **Tabela `RACA`:** Catálogo de raças caninas e felinas:
  ![Tabela RACA](docs/racas.png)
* **Tabela `TAREFA`:** Rotinas de cuidado agendadas com pontuação gamificada:
  ![Tabela TAREFA](docs/tarefas.png)
* **Tabela `STATUS`:** Ciclo de vida das tarefas (`PENDENTE`, `CONCLUIDO`, `EXPIRADO`):
  ![Tabela STATUS](docs/status.png)

#### 3. Localização e Resolução Automática de Endereços
* **Tabela `ENDERECO`:** Endereços cadastrados e enriquecidos via integração ViaCEP:
  ![Tabela ENDERECO](docs/enderecos.png)
* **Tabela `BAIRRO`:** Bairros normalizados integrados à estrutura territorial:
  ![Tabela BAIRRO](docs/bairros.png)
* **Tabela `CIDADE`:** Cidades federadas vinculadas aos estados:
  ![Tabela CIDADE](docs/cidades.png)
* **Tabela `ESTADO`:** Unidades Federativas (UF) brasileiras:
  ![Tabela ESTADO](docs/estados.png)

#### 4. Trilhas, Módulos e Aulas de Capacitação
* **Tabela `TRILHA`:** Trilhas temáticas de aprendizado para capacitação de tutores:
  ![Tabela TRILHA](docs/trilha.png)
* **Tabela `MODULO`:** Módulos de conteúdo organizados por trilha educativa:
  ![Tabela MODULO](docs/modulo.png)
* **Tabela `AULA`:** Aulas individuais com pontuação de gamificação educativa:
  ![Tabela AULA](docs/aula.png)
* **Coleção `conteudos_aula` (MongoDB Atlas):** Conteúdos didáticos em Markdown, links de recursos e formato pedagógico vinculados à aula:
  ![Conteúdos Aulas](docs/mongoDB.png)

#### 5. Controle de Versão e Migrações de Banco de Dados
* **Tabela `__EFMigrationsHistory`:** Histórico de migrações e sincronização do esquema no Oracle:
  ![Tabela __EFMigrationsHistory](docs/migration.png)

---

## 🏛️ Estrutura de Camadas da Solução (Clean Architecture & SOLID)

A solução segue estritamente a separação em camadas do **Domain-Driven Design (DDD)** e princípios **SOLID / KISS / YAGNI / DRY / Clean Code**:

```text
PetGuardian/
├── PetGuardian.Domain/             # Camada de Domínio: POCOs puros, Entidades, Enums e Invariantes de Negócio
│   ├── Common/                     # BaseEntity (Id Guid)
│   ├── Entities/                   # 15 Entidades relacionais (Pet, Usuario, Tarefa, Trilha, Modulo, Aula, Historico, etc.)
│   ├── Enums/                      # PortePet, SexoPet, RoleUsuario
│   ├── Exceptions/                 # DomainException (regras de validação do domínio)
│   └── Helpers/                    # HashHelper (BCrypt + Salt criptográfico)
│
├── PetGuardian.Application/        # Camada de Aplicação: Casos de Uso, Orquestração, DTOs e Interfaces
│   ├── Common/                     # PageQuery, PagedResult<T>, PagedResponse<T>, Link e PetGuardianActivitySource
│   ├── DTOs/                       # Requests, UpdateRequests, Filters e Responses tipados com _links (Records imutáveis)
│   ├── Repositories/               # Contratos IRepository<T>, IConteudoAulaRepository e interfaces especializadas
│   └── Services/                   # 16 Serviços de Negócio + AuthService, TokenService e ViaCepService
│
├── PetGuardian.Infrastructure/     # Camada de Infraestrutura: EF Core (Oracle) + MongoDB.Driver (NoSQL)
│   ├── Migrations/                 # Migração relacional EF Core (InitialCreate)
│   ├── Mongo/                      # ConteudoAulaDocument, ConteudoAulaRepository, MongoIndexesInitializer
│   └── Persistence/                # PetGuardianContext, Repositórios Especializados e Configurations Fluent API
│
├── PetGuardian.API/                # Camada de Apresentação & Hosting: Controladores REST, Segurança e Observabilidade
│   ├── Controllers/                # 17 Controllers RESTful (16 recursos + AuthController) herdando de ApiControllerBase
│   ├── Exceptions/                 # GlobalExceptionHandler (RFC 7807 ProblemDetails + X-Correlation-ID)
│   ├── Extensions/                 # ServiceCollectionExtensions, MongoExtensions e ObservabilityExtensions
│   ├── Health/                     # HealthCheckResponseWriter (Serialização JSON estruturada)
│   ├── HealthChecks/               # OracleDbHealthCheck, MongoDbHealthCheck e ViaCepHealthCheck
│   ├── Middleware/                 # CorrelationIdMiddleware e RequestMetricsMiddleware
│   └── Security/                   # AuthorizationPolicies, RequireAdminForWritesConvention e BearerSecurityOperationFilter
│
├── PetGuardian.UnitTests/          # Suíte de Testes Unitários xUnit (212 testes — Domínio + Aplicação com Moq, Padrão AAA)
│   ├── Domain/                     # 16 classes testando invariantes, mutações e hashing de senha
│   ├── Application/                # 17 classes testando regras de negócio, paginação e filtros com Moq
│   └── Fixtures/                   # TestFixture e UnitTestCollection
│
└── PetGuardian.IntegrationTests/   # Suíte de Testes de Integração (146 testes — WebApplicationFactory e InMemory DB)
    ├── Endpoints/                  # Testes HTTP ponta a ponta (CRUD, Paginação, HATEOAS, Autorização, Observabilidade)
    └── Fixtures/                   # CustomWebApplicationFactory, InMemoryConteudoAulaRepository e IntegrationTestCollection
```

---

## 📊 Monitoramento e Observabilidade Corporativa

A aplicação implementa os três pilares de observabilidade:

### 1. Health Checks Estruturados (`HealthCheckResponseWriter`)
A API expõe endpoints estruturados para monitoramento de liveness e readiness de orquestradores (Docker, Kubernetes, Azure App Service / ACI):

| Endpoint | Propósito | Componentes Verificados | Status Esperado |
| :--- | :--- | :--- | :---: |
| **`/health`** | Visão geral da saúde da aplicação | API + Oracle Database + MongoDB Atlas + ViaCEP | `200 Healthy` |
| **`/health/ready`** | Prontidão para receber tráfego | Conectividade com Banco Oracle (`CanConnectAsync`), MongoDB (`ping`) e ViaCEP | `200 Healthy` |
| **`/health/live`** | Liveness probe básica | Processo da API ativo | `200 Healthy` |

**Exemplo de Resposta Estruturada em JSON (`GET /health`):**
```json
{
  "status": "Healthy",
  "duration": "00:00:00.0421500",
  "checks": [
    {
      "name": "oracle-database",
      "status": "Healthy",
      "description": "Conexão com o Oracle estabelecida com sucesso.",
      "duration": "00:00:00.0284000",
      "error": null
    },
    {
      "name": "mongodb",
      "status": "Healthy",
      "description": "MongoDB operacional (ping OK).",
      "duration": "00:00:00.0112000",
      "error": null
    },
    {
      "name": "external-service-viacep",
      "status": "Healthy",
      "description": "Serviço externo ViaCEP operacional.",
      "duration": "00:00:00.0137500",
      "error": null
    }
  ]
}
```

**Evidência da Execução do Health Check (`/health`):**
![Health Checks](docs/health.png)

### 2. Logging Estruturado com Serilog
- **Níveis de Log:** `Information`, `Warning` e `Error` configurados via `appsettings.json`.
- **Saídas (Sinks):** 
  - **Console:** Formatação rica para ambiente de desenvolvimento e containers.
  - **Arquivo:** Gravação com rotação diária em `logs/petguardian-.log` (retenção de 14 dias).
- **Correlation ID:** Middleware `CorrelationIdMiddleware` captura ou gera o identificador `X-Correlation-ID`, injeta no `LogContext` e devolve no header de resposta HTTP para rastreabilidade de ponta a ponta.

### 3. Distributed Tracing & Métricas com OpenTelemetry
- **Distributed Tracing Entre Camadas (`ActivitySource`):** Rastreamento distribuído ponta a ponta integrando:
  - **Camada de Apresentação:** Instrumentação automática de requisições HTTP recebidas (`AddAspNetCoreInstrumentation`);
  - **Camada de Aplicação:** `ActivitySource` dedicado (`PetGuardian.Application`) instrumentado nos serviços centrais de negócio (`PetService`, `TarefaService`, `UsuarioService`, `HistoricoService`, `UsuarioPetService`) para rastrear o fluxo entre camadas;
  - **Camada de Integração Externa:** Instrumentação de chamadas de saída HTTP (`AddHttpClientInstrumentation`) para o serviço ViaCEP;
  - **Exportação:** Exportador de traces integrado ao console para auditoria em tempo real.
- **Métricas de Performance (`RequestMetricsMiddleware` & `Meter`):**
  - `petguardian.http.request.duration`: Histograma do tempo de resposta (ms) categorizado por rota e status code.
  - `petguardian.http.request.total`: Contador de volume total de requisições.
  - `petguardian.http.request.errors`: Contador de requisições finalizadas com erro (`status >= 500`).
- **Endpoint de Scraping Prometheus (`/metrics`):** Exposição nativa de métricas no padrão Prometheus via `OpenTelemetry.Exporter.Prometheus.AspNetCore` (`app.UseOpenTelemetryPrometheusScrapingEndpoint()`) para coleta contínua por agentes Prometheus e visualização em dashboards Grafana.

**Evidência do Endpoint de Métricas Prometheus (`/metrics`):**
![Métricas Prometheus com OpenTelemetry](docs/metrics.png)

---

## 🧪 Suíte de Testes Automatizados (358 Testes / Padrão AAA)

A solução conta com **358 testes automatizados** (`212` unitários + `146` de integração) distribuídos entre as camadas de Domínio, Aplicação e Apresentação, seguindo rigorosamente o padrão **AAA (Arrange, Act, Assert)** e convenção de nomenclatura `MetodoTestado_Cenario_ResultadoEsperado`. A arquitetura mantém **simetria 1:1** em todas as camadas (15 Entidades relacionais no Domínio + 1 Documento NoSQL no MongoDB, 16 Serviços de Recursos na Aplicação e 17 Controllers na API incluindo `AuthController`).

### 🧩 Compartilhamento de Contexto com Fixtures e Collection Fixtures:
1. **Testes Unitários (`TestFixture` + `UnitTestCollection`):**
   * **`TestFixture.cs`:** Fábrica determinística centralizada (DRY) contendo métodos geradores para todas as 15 entidades de domínio (`CriarPetValido`, `CriarUsuarioValido`, `CriarEnderecoValido`, etc.).
   * **`UnitTestCollection.cs`:** Define a Collection Fixture decorada com `[CollectionDefinition(Name)]` implementando `ICollectionFixture<TestFixture>`.
   * **Injeção Padronizada:** Todas as classes de testes unitários (`Domain/` e `Application/`) são decoradas com `[Collection(UnitTestCollection.Name)]` e recebem `TestFixture` via injeção de construtor primário.
2. **Testes de Integração (`CustomWebApplicationFactory` + `IntegrationTestCollection`):**
   * **`CustomWebApplicationFactory.cs`:** Especialização de `WebApplicationFactory<Program>` que sobe o pipeline HTTP completo da API em memória, substituindo o Oracle por InMemory Database isolado, o repositório do MongoDB por `InMemoryConteudoAulaRepository` e o ViaCEP por mock determinístico.
   * **`IntegrationTestCollection.cs`:** Define a Collection Fixture decorada com `[CollectionDefinition(Name)]` implementando `ICollectionFixture<CustomWebApplicationFactory>`.

### Organização das Suítes de Teste:
1. **PetGuardian.UnitTests (212 testes):**
   * **Domínio (16 Classes / Validação de Invariantes & Criptografia):**
     * `PetTests`, `UsuarioTests`, `TarefaTests`, `TrilhaTests`, `ModuloTests`, `AulaTests` (incluindo `Concluir` e `Desmarcar`), `HistoricoTests`, `UsuarioPetTests`, `EnderecoTests`, `StatusTests`, `RacaTests`, `TelefoneTests`, `EstadoTests`, `CidadeTests`, `BairroTests`, `HashHelperTests`.
   * **Aplicação (17 Classes / Orquestração de Casos de Uso, Paginação e Filtros com Moq):**
     * `PetServiceTests`, `UsuarioServiceTests`, `TarefaServiceTests`, `UsuarioPetServiceTests`, `EnderecoServiceTests`, `BairroServiceTests`, `CidadeServiceTests`, `EstadoServiceTests`, `StatusServiceTests`, `RacaServiceTests`, `TelefoneServiceTests`, `HistoricoServiceTests`, `TrilhaServiceTests`, `ModuloServiceTests`, `AulaServiceTests`, `ConteudoAulaServiceTests`, `PagedResponseTests`.
2. **PetGuardian.IntegrationTests (146 testes):**
   * **Autenticação, Autorização & Segurança JWT:**
     * `AuthControllerIntegrationTests` e `AuthorizationIntegrationTests` (Login 200 com JWT, 401 credenciais inválidas, `/api/auth/me`, RBAC Admin vs Comum/Premium, bloqueio de escalada de privilégio 403).
   * **Paginação, Ordenação, Filtros & HATEOAS:**
     * `PaginacaoHateoasIntegrationTests` e `TrilhaModuloAulaPaginacaoIntegrationTests` (paginação `page`/`pageSize`, ordenação `sortBy`/`sortDir`, rejeição 400 de `sortBy=Senha` ou campo inexistente, filtros combinados, links HATEOAS `self`/`next`/`prev`/`concluir`/`desmarcar`).
   * **16 Suítes Dedicadas de Recursos (1:1 com a API):**
     * `PetControllerIntegrationTests`, `RacaControllerIntegrationTests`, `UsuarioControllerIntegrationTests`, `TelefoneControllerIntegrationTests`, `EnderecoControllerIntegrationTests`, `BairroControllerIntegrationTests`, `CidadeControllerIntegrationTests`, `EstadoControllerIntegrationTests`, `UsuarioPetControllerIntegrationTests`, `TarefaControllerIntegrationTests`, `StatusControllerIntegrationTests`, `TrilhaControllerIntegrationTests`, `ModuloControllerIntegrationTests`, `AulaControllerIntegrationTests`, `ConteudoAulaIntegrationTests`, `HistoricoControllerIntegrationTests`.
   * **Observabilidade & Auditoria Geral:**
     * `HealthCheckEndpointsTests`, `ObservabilityMiddlewareTests` e `EndpointAuditTests` (varredura de **101 operações HTTP** em todos os controllers, Health Checks e exportação automática de [`docs/swagger.json`](docs/swagger.json)).

### Como Executar os Testes

Para executar toda a suíte de testes (Unitários e de Integração) a partir da pasta `PetGuardian`:

```powershell
# Executar todos os 358 testes da solução com relatório detalhado
dotnet test PetGuardian.sln --logger "console;verbosity=normal"
```

Para executar separadamente por projeto:

```powershell
# Executar os 212 testes unitários (Domínio + Aplicação com Moq e TestFixture)
dotnet test PetGuardian.UnitTests\PetGuardian.UnitTests.csproj

# Executar os 146 testes de integração (WebApplicationFactory e IntegrationTestCollection)
dotnet test PetGuardian.IntegrationTests\PetGuardian.IntegrationTests.csproj

# Executar a auditoria completa de todas as 101 operações HTTP da API
dotnet test PetGuardian.IntegrationTests\PetGuardian.IntegrationTests.csproj --filter "FullyQualifiedName~EndpointAuditTests"
```

### Resumo da Execução:
```text
Passed!  - Failed: 0, Passed: 212, Skipped: 0, Total: 212 - PetGuardian.UnitTests.dll (net10.0)
Passed!  - Failed: 0, Passed: 146, Skipped: 0, Total: 146 - PetGuardian.IntegrationTests.dll (net10.0)
Total Geral: 358 Testes Passando (100% de sucesso)
```

---

## 🚀 Como Executar a Aplicação Localmente

### Opção A: Execução via Docker Compose (Recomendado)

1. Navegue até a pasta da solução:
   ```bash
   cd Advanced-Business-Development-with-Dot-Net/PetGuardian
   ```
2. Suba o container da aplicação e do banco Oracle:
   ```bash
   docker compose up --build -d
   ```
3. Acesse a documentação interativa Swagger UI:
   * **URL:** `http://localhost:8080/`

---

### Opção B: Execução Local via .NET CLI & Conexão com os Bancos de Dados

Para conectar ao banco de dados Oracle oficial da FIAP e ao MongoDB Atlas, configure as credenciais da equipe conforme detalhado na [Seção 22](#22--credenciais-e-configuração-de-ambiente-env--net-user-secrets) (via arquivo `.env` ou `dotnet user-secrets`):

```powershell
# 1. Configurar credenciais via .env (ou via dotnet user-secrets — ver Seção 22)
cp .env.example .env

# 2. Executar a API (.NET 10):
dotnet run --project .\PetGuardian\PetGuardian.API
```

#### Validar Conectividade com o Banco de Dados em Execução
Com a aplicação rodando (porta padrão `5289` no CLI ou `8080` no Docker), consulte a probe de prontidão que valida a conectividade com o Oracle, MongoDB e ViaCEP:
```powershell
# Teste via PowerShell:
Invoke-RestMethod -Uri "http://localhost:5289/health/ready" -Method Get | ConvertTo-Json

# Teste via cURL:
curl -i http://localhost:5289/health/ready
```

---

## 📋 Catálogo Completo de Endpoints REST (CRUD / OpenAPI)

Todas as entidades e agregados contam com rotas padronizadas, suporte completo a atualização com validação de regras de negócio (`PUT`/`PATCH`), deleção (`DELETE`), persistência (`POST`), consultas paginadas e especializadas (`GET`), hipermídia **HATEOAS (`_links`)** e autenticação stateless via **JWT Bearer**.
- **Especificação OpenAPI exportada:** [`docs/swagger.json`](docs/swagger.json)

**Interface Interativa Swagger UI (`/index.html`):**
![Documentação Swagger OpenAPI](docs/swagger.png)

---

### 🔐 0. Autenticação & Sessão JWT (`/api/auth` & `/login`)
*Emissão e validação de tokens JWT (RFC 7519 / HS256) compatível com o aplicativo Mobile.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Autentica o usuário com e-mail/senha e emite token JWT assinado digitalmente (`[AllowAnonymous]`) |
| `POST` | `/login` | Rota alternativa de autenticação para integração direta com clientes Mobile (`[AllowAnonymous]`) |
| `GET` | `/api/auth/me` | Retorna o perfil do usuário logado com `_links` através das claims do token (`[Authorize]`) |

---

### 🐾 1. Pets (`/api/pet`)
*Gerenciamento dos animais domésticos, porte, score de saúde e histórico consolidado.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/pet` | Lista pets com **paginação, ordenação, filtros** (`nome`, `racaId`, `porte`, `sexo`, `castrado`) e **HATEOAS** |
| `GET` | `/api/pet/{id}` | Busca os detalhes de um pet específico por ID (com `_links`) |
| `GET` | `/api/pet/by-raca/{racaId}` | Filtra e lista todos os pets associados a uma raça |
| `GET` | `/api/pet/{id}/historico` | Retorna a linha do tempo consolidada e cronológica de eventos do pet |
| `POST` | `/api/pet` | Cadastra um novo pet |
| `PUT` | `/api/pet/{id}` | Atualiza dados cadastrais e editáveis do pet |
| `DELETE` | `/api/pet/{id}` | Remove o registro de um pet do sistema |

---

### 🏷️ 2. Raças (`/api/raca`)
*Catálogo de raças caninas e felinas cadastradas (escrita exclusiva Admin).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/raca` | Lista todas as raças cadastradas (com `_links`) |
| `GET` | `/api/raca/{id}` | Busca uma raça específica por ID (com `_links`) |
| `POST` | `/api/raca` | Cadastra uma nova raça (`AdminOnly`) |
| `PUT` | `/api/raca/{id}` | Atualiza o nome da raça (`AdminOnly`) |
| `DELETE` | `/api/raca/{id}` | Remove uma raça (`AdminOnly`) |

---

### 👤 3. Usuários & Tutores (`/api/usuario`)
*Gestão de tutores e cuidadores, com segurança criptográfica de senhas (BCrypt + Salt).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/usuario` | Lista usuários com **paginação, ordenação, filtros** (`nome`, `email`, `role`) e **HATEOAS** (`AdminOnly`) |
| `GET` | `/api/usuario/{id}` | Busca um usuário por ID (com `_links`) |
| `GET` | `/api/usuario/by-email` | Busca usuário por endereço de e-mail (`?email=...`) |
| `GET` | `/api/usuario/{id}/score` | Consulta a pontuação gamificada acumulada do usuário |
| `POST` | `/api/usuario` | Cadastra um novo usuário (`[AllowAnonymous]`; criação de perfil `Admin` exige token `Admin`) |
| `PUT` | `/api/usuario/{id}` | Atualiza dados do usuário (próprio usuário ou `Admin`) |
| `DELETE` | `/api/usuario/{id}` | Remove um usuário do sistema (próprio usuário ou `Admin`) |

---

### 📱 4. Telefones (`/api/telefone`)
*Contatos telefônicos vinculados aos tutores.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/telefone` | Lista todos os telefones cadastrados (com `_links`) |
| `GET` | `/api/telefone/{id}` | Busca um telefone por ID (com `_links`) |
| `POST` | `/api/telefone` | Cadastra um novo telefone (`[AllowAnonymous]`) |
| `PUT` | `/api/telefone/{id}` | Atualiza número de telefone e DDD |
| `DELETE` | `/api/telefone/{id}` | Remove um telefone |

---

### 🏠 5. Endereços (`/api/endereco`)
*Endereços com enriquecimento e resolução automática via integração com o ViaCEP.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/endereco` | Lista todos os endereços cadastrados (com `_links`) |
| `GET` | `/api/endereco/{id}` | Busca um endereço por ID (com `_links`) |
| `POST` | `/api/endereco` | Cadastra endereço com resolução automática de CEP via ViaCEP (`[AllowAnonymous]`) |
| `PUT` | `/api/endereco/{id}` | Atualiza número ou CEP com re-resolução automática de localidade |
| `DELETE` | `/api/endereco/{id}` | Remove um endereço |

---

### 📍 6. Bairros (`/api/bairro`)
*Bairros normalizados integrados à estrutura de endereços (escrita exclusiva Admin).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/bairro` | Lista todos os bairros (com `_links`) |
| `GET` | `/api/bairro/{id}` | Busca bairro por ID (com `_links`) |
| `GET` | `/api/bairro/by-cidade/{cidadeId}` | Lista bairros pertencentes a uma cidade |
| `POST` | `/api/bairro` | Cadastra um novo bairro (`AdminOnly`) |
| `PUT` | `/api/bairro/{id}` | Atualiza o nome do bairro (`AdminOnly`) |
| `DELETE` | `/api/bairro/{id}` | Remove um bairro (`AdminOnly`) |

---

### 🏙️ 7. Cidades (`/api/cidade`)
*Cidades federadas cadastradas e vinculadas aos estados (escrita exclusiva Admin).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/cidade` | Lista todas as cidades (com `_links`) |
| `GET` | `/api/cidade/{id}` | Busca cidade por ID (com `_links`) |
| `GET` | `/api/cidade/by-estado/{estadoId}` | Lista cidades pertencentes a um estado |
| `POST` | `/api/cidade` | Cadastra uma nova cidade vinculada a um estado (`AdminOnly`) |
| `PUT` | `/api/cidade/{id}` | Atualiza o nome da cidade (`AdminOnly`) |
| `DELETE` | `/api/cidade/{id}` | Remove uma cidade (`AdminOnly`) |

---

### 🗺️ 8. Estados (`/api/estado`)
*Unidades Federativas (UF) (escrita exclusiva Admin).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/estado` | Lista todos os estados cadastrados (com `_links`) |
| `GET` | `/api/estado/{id}` | Busca estado por ID (com `_links`) |
| `POST` | `/api/estado` | Cadastra um novo estado (`AdminOnly`) |
| `PUT` | `/api/estado/{id}` | Atualiza dados do estado (`AdminOnly`) |
| `DELETE` | `/api/estado/{id}` | Remove um estado (`AdminOnly`) |

---

### 🍃 9. Conteúdo Didático das Aulas — NoSQL MongoDB (`/api/aula/{aulaId}/conteudo`)
*Conteúdo rico em Markdown e links de recursos armazenados na coleção `conteudos_aula` no MongoDB Atlas.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/aula/{aulaId}/conteudo` | Obtém o conteúdo didático rico (Markdown + links + HATEOAS `_links`) da aula |
| `POST` | `/api/aula/{aulaId}/conteudo` | Cadastra o conteúdo da aula no MongoDB Atlas (`AdminOnly`) |
| `PUT` | `/api/aula/{aulaId}/conteudo` | Atualiza o conteúdo da aula no MongoDB Atlas (`AdminOnly`) |
| `DELETE` | `/api/aula/{aulaId}/conteudo` | Remove o conteúdo da aula no MongoDB Atlas (`AdminOnly`) |

---

### 🤝 10. Rede de Cuidado Colaborativo (`/api/usuariopet`)
*Care Circle: Relação N:N entre cuidadores e pets, convites e salvaguarda do animal.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/usuariopet` | Lista todos os vínculos de cuidadores cadastrados (com `_links`) |
| `GET` | `/api/usuariopet/by-usuario/{usuarioId}` | Lista todos os vínculos sob o cuidado de um tutor |
| `GET` | `/api/usuariopet/by-pet/{petId}` | Lista todos os cuidadores vinculados a um pet |
| `GET` | `/api/usuariopet/rede-cuidado/{usuarioId}` | Retorna a árvore completa de cuidado colaborativo otimizada com batch queries |
| `POST` | `/api/usuariopet` | Vincula um cuidador a um pet |
| `POST` | `/api/usuariopet/invite/by-usuario` | Convite de co-cuidador por ID de usuário (ação exclusiva do tutor principal) |
| `POST` | `/api/usuariopet/invite/by-email` | Convite de co-cuidador via endereço de e-mail |
| `PUT` | `/api/usuariopet/{usuarioId}/{petId}` | Atualiza status do vínculo e permite alternância do Responsável Principal |
| `DELETE` | `/api/usuariopet/{usuarioId}/{petId}` | Desvincula cuidador (com salvaguarda: impede deixar o animal sem tutor principal) |

---

### ✅ 11. Tarefas & Rotinas de Cuidado (`/api/tarefa`)
*Agendamento, rotinas diárias e conclusão gamificada de tarefas.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/tarefa` | Lista tarefas com **paginação, ordenação, filtros** (`titulo`, `petId`, `usuarioId`, `statusId`, `concluida`, `prazoDe`, `prazoAte`) e **HATEOAS** |
| `GET` | `/api/tarefa/{id}` | Busca detalhes de uma tarefa por ID (com `_links` contextuais) |
| `GET` | `/api/tarefa/by-pet/{petId}` | Lista todas as tarefas associadas a um pet específico |
| `GET` | `/api/tarefa/by-usuario/{usuarioId}` | Lista tarefas atribuídas a um cuidador/usuário |
| `GET` | `/api/tarefa/by-status/{statusId}` | Filtra e lista tarefas por status |
| `POST` | `/api/tarefa` | Cria uma nova rotina/tarefa de cuidado para o pet |
| `PUT` | `/api/tarefa/{id}` | Atualiza título, descrição, prazo ou pontuação de uma tarefa não concluída |
| `POST` | `/api/tarefa/{id}/concluir` | Conclui tarefa, credita pontuação ao usuário e gera marco no histórico do pet |
| `DELETE` | `/api/tarefa/{id}` | Remove uma tarefa agendada |

---

### 🚦 12. Status de Tarefas (`/api/status`)
*Estados padronizados do ciclo de vida das rotinas (`PENDENTE`, `CONCLUIDO`, `EXPIRADO`) (escrita exclusiva Admin).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/status` | Lista os status disponíveis no sistema (com `_links`) |
| `GET` | `/api/status/{id}` | Busca status por ID (com `_links`) |
| `POST` | `/api/status` | Cadastra um novo status (`AdminOnly`) |
| `PUT` | `/api/status/{id}` | Atualiza a descrição do status (`AdminOnly`) |
| `DELETE` | `/api/status/{id}` | Remove um status (`AdminOnly`) |

---

### 🎓 13. Trilhas de Aprendizado (`/api/trilha`)
*Capacitação continuada dos tutores com conteúdos educativos (escrita exclusiva Admin).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/trilha` | Lista trilhas com **paginação, ordenação, filtros** (`nome`, `petId`) e **HATEOAS** |
| `GET` | `/api/trilha/{id}` | Busca uma trilha por ID (com `_links`) |
| `GET` | `/api/trilha/by-pet/{petId}` | Lista trilhas recomendadas para um pet específico |
| `POST` | `/api/trilha` | Cria uma nova trilha de aprendizado (`AdminOnly`) |
| `PUT` | `/api/trilha/{id}` | Atualiza dados da trilha (`AdminOnly`) |
| `DELETE` | `/api/trilha/{id}` | Remove uma trilha (`AdminOnly`) |

---

### 📚 14. Módulos de Aprendizado (`/api/modulo`)
*Divisões temáticas e estruturais dentro de uma trilha (escrita exclusiva Admin).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/modulo` | Lista módulos com **paginação, ordenação, filtros** (`nome`, `trilhaId`) e **HATEOAS** |
| `GET` | `/api/modulo/{id}` | Busca módulo por ID (com `_links`) |
| `GET` | `/api/modulo/by-trilha/{trilhaId}` | Lista módulos pertencentes a uma trilha |
| `POST` | `/api/modulo` | Cadastra um novo módulo vinculado a uma trilha (`AdminOnly`) |
| `PUT` | `/api/modulo/{id}` | Atualiza dados do módulo (`AdminOnly`) |
| `DELETE` | `/api/modulo/{id}` | Remove um módulo (`AdminOnly`) |

---

### 📖 15. Aulas de Capacitação (`/api/aula`)
*Aulas individuais com pontuação educativa para o tutor (enriquecidas com Markdown do MongoDB Atlas).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/aula` | Lista aulas com **paginação, ordenação, filtros** (`nome`, `moduloId`, `dificuldade`, `concluida`), `conteudo` do MongoDB e **HATEOAS** |
| `GET` | `/api/aula/{id}` | Busca aula por ID (com `conteudo` resolvido no MongoDB e `_links` contextuais) |
| `GET` | `/api/aula/by-modulo/{moduloId}` | Lista aulas de um módulo (com `conteudo` resolvido em lote no MongoDB) |
| `POST` | `/api/aula` | Cadastra uma nova aula associada a um módulo (`AdminOnly`) |
| `PUT` | `/api/aula/{id}` | Atualiza dados estruturados da aula (`AdminOnly`) |
| `PATCH` | `/api/aula/{id}/concluir` | Marca a aula como concluída (qualquer usuário autenticado) |
| `PATCH` | `/api/aula/{id}/desmarcar` | Desmarca a conclusão da aula (qualquer usuário autenticado) |
| `DELETE` | `/api/aula/{id}` | Remove a aula no Oracle e exclui em cascata o conteúdo em `conteudos_aula` no MongoDB (`AdminOnly`) |

---

### 📜 16. Histórico & Linha do Tempo (`/api/historico`)
*Registro histórico unificado de intervenções, vacinas, tarefas e marcos de saúde do animal.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/historico` | Lista registros históricos com **paginação, ordenação, filtros** (`petId`, `tipoHist`, `dataDe`, `dataAte`) e **HATEOAS** |
| `GET` | `/api/historico/{id}` | Busca registro histórico específico por ID (com `_links`) |
| `GET` | `/api/historico/by-pet/{petId}` | Lista os registros históricos de um pet específico |
| `POST` | `/api/historico` | Registra um novo marco clínico ou histórico para o pet |
| `PUT` | `/api/historico/{id}` | Atualiza detalhes de um registro histórico |
| `DELETE` | `/api/historico/{id}` | Remove um registro histórico |

---

### 🩺 17. Health Checks & Observabilidade (`/health`)
*Monitoramento de integridade e liveness/readiness probes para orquestradores.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/health` | Visão completa da saúde da API, banco Oracle, MongoDB Atlas e serviço ViaCEP em JSON estruturado |
| `GET` | `/health/ready` | Readiness probe: verifica conectividade com Oracle (`CanConnectAsync`), MongoDB Atlas e ViaCEP |
| `GET` | `/health/live` | Liveness probe: verifica se o processo da aplicação ASP.NET Core está ativo |

---

## 18. 🔐 Autenticação e Autorização (JWT + RBAC)

Toda rota exige `Authorization: Bearer {token}`, exceto `POST /api/auth/login`, `POST /login`, `POST /api/usuario` (auto-cadastro), `POST /api/endereco` (auto-cadastro), `POST /api/telefone` (auto-cadastro) e `/health*`. No Swagger UI, o filtro `BearerSecurityOperationFilter` exibe o cadeado de segurança exclusivamente nas operações protegidas.

| Perfil | Permissões |
| :--- | :--- |
| **Comum / Premium** | Consultar recursos; criar/editar seus pets, tarefas, históricos e vínculos (`UsuarioPet`); concluir/desmarcar aulas (`PATCH /api/aula/{id}/concluir` e `/desmarcar`); editar/excluir **a si mesmo** |
| **Admin** | Acesso total, incluindo listar todos os usuários (`GET /api/usuario`), gerenciar tabelas de referência (`Estado`, `Cidade`, `Bairro`, `Raca`, `Status`) e gerenciar o catálogo educativo (`Trilha`, `Modulo`, `Aula` e `conteudos_aula` no MongoDB) |

O perfil `Admin` só pode ser atribuído por outro Admin (proteção anti-escalada de privilégio).

---

## 19. 📄 Paginação, Ordenação e Filtros

Implementada nos 7 agregados de negócio (`GET /api/pet`, `/api/tarefa`, `/api/usuario`, `/api/historico`, `/api/trilha`, `/api/modulo` e `/api/aula`):

- **Parâmetros de página e ordenação (`PageQuery`):** `?page=1&pageSize=10&sortBy=Nome&sortDir=desc` (`pageSize` limitado a `1..100`).
- **Filtros dedicados por recurso:** `PetFilter`, `TarefaFilter`, `UsuarioFilter`, `HistoricoFilter`, `TrilhaFilter`, `ModuloFilter` e `AulaFilter`.
- **Segurança na ordenação:** `Repository<T>.GetPaged` valida `sortBy` contra os metadados do EF Core e bloqueia campos sensíveis (`Senha`, `Salt`), retornando `400 Bad Request` via `GlobalExceptionHandler` para campos inválidos.

---

## 20. 🔗 HATEOAS

Todas as respostas de recursos incluem `_links` (`self`, `update`, `delete`, links relacionais e transições de estado como `concluir`/`desmarcar`), e as listagens paginadas trazem links de navegação de página (`self`, `first`, `prev`, `next`, `last`):

```json
{
  "items": [
    {
      "id": "…",
      "nome": "Thor",
      "_links": [
        { "href": "http://…/api/pet/{id}", "rel": "self", "method": "GET" },
        { "href": "http://…/api/pet/{id}/historico", "rel": "historico", "method": "GET" }
      ]
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 3,
  "totalPages": 1,
  "_links": [
    { "href": "http://…/api/pet?page=1&pageSize=10", "rel": "self", "method": "GET" },
    { "href": "http://…/api/pet?page=1&pageSize=10", "rel": "first", "method": "GET" },
    { "href": "http://…/api/pet?page=1&pageSize=10", "rel": "last", "method": "GET" }
  ]
}
```

---

## 21. 🍃 MongoDB Atlas (Persistência Poliglota)

![Coleção conteudos_aula no MongoDB Atlas](docs/mongoDB.png)

- **Oracle** é a fonte relacional transacional das 15 entidades (`TRILHA`, `MODULO`, `AULA`, `USUARIO`, `PET`, etc.).
- **MongoDB Atlas** armazena exclusivamente a coleção **`conteudos_aula`** (1:1 com `AULA` via `aula_id`), contendo `tipo_conteudo`, `corpo_markdown`, `links_recursos` e `atualizado_em`, com índice único em `aula_id` garantido automaticamente por `MongoIndexesInitializer`.
- Endpoints RESTful com HATEOAS em `/api/aula/{aulaId}/conteudo` (`GET`, `POST`, `PUT`, `DELETE`).
- As consultas de `Aula` (`/api/aula`, `/api/aula/{id}`, `/api/aula/by-modulo/{moduloId}`) enriquecem automaticamente o campo `conteudo` a partir do MongoDB Atlas (em lote ou individualmente) e o `DELETE /api/aula/{id}` remove em cascata o documento correspondente.
- Health check `mongodb` integrado em `/health` e `/health/ready`.

---

## 22. 🔐 Credenciais e Configuração de Ambiente (.env & .NET User Secrets)

Para executar a aplicação com conectividade total aos bancos corporativos (**Oracle Database 19c** e **MongoDB Atlas NoSQL**), a aplicação suporta nativamente três formas padronizadas de configuração:

### Opção 1: Via Arquivo `.env` (Recomendado e Automático)
Crie um arquivo `.env` na raiz da solução ou dentro de `PetGuardian/PetGuardian.API/` (baseado no template [`.env.example`](.env.example)) com as credenciais oficiais da avaliação:

```env
# ==============================================================================
# Credenciais Oficiais PetGuardian — Challenge 2026 (FIAP)
# ==============================================================================

# 1. Oracle Database 19c (FIAP)
ORACLE_APP_USER=rm566422
ORACLE_APP_PASSWORD=201006
ORACLE_DATA_SOURCE=oracle.fiap.com.br:1521/ORCL

# 2. MongoDB Atlas (Cloud NoSQL — Coleção conteudos_aula)
MONGODB_URI=mongodb+srv://enzookuizumifiap_db_user:PAEdhlPTE5QRMzjC@petguardian.agoderb.mongodb.net/?retryWrites=true&w=majority&appName=PetGuardian
MONGODB_DATABASE=petguardian

# 3. JWT Secret Key (RFC 7519 — HS256, mínimo 32 caracteres)
JWT_SECRET_KEY=PetGuardian-Super-Secret-Jwt-Key-Sprint4-Fiap-2026!
```

> **Nota:** O `Program.cs` carrega o arquivo `.env` automaticamente no startup da aplicação através do pacote oficial [`dotenv.net`](https://github.com/bolorundurowb/dotenv.net) (`DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 6))`), que faz a busca dinâmica subindo a árvore de diretórios.

---

### Opção 2: Via .NET User Secrets (`dotnet user-secrets`)
Para gerenciar as credenciais no perfil local do desenvolvedor de forma segura via CLI do .NET (sem arquivos locais), execute a partir da raiz da solução:

```powershell
# 1. Connection string do Oracle Database 19c (FIAP):
dotnet user-secrets set "ConnectionStrings:PetGuardianOracle" "User Id=rm566422;Password=201006;Data Source=oracle.fiap.com.br:1521/ORCL;" --project .\PetGuardian\PetGuardian.API

# 2. Conexão e Banco do MongoDB Atlas (NoSQL):
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb+srv://enzookuizumifiap_db_user:PAEdhlPTE5QRMzjC@petguardian.agoderb.mongodb.net/?retryWrites=true&w=majority&appName=PetGuardian" --project .\PetGuardian\PetGuardian.API
dotnet user-secrets set "MongoDb:DatabaseName" "petguardian" --project .\PetGuardian\PetGuardian.API

# 3. Chave Secreta de Assinatura JWT (HS256):
dotnet user-secrets set "Jwt:SecretKey" "PetGuardian-Super-Secret-Jwt-Key-Sprint4-Fiap-2026!" --project .\PetGuardian\PetGuardian.API
```

---

### Opção 3: Via Variáveis de Ambiente da Sessão (PowerShell / Bash)

**PowerShell:**
```powershell
$env:ORACLE_APP_USER = "rm566422"
$env:ORACLE_APP_PASSWORD = "201006"
$env:ORACLE_DATA_SOURCE = "oracle.fiap.com.br:1521/ORCL"
$env:MONGODB_URI = "mongodb+srv://enzookuizumifiap_db_user:PAEdhlPTE5QRMzjC@petguardian.agoderb.mongodb.net/?retryWrites=true&w=majority&appName=PetGuardian"
$env:MONGODB_DATABASE = "petguardian"
$env:JWT_SECRET_KEY = "PetGuardian-Super-Secret-Jwt-Key-Sprint4-Fiap-2026!"

dotnet run --project .\PetGuardian\PetGuardian.API
```

**Bash / Linux / macOS:**
```bash
export ORACLE_APP_USER="rm566422"
export ORACLE_APP_PASSWORD="201006"
export ORACLE_DATA_SOURCE="oracle.fiap.com.br:1521/ORCL"
export MONGODB_URI="mongodb+srv://enzookuizumifiap_db_user:PAEdhlPTE5QRMzjC@petguardian.agoderb.mongodb.net/?retryWrites=true&w=majority&appName=PetGuardian"
export MONGODB_DATABASE="petguardian"
export JWT_SECRET_KEY="PetGuardian-Super-Secret-Jwt-Key-Sprint4-Fiap-2026!"

dotnet run --project ./PetGuardian/PetGuardian.API
```
