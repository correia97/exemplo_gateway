# Dragon Ball & Music APIs - Prova de Conceito com API Gateway Kong

Uma arquitetura de microserviços completa com duas APIs CRUD (.NET 10), API Gateway Kong, autenticação centralizada (Keycloak) e frontends moderno (Angular e React + Vite).

**Status do Projeto:** ✅ Pronto para execução com Docker Compose ou .NET Aspire

---

## 📋 Visão Geral

Este é um **proof of concept** que demonstra uma arquitetura profissional com:

- **2 APIs CRUD** independentes em **.NET 10** (personagens Dragon Ball + catálogo de músicas)
- **Kong API Gateway** para roteamento, autenticação e observabilidade centralizada
- **Keycloak** para autenticação OAuth2/OIDC e autorização
- **PostgreSQL** compartilhado com schemas isolados por serviço
- **2 Frontends** (Angular e React + Vite) com integração Keycloak
- **Observabilidade completa** com OpenTelemetry, Jaeger e Correlation ID
- **Pronto para execução** via Docker Compose ou .NET Aspire

---

## 🏗️ Stack Tecnológico

### Backend
- **.NET 10** + **ASP.NET Core** + **EF Core** + **Npgsql**
- **PostgreSQL 17** (schemas: `dragonball`, `music`)
- **Kong 3.x** (API Gateway: roteamento, CORS, OIDC, autenticação)
- **Keycloak 26+** (autenticação OIDC/OAuth2, `bearer_only` em operações de escrita)
- **OpenTelemetry + Jaeger** (traças distribuídas, métricas)
- **.NET Aspire 13.x** (orquestração e gerenciamento de infraestrutura)

### Frontend
- **React 19** + **Vite** + **TypeScript**
- **Angular 21** + **TypeScript**
- Ambos integrados com **Keycloak** para autenticação
- **Scalar UI** para documentação interativa das APIs

---

## 🚀 Como Executar

### Opção 1: Docker Compose (Recomendado para produção)

```bash
# Iniciar todos os serviços
docker compose up -d

# Acompanhar logs
docker compose logs -f

# Parar todos os serviços
docker compose down
```

Serviços disponíveis:
- **PostgreSQL**: `localhost:5432`
- **Keycloak**: `http://localhost:8080`
- **Kong Gateway**: `http://localhost:8000`
- **Kong Admin**: `http://localhost:8001`
- **Jaeger UI**: `http://localhost:16686`
- **Dragon Ball API**: `http://localhost:5000`
- **Music API**: `http://localhost:5002`
- **Frontend React**: `http://localhost:5173`
- **Frontend Angular**: `http://localhost:4200`

---

### Opção 2: .NET Aspire (Recomendado para desenvolvimento)

**Pré-requisitos:**
- .NET 10 SDK
- Node.js 22+ (para frontends)
- Docker Desktop (Aspire gerencia os containers automaticamente)

**Executar:**

```bash
dotnet run --project src/OpenCode.AppHost
```

O dashboard do Aspire abre automaticamente em `https://localhost:17000` e gerencia toda a infraestrutura (PostgreSQL, Keycloak, Kong, Jaeger, etc).

**Serviços disponíveis (gerenciados pelo Aspire):**

| Serviço | Porta | URL |
|---------|-------|-----|
| Dragon Ball API | 5000 | `http://localhost:5000` |
| Music API | 5002 | `http://localhost:5002` |
| Kong Gateway | 8000 | `http://localhost:8000` |
| Kong Admin API | 8001 | `http://localhost:8001` |
| Keycloak | 8080 | `http://localhost:8080` |
| PostgreSQL | 5432 | `localhost:5432` |
| Jaeger UI | 16686 | `http://localhost:16686` |
| Frontend React | 5173 | `http://localhost:5173` |
| Frontend Angular | 4200 | `http://localhost:4200` |

> ℹ️ Com o Aspire, você não precisa iniciar Docker Compose manualmente. O Aspire orquestra toda a infraestrutura através do AppHost.

---

## 📚 Usando as APIs

### Documentação Interativa (Scalar UI)

Com o ambiente rodando, acesse a documentação interativa:

- **Dragon Ball API**: `http://localhost:5000/scalar`
- **Music API**: `http://localhost:5002/scalar`

> ℹ️ Disponível apenas em modo `Development`

### Acessando via Kong Gateway

As APIs também estão disponíveis através do Kong Gateway:

- **Dragon Ball**: `http://localhost:8000/dragonball/api/characters`
- **Music**: `http://localhost:8000/music/api/songs`

### Rotas Principais

#### Dragon Ball API
- `GET /api/characters` - Listar personagens (público)
- `GET /api/characters/{id}` - Obter personagem (público)
- `POST /api/characters` - Criar personagem (requer autenticação)
- `PUT /api/characters/{id}` - Atualizar personagem (requer autenticação)
- `DELETE /api/characters/{id}` - Deletar personagem (requer autenticação)

#### Music API
- `GET /api/songs` - Listar músicas (público)
- `GET /api/songs/{id}` - Obter música (público)
- `POST /api/songs` - Criar música (requer autenticação)
- `PUT /api/songs/{id}` - Atualizar música (requer autenticação)
- `DELETE /api/songs/{id}` - Deletar música (requer autenticação)

### Autenticação

