# Agenda de Contatos — Backend

Desafio técnico Tech Lead: CRUD de agenda de contatos com Clean Architecture.

## Stack e Versões

| Componente | Versão |
|---|---|
| .NET SDK | 9.0.x (target: `net9.0`) |
| C# | 13 (nullable enabled) |
| EF Core + Npgsql | 9.0.6 |
| PostgreSQL | 17 (Alpine) |
| Swashbuckle | 7.3.1 |
| Docker / Compose | 29+ |

> **Nota .NET 10:** O projeto está configurado para `net9.0` pois o SDK 10 não estava disponível na máquina de desenvolvimento. Para migrar: altere `<TargetFramework>` em todos os `.csproj` para `net10.0` e atualize os pacotes para `10.x`.

## Arquitetura

```
Agenda.sln
├── src/
│   ├── Agenda.Domain/          # Entidades, invariantes de domínio
│   ├── Agenda.Application/     # Casos de uso (fase 2+)
│   ├── Agenda.Infrastructure/  # EF Core, DbContext, Migrations
│   └── Agenda.Api/             # Web API, DI, Swagger
└── tests/
    └── Agenda.Tests/           # xUnit (fase 2+)
```

Referências: `Api → Application, Infrastructure` | `Application → Domain` | `Infrastructure → Application, Domain`

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (com Docker Compose v2)
- `dotnet-ef` CLI: `dotnet tool install --global dotnet-ef`

## Como Rodar

### 1. Subir o PostgreSQL

```bash
docker compose up -d
```

Aguarde o healthcheck passar (`docker compose ps` mostrará `healthy`).

### 2. Aplicar a Migration

```bash
dotnet ef database update \
    --project src/Agenda.Infrastructure/Agenda.Infrastructure.csproj \
    --startup-project src/Agenda.Api/Agenda.Api.csproj
```

Isso cria a tabela `contatos` com índice único em `email`.

### 3. Rodar a API

```bash
dotnet run --project src/Agenda.Api/Agenda.Api.csproj
```

### 4. Acessar o Swagger

Abra no navegador: [http://localhost:5156/swagger](http://localhost:5156/swagger)

O endpoint `/health` estará disponível e retornará:
```json
{ "status": "healthy", "timestamp": "2026-..." }
```

### 5. Parar o banco

```bash
docker compose down
# Para remover também o volume de dados:
docker compose down -v
```

## Connection String

Configurada em `src/Agenda.Api/appsettings.json`:

```
Host=localhost;Port=5432;Database=agenda;Username=agenda_user;Password=agenda_pass
```

Para ambientes diferentes, use `appsettings.{Environment}.json` ou variáveis de ambiente:
```
ConnectionStrings__Default=Host=...
```

## Fases do Projeto

| Fase | Status | Escopo |
|---|---|---|
| 1 — Fundação | ✅ Completo | Solution, Domain, EF Core, Swagger, /health, Migration |
| 2 — CRUD | Pendente | ContatosController, Application layer, DTOs, validação |
| 3 — Frontend | Pendente | Vue 3, integração com a API |
| 4 — Deploy | Pendente | Dockerfile API, docker-compose completo, CI |
