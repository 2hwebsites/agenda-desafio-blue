# Agenda de Contatos — Backend

Desafio técnico Tech Lead: CRUD de agenda de contatos com Clean Architecture.

## Stack e Versões

| Componente | Versão |
|---|---|
| .NET SDK | 10.0.x (target: `net10.0`) |
| C# | 14 (nullable enabled) |
| EF Core + Npgsql | 10.0.9 / 10.0.2 |
| PostgreSQL | 17 (Alpine) |
| MediatR | 14.1.0 |
| AutoMapper | 16.1.1 |
| FluentValidation | 12.1.1 |
| Swashbuckle | 7.3.1 |
| Docker / Compose | 29+ |

## Arquitetura

```
Agenda.sln
├── src/
│   ├── Agenda.Domain/          # Entities, domain invariants, domain exceptions
│   ├── Agenda.Application/     # CQRS use cases (MediatR), DTOs, FluentValidation, AutoMapper
│   ├── Agenda.Infrastructure/  # EF Core, DbContext, Migrations, repository impl
│   └── Agenda.Api/             # Controllers, DI composition root, Swagger
└── tests/
    └── Agenda.Tests/           # xUnit
```

Referências: `Api → Application, Infrastructure` | `Application → Domain` | `Infrastructure → Application, Domain`

## Decisões arquiteturais

### CQRS com MediatR como Application Service

A camada Application expõe somente Commands e Queries (MediatR). Os controllers não conhecem repositórios, nem o domínio diretamente — apenas enviam mensagens via `ISender`. Isso mantém o domínio livre de dependências de framework e facilita testar cada handler de forma isolada.

### Soft delete

Deleção não remove o registro do banco. O EF Core aplica `HasQueryFilter(c => !c.IsDeleted)` globalmente, tornando os registros excluídos invisíveis a todas as queries sem necessidade de filtro manual.

### ProblemDetails (RFC 7807)

Erros são sempre serialized como `ProblemDetails` via `IExceptionHandler` (`GlobalExceptionHandler`). Isso garante contrato uniforme de erros para qualquer cliente REST — sem exceções "cruas" escapando para a resposta HTTP.

## Bibliotecas e licenciamento

