# 🚨 AlertaCom API - Plataforma CitizenTech de Prevenção & Zeladoria Urbana
> **Advanced Business Development with .NET - Global Solution FIAP 2026/2**  
> *Tecnologia com o Cidadão no Centro da Inovação*

---

## 👥 Integrantes do Grupo

| Nome | RM | Papel no Projeto |
|---|---|---|
| **Thomas Fontes** | **562254** | **Backend .NET & Mobile Application Development** |
| **Gabriel Maciel** | **562795** | **Java Advanced (Spring Boot & Pitch)** |
| **Matheus Molina** | **563399** | **Disruptive Architectures (Visão Computacional & GenAI)** |
| **Vitória Rodrigues** | **565160** | **Database (Oracle PL/SQL & MongoDB)** |

---

## 📌 1. Visão Geral da Solução

O **AlertaCom API** é uma solução corporativa desenvolvida em **.NET 10** (C#) aplicando os padrões arquiteturais mais modernos: **Clean Architecture**, **Minimal APIs**, **Repository Pattern**, **OperationResult**, **PageResult**, **HATEOAS**, **Idempotência de Requisições**, **Rate Limiting nativo (HTTP 429)**, persistência híbrida (**Oracle Database Relacional** via Entity Framework Core e **MongoDB NoSQL** com MongoDB.Driver), **Autenticação JWT Bearer**, **Observabilidade completa (Serilog, OpenTelemetry e Health Checks)** e uma suíte completa de **testes automatizados (xUnit)**.

A plataforma centraliza a gestão de riscos climáticos e zeladoria urbana, permitindo que cidadãos registrem ocorrências (alagamentos, bueiros obstruídos, quedas de árvores), acompanhem o atendimento e recebam alertas emitidos pela Defesa Civil.

---

## 🏗️ 2. Arquitetura da Solução (Clean Architecture)

A solução é dividida rigorosamente segundo a Clean Architecture:

```mermaid
graph TD
    subgraph Core["Core da Aplicação (Independente de Frameworks)"]
        Domain["🧠 AlertaComApi.Domain<br/>- Entidades: User, Ocorrencia, AlertaClimatico, Auditoria...<br/>- Interfaces de Repositório<br/>- Exceptions Customizadas<br/>- OperationResult & PageResult<br/>- LinkDto (HATEOAS)"]
        App["⚙️ AlertaComApi.Application<br/>- DTOs & Mappers Tipados<br/>- FluentValidation<br/>- Interfaces de Serviços<br/>- Serviços de Domínio<br/>- HateoasService"]
    end

    subgraph Adapters["Infraestrutura & Persistência"]
        Infra["🗄️ AlertaComApi.Infrastructure<br/>- AlertaComContext (EF Core Oracle / In-Memory)<br/>- DbInitializer (Carga Seed de Testes)<br/>- Repositórios Relacionais<br/>- MongoDbContext & OcorrenciaNoSqlRepository (NoSQL)<br/>- JwtTokenGenerator (HMAC-SHA256)"]
    end

    subgraph EntryPoint["Apresentação & API"]
        Api["🌐 AlertaComApi (Minimal API Host)<br/>- MapGroup (/api/auth, /api/ocorrencias, /api/alertas)<br/>- IdempotencyMiddleware (Header Idempotency-Key)<br/>- RateLimiter (HTTP 429 Too Many Requests)<br/>- GlobalExceptionMiddleware (RFC 7807 ProblemDetails)<br/>- Serilog & OpenTelemetry<br/>- Health Checks (/health, /live, /ready)"]
    end

    subgraph Quality["Garantia de Qualidade & Testes"]
        Unit["🧪 AlertaComApi.Tests.Unit<br/>(xUnit, Moq, FluentAssertions)"]
        Integration["🔬 AlertaComApi.Tests.Integration<br/>(WebApplicationFactory, In-Memory DB)"]
    end

    App -->|Depende de| Domain
    Infra -->|Depende de| Domain
    Infra -->|Depende de| App
    Api -->|Depende de| App
    Api -->|Depende de| Infra
    Api -->|Depende de| Domain
    Unit --> Domain
    Unit --> App
    Integration --> Api
```

---

## 🚀 3. Requisitos Atendidos da Global Solution

| Requisito | Pontos | Como foi implementado |
|---|---|---|
| **01. Minimal API RESTful & Swagger** | 20 pts | Endpoints mapeados via `MapGroup` (`/api/auth`, `/api/ocorrencias`, `/api/alertas`), validações via FluentValidation, códigos HTTP semânticos e documentação interativa Swagger/OpenAPI com Bearer Token. |
| **02. Autenticação & Autorização JWT** | 20 pts | Proteção de rotas com JWT Bearer, claims de roles (`Cidadao`, `DefesaCivil`, `Admin`) e dados contextuais (bairro, CPF). |
| **03. Idempotência em Requisições** | 15 pts | Middleware `IdempotencyMiddleware` interceptando o cabeçalho `Idempotency-Key`. Evita duplicações em conexões instáveis do app e retorna `X-Idempotent-Replay: true`. |
| **04. Rate Limiting Funcional** | 15 pts | Middleware nativo de Rate Limiting com `FixedWindowLimiter` e resposta padronizada RFC 6585 com status `HTTP 429 Too Many Requests`. |
| **05. Testes Unitários e Integração** | 20 pts | Suíte xUnit completa em `AlertaComApi.Tests.Unit` (Moq, FluentAssertions) e `AlertaComApi.Tests.Integration` (`WebApplicationFactory`), com 100% de aprovação. |
| **06. Health Checks Completos** | 10 pts | Endpoints `/health`, `/health/live` e `/health/ready` monitorando a API, banco de dados Oracle/InMemory, MongoDB NoSQL e serviços externos. |

---

## 🔑 4. Credenciais de Teste (Seed Inicial)

O banco é inicializado automaticamente com dados reais de São Paulo e os seguintes usuários:

| Perfil | Usuário (Username) | Senha | Papel (Role) | Bairro |
|---|---|---|---|---|
| **Cidadão** | `cidadao` | `123456` | `Cidadao` | Vila Prudente |
| **Defesa Civil** | `defesacivil` | `123456` | `DefesaCivil` | Sé (Centro) |
| **Administrador** | `admin` | `123456` | `Admin` | Consolação |

---

## 💻 5. Como Executar

### 1. Clonar e Rodar a API:
```powershell
cd AlertaComApi/AlertaComApi
dotnet run
```
Acesse a documentação Swagger interativa em:
👉 **`http://localhost:5208/swagger`** (ou simplesmente **`http://localhost:5208/`**)

### 2. Executar os Testes Automatizados:
```powershell
cd AlertaComApi
dotnet test
```

### 3. Verificar os Health Checks:
- `/health` $\rightarrow$ Diagnóstico geral em JSON.
- `/health/live` $\rightarrow$ Liveness probe (API no ar).
- `/health/ready` $\rightarrow$ Readiness probe (banco de dados e serviços).
