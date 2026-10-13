# QueryQuest

> **Learn SQL by solving real challenges.**

An interactive, polished SQL learning platform and execution playground built with C#, ASP.NET Core, and SQLite. Learners practice real SQL queries against realistic relational databases directly in their browser without creating an account or provisioning cloud infrastructure.

---

## 🌟 Key Features

- **Actual SQLite Execution Engine**: Executes genuine SQL in a hardened, read-only isolated SQLite sandbox (not simulated or mocked).
- **Curriculum of 24 Structured Challenges**: Spans 6 key SQL categories:
  1. **SQL Basics**: `SELECT`, column projection, `WHERE` equality, numerical boundaries.
  2. **Filtering and Sorting**: `ORDER BY`, `BETWEEN`, `IN`, `LIKE` pattern matching.
  3. **Aggregation**: `COUNT`, `AVG`, `ROUND`, `GROUP BY`, `HAVING`.
  4. **Joins**: `INNER JOIN`, multi-table joins, computed expressions, anti-joins with `LEFT JOIN ... WHERE IS NULL`.
  5. **Subqueries & Advanced Queries**: Scalar subqueries, group extrema, correlated subqueries, Common Table Expressions (`WITH`).
  6. **Data Analysis**: Product revenue calculations, time-series monthly aggregates, window functions (`DENSE_RANK() OVER (...)`), Top-N analysis with `LIMIT`.
- **Result-Based Validation**: Evaluates actual data output equivalence instead of fragile string comparison. Supports multiple valid query syntax variants and handles NULLs and floating-point precision.
- **Intelligent Feedback & Progressive Hints**: Explains why queries fail (row count mismatch, column mismatches, sorting issues) and provides 3-tier progressive hints.
- **Persistent Local Progress & Gamification**: Local EF Core SQLite database records completed challenges, quest points, real accuracy rates, consecutive day streaks, and achievement badges.
- **Playground Mode**: Free exploration environment to inspect database tables, schemas, sample rows, and execute ad-hoc SQL queries.
- **Modern Developer UI**: Deep navy dark mode with support for light theme, responsive 3-panel learning layout, and accessible controls.

---

## 📐 Architecture & Technology Stack

```
QueryQuest/
├── src/
│   └── QueryQuest.Api/
│       ├── Controllers/       # Challenges, Query, Progress, and Health endpoints
│       ├── Data/              # EF Core AppDbContext, ChallengeCatalog (24 challenges), Seed data
│       ├── Models/            # Domain entities and strongly-typed DTOs
│       ├── Services/          # SqlExecutionEngine, ChallengeValidator, ProgressService
│       ├── wwwroot/           # Frontend SPA (Vanilla ES Modules, CSS3, HTML5)
│       └── Program.cs         # Dependency injection, middleware pipeline, SPA fallback
├── tests/
│   └── QueryQuest.Tests/      # xUnit tests, SqlExecutionEngine tests, Validator tests, Integration tests
├── .github/workflows/         # GitHub Actions CI build & test automation
└── QueryQuest.sln             # Root Solution file
```

- **Backend**: C# 12 / .NET 8, ASP.NET Core Web API, Entity Framework Core 8, Microsoft.Data.Sqlite.
- **Frontend**: HTML5, Modern CSS3 with CSS variables, Vanilla JavaScript ES Modules, Fetch API.
- **Databases (Separation of Concerns)**:
  - `sample_educational.db`: Disposable, read-only educational sandbox containing `employees`, `departments`, `customers`, `products`, `orders`, and `order_items`.
  - `queryquest_metadata.db`: Application metadata database tracking user progress, submission logs, and badge states.

---

## 🔒 Security Model

QueryQuest treats all user SQL queries as untrusted input:
- **Read-Only Mode**: SQLite opened with `Mode=ReadOnly` and isolated from system tables.
- **Mutation Rejection**: Rejects `INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `REPLACE`, `TRUNCATE`, and schema modifications.
- **Sandbox Boundary**: Denies `ATTACH`, `DETACH`, `PRAGMA`, and file system access.
- **Execution Limits**: Hard timeout of 4 seconds and maximum row limit of 200 rows to prevent Denial-of-Service and runaway Cartesian joins.
- **Multi-Statement Guard**: Semicolon-separated compound statements are rejected.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (version 8.0.x or higher)

### Installation & Run

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/queryquest.git
   cd queryquest
   ```

2. **Restore and Build**:
   ```bash
   dotnet restore QueryQuest.sln
   dotnet build QueryQuest.sln
   ```

3. **Run the Application**:
   ```bash
   dotnet run --project src/QueryQuest.Api/QueryQuest.Api.csproj
   ```

4. **Open in Browser**:
   Navigate to `http://localhost:5128` (or the URL displayed in the terminal console).

---

## 🧪 Running Automated Tests

Run the full suite of unit, security, and integration tests:

```bash
dotnet test QueryQuest.sln --verbosity normal
```

The test suite verifies:
- Read queries, joins, aggregates, CTEs, and window functions execution.
- Security rejection of mutation statements (`DROP`, `DELETE`, `INSERT`, `UPDATE`, `ATTACH`).
- Multi-statement injection denial.
- Timeout protection and row capping.
- Result-based validation with ordering requirements and floating-point tolerance.
- End-to-end HTTP API integration with progress persistence and reset.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