| Biblioteca | Licença | Observação |
|---|---|---|
| **MediatR 14** | Comercial (Lucky Penny Software) | Tier Community gratuito para OSS/pequenos projetos. Logs de licença são esperados — não são erros de build. |
| **AutoMapper 16** | Comercial (Lucky Penny Software) | Mesmo modelo do MediatR. |
| **FluentValidation 12** | MIT | Livre para uso comercial. |
| Alternativas sem custo | — | [mediator](https://github.com/martinothamar/Mediator) (source generator), [Mapperly](https://github.com/riok/mapperly) (source gen), [mapster](https://github.com/MapsterMapper/Mapster) |

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

Em ambiente `Development`, as migrations pendentes são aplicadas automaticamente no startup e 3 contatos de exemplo são inseridos caso a tabela esteja vazia.

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

## Endpoints

| Método | Rota | Autenticação | Descrição | Status codes |
|---|---|---|---|---|
| POST | `/api/auth/login` | Pública | Obtém token JWT | 200, 401 |
| GET | `/api/contacts` | Bearer JWT | Lista paginada (`?search=&page=1&pageSize=20`) | 200, 401 |
| GET | `/api/contacts/{id}` | Bearer JWT | Busca por ID | 200, 401, 404 |
| POST | `/api/contacts` | Bearer JWT | Cria contato | 201, 400, 401, 409 |
| PUT | `/api/contacts/{id}` | Bearer JWT | Atualiza contato | 200, 400, 401, 404, 409 |
| DELETE | `/api/contacts/{id}` | Bearer JWT | Remove (soft delete) | 204, 401, 404 |
| GET | `/health` | Pública | Health check | 200 |

**Erros de validação → 400 com `ValidationProblemDetails`:** erros são agrupados por campo (não concatenados numa string), unificando com o padrão do ASP.NET Core e permitindo exibição de erro por campo no front-end.

## Autenticação

### Visão geral

A API usa **JWT Bearer (HMAC-SHA256)**. Todos os endpoints de `/api/contacts` exigem o header `Authorization: Bearer <token>`. Os endpoints `/api/auth/login` e `/health` são públicos.

> **Implementação demonstrativa:** a credencial de acesso é uma "semente" definida em configuração (`AuthSeed`), sem tabela de usuários, cadastro ou hash persistido. Em um cenário real, haveria store de usuários com hash de senha (ex.: ASP.NET Core Identity). Aqui a credencial-semente demonstra a emissão e validação de JWT de ponta a ponta.

### Fluxo de login

```bash
# 1. Obter token
curl -X POST http://localhost:5196/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'

# Resposta:
# { "token": "eyJhbGci...", "expiresAt": "2026-06-28T14:00:00Z" }

# 2. Usar o token
curl http://localhost:5196/api/contacts \
  -H "Authorization: Bearer eyJhbGci..."
```

### Usar no Swagger

1. Acesse [http://localhost:5196/swagger](http://localhost:5196/swagger)
2. Faça `POST /api/auth/login` com as credenciais de dev
3. Copie o valor do campo `token` da resposta
4. Clique no botão **Authorize** (canto superior direito)
5. No campo `Value`, insira apenas o token (sem o prefixo "Bearer")
6. Clique **Authorize** → todos os endpoints passam a enviar o header automaticamente

### Configuração JWT

| Chave | `appsettings.json` | `appsettings.Development.json` |
|---|---|---|
| `Jwt:Issuer` | `<JWT_ISSUER>` | `agenda-api-dev` |
| `Jwt:Audience` | `<JWT_AUDIENCE>` | `agenda-frontend-dev` |
| `Jwt:Key` | `<JWT_SECRET>` | valor longo ≥ 32 chars (ver `.example`) |
| `Jwt:ExpiresMinutes` | `60` | `60` |
| `AuthSeed:Username` | `<SEED_USERNAME>` | `admin` |
| `AuthSeed:Password` | `<SEED_PASSWORD>` | `admin123` |

Para ambientes não-Development, use variáveis de ambiente:

```
Jwt__Key=<seu-secret-de-producao-com-min-32-chars>
Jwt__Issuer=<issuer>
Jwt__Audience=<audience>
AuthSeed__Username=<username>
AuthSeed__Password=<senha-forte>
```

## Connection String

`appsettings.json` (versionado) contém apenas placeholders. Os valores reais ficam em
`appsettings.Development.json` (gitignored). Para outros ambientes, use variáveis de ambiente:

```
ConnectionStrings__Default=Host=...;Port=5432;Database=agenda;Username=...;Password=...
```

## Aplicando Migrations Manualmente

Se precisar rodar migrations fora do startup da API (ex: CI ou produção):

```bash
dotnet ef database update \
    --project src/Agenda.Infrastructure/Agenda.Infrastructure.csproj \
    --startup-project src/Agenda.Api/Agenda.Api.csproj
```

## Testes e cobertura

```
tests/
└── Agenda.Tests/
    ├── Unit/
    │   ├── Domain/              # Testes de entidade (Contact.Create, Update, MarkAsDeleted)
    │   ├── Validators/          # FluentValidation — casos válidos e inválidos
    │   └── Handlers/            # Handlers CQRS com NSubstitute (sem banco, sem Docker)
    └── Integration/             # HTTP end-to-end com Testcontainers (PostgreSQL real)
```

### Executar testes

```bash
# Apenas unitários (sem Docker)
dotnet test --filter "Category!=Integration"

# Todos (exige Docker)
dotnet test

# Com relatório de cobertura
dotnet test --filter "Category!=Integration" --collect:"XPlat Code Coverage"
```

### Cobertura (testes unitários, unit only)

| Projeto | Line rate | Branch rate |
|---|---|---|
| Agenda.Domain | 96.7% | 100% |
| Agenda.Application | 75.6% | 60% |

> `Agenda.Api` e `Agenda.Infrastructure` são cobertos pelos testes de integração.

### Pacotes de teste

| Pacote | Versão | Papel |
|---|---|---|
| xUnit | 2.9.2 | Framework de testes |
| Shouldly | 4.3.0 | Assertions fluentes |
| NSubstitute | 5.3.0 | Mocks (testes unitários) |
| Testcontainers.PostgreSql | 4.12.0 | PostgreSQL real via Docker (integração) |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.9 | WebApplicationFactory (integração) |

## Fases do Projeto

| Fase | Status | Escopo |
|---|---|---|
| 1 — Fundação | ✅ Completo | Solution, Domain, EF Core, Swagger, /health, Migration |
| 2 — CRUD + Application Layer | ✅ Completo | CQRS, MediatR, FluentValidation, AutoMapper, ProblemDetails |
| 2.1 — Contratos de erro | ✅ Completo | 400 ValidationProblemDetails, 409 race condition, 404 pt-BR |
| 3 — Testes | ✅ Completo | Unit + Integration (Testcontainers), 80 testes, cobertura reportada |
| 3.1 — Auth JWT | ✅ Completo | Login com credencial-semente, Bearer JWT, Swagger Authorize |
| 3.2 — Mensageria | ✅ Completo | RabbitMQ, domain events, Agenda.Contracts, Agenda.Worker |
| 4 — Frontend | Pendente | Vue 3, integração com a API |
| 5 — Deploy | Pendente | Dockerfile API, docker-compose completo, CI |

## Mensageria (RabbitMQ)

### Fluxo de eventos

```
POST /api/contacts
       │
       ▼
CreateContactHandler
  (salva no banco)
       │
       │  IPublisher.Publish(ContactCreatedDomainEvent)
       ▼
ContactCreatedDomainEventHandler
  (Application layer)
       │
       │  IIntegrationEventPublisher.PublishAsync(ContactCreatedIntegrationEvent)
       ▼
RabbitMqIntegrationEventPublisher
  (Infrastructure layer)
       │
       │  BasicPublishAsync — JSON, persistent=true
       ▼
Exchange: agenda.events  (topic, durable)
  routing key: contact.created
       │
       ▼
Queue: contact.created.welcome-email  (durable)
       │
       ▼
Agenda.Worker / ContactCreatedConsumer
  → LOG: "Sending welcome email to {Email} for contact {Name}"
  → BasicAckAsync (sucesso) / BasicNackAsync requeue=false (erro)
```

### Como rodar API + Worker juntos

**Terminal 1 — infraestrutura:**
```bash
docker compose up -d
# Aguarde postgres e rabbitmq ficarem healthy:
docker compose ps
```

**Terminal 2 — copiar config do Worker (primeira vez):**
```bash
cp src/Agenda.Worker/appsettings.Development.json.example \
   src/Agenda.Worker/appsettings.Development.json
```

> O `.example` já aponta para `localhost/guest/guest` — funciona sem alterações com o docker-compose local.

**Terminal 2 — API:**
```bash
cp src/Agenda.Api/appsettings.Development.json.example \
   src/Agenda.Api/appsettings.Development.json   # se ainda não fez
dotnet run --project src/Agenda.Api/Agenda.Api.csproj
```

**Terminal 3 — Worker:**
```bash
dotnet run --project src/Agenda.Worker/Agenda.Worker.csproj
```

Você verá no log do Worker:
```
info: Agenda.Worker.ContactCreatedConsumer[0]
      Connected to RabbitMQ at localhost:5672. Consuming from queue 'contact.created.welcome-email'
```

**Terminal 4 — criar um contato:**
```bash
# 1. Obter token
TOKEN=$(curl -s -X POST http://localhost:5196/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' | \
  grep -o '"token":"[^"]*"' | cut -d'"' -f4)

# 2. Criar contato
curl -X POST http://localhost:5196/api/contacts \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"João Silva","email":"joao@exemplo.com","phone":"11999999999"}'
```

O Worker logará imediatamente:
```
info: Agenda.Worker.ContactCreatedConsumer[0]
      Sending welcome email to joao@exemplo.com for contact João Silva (Id: <guid>)
```

### RabbitMQ Management UI

Acesse [http://localhost:15672](http://localhost:15672) com `guest / guest` para inspecionar exchanges, filas e mensagens.

### Nota de arquitetura — publish best-effort

O publisher atual é **best-effort**: se o RabbitMQ estiver indisponível no momento do `POST /api/contacts`, o erro é apenas logado e o contato é salvo normalmente. O evento de integração é perdido silenciosamente.

**Em produção o correto seria o padrão Transactional Outbox:**
1. Ao salvar o contato, gravar também o evento numa tabela `outbox_messages` — na mesma transação de banco.
2. Um background job lê a outbox e publica no broker.
3. Após confirmação do broker (BasicAck), marcar a mensagem como processada.

Isso garante entrega atômica: ou o contato e o evento são persistidos juntos, ou nenhum deles é. A implementação atual foi omitida intencionalmente para manter o escopo do desafio.

### Configuração RabbitMQ

| Chave | `appsettings.json` | `appsettings.Development.json` |
|---|---|---|
| `RabbitMq:Host` | `<RABBITMQ_HOST>` | `localhost` |
| `RabbitMq:Port` | `5672` | `5672` |
| `RabbitMq:Username` | `<RABBITMQ_USERNAME>` | `guest` |
| `RabbitMq:Password` | `<RABBITMQ_PASSWORD>` | `guest` |

Para ambientes não-Development, use variáveis de ambiente:
```
RabbitMq__Host=<host>
RabbitMq__Port=5672
RabbitMq__Username=<user>
RabbitMq__Password=<password>
```

Se a seção `RabbitMq` estiver ausente na configuração (ex.: testes), a Infrastructure registra automaticamente um `NoOpIntegrationEventPublisher` que descarta eventos silenciosamente — os 80 testes existentes passam sem nenhum broker.
