# Dragon Ball & Music APIs

Prova de conceito .NET 10 com duas APIs CRUD (personagens Dragon Ball + catálogo musical), banco PostgreSQL único com esquemas isolados, gateway Apache APISIX, autenticação Keycloak, observabilidade OpenTelemetry, orquestração .NET Aspire e frontends React + Angular.

## Stack

- **.NET 10** + **ASP.NET Core** + **EF Core** + **Npgsql**
- **PostgreSQL 17** (schemas: `dragonball`, `music`, `keycloak`)
- **Apache APISIX 3.x** (roteamento, CORS, OIDC, correlação)
- **Keycloak 26+** (OIDC/OAuth2, `bearer_only` em escrita)
- **OpenTelemetry** + **Jaeger** (traças distribuídas, métricas)
- **.NET Aspire 13.x** (orquestração local)
- **React 19** + **Angular 21** (frontends)
- **Scalar UI** (documentação OpenAPI)

## Pré-requisitos

- .NET 10 SDK
- Docker Desktop
- Node.js 22+
- Angular CLI 19+ (para o frontend Angular)

## Como rodar

### 1. Inicie os serviços de infraestrutura

```bash
docker run -d --name postgres -e POSTGRES_DB=opencode -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:17
```

Ou use o Docker Compose completo:

```bash
docker compose up -d postgres keycloak etcd apisix
```

### 2. Execute o AppHost (Aspire)

```bash
dotnet run --project src/OpenCode.AppHost
```

O dashboard do Aspire abre em `https://localhost:17000`. Os serviços ficam disponíveis em:

| Serviço | Porta |
|---------|-------|
| Dragon Ball API | `http://localhost:5000` |
| Music API | `http://localhost:5002` |
| APISIX (proxy) | `http://localhost:9080` |
| APISIX (admin) | `http://localhost:9180` |
| Keycloak | `http://localhost:8080` |
| Jaeger (UI) | `http://localhost:16686` |
| Frontend React | `http://localhost:5173` |
| Frontend Angular | `http://localhost:4200` |

### 3. Scalar UI (documentação interativa)

Com o ambiente rodando, acesse no navegador:

- Dragon Ball: `http://localhost:5000/scalar`
- Music: `http://localhost:5002/scalar`

Disponível apenas em modo `Development`.

## Estrutura do projeto

```
src/
├── OpenCode.AppHost/          # Orquestrador Aspire
├── OpenCode.Domain/           # Entidades, DbContext, Migrations, Repositórios
├── OpenCode.DragonBall.Api/   # API CRUD de personagens
├── OpenCode.Music.Api/        # API CRUD de catálogo musical
├── OpenCode.ServiceDefaults/  # OpenTelemetry, Correlation ID
├── OpenCode.Frontend/         # Frontend React
└── OpenCode.AngularFrontend/  # Frontend Angular
tests/
└── OpenCode.Domain.Tests/     # Testes unitários (xUnit)
```

## Arquitetura

- **Gateway-first**: Todo tráfego passa pelo APISIX (porta `9080`)
- **Leitura pública, escrita protegida**: Rotas GET são abertas; POST/PUT/DELETE exigem token JWT com role `editor`
- **Schema isolation**: `DbContext` separados (`DragonBallContext`, `MusicContext`) apontando para schemas distintos no mesmo banco
- **Correlation ID**: Header `X-Correlation-Id` propagado em todas as requisições/respostas
- **Repository pattern**: `IRepository<T>` com `PagedResult<T>` para listagem paginada
- **REPR pattern**: Endpoints organizados em classes estáticas por recurso

## Credenciais (desenvolvimento)

| Sistema | Usuário | Senha |
|---------|---------|-------|
| Keycloak admin | `admin` | `admin` |
| PostgreSQL | `postgres` | `postgres` |
