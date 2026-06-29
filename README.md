# Agenda de Contatos

Desafio técnico Tech Lead: CRUD de agenda de contatos full-stack — backend .NET 10 (Clean Architecture, CQRS, JWT, RabbitMQ) + frontend Vue 3 + PrimeVue, tudo conteinerizado em um único `docker compose up --build`.

## Por que uma solução robusta para um problema simples?

Este desafio é, em essência, um CRUD. Uma solução proporcional ao problema caberia em poucas classes. A robustez aqui é deliberada e consciente: o objetivo foi demonstrar domínio de padrões e práticas esperados de uma posição de Tech Lead, não sugerir que toda essa estrutura seria apropriada para um CRUD deste porte em produção.

Cada decisão abaixo foi tomada sabendo do trade-off. A intenção é mostrar tanto a capacidade de aplicar essas ferramentas quanto o julgamento de saber quando elas se justificam — porque reconhecer que algo é over-engineering para um dado contexto é, em si, uma competência de liderança técnica.

### Decisões e seus trade-offs

- **Clean Architecture (Domain, Application, Infrastructure, Api).** Para um CRUD, uma estrutura em menos camadas bastaria. A separação foi mantida para demonstrar isolamento de responsabilidades e testabilidade. O domínio não depende de framework; a lógica de negócio é testável sem banco.

- **CQRS com MediatR.** Reconhecidamente over-engineering para este escopo. Aplicado para demonstrar o padrão. Sob CQRS, cada handler atua como o serviço de aplicação daquele caso de uso — isolado e testável. Seria justificado, em produção, com modelos de leitura e escrita divergentes ou alto volume de leitura. Alternativa proporcional ao problema: serviços de aplicação tradicionais.

- **Repositório sobre o EF Core.** O DbContext já implementa Unit of Work e Repository. A abstração foi mantida por aderência ao pedido do desafio e por testabilidade, com plena ciência da redundância.

- **Autenticação JWT com credencial-semente.** Demonstra emissão e validação de JWT de ponta a ponta sem inflar o escopo com um módulo completo de gestão de usuários. Em produção: store de usuários com hash de senha (ex.: ASP.NET Core Identity).

- **Mensageria com RabbitMQ (publisher + worker consumidor).** O caso de uso real para mensageria assíncrona é artificial num CRUD de contatos. Foi implementado por completo — publisher na API e um worker consumidor separado — para demonstrar uma arquitetura de eventos de verdade, não um publisher sem consumidor. A publicação é best-effort; em produção, o padrão correto para garantir entrega seria o Transactional Outbox (persistir o evento na mesma transação e publicá-lo de forma confiável depois).

- **Frontend desacoplado em camadas.** Services (HTTP) → composables (estado/lógica reativa) → views/componentes (UI), com componentes reutilizáveis via props/emits. Demonstra "uso forte de componentes" e separação de responsabilidades no front. O token JWT é mantido no localStorage pela praticidade do desafio; a alternativa mais segura contra XSS seria cookie httpOnly.

- **Cobertura de testes (80 no backend, 19 no front).** A suíte prioriza regras de negócio e contratos de erro, não a perseguição de um número. O que fica descoberto é startup, configuração e código gerado (migrations) — não lógica de negócio.

### Em resumo

A pergunta que guiou cada escolha não foi "qual a solução mais simples para um CRUD?", e sim "como demonstrar, num problema pequeno, as competências exigidas de um Tech Lead — incluindo o discernimento de saber o que seria exagero em produção?".

## Stack e Versões

### Backend

| Componente | Versão |
|---|---|
| .NET SDK | 10.0.x (target: `net10.0`) |
| C# | 14 (nullable enabled) |
| EF Core + Npgsql | 10.0.9 / 10.0.2 |
| PostgreSQL | 17 (Alpine) |
| RabbitMQ | 4 (management-alpine) |
| RabbitMQ.Client | 7.2.1 |
| MediatR | 14.1.0 |
| AutoMapper | 16.1.1 |
| FluentValidation | 12.1.1 |
| Swashbuckle | 7.3.1 |
| Docker / Compose | 29+ |

