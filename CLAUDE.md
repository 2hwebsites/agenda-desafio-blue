# CLAUDE.md — Project conventions for Claude Code

## Coding conventions

### Language of identifiers
All code identifiers **must be in English**: class names, interfaces, methods, properties,
fields, variables, parameters, enums, namespaces, file names, and database table/column names
(including migration names).

This applies to every layer — Domain, Application, Infrastructure, API, and tests.

### C#/.NET naming rules
Follow standard C#/.NET conventions:
- **PascalCase** for types (classes, records, structs, enums, interfaces) and public members
  (methods, properties, events).
- **camelCase** for local variables and method parameters.
- **_camelCase** (underscore prefix) for private fields.
- Interfaces prefixed with `I` (e.g. `IContactRepository`).
- `async` methods suffixed with `Async` (e.g. `GetByIdAsync`).

### Database naming
- Table names: **snake_case plural** (e.g. `contacts`, `audit_logs`).
- Column names: **snake_case** (e.g. `created_at`, `is_deleted`).
- Index names: `ix_<table>_<columns>` (e.g. `ix_contacts_email`).
- Migration names: PascalCase English description (e.g. `InitialCreate`, `AddContactPhone`).

### Exceptions — intentional deviations
1. **"Agenda" is the product/solution name** — keep it as-is in project names and namespaces
   (`Agenda.Domain`, `Agenda.Application`, `Agenda.Infrastructure`, `Agenda.Api`,
   `Agenda.Tests`). Do **not** rename the solution or projects.
2. **User-facing validation messages** may remain in Portuguese (`pt-BR`) because the frontend
   targets Brazilian users. Keep them consistent across the codebase. When adding new validation
   messages, write them in Portuguese.

### Commit messages and documentation
- Git commit messages: English, conventional commits style
  (`feat:`, `fix:`, `refactor:`, `chore:`, `docs:`, `test:`).
- Code comments: English only.
- Technical documentation (README sections, ADRs): English.
- Exception: `README.md` step-by-step instructions for running the app may mix English headings
  with Portuguese prose to stay accessible to the local team.

### File organization
- One top-level type per file; file name matches the type name (e.g. `Contact.cs` contains
  `class Contact`).
- Folder structure follows Clean Architecture layers; keep each layer's concerns inside its
  own project.

---

## Workflow / Definition of Done

These rules apply automatically to **every change**, without needing to be requested each time.

### Build and tests must be green before committing
1. After completing any requested change, run `dotnet build Agenda.sln` — the build **must**
   succeed with **zero errors and zero warnings**.
2. When tests exist, run `dotnet test` — all tests must pass.
3. When a frontend exists, run its type-check/lint/build step (e.g. `npm run type-check`,
   `npm run build`) — it must pass.
4. **Never commit or push if anything fails.** Fix the root cause first, then commit.

### Commit discipline
- Only after the build/tests are green: stage the relevant files, write a clear commit message
  in **conventional commits** format (`feat:`, `fix:`, `refactor:`, `test:`, `chore:`,
  `docs:`), and run `git push` to keep the remote in sync.
- Keep commits **incremental and meaningful**: one logical change per commit. Never bundle
  unrelated changes into a single commit.
- Commit messages must be in **English**.

### Secret hygiene — non-negotiable
- **Never commit secrets**: real connection strings, JWT signing keys, API tokens, license
  keys, OAuth client secrets, or any `.env` file.
- Keep secrets out of version control via `.gitignore` and use `appsettings.Development.json`
  (gitignored), `dotnet user-secrets`, or environment variables for local development.
- `appsettings.json` (committed) must contain only **placeholder values** such as
  `<DB_USER>`, `<DB_PASS>`, `<JWT_SECRET>`.
- **Before every commit**: verify that no secret is staged — check `git diff --cached` and
  `git status`.

### Never push broken code
- Pushing non-compiling, failing-test, or partially-implemented code to `main` is forbidden.
- If a change is too large for a single safe commit, break it into smaller, independently
  compilable steps.