1. **Frontend**: Faça login via Keycloak nos frontends (React ou Angular)
2. **cURL / Postman**: Obtenha um token do Keycloak
   ```bash
   curl -X POST http://localhost:8080/realms/opencode/protocol/openid-connect/token \
     -H "Content-Type: application/x-www-form-urlencoded" \
     -d "client_id=api-client&client_secret=secret&grant_type=password&username=user&password=password"
   ```
3. Use o token no header `Authorization: Bearer {token}`

---

## 📁 Estrutura do Projeto

```
exemplo_gateway/
├── src/
│   ├── OpenCode.AppHost/              # Orquestrador .NET Aspire
│   │   └── Program.cs                 # Configuração da infraestrutura
│   │
│   ├── OpenCode.Domain/               # Entidades, DbContext, Migrations, Repositórios
│   │   ├── Entities/
│   │   │   ├── Character.cs
│   │   │   └── Song.cs
│   │   └── Data/
│   │       ├── DragonBallContext.cs   # Schema: dragonball
│   │       ├── MusicContext.cs        # Schema: music
│   │       └── Migrations/
│   │
│   ├── OpenCode.DragonBall.Api/       # API CRUD de personagens
│   │   ├── Endpoints/
│   │   └── Services/
│   │
│   ├── OpenCode.Music.Api/            # API CRUD de catálogo musical
│   │   ├── Endpoints/
│   │   └── Services/
│   │
│   ├── OpenCode.ServiceDefaults/      # Configuração compartilhada
│   │   ├── OpenTelemetry
│   │   ├── Correlation ID
│   │   └── Middleware
│   │
│   ├── OpenCode.Frontend/             # Frontend React + Vite
│   │   ├── src/
│   │   │   ├── components/
│   │   │   ├── pages/
│   │   │   └── services/
│   │   └── package.json
│   │
│   └── OpenCode.AngularFrontend/      # Frontend Angular
│       ├── src/
│       │   ├── app/
│       │   ├── components/
│       │   └── services/
│       └── package.json
│
├── tests/
│   └── OpenCode.Domain.Tests/         # Testes unitários (xUnit)
│
├── docker-compose.yml                 # Orquestração Docker (alternativa)
└── README.md
```

---

## 🔐 Credenciais Padrão (Desenvolvimento)

| Sistema | Usuário | Senha |
|---------|---------|-------|
| Keycloak | `admin` | `admin` |
| PostgreSQL | `postgres` | `postgres` |

> ⚠️ **Atenção**: Altere as credenciais antes de usar em produção

---

## 🏛️ Arquitetura

### Fluxo de Requisições

```
Cliente (Frontend/cURL)
        ↓
   Keycloak (Login/Token)
        ↓
   Kong API Gateway
   ├── Roteamento de serviços
   ├── Validação de Token
   ├── CORS
   ├── Rate Limiting
   └── Correlation ID
        ↓
   API específica (Dragon Ball / Music)
        ↓
   PostgreSQL (schema isolado)
```

### Características Principais

| Aspecto | Implementação |
|---------|---------------|
| **Leitura** | Pública (sem autenticação) |
| **Escrita** | Protegida (requer JWT com role `editor`) |
| **Isolamento** | `DbContext` separados apontando para schemas distintos |
| **Gateway** | Kong com autenticação OIDC e validação de JWT |
| **Correlation ID** | Header `X-Correlation-Id` propagado em todas requisições |
| **Padrões** | Repository Pattern + REPR Pattern |
| **Paginação** | `PagedResult<T>` nos endpoints de listagem |
| **Observabilidade** | OpenTelemetry + Jaeger com rastreamento distribuído |

---

## 🛠️ Desenvolvimento

### Executar testes unitários

```bash
dotnet test tests/OpenCode.Domain.Tests
```

### Rodar frontend React em desenvolvimento

```bash
cd src/OpenCode.Frontend
npm install
npm run dev
```

### Rodar frontend Angular em desenvolvimento

```bash
cd src/OpenCode.AngularFrontend
npm install
ng serve
```

### Aplicar migrations do banco de dados

```bash
dotnet ef database update --project src/OpenCode.Domain --startup-project src/OpenCode.DragonBall.Api
```

### Visualizar logs do Aspire

No dashboard do Aspire (`https://localhost:17000`), você pode:
- Monitorar logs de todos os serviços em tempo real
- Visualizar métricas e performance
- Acessar endpoints dos serviços diretamente
- Gerenciar ciclo de vida dos containers

---

## 📊 Observabilidade

### Jaeger UI

Acesse `http://localhost:16686` para visualizar:
- Traces distribuídos entre serviços
- Latências e performance
- Erros e exceções
- Dependências entre componentes

### Logs Estruturados

Todos os serviços utilizam Serilog com estrutura de logs JSON para melhor análise.

### Kong Admin API

Acesse `http://localhost:8001` para:
- Visualizar rotas configuradas
- Gerenciar plugins
- Monitorar upstream services
- Configurar autenticação

---

## 🔄 CI/CD (Futuro)

Estrutura pronta para integração com:
- GitHub Actions
- GitLab CI/CD
- Jenkins
- Docker Registry

---

## 📝 Licença

Este projeto é um proof of concept e está disponível para fins educacionais.

---

## 🤝 Contribuindo

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## ❓ Dúvidas ou Sugestões?

Abra uma issue no repositório ou entre em contato com o autor.

**Happy coding! 🚀**