### Frontend

| Componente | Versão |
|---|---|
| Vue | 3.5.x (Composition API, `<script setup>`) |
| TypeScript | 6.0.x |
| Vite | 8.x |
| PrimeVue | 4.5.x (@primeuix/themes Aura) |
| Pinia | 3.x |
| Vue Router | 5.x |
| Axios | 1.x |
| Vitest + @vue/test-utils | 4.x / 2.x |
| nginx (produção) | Alpine |

## Arquitetura

```
agenda/
├── src/
│   ├── Agenda.Domain/          # Entities, domain invariants, domain exceptions
│   ├── Agenda.Contracts/       # Integration event records and messaging constants (no deps)
│   ├── Agenda.Application/     # CQRS use cases (MediatR), DTOs, FluentValidation, AutoMapper
│   ├── Agenda.Infrastructure/  # EF Core, DbContext, Migrations, RabbitMQ publisher
│   ├── Agenda.Api/             # Controllers, DI composition root, Swagger
│   └── Agenda.Worker/          # Background service — RabbitMQ consumer
├── tests/
│   └── Agenda.Tests/           # xUnit (unit + integration with Testcontainers)
└── frontend/                   # Vue 3 SPA
    ├── src/
    │   ├── services/           # Axios HTTP client + contactService + authService
    │   ├── composables/        # useContacts — reactive state and mutation logic
    │   ├── stores/             # Pinia: auth store (JWT token lifecycle)
    │   ├── views/              # LoginView, ContactsView
    │   └── components/         # ContactFormDialog (create/edit, client validation)
    └── Dockerfile              # multi-stage: node:22 build → nginx:alpine serve
```

Referências backend: `Api → Application, Infrastructure` | `Application → Domain, Contracts` | `Infrastructure → Application, Contracts` | `Worker → Contracts`

## Frontend

### Arquitetura em camadas

```
services (Axios)  →  composables (estado reativo)  →  views / components (UI)
```

- **`services/`** — funções puras que encapsulam as chamadas HTTP. `http.ts` configura o cliente Axios com interceptores de request (injeta `Bearer` token) e response (redireciona para `/login` em 401).
- **`composables/useContacts`** — encapsula `contacts`, `loading`, `errorMessage`, `page`, `pageSize`, `search` como refs reativas, além de `fetchContacts`, `createContact`, `updateContact`, `removeContact`. Os mutadores propagam erros para que a view possa tratá-los (ex.: 409 → mensagem de e-mail duplicado no dialog).
- **`stores/auth`** — Pinia store com `token` persistido em `localStorage`, `isAuthenticated` computed, `login` e `logout`.
- **Vue Router** — `beforeEach` guard protege `/contacts` (redireciona para `/login` sem token) e redireciona usuários autenticados que acessam `/login`.

### Componentes principais

- **`ContactsView`** — DataTable server-side (paginação `lazy`, `@page`), busca com debounce de 400 ms, botões de ação por linha.
- **`ContactFormDialog`** — Dialog reutilizável para criar e editar. Usa `v-model:visible` + `emit('submit', payload)`. Validação no cliente (nome 2–150 chars, e-mail formato, telefone opcional) com mensagens em pt-BR. Erros de servidor (409 e-mail duplicado) passados via prop `emailError` e exibidos no campo correspondente sem fechar o dialog.
- **PrimeVue ToastService + ConfirmationService** — toasts de sucesso/erro e confirmação antes de excluir.

### Testes (Vitest)

19 testes — executar com `cd frontend && npm run test:unit`:

| Suite | Testes | O que cobre |
|---|---|---|
| `useContacts.spec.ts` | 10 | loading, sucesso/erro do fetch, propagação de erro em mutações, refresh após mutação |
| `ContactFormDialog.spec.ts` | 8 | validação de campos, payload no emit, modo edição pré-preenchido, emailError prop |
| `App.spec.ts` | 1 | mount sem erro |

## Decisões arquiteturais (backend)

### CQRS com MediatR como Application Service

