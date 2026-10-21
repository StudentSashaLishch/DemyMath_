# DemyMath Project Policy

## 1. Scope

Applies to all source projects, shared development rules, and documentation in the DemyMath repository.

## 2. Policy Changes

- Policy changes must be intentional and must not be bundled with implementation changes.
- Each policy change requires a separate commit containing only policy changes.

## 3. Technology

- Backend: C# and ASP.NET Core MVC.
- Database: PostgreSQL and Entity Framework Core.
- UI: Razor Views and Bootstrap.
- Additional dependencies require a clear technical justification.

## 4. Architecture

- `Domain` contains core entities and business rules and has no project dependencies.
- `Application` contains use cases and abstractions and references `Domain`.
- `Infrastructure` implements persistence and external integrations and references `Application` and `Domain`.
- `Web` contains HTTP handling, controllers, view models, views, and static assets. It references `Application` and `Infrastructure`.
- Controllers and views must not contain business logic or access the database directly.
- Business rules must not depend on UI or infrastructure implementation details.

## 5. Localization and Content

- Resource files contain only stable interface strings.
- Topics, theory, examples, exercises, hints, and other learning content are stored as data, with localized variants separated by language.
- Learning content must not be hardcoded in views or application logic.

## 6. Project Structure

The repository must follow this structure:

    DemyMath/
      DemyMath.sln
      README.md
      docs/
        project_policy.md
        business_logic.md
        security_policy.md
        tech_debt.md
        change_log.md
      src/
        DemyMath.Domain/
		...
        DemyMath.Application/
		...
        DemyMath.Infrastructure/
		...
        DemyMath.Web/
		...

- `README.md` describes the project and how to run it.
- `docs/project_policy.md` defines development and architectural constraints.
- `docs/business_logic.md` describes domain rules and learning logic.
- `docs/security_policy.md` records security requirements and decisions.
- `docs/tech_debt.md` tracks known technical debt.
- `docs/change_log.md` records significant project changes.
- `src` contains application projects. Each project owns its implementation and relevant tests or supporting files.

Documentation of individual types and members belongs in the source code when it clarifies non-obvious behavior or a public contract. Avoid comments that merely repeat the code.

## 7. Naming Policy

- Projects, namespaces, classes, records, interfaces, enums, methods, properties, and events use `PascalCase`.
- Local variables, parameters, and private fields use `camelCase`.
- Private instance fields use the `_camelCase` form.
- Interfaces start with `I`, for example `ITopicRepository`.
- Async methods end with `Async`, for example `GetTopicAsync`.
- Boolean members use meaningful predicates, such as `IsAvailable` or `HasPrerequisites`.
- Constants use `PascalCase`.
- Names must express intent and avoid unexplained abbreviations.
- Source files containing a single primary type must be named after that type.
- Project and namespace names must reflect their architectural role.

## 8. Anti-patterns

| Prohibited practice | Required approach |
| --- | --- |
| Business rules in controllers or views | Put rules in the appropriate domain or application component. |
| Direct database access from controllers or views | Use application use cases and infrastructure abstractions. |
| Domain depending on ASP.NET Core, EF Core, or database-specific code | Keep domain rules independent of delivery and persistence technologies. |
| Duplicated business rules across layers | Maintain one authoritative implementation of each rule. |
| Embedding mutable application data in source code or UI templates instead of retrieving it from the appropriate data source | Store and retrieve mutable data through its designated data source. |
| Database queries and persistence details scattered across the application | Centralize persistence in Infrastructure. |
| Introducing abstractions, layers, or dependencies without a defined responsibility or a concrete use case | Ensure each abstraction, layer, and dependency has a clear architectural purpose. |
| Silent changes to documented business rules or architecture | Update the relevant documentation and obtain an explicit decision. |