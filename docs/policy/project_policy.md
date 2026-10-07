# DemyMath Project Policy

| | |
|---|---|
|Name|DemyMath Project Policy|
|Version|0.1.0|
|Status|Draft|
|Classification|Internal|
|Last Updated|2026-09-24|
|Owner|Project Author|

## Contents

1. Scope
2. Policy Changes
3. Technology Stack
4. Architectural Rules
5. Project Structure
6. Domain and Application Boundaries
7. Naming Conventions
8. Dependency Policy
9. Database Policy
10. API and HTTP Policy
11. Error Handling
12. Security
13. Testing Policy
14. Documentation Policy
15. AI-Assisted Development
16. Technical Debt
17. Anti-Patterns
18. Compliance

## 1. Scope

This policy defines mandatory engineering rules for the DemyMath web information system. It applies to source code, tests, configuration, database migrations, dependencies, and project documentation.

The policy applies to all project contributors and AI-assisted development tools.

## 2. Policy Changes

Changes to this policy are explicit project changes and require review by the project author.

A policy change that affects development rules, technologies, architecture, or project structure increments the policy version and is recorded in `change_log.md`.

Existing code is not changed solely to conform to a newly introduced rule unless the change is required for correctness, security, maintainability, or further development.

## 3. Technology Stack

The project uses the following technologies:

|Area|Technology|
|---|---|
|Backend language|C#|
|Runtime and framework|.NET 10 LTS|
|Web framework|ASP.NET Core 10|
|ORM|Entity Framework Core 10|
|Relational database|PostgreSQL 18|
|API description|OpenAPI|
|Client-side language|TypeScript|
|Automated testing|xUnit|
|Containerization|Docker|
|Version control|Git|


Additional frameworks and libraries require a concrete technical justification and must comply with the Dependency Policy.

Project dependencies use supported versions compatible with the selected technology stack.

## 4. Architectural Rules

The web application follows the **Model-View-Controller (MVC)** architectural pattern.

Controllers handle HTTP requests and coordinate application operations. Views are responsible for presentation. Models represent the data and behavior required by the corresponding application functionality.

Business rules do not belong in controllers or views.

The presentation layer does not directly implement domain rules or mathematical correctness.

The architecture separates presentation, application behavior, domain logic, and infrastructure concerns where such separation is required by their responsibilities.

## 5. Project Structure

The source code follows the MVC-based structure defined by the project architecture.

The main application structure is organized by responsibility rather than by individual framework types:

```text
src/
├── Controllers/
├── Models/
├── Views/
├── Services/
├── Domain/
├── Data/
└── ...
```

New directories and architectural layers are introduced only when they represent a distinct responsibility required by the system.

## 6. Domain and Application Boundaries

Business rules and mathematical behavior are independent of the presentation layer.

Controllers coordinate operations but do not contain domain rules.

Views do not determine application state or business correctness.

Client-side code may provide immediate validation and interaction logic, but server-side application logic remains authoritative for state-changing operations.

Domain logic does not depend on views, controllers, or client-side implementation details.

## 7. Naming Conventions

The project uses the following naming conventions:

|Element|Convention|Example|
|---|---|---|
|Classes|`PascalCase`|`EquationService`|
|Interfaces|`IPascalCase`|`IEquationService`|
|Methods|`PascalCase`|`SolveEquation()`|
|Properties|`PascalCase`|`SolutionCount`|
|Local variables|`camelCase`|`solutionCount`|
|Parameters|`camelCase`|`equation`|
|Constants|`PascalCase`|`DefaultTimeout`|
|Private fields|`_camelCase`|`_equationService`|
|Namespaces|`PascalCase`|`DemyMath.Models`|

MVC and application component names use their responsibility as the suffix:

| Component     | Convention              | Example                   |
| ------------- | ----------------------- | ------------------------- |
| Controller    | `<Name>Controller`      | `EquationController`      |
| View model    | `<Name>ViewModel`       | `CreateEquationViewModel` |
| Service       | `<Name>Service`         | `EquationService`         |
| Interface     | `I<Name>`               | `IEquationService`        |
| Repository    | `<Name>Repository`      | `EquationRepository`      |
| DTO           | `<Name>Dto`             | `EquationDto`             |
| Validator     | `<Name>Validator`       | `EquationValidator`       |
| Entity        | Descriptive domain name | `Equation`                |
| Configuration | `<Name>Configuration`   | `EquationConfiguration`   |
| Test class    | `<Name>Tests`           | `EquationServiceTests`    |

Names describe the responsibility or meaning of the element.

Abbreviations are avoided unless they are established technical terms.

Files normally contain one primary type, and the file name matches the primary type name.

## 8. Dependency Policy

A dependency is added only when it provides functionality that is required by the project and is not reasonably provided by the existing stack.

