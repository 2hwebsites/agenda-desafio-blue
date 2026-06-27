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
