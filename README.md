# 🐾 PetGuardian — Plataforma de Cuidado Colaborativo e Saúde Animal

> **Advanced Business Development with .NET** — FIAP (2º Ano ADS / 2TDSPG — Challenge 2026 - 2º Semestre)  
> API RESTful corporativa desenvolvida em **.NET 10** fundamentada em **Clean Architecture (DDD)**, princípios **SOLID**, **DRY** e **Clean Code**, camadas completas de **Monitoramento e Observabilidade** (Health Checks com `HealthCheckResponseWriter`, Logging Estruturado Serilog com `X-Correlation-ID`, OpenTelemetry Distributed Tracing e Métricas), segurança de senhas com **BCrypt + Salt criptográfico**, **Autenticação JWT Bearer (RFC 7519 / HS256)** nativa e suíte de **301 Testes Automatizados (Padrão AAA)** com xUnit, Moq e WebApplicationFactory.

---

## 🛠️ Tecnologias & Badges

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white&style=for-the-badge)
![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp&logoColor=white&style=for-the-badge)
![Entity Framework](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=nuget&logoColor=white&style=for-the-badge)
![Oracle Database](https://img.shields.io/badge/Oracle-19c%20%2F%2021c-F80000?logo=oracle&logoColor=white&style=for-the-badge)
![Serilog](https://img.shields.io/badge/Serilog-Structured%20Logs-000000?logo=serilog&logoColor=white&style=for-the-badge)
![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-Tracing%20%26%20Metrics-4A154B?logo=opentelemetry&logoColor=white&style=for-the-badge)
![JWT](https://img.shields.io/badge/JWT-Bearer%20HS256-orange?style=for-the-badge)
![BCrypt](https://img.shields.io/badge/BCrypt-Security%20%26%20Salt-green?style=for-the-badge)
![xUnit](https://img.shields.io/badge/xUnit-301%20Tests%20Passing-brightgreen?style=for-the-badge)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white&style=for-the-badge)

---

## 👥 Integrantes do Grupo (2TDSPG)

| Nome | RM | GitHub | LinkedIn                                                                            |
| :--- | :---: | :--- |:------------------------------------------------------------------------------------|
| **Enzo Okuizumi** | **561432** | [EnzoOkuizumiFiap](https://github.com/EnzoOkuizumiFiap) | [Enzo Okuizumi](https://www.linkedin.com/in/enzo-okuizumi-b60292256/)               |
| **Gustavo Okada** | **563428** | [Gdev3356](https://github.com/Gdev3356) | [Gustavo Okada](https://www.linkedin.com/in/gustavo-okada-53a3b8359/)               |
| **Lucas Barros Gouveia** | **566422** | [LuzBGouveia](https://github.com/LuzBGouveia) | [Lucas Barros Gouveia](https://www.linkedin.com/in/lucas-barros-gouveia-09b147355/) |
| **Luna de Carvalho Guimarães** | **562290** | [lunaguima](https://github.com/lunaguima) | [Luna Guimarães](https://www.linkedin.com/in/luna-guimar%C3%A3es-b0ba82309/)        |
| **Milton Marcelino** | **564836** | [MiltonMarcelino](https://github.com/MiltonMarcelino) | [Milton Marcelino](http://linkedin.com/in/milton-marcelino-250298142)               |

---

## 💡 Sobre o Projeto & Funcionalidades

O **PetGuardian** foi concebido para resolver o problema da descentralização do cuidado diário de animais domésticos quando múltiplos cuidadores, famílias ou profissionais estão envolvidos. A plataforma centraliza rotinas, registra o histórico de saúde, monitora tarefas e incentiva a realização de cuidados através de um sistema gamificado centrado no animal.

### 🌟 Destaques Arquiteturais & Negócio
1. **Rede de Cuidado Colaborativo (`UsuarioPet`):** Relação N:N entre Cuidadores (`Usuario`) e `Pet`. Suporta definição de **Responsável Principal** (`ResponPrinc`), envio de convites para novos co-cuidadores (por ID ou E-mail) e salvaguarda do animal (impede desvinculação sem um tutor ativo).
2. **Ciclo Completo de Tarefas (`Tarefa`):** Agendamento e execução de cuidados com pontuação gamificada (`PontosTarefa`). Conclusão de tarefas (`POST /api/tarefa/{id}/concluir`) audita a ação e gera automaticamente registro no histórico do pet (`TAREFA_CONCLUIDA`).
3. **Trilhas e Módulos de Aprendizado (`Trilha`, `Modulo`, `Aula`):** Sistema de capacitação dos tutores com aulas interativas, pontuação e gamificação.
4. **Histórico Consolidado do Pet (`Historico` & `GetHistorico`):** Linha do tempo unificada de eventos clínicos, marcos de saúde, vacinas e tarefas realizadas em ordem cronológica decrescente.
5. **Segurança de Senhas no Domínio (`HashHelper` + Salt):** Criptografia com BCrypt e Salt criptográfico exclusivo (`Guid.NewGuid().ToString("N")`) por usuário, prevenindo persistência de senhas em texto puro.
6. **Resolução Inteligente de Endereço via ViaCEP (`IViaCepService`):** Cadastro automatizado de endereços (`Cep`, `Numero`) resolvendo de forma transparente `Bairro`, `Cidade` e `Estado` com reaproveitamento de entidades no banco.
7. **Auditoria Completa de Atualização (CRUD com PUT):** Todos os 16 agregados e entidades implementam operações de atualização com validação de invariantes no domínio (`Atualizar(...)`) e endpoints `[HttpPut]` padronizados.
8. **Eliminação de N+1 Queries & Otimização DRY (Consultas Batch & ToLookup):** Consultas do EF Core executadas com expressões semânticas (`Find`, `FirstOrDefault`) e `.AsNoTracking()`. Na montagem da Rede de Cuidado (`UsuarioPetService.GetRedeCuidadoByUsuarioId`), as tarefas, históricos e vínculos de todos os pets são carregados em consultas batch prévias indexadas (`GetByPetIds`, `Find`) e organizados em memória com `ToLookup`, reduzindo o overhead de $O(N)$ para $O(1)$ roundtrips de banco de dados.

---

## 🗄️ Modelagem Lógica e Relacional do Banco de Dados

A base é composta exatamente por **16 tabelas oficiais normalizadas**:

### Modelo Lógico
![Modelo Lógico](docs/Logical.png)

### Modelo Relacional
![Modelo Relacional](docs/Relational.png)

### 📋 Mapeamento das 16 Tabelas Oficiais e Entidades

| Tabela Oracle | Entidade C# (.NET) | Descrição & Propósito no Ecossistema |
| :--- | :--- | :--- |
| `AULA` | `Aula` | Aulas de capacitação dos tutores com pontuação educativa |
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
| `USUARIO` | `Usuario` | Tutores e cuidadores (senhas protegidas com BCrypt + Salt exclusivo) |
| `USUARIO_ENDERECO` | `UsuarioEndereco` | Relação associativa entre usuários e endereços |
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
* **Tabela `USUARIO_ENDERECO`:** Vínculo associativo entre usuários e endereços cadastrados:
  ![Tabela USUARIO_ENDERECO](docs/usuario_endereco.png)

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

#### 5. Controle de Versão e Migrações de Banco de Dados
* **Tabela `__EFMigrationsHistory`:** Histórico de migrações e sincronização do esquema no Oracle:
  ![Tabela __EFMigrationsHistory](docs/migration.png)

---

## 🏛️ Arquitetura da Solução (Clean Architecture & SOLID)

A solução segue estritamente a separação em camadas do **Domain-Driven Design (DDD)** e princípios **SOLID / DRY / Clean Code**:

```text
PetGuardian/
├── PetGuardian.Domain/             # Camada de Domínio: POCOs puros, Entidades, Enums e Invariantes de Negócio
│   ├── Common/                     # BaseEntity (Id Guid)
│   ├── Entities/                   # Pet, Usuario, Tarefa, Trilha, Modulo, Aula, Historico, Endereco, Bairro, Cidade, Estado, etc.
│   ├── Enums/                      # PortePet, SexoPet, RoleUsuario, StatusTarefa
│   ├── Exceptions/                 # DomainException (regras de validação do domínio)
│   └── Helpers/                    # HashHelper (BCrypt + Salt criptográfico)
│
├── PetGuardian.Application/        # Camada de Aplicação: Casos de Uso, Orquestração, DTOs e Interfaces
│   ├── Common/                     # PetGuardianActivitySource (Distributed Tracing OpenTelemetry inter-camadas)
│   ├── DTOs/                       # Requests, UpdateRequests e Responses tipados (Records imutáveis)
│   ├── Repositories/               # Contratos IRepository<T> e interfaces especializadas
│   └── Services/                   # Interfaces e Implementações de Serviços (PetService, TarefaService, UsuarioPetService, etc.)
│
├── PetGuardian.Infrastructure/     # Camada de Infraestrutura: EF Core, Mapeamentos Oracle e Repositórios Concretos
│   └── Persistence/                # PetGuardianContext, Repositórios Especializados e Mapeamentos Fluent API
│
├── PetGuardian.API/                # Camada de Apresentação & Hosting: Controladores REST, Middlewares e Observabilidade
│   ├── Controllers/                # 16 Controllers RESTful com suporte a CRUD completo (GET, POST, PUT, DELETE)
│   ├── Exceptions/                 # GlobalExceptionHandler (RFC 7807 ProblemDetails + X-Correlation-ID)
│   ├── Extensions/                 # ServiceCollectionExtensions e ObservabilityExtensions
│   ├── Health/                     # HealthCheckResponseWriter (Serialização JSON estruturada)
│   ├── HealthChecks/               # OracleDbHealthCheck e ViaCepHealthCheck
│   └── Middleware/                 # CorrelationIdMiddleware e RequestMetricsMiddleware
│
├── PetGuardian.UnitTests/          # Suíte de Testes Unitários xUnit (Domínio + Aplicação com Moq, Padrão AAA)
│   ├── Domain/                     # Testes de invariantes, mutações e hashing de senha
│   ├── Application/                # Testes de regras de negócio com mocks Moq (Times.Once / Times.Never)
│   └── Fixtures/                   # Fixtures compartilhadas e factories de dados
│
└── PetGuardian.IntegrationTests/   # Suíte de Testes de Integração com WebApplicationFactory e InMemory DB
    ├── Endpoints/                  # Testes de requisição HTTP ponta a ponta (CRUD, PUT, Validação 400, 404, 201)
    └── Fixtures/                   # CustomWebApplicationFactory e IntegrationTestCollection
```

---

## 📊 Monitoramento e Observabilidade Corporativa

A aplicação implementa os três pilares de observabilidade:

### 1. Health Checks Estruturados (`HealthCheckResponseWriter`)
A API expõe endpoints estruturados para monitoramento de liveness e readiness de orquestradores (Docker, Kubernetes, Azure App Service / ACI):

| Endpoint | Propósito | Componentes Verificados | Status Esperado |
| :--- | :--- | :--- | :---: |
| **`/health`** | Visão geral da saúde da aplicação | API + Oracle Database + ViaCEP | `200 Healthy` |
| **`/health/ready`** | Prontidão para receber tráfego | Conectividade com Banco Oracle (`CanConnectAsync`) e API externa ViaCEP | `200 Healthy` |
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
  - **Camada de Aplicação:** `ActivitySource` dedicado (`PetGuardian.Application`) instrumentado nos serviços centrais de negócio (`PetService`, `TarefaService`, `UsuarioPetService`) para rastrear o fluxo entre camadas;
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

## 🧪 Suíte de Testes Automatizados (301 Testes / Padrão AAA)

A solução conta com **301 testes automatizados** distribuídos entre as camadas de Domínio, Aplicação e Apresentação, seguindo rigorosamente o padrão **AAA (Arrange, Act, Assert)** e convenção de nomenclatura `MetodoTestado_Cenario_ResultadoEsperado`. A arquitetura estabelece uma **simetria 1:1 perfeita** em todas as camadas (16 Entidades no Domínio, 16 Serviços na Aplicação e 17 Controllers na API com o módulo de autenticação).

### 🧩 Compartilhamento de Contexto com Fixtures e Collection Fixtures (15 pts):
A solução atende ao requisito formal de organização e compartilhamento de contexto via xUnit:
1. **Testes Unitários (`TestFixture` + `UnitTestCollection`):**
   * **`TestFixture.cs`:** Fábrica determinística centralizada (DRY) contendo métodos geradores para todas as 16 entidades de domínio (`CriarPetValido`, `CriarUsuarioValido`, `CriarEnderecoValido`, `CriarUsuarioEnderecoValido`, etc.).
   * **`UnitTestCollection.cs`:** Define a Collection Fixture decorada com `[CollectionDefinition(Name)]` implementando `ICollectionFixture<TestFixture>`.
   * **Injeção Padronizada:** 100% das 33 classes de testes unitários (`Domain/` e `Application/`) são decoradas com `[Collection(UnitTestCollection.Name)]` e recebem `TestFixture` via injeção de construtor primário para geração de entidades de teste.
2. **Testes de Integração (`CustomWebApplicationFactory` + `IntegrationTestCollection`):**
   * **`CustomWebApplicationFactory.cs`:** Especialização de `WebApplicationFactory<Program>` que sobe o pipeline HTTP completo da API em memória, substituindo o Oracle por InMemory Database isolado e o ViaCEP por mock determinístico.
   * **`IntegrationTestCollection.cs`:** Define a Collection Fixture decorada com `[CollectionDefinition(Name)]` implementando `ICollectionFixture<CustomWebApplicationFactory>`.
   * **Ciclo de Vida Compartilhado:** Todas as 20 classes de testes de integração compartilham o mesmo contexto HTTP através de `[Collection(IntegrationTestCollection.Name)]`, eliminando o overhead de subir múltiplos servidores e assegurando isolamento transacional.

### Organização das Suítes de Teste:
1. **PetGuardian.UnitTests (191 testes):**
   * **Domínio (17 Classes / Validação de Invariantes & Criptografia):**
     * `PetTests` (Invariantes de nome, datas de nascimento, castração e cálculo de idade)
     * `UsuarioTests` (Validações de tamanho de nome, e-mail, senha mínima, salt e BCrypt)
     * `TarefaTests` (Invariantes de prazo futuro, pontuação e bloqueio de edição após conclusão)
     * `TrilhaTests` (Invariantes de nome, limites de caracteres e vínculo de pet)
     * `ModuloTests` (Validações de tempo de conclusão, descrição e vínculo de trilha)
     * `AulaTests` (Pontos não negativos, conclusão de aula e impedimento de re-conclusão)
     * `HistoricoTests` (Invariantes de tipo de evento, data UTC e vínculo de pet)
     * `UsuarioPetTests` (Chaves compostas e alternância de responsabilidade principal)
     * `UsuarioEnderecoTests` (Validações de chaves compostas e integridade relacional)
     * `EnderecoTests` (Validação de formato de CEP com 8 dígitos, número e bairro)
     * `StatusTests` (Validação estrita de valores aceitos: PENDENTE, CONCLUIDO, EXPIRADO)
     * `RacaTests` (Invariantes de nome não vazio e limites de caracteres)
     * `TelefoneTests` (Validação de DDD com 2 dígitos e número com até 9 dígitos)
     * `EstadoTests` (Validação de nome obrigatório de estado da federação)
     * `CidadeTests` (Validação de nome e vínculo com estado válido)
     * `BairroTests` (Validação de nome e vínculo com cidade válida)
     * `HashHelperTests` (Criptografia com Salt e verificação segura BCrypt)
   * **Aplicação (16 Classes / Orquestração de Casos de Uso com Moq):**
     * `PetServiceTests`, `UsuarioServiceTests`, `TarefaServiceTests`, `UsuarioPetServiceTests`
     * `EnderecoServiceTests`, `BairroServiceTests`, `CidadeServiceTests`, `EstadoServiceTests`
     * `StatusServiceTests`, `RacaServiceTests`, `TelefoneServiceTests`, `HistoricoServiceTests`
     * `UsuarioEnderecoServiceTests`, `TrilhaServiceTests`, `ModuloServiceTests`, `AulaServiceTests`
2. **PetGuardian.IntegrationTests (110 testes):**
   * **Autenticação & Segurança JWT:**
     * `AuthControllerIntegrationTests` (Login 200 com JWT, senha incorreta 401, e-mail inexistente 401, rota alternativa `/login` 200, `/api/auth/me` sem token 401, `/api/auth/me` com token válido 200, `/api/auth/me` com token inválido 401).
   * **16 Suítes Dedicadas de Controllers (1:1 com a API):** Cada um dos 16 controllers da API possui sua própria classe de teste independente validando respostas de sucesso (200, 201, 204) e tratamento estrito de erros (400 Bad Request com validação de payload/domínio, 404 Not Found):
     * `PetControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 dados inválidos)
     * `RacaControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 nome inválido)
     * `UsuarioControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 e-mail duplicado, POST 400 dados inválidos)
     * `TelefoneControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 telefone inválido)
     * `EnderecoControllerIntegrationTests` (POST 201 com ViaCEP, GET 200, GET 404, PUT 200, DELETE 204, POST 400 CEP inválido)
     * `BairroControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 dados inválidos)
     * `CidadeControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 dados inválidos)
     * `EstadoControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 nome inválido)
     * `UsuarioEnderecoIntegrationTests` (POST 201, GET 200 por usuário, GET 200 por endereço, DELETE 204, POST 400 IDs inválidos)
     * `UsuarioPetControllerIntegrationTests` (POST 201, GET 200 rede de cuidado, PUT 200 troca de principal, DELETE 204, POST 400 IDs inválidos)
     * `TarefaControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, POST 200 concluir, DELETE 204, POST 400 prazo inválido)
     * `StatusControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 status inválido)
     * `TrilhaControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 dados inválidos)
     * `ModuloControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 dados inválidos)
     * `AulaControllerIntegrationTests` (POST 201, GET 200, GET 404, PUT 200, DELETE 204, POST 400 dados inválidos)
     * `HistoricoControllerIntegrationTests` (POST 201, GET 200, GET 404, GET 200 por pet, PUT 200, DELETE 204, POST 400 dados inválidos)
   * **Observabilidade & Health Probes:**
     * `HealthCheckEndpointsTests` (Liveness `/health/live`, readiness `/health/ready`, integridade `/health` e endpoint Prometheus `/metrics`)
     * `ObservabilityMiddlewareTests` (Propagação de Correlation ID e métricas HTTP)
   * **Auditoria Completa da API (`EndpointAuditTests`):** Execução automatizada e validação de **99 operações HTTP** em todos os controllers, Health Checks e Swagger OpenAPI com 100% de conformidade.

### Como Executar os Testes

Para executar toda a suíte de testes (Unitários e de Integração) a partir da raiz da solução:

```powershell
# Executar todos os 301 testes da solução com relatório detalhado
dotnet test PetGuardian.sln --logger "console;verbosity=normal"
```

Para executar separadamente por projeto:

```powershell
# Executar os 191 testes unitários (Domínio + Aplicação com Moq e TestFixture)
dotnet test PetGuardian.UnitTests\PetGuardian.UnitTests.csproj

# Executar os 110 testes de integração (WebApplicationFactory e IntegrationTestCollection)
dotnet test PetGuardian.IntegrationTests\PetGuardian.IntegrationTests.csproj

# Executar a auditoria completa de todas as 99 operações HTTP da API
dotnet test PetGuardian.IntegrationTests\PetGuardian.IntegrationTests.csproj --filter "FullyQualifiedName~EndpointAuditTests"
```

### Resumo da Execução:
```text
Passed!  - Failed: 0, Passed: 191, Skipped: 0, Total: 191 - PetGuardian.UnitTests.dll (net10.0)
Passed!  - Failed: 0, Passed: 110, Skipped: 0, Total: 110 - PetGuardian.IntegrationTests.dll (net10.0)
Total Geral: 301 Testes Passando (100% de sucesso)
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

### Opção B: Execução Local via .NET CLI & Conexão com o Banco de Dados

Para conectar ao banco de dados Oracle oficial da FIAP para avaliação, utilize a connection string configurada com as credenciais da equipe:

#### 1. Via `dotnet user-secrets` (Recomendado para Execução Local)
Navegue até a pasta da solução e execute:
```powershell
# Configurar a connection string oficial do banco Oracle FIAP nos secrets locais:
dotnet user-secrets set "ConnectionStrings:PetGuardianOracle" "User Id=RM561432;Password=301006;Data Source=oracle.fiap.com.br:1521/orcl;" --project .\PetGuardian.API
```

#### 2. Via Variável de Ambiente (PowerShell / Terminal da Sessão)
```powershell
# Configurar a variável para a sessão atual do terminal PowerShell:
$env:ConnectionStrings__PetGuardianOracle = "User Id=RM561432;Password=301006;Data Source=oracle.fiap.com.br:1521/orcl;"

# Executar a API:
dotnet run --project .\PetGuardian.API
```

#### 3. Via Variável de Ambiente (Bash / Linux / MacOS)
```bash
export ConnectionStrings__PetGuardianOracle="User Id=RM561432;Password=301006;Data Source=oracle.fiap.com.br:1521/orcl;"
dotnet run --project ./PetGuardian.API
```

#### 4. Validar Conectividade com o Banco de Dados em Execução
Com a aplicação rodando (porta padrão `5289` no CLI ou `8080` no Docker), consulte a probe de prontidão que valida a conectividade com o Oracle via `Database.CanConnectAsync`:
```powershell
# Teste via PowerShell:
Invoke-RestMethod -Uri "http://localhost:5289/health/ready" -Method Get | ConvertTo-Json

# Teste via cURL:
curl -i http://localhost:5289/health/ready
```
*Se a conexão for bem-sucedida, o status retornado será `200 OK` com `"status": "Healthy"` no componente `"oracle-database"`.*

---

## 📋 Catálogo Completo de Endpoints REST (CRUD / OpenAPI)

Todas as entidades e agregados contam com rotas padronizadas, suporte completo a atualização com validação de regras de negócio (`PUT`), deleção (`DELETE`), persistência (`POST`), consultas especializadas (`GET`) e módulo de autenticação stateless via **JWT Bearer**.

**Interface Interativa Swagger UI (`/index.html`):**
![Documentação Swagger OpenAPI](docs/swagger.png)

---

### 🔐 0. Autenticação & Sessão JWT (`/api/auth` & `/login`)
*Emissão e validação de tokens JWT (RFC 7519 / HS256) compatível com o aplicativo Mobile.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Autentica o usuário com e-mail/senha e emite token JWT assinado digitalmente |
| `POST` | `/login` | Rota alternativa de autenticação para integração direta e transparente com o front-end Mobile |
| `GET` | `/api/auth/me` | Retorna o perfil do usuário logado através da validação do cabeçalho `Authorization: Bearer {token}` (`[Authorize]`) |

---

### 🐾 1. Pets (`/api/pet`)
*Gerenciamento dos animais domésticos, porte, score de saúde e histórico consolidado.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/pet` | Lista todos os pets cadastrados no sistema |
| `GET` | `/api/pet/{id}` | Busca os detalhes de um pet específico por ID |
| `GET` | `/api/pet/by-raca/{racaId}` | Filtra e lista todos os pets associados a uma raça |
| `GET` | `/api/pet/{id}/historico` | Retorna a linha do tempo consolidada e cronológica de eventos do pet |
| `POST` | `/api/pet` | Cadastra um novo pet (com validação de porte, sexo e score inicial) |
| `PUT` | `/api/pet/{id}` | Atualiza dados cadastrais e editáveis do pet (nome, porte, raça, data de nascimento) |
| `DELETE` | `/api/pet/{id}` | Remove o registro de um pet do sistema |

---

### 🏷️ 2. Raças (`/api/raca`)
*Catálogo de raças caninas e felinas cadastradas.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/raca` | Lista todas as raças cadastradas |
| `GET` | `/api/raca/{id}` | Busca uma raça específica por ID |
| `POST` | `/api/raca` | Cadastra uma nova raça |
| `PUT` | `/api/raca/{id}` | Atualiza o nome da raça |
| `DELETE` | `/api/raca/{id}` | Remove uma raça |

---

### 👤 3. Usuários & Tutores (`/api/usuario`)
*Gestão de tutores e cuidadores, com segurança criptográfica de senhas (BCrypt + Salt).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/usuario` | Lista todos os usuários cadastrados |
| `GET` | `/api/usuario/{id}` | Busca um usuário por ID |
| `GET` | `/api/usuario/by-email` | Busca usuário por endereço de e-mail (query string `?email=...`) |
| `GET` | `/api/usuario/{id}/score` | Consulta a pontuação gamificada acumulada do usuário |
| `POST` | `/api/usuario` | Cadastra um novo usuário com senha criptografada (BCrypt + Salt criptográfico) |
| `PUT` | `/api/usuario/{id}` | Atualiza dados do usuário (nome, e-mail, perfil/role e senha com novo salt) |
| `DELETE` | `/api/usuario/{id}` | Remove um usuário do sistema |

---

### 📱 4. Telefones (`/api/telefone`)
*Contatos telefônicos vinculados aos tutores.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/telefone` | Lista todos os telefones cadastrados |
| `GET` | `/api/telefone/{id}` | Busca um telefone por ID |
| `POST` | `/api/telefone` | Cadastra um novo telefone vinculado a um usuário |
| `PUT` | `/api/telefone/{id}` | Atualiza número de telefone e DDD |
| `DELETE` | `/api/telefone/{id}` | Remove um telefone |

---

### 🏠 5. Endereços (`/api/endereco`)
*Endereços com enriquecimento e resolução automática via integração com o ViaCEP.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/endereco` | Lista todos os endereços cadastrados |
| `GET` | `/api/endereco/{id}` | Busca um endereço por ID |
| `POST` | `/api/endereco` | Cadastra endereço com resolução automática de CEP via serviço ViaCEP |
| `PUT` | `/api/endereco/{id}` | Atualiza logradouro, número ou CEP com re-resolução automática de localidade |
| `DELETE` | `/api/endereco/{id}` | Remove um endereço |

---

### 📍 6. Bairros (`/api/bairro`)
*Bairros normalizados integrados à estrutura de endereços.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/bairro` | Lista todos os bairros |
| `GET` | `/api/bairro/{id}` | Busca bairro por ID |
| `POST` | `/api/bairro` | Cadastra um novo bairro |
| `PUT` | `/api/bairro/{id}` | Atualiza o nome do bairro |
| `DELETE` | `/api/bairro/{id}` | Remove um bairro |

---

### 🏙️ 7. Cidades (`/api/cidade`)
*Cidades federadas cadastradas e vinculadas aos estados.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/cidade` | Lista todas as cidades |
| `GET` | `/api/cidade/{id}` | Busca cidade por ID |
| `POST` | `/api/cidade` | Cadastra uma nova cidade vinculada a um estado |
| `PUT` | `/api/cidade/{id}` | Atualiza o nome da cidade |
| `DELETE` | `/api/cidade/{id}` | Remove uma cidade |

---

### 🗺️ 8. Estados (`/api/estado`)
*Unidades Federativas (UF).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/estado` | Lista todos os estados cadastrados |
| `GET` | `/api/estado/{id}` | Busca estado por ID |
| `POST` | `/api/estado` | Cadastra um novo estado (com UF e nome) |
| `PUT` | `/api/estado/{id}` | Atualiza dados do estado |
| `DELETE` | `/api/estado/{id}` | Remove um estado |

---

### 🔗 9. Vínculo Usuário-Endereço (`/api/usuarioendereco`)
*Tabela associativa entre usuários e múltiplos endereços.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/usuarioendereco` | Lista todos os vínculos usuário-endereço |
| `GET` | `/api/usuarioendereco/{usuarioId}/{enderecoId}` | Busca vínculo específico por chave composta (`usuarioId` + `enderecoId`) |
| `POST` | `/api/usuarioendereco` | Associa um endereço a um usuário |
| `PUT` | `/api/usuarioendereco/{usuarioId}/{enderecoId}` | Atualiza propriedades do vínculo usuário-endereço |
| `DELETE` | `/api/usuarioendereco/{usuarioId}/{enderecoId}` | Desvincula o endereço do usuário |

---

### 🤝 10. Rede de Cuidado Colaborativo (`/api/usuariopet`)
*Care Circle: Relação N:N entre cuidadores e pets, convites e salvaguarda do animal.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/usuariopet` | Lista todos os vínculos de cuidadores cadastrados |
| `GET` | `/api/usuariopet/{usuarioId}/{petId}` | Busca vínculo individual por chave composta (`usuarioId` + `petId`) |
| `GET` | `/api/usuariopet/by-usuario/{usuarioId}` | Lista todos os pets sob o cuidado de um tutor |
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
| `GET` | `/api/tarefa` | Lista todas as tarefas cadastradas |
| `GET` | `/api/tarefa/{id}` | Busca detalhes de uma tarefa por ID |
| `GET` | `/api/tarefa/by-pet/{petId}` | Lista todas as tarefas associadas a um pet específico |
| `GET` | `/api/tarefa/by-usuario/{usuarioId}` | Lista tarefas atribuídas a um cuidador/usuário |
| `GET` | `/api/tarefa/by-status/{statusId}` | Filtra e lista tarefas por status (ex.: pendentes) |
| `POST` | `/api/tarefa` | Cria uma nova rotina/tarefa de cuidado para o pet |
| `PUT` | `/api/tarefa/{id}` | Atualiza título, descrição, data agendada ou pontuação de uma tarefa não concluída |
| `POST` | `/api/tarefa/{id}/concluir` | Conclui tarefa, credita pontuação ao usuário e gera marco no histórico do pet |
| `DELETE` | `/api/tarefa/{id}` | Remove uma tarefa agendada |

---

### 🚦 12. Status de Tarefas (`/api/status`)
*Estados padronizados do ciclo de vida das rotinas (`PENDENTE`, `CONCLUIDO`, `EXPIRADO`).*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/status` | Lista os status disponíveis no sistema |
| `GET` | `/api/status/{id}` | Busca status por ID |
| `POST` | `/api/status` | Cadastra um novo status |
| `PUT` | `/api/status/{id}` | Atualiza a descrição do status |
| `DELETE` | `/api/status/{id}` | Remove um status |

---

### 🎓 13. Trilhas de Aprendizado (`/api/trilha`)
*Capacitação continuada dos tutores com conteúdos educativos.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/trilha` | Lista todas as trilhas de aprendizado |
| `GET` | `/api/trilha/{id}` | Busca uma trilha por ID |
| `GET` | `/api/trilha/by-pet/{petId}` | Lista trilhas recomendadas para um pet específico |
| `POST` | `/api/trilha` | Cria uma nova trilha de aprendizado |
| `PUT` | `/api/trilha/{id}` | Atualiza dados da trilha (título, descrição, pontuação) |
| `DELETE` | `/api/trilha/{id}` | Remove uma trilha |

---

### 📚 14. Módulos de Aprendizado (`/api/modulo`)
*Divisões temáticas e estruturais dentro de uma trilha.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/modulo` | Lista todos os módulos |
| `GET` | `/api/modulo/{id}` | Busca módulo por ID |
| `POST` | `/api/modulo` | Cadastra um novo módulo vinculado a uma trilha |
| `PUT` | `/api/modulo/{id}` | Atualiza dados do módulo (ordem, título, conteúdo) |
| `DELETE` | `/api/modulo/{id}` | Remove um módulo |

---

### 📖 15. Aulas de Capacitação (`/api/aula`)
*Aulas individuais com pontuação educativa para o tutor.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/aula` | Lista todas as aulas cadastradas |
| `GET` | `/api/aula/{id}` | Busca aula por ID |
| `POST` | `/api/aula` | Cadastra uma nova aula associada a um módulo |
| `PUT` | `/api/aula/{id}` | Atualiza dados da aula (título, conteúdo, pontuação) |
| `DELETE` | `/api/aula/{id}` | Remove uma aula |

---

### 📜 16. Histórico & Linha do Tempo (`/api/historico`)
*Registro histórico unificado de intervenções, vacinas, tarefas e marcos de saúde do animal.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/api/historico` | Lista registros históricos de saúde e cuidados |
| `GET` | `/api/historico/{id}` | Busca registro histórico específico por ID |
| `POST` | `/api/historico` | Registra um novo marco clínico ou histórico para o pet |
| `PUT` | `/api/historico/{id}` | Atualiza detalhes de um registro histórico |
| `DELETE` | `/api/historico/{id}` | Remove um registro histórico |

---

### 🩺 17. Health Checks & Observabilidade (`/health`)
*Monitoramento de integridade e liveness/readiness probes para orquestradores.*

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `GET` | `/health` | Visão completa da saúde da API, banco Oracle e serviço ViaCEP em JSON estruturado |
| `GET` | `/health/ready` | Readiness probe: verifica conectividade com Oracle (`CanConnectAsync`) e ViaCEP |
| `GET` | `/health/live` | Liveness probe: verifica se o processo da aplicação ASP.NET Core está ativo |