Before adding a dependency, its purpose, maintenance status, security implications, license compatibility, and compatibility with the project stack are considered.

Dependencies that duplicate existing functionality without a sufficient technical reason are not added.

Unused dependencies are removed.

Dependency versions are managed through the project's package management system and are committed as part of the project configuration.

Dependencies with known unresolved security issues are not introduced or intentionally retained when a supported alternative or update is available.

## 9. Database Policy

PostgreSQL is the project's relational database.

Entity Framework Core is the primary persistence framework.

Database schema changes are represented by version-controlled EF Core migrations.

Manual database schema modifications are not used as the normal development process.

Database-specific implementation details remain outside the domain logic.

Database constraints are used where they provide meaningful protection of persistent data integrity.

## 10. API and HTTP Policy

HTTP endpoints use explicit request and response contracts.

Persistence entities are not exposed directly through API contracts unless there is a documented technical reason.

State-changing requests are validated on the server.

HTTP status codes correspond to the result of the requested operation and its validation state.

API contracts are documented through OpenAPI.

Breaking changes to established API contracts are recorded in `change_log.md`.

## 11. Error Handling

Errors are represented at the level where they can be handled meaningfully.

Domain and application errors do not expose infrastructure-specific implementation details.

Infrastructure exceptions are not returned directly to clients.

API responses do not expose stack traces, database details, internal paths, credentials, or other sensitive implementation information.

Errors contain sufficient information for appropriate logging and diagnosis without exposing sensitive data to clients.

## 12. Security

Authentication and authorization are enforced on the server.

Client-side access restrictions are not treated as a security boundary.

User input is validated before it is used in application or persistence operations.

Secrets and credentials are not stored in source code or committed configuration files.

Sensitive configuration is supplied through the appropriate environment or deployment configuration.

The system stores only user data required by its functionality.

## 13. Testing Policy

The project uses automated tests for testable application behavior.

xUnit is the primary test framework.

Tests are deterministic and reproducible.

A defect fix includes a regression test when the affected behavior is testable.

The complete testing strategy, test types, test organization, and acceptance criteria are defined in `testing.md`.

## 14. Documentation Policy

Each project document has one defined responsibility. The following documents are authoritative for their respective concerns:

| Document                        | Responsibility                     |
| ------------------------------- | ---------------------------------- |
| `docs/business/domain.md`       | Domain concepts and business rules |
| `docs/testing.md`               | Testing strategy and rules         |
| `docs/README.md`                | Project setup and usage            |
| `docs/tech_debt.md`             | Known technical debt               |
| `docs/change_log.md`            | Significant project changes        |
| `docs/policy/project_policy.md` | Development policies               |

Documentation is updated when a change makes the affected documentation inaccurate.

Requirements, architecture, testing rules, AI-agent instructions, technical debt, and significant changes are documented in their respective project files rather than duplicated across documents.

## 15. AI-Assisted Development

AI-generated code is reviewed before being considered part of the project.

AI tools do not independently change requirements, architectural decisions, business rules, database schema, or project policy.

Changes generated with AI are subject to the same testing and review requirements as manually written code.

## 16. Technical Debt

Technical debt is recorded in `tech_debt.md` when it creates a meaningful maintenance, reliability, security, or architectural risk.

Technical debt entries contain a description, impact, location, priority, status, and planned resolution.

Temporary implementation compromises are not recorded as technical debt unless they create a meaningful future risk.

## 17. Anti-Patterns

The following practices are prohibited unless an explicit exception is documented:

|Anti-pattern|Prohibited practice|
|---|---|
|Layer leakage|A component directly performs responsibilities belonging to another architectural layer without justification|
|Business logic in controllers|Controllers contain domain rules or complex business operations|
|Business logic in views|Views determine authoritative application behavior|
|Client-authoritative validation|Client-side validation is treated as sufficient for state-changing operations|
|Persistence-driven domain|Domain logic depends directly on EF Core or database implementation details|
|God class|One class accumulates unrelated responsibilities|
|Unjustified abstraction|Interfaces, services, or abstractions exist without a concrete responsibility or use|
|Duplicate business rules|The same authoritative rule is independently implemented in multiple places|
|Hidden state changes|An operation changes application state without a clear and traceable code path|
|Uncontrolled dependencies|Libraries are added without technical justification or dependency review|
|Secret in source code|Credentials, tokens, or other secrets are committed to the repository|
|Outdated documentation|Documentation knowingly contradicts the implemented system|

## 18. Compliance

Compliance with this policy is mandatory.

A change that violates this policy is not considered complete until the violation is removed or an explicit exception is recorded.

Policy exceptions require a documented technical justification and approval by the project author. Convenience is not a sufficient justification.

Repeated or intentional violations are resolved before the affected work is accepted.