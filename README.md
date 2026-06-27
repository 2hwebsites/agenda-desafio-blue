# Agenda de Contatos — Backend

Desafio técnico Tech Lead: CRUD de agenda de contatos com Clean Architecture.

## Stack e Versões

| Componente | Versão |
|---|---|
| .NET SDK | 10.0.x (target: `net10.0`) |
| C# | 14 (nullable enabled) |
| EF Core + Npgsql | 10.0.9 / 10.0.2 |
| PostgreSQL | 17 (Alpine) |
| Swashbuckle | 7.3.1 |
| Docker / Compose | 29+ |

## Arquitetura

```
Agenda.sln
├── src/
│   ├── Agenda.Domain/          # Entities, domain invariants
│   ├── Agenda.Application/     # Use cases (phase 2+)
│   ├── Agenda.Infrastructure/  # EF Core, DbContext, Migrations
│   └── Agenda.Api/             # Web API, DI, Swagger
└── tests/
    └── Agenda.Tests/           # xUnit (phase 2+)
```

Referências: `Api → Application, Infrastructure` | `Application → Domain` | `Infrastructure → Application, Domain`

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (com Docker Compose v2)
- `dotnet-ef` CLI: `dotnet tool install --global dotnet-ef`

## Como Rodar

### 1. Subir o PostgreSQL

```bash
docker compose up -d
```

Aguarde o healthcheck passar (`docker compose ps` mostrará `healthy`).

### 2. Configurar a connection string local

Copie o arquivo de exemplo e ajuste se necessário:

```bash
cp src/Agenda.Api/appsettings.Development.json.example src/Agenda.Api/appsettings.Development.json
```

> O arquivo `.example` já contém as credenciais do docker-compose local e funciona sem alterações.
> `appsettings.Development.json` está no `.gitignore` e nunca é commitado.

### 3. Rodar a API

```bash
dotnet run --project src/Agenda.Api/Agenda.Api.csproj
```

Em ambiente `Development`, as migrations pendentes são aplicadas automaticamente no startup.

### 4. Acessar o Swagger

Abra no navegador: [http://localhost:5196/swagger](http://localhost:5196/swagger)

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

`appsettings.json` (versionado) contém apenas placeholders. Os valores reais ficam em
`appsettings.Development.json` (gitignored). Para outros ambientes, use variáveis de ambiente:

```
ConnectionStrings__Default=Host=...;Port=5432;Database=agenda;Username=...;Password=...
```

## Applying Migrations Manually

If you need to run migrations outside of the API startup (e.g. in CI or production):

```bash
dotnet ef database update \
    --project src/Agenda.Infrastructure/Agenda.Infrastructure.csproj \
    --startup-project src/Agenda.Api/Agenda.Api.csproj
```

## Fases do Projeto

| Fase | Status | Escopo |
|---|---|---|
| 1 — Fundação | ✅ Completo | Solution, Domain, EF Core, Swagger, /health, Migration |
| 2 — CRUD | Pendente | ContactsController, Application layer, DTOs, validation |
| 3 — Frontend | Pendente | Vue 3, integração com a API |
| 4 — Deploy | Pendente | Dockerfile API, docker-compose completo, CI |
