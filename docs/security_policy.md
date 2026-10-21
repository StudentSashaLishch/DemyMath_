# DemyMath Security Policy and Threat Model

## 1. Security Rules
- The server is authoritative for authentication, authorization, mathematical correctness, and persistent learning state.
- All client input must be validated on the server.
- Users may access only their own learning data. Administrative functions are restricted to administrators.
- Passwords must be securely hashed.
- Secrets and credentials must not be committed to Git or exposed in logs and error messages.
- State-changing form submissions must use ASP.NET Core antiforgery protection.
- Database queries must not concatenate untrusted input into SQL commands.

## 2. Administrator Access
- Public registration must create regular user accounts only.
- The initial administrator account must be created through a separate setup process unavailable through public registration.
- Additional administrator accounts may be created only by an authenticated administrator.
- Administrator privileges must be explicitly assigned and must not be granted by default.

## 3. Threat Model

| Threat | Scenario and potential impact |
|---|---|
| Account compromise | An attacker gains access to another user's account and views or changes their learning progress. |
| Privilege escalation | A regular user obtains administrator privileges and changes learning content or system settings. |
| Learning data tampering | A user manipulates exercise results or learning progress through requests to the server. |
| Unauthorized data access | A user accesses another user's data by changing an identifier in a request. |
| SQL injection | Crafted input alters a database query, potentially exposing or modifying stored data. |
| Cross-site request forgery (CSRF) | An authenticated user is tricked into performing an unwanted action. |
| Resource exhaustion | Excessive requests or resource-intensive operations make the application unavailable to other users. |
| Sensitive information disclosure | Passwords, secrets, or private data appear in logs, error messages, or responses to unauthorized users. |

## 4. Security Changes
Changes affecting authentication, authorization, administrator access, data protection, or mathematical correctness must be reviewed for security implications.