# Security Policy

## Security Model in QueryQuest

QueryQuest executes SQL queries submitted by users in a sandboxed learning environment.

### Sandbox Protections
1. **Isolated SQLite Sandbox**: Learner queries are executed strictly against a dedicated `sample_educational.db` database. No application data, progress logs, user data, or host system tables are accessible.
2. **Read-Only Enforcements**: The database connection string specifies `Mode=ReadOnly`.
3. **Keyword & AST Pre-Execution Guards**: Mutation operations (`INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `REPLACE`), database attachments (`ATTACH`, `DETACH`), and administrative PRAGMAs or extension loaders are rejected before query invocation.
4. **Statement Multiplexing Prevention**: Semicolon-separated multi-statements are rejected to avoid compound injection attempts.
5. **Execution Bounds**:
   - Queries are bounded to a maximum timeout of 4 seconds to eliminate infinite loops and explosive cartesian joins.
   - Result sets are capped at 200 rows.
   - Input queries are restricted to 4,000 characters.

## Reporting a Vulnerability

If you discover a sandbox escape or security vulnerability, please open an issue or contact the project maintainers directly with details and reproduction steps.