A camada Application expõe somente Commands e Queries (MediatR). Os controllers não conhecem repositórios, nem o domínio diretamente — apenas enviam mensagens via `ISender`. Isso mantém o domínio livre de dependências de framework e facilita testar cada handler de forma isolada.

### Soft delete

Deleção não remove o registro do banco. O EF Core aplica `HasQueryFilter(c => !c.IsDeleted)` globalmente, tornando os registros excluídos invisíveis a todas as queries sem necessidade de filtro manual.

### ProblemDetails (RFC 7807)

Erros são sempre serializados como `ProblemDetails` via `IExceptionHandler` (`GlobalExceptionHandler`). Isso garante contrato uniforme de erros para qualquer cliente REST. O frontend consome `ValidationProblemDetails` (400) por campo para exibir erros inline nos inputs.

## Bibliotecas e licenciamento

| Biblioteca | Licença | Observação |
|---|---|---|
| **MediatR 14** | Comercial (Lucky Penny Software) | Tier Community gratuito para OSS/pequenos projetos. Logs de licença são esperados — não são erros de build. |
| **AutoMapper 16** | Comercial (Lucky Penny Software) | Mesmo modelo do MediatR. |
| **FluentValidation 12** | MIT | Livre para uso comercial. |
| **PrimeVue 4** | MIT | Componentes UI; tema Aura incluído em `@primeuix/themes`. |
| Alternativas backend sem custo | — | [mediator](https://github.com/martinothamar/Mediator) (source generator), [Mapperly](https://github.com/riok/mapperly) (source gen), [mapster](https://github.com/MapsterMapper/Mapster) |

## Pré-requisitos

| Caminho | Requisitos |
|---|---|
| Docker (recomendado) | [Docker Desktop](https://www.docker.com/products/docker-desktop/) com Compose v2 |
| Desenvolvimento local (backend) | Docker Desktop + [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) + `dotnet tool install -g dotnet-ef` |
| Desenvolvimento local (frontend) | [Node.js 22+](https://nodejs.org/) |

## Como Rodar

```bash
git clone https://github.com/2hwebsites/agenda-desafio-blue.git
cd agenda-desafio-blue
docker compose up --build
```

O compose sobe 5 serviços em ordem: **postgres** → **rabbitmq** → **api** (migrations + seed) → **worker** → **frontend** (build Vite → nginx). Aguarde 60–120 s no primeiro build.

| Recurso | URL |
|---|---|
| **Aplicação web** | **http://localhost:5173** |
| Swagger UI | http://localhost:8080/swagger |
| Health check | http://localhost:8080/health |
| RabbitMQ Management | http://localhost:15672 (guest / guest) |

**Credenciais de login:** usuário `admin` · senha `admin123`

> As credenciais no `docker-compose.yml` são descartáveis e servem apenas para demonstração local. O `appsettings.json` versionado contém apenas placeholders; valores reais entram por variáveis de ambiente em produção.

```bash
docker compose down      # mantém volumes
docker compose down -v   # remove volumes (banco limpo)
```

### Alternativa: desenvolvimento local (sem Docker)

```bash
# 1. Infra
docker compose up -d postgres rabbitmq

# 2. Backend (uma vez: copiar .example → .Development.json)
cp src/Agenda.Api/appsettings.Development.json.example src/Agenda.Api/appsettings.Development.json
cp src/Agenda.Worker/appsettings.Development.json.example src/Agenda.Worker/appsettings.Development.json
dotnet run --project src/Agenda.Api/Agenda.Api.csproj      # terminal 1 — Swagger em :5196
dotnet run --project src/Agenda.Worker/Agenda.Worker.csproj # terminal 2

# 3. Frontend
cd frontend && cp .env.example .env.development && npm install && npm run dev
# UI em http://localhost:5173
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

**Erros de validação → 400 com `ValidationProblemDetails`:** erros agrupados por campo, exibíveis inline no frontend.

## Autenticação

A API usa **JWT Bearer (HMAC-SHA256)**. Endpoints de `/api/contacts` exigem `Authorization: Bearer <token>`; `/api/auth/login` e `/health` são públicos.

> **Implementação demonstrativa:** credencial definida em configuração (`AuthSeed`), sem tabela de usuários ou hash persistido. Em produção: ASP.NET Core Identity com hash de senha.

Para autenticar no Swagger: faça `POST /api/auth/login`, copie o `token` da resposta, clique em **Authorize** e cole o token (sem o prefixo "Bearer").

### Configuração JWT

| Chave | `appsettings.json` | `appsettings.Development.json` |
|---|---|---|
| `Jwt:Key` | `<JWT_SECRET>` | valor longo ≥ 32 chars |
| `Jwt:Issuer` | `<JWT_ISSUER>` | `agenda-api-dev` |
| `Jwt:Audience` | `<JWT_AUDIENCE>` | `agenda-frontend-dev` |
| `Jwt:ExpiresMinutes` | `60` | `60` |
| `AuthSeed:Username` | `<SEED_USERNAME>` | `admin` |
| `AuthSeed:Password` | `<SEED_PASSWORD>` | `admin123` |

## Connection String

`appsettings.json` contém apenas placeholders. Para outros ambientes, use variável de ambiente:

```
ConnectionStrings__Default=Host=...;Port=5432;Database=agenda;Username=...;Password=...
```

## Migrations

Para rodar migrations fora do startup (ex: CI):

```bash
dotnet ef database update \
    --project src/Agenda.Infrastructure/Agenda.Infrastructure.csproj \
    --startup-project src/Agenda.Api/Agenda.Api.csproj
```

## Testes e cobertura

```
tests/Agenda.Tests/
├── Unit/        # Domain, Validators, Handlers (NSubstitute, sem banco)
└── Integration/ # HTTP end-to-end com Testcontainers (PostgreSQL real)
```

```bash
dotnet test --filter "Category!=Integration"   # unitários (sem Docker)
dotnet test                                    # todos (exige Docker)
dotnet test --collect:"XPlat Code Coverage"    # cobertura combinada
```

### Cobertura (unit + integração combinados)

| Projeto | Line rate | Branch rate |
|---|---|---|
| Agenda.Application | 100% | 100% |
| Agenda.Domain | 96.6% | 100% |
| Agenda.Infrastructure | 93.9% | 100% |
| Agenda.Api | 84.5% | 62.5% |

80 testes: 57 unitários + 23 de integração com Testcontainers. O que fica descoberto é startup/configuração e código gerado pelo EF Core (migrations) — não lógica de negócio.

Pacotes: xUnit 2.9.2, Shouldly 4.3.0, NSubstitute 5.3.0, Testcontainers.PostgreSql 4.12.0, Microsoft.AspNetCore.Mvc.Testing 10.0.9.

Frontend: `cd frontend && npm run test:unit` — 19 testes Vitest.

## Fases do Projeto

| Fase | Status | Escopo |
|---|---|---|
| 1 — Fundação | ✅ Completo | Solution, Domain, EF Core, Swagger, /health, Migration |
| 2 — CRUD + Application Layer | ✅ Completo | CQRS, MediatR, FluentValidation, AutoMapper, ProblemDetails |
| 2.1 — Contratos de erro | ✅ Completo | 400 ValidationProblemDetails, 409 race condition, 404 pt-BR |
| 3 — Testes | ✅ Completo | Unit + Integration (Testcontainers), 80 testes, cobertura reportada |
| 3.1 — Auth JWT | ✅ Completo | Login com credencial-semente, Bearer JWT, Swagger Authorize |
| 3.2 — Mensageria | ✅ Completo | RabbitMQ, domain events, Agenda.Contracts, Agenda.Worker |
| 4 — Frontend | ✅ Completo | Vue 3 + PrimeVue, login JWT, CRUD completo, 19 testes Vitest |
| 5 — Deploy | ✅ Completo | Dockerfiles multi-stage, docker-compose 5 serviços, clone-and-run |

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

Se a seção `RabbitMq` estiver ausente na configuração (ex.: testes), a Infrastructure registra automaticamente um `NoOpIntegrationEventPublisher` que descarta eventos silenciosamente — os 80 testes existentes passam sem nenhum broker.
