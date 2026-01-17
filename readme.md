# MyDB – Custom Database Engine with API and Laravel Client

MyDB is a lightweight custom relational database system built from scratch using C#. It implements the core internals of a database engine, exposes database functionality through a REST API, and demonstrates real-world usage via a Laravel web application.

The project focuses on understanding how a relational database works internally rather than competing with production databases like MySQL or PostgreSQL.

---

## Project Structure

```
MyDB/
├── MyDB.Core/        # Core database engine
│   ├── AST/          # Abstract Syntax Tree (statements & expressions)
│   ├── Parser/       # SQL parser
│   ├── Tokenizer/    # SQL tokenizer / lexer
│   ├── Storage/      # Table, schema, disk persistence
│   ├── REPL/         # Interactive SQL shell
│   └── data/         # JSON-backed database storage
│
├── MyDBApi/          # ASP.NET Core REST API
│   └── Program.cs
│
├── LaravelApp/       # Laravel client application
│   ├── app/
│   ├── routes/
│   └── resources/views/
│
└── README.md
```

Each layer is intentionally separated to keep responsibilities clear and the system easy to reason about.

---

## Core Database Engine (`MyDB.Core`)

The database engine is implemented entirely in C# and is responsible for parsing, validating, executing, and persisting SQL-like commands.

### Tokenizer

The tokenizer (lexer) reads raw SQL input and converts it into a stream of tokens such as keywords, identifiers, literals, operators, and symbols. This is the first stage of query processing and ensures the input can be understood structurally.

### Parser

The parser consumes tokens and builds an **Abstract Syntax Tree (AST)**. Each SQL command is represented as a strongly-typed AST node such as:

- `SelectStatement`
- `InsertStatement`
- `UpdateStatement`
- `DeleteStatement`
- `CreateTableStatement`

This design avoids string-based execution and allows safe, structured query handling.

### AST & Expressions

Expressions like column references, literals, and binary comparisons are represented explicitly (e.g. `ColumnExpression`, `LiteralExpression`, `BinaryExpression`). These are used for WHERE clauses, updates, joins, and validation logic.

### Execution & Validation

The executor walks the AST and applies logic against in-memory table structures:

- Column existence checks
- Primary key enforcement
- Unique constraint validation
- Type consistency
- Join resolution

Errors are raised early and clearly when constraints are violated.

---

## Data Storage (`/data` folder)

MyDB uses **JSON files as its persistence layer**, making the system transparent and easy to inspect.

The `/data` folder contains:

- `schema.json` – stores table definitions, columns, data types, primary keys, and unique constraints
- `<table>.json` – one file per table, containing all rows for that table

Example:

```
data/
├── schema.json
├── users.json
├── employees.json
```

Whenever a table is created, updated, or deleted, both the schema and table data are written back to disk. This ensures that:

- Data survives application restarts
- The database is human-readable
- No external database dependency is required

---

## Interactive REPL (SQL Shell)

MyDB includes an interactive **REPL (Read–Eval–Print Loop)** that allows users to execute SQL commands directly against the database engine without using the API or Laravel frontend.

The REPL is ideal for:

- Testing queries
- Debugging parser and execution logic
- Exploring database behavior interactively

### Using the REPL

Run the REPL project from the command line:

```bash
cd MyDB.Core
dotnet run
```

Once started, the REPL continuously accepts SQL input, parses it, executes it, and prints results or errors immediately.

---

## Supported REPL Commands

The REPL supports a focused subset of SQL, including:

### Table Creation

```sql
CREATE TABLE users (
    id INT PRIMARY KEY,
    name TEXT,
    email TEXT UNIQUE,
    phone TEXT
);
```

### Insert

```sql
INSERT INTO users (id, name, email, phone)
VALUES (1, 'Benjamin', 'b@example.com', '07064447205');
```

### Select

```sql
SELECT * FROM users;
```

### Where Clauses

```sql
SELECT * FROM users WHERE id = 1;
```

### Update

```sql
UPDATE users
SET name = 'Ben Ayo'
WHERE id = 1;
```

### Delete

```sql
DELETE FROM users WHERE id = 1;
```

### Inner Join

```sql
SELECT *
FROM users
INNER JOIN orders
ON users.id = orders.user_id;
```

INNER JOIN operations are parsed into join AST nodes and executed by matching rows across tables based on join conditions.

---

## REST API (`MyDBApi`)

The MyDB API is a minimal ASP.NET Core application that exposes the database engine over HTTP.

### Supported Operations

- `GET /{table}` – fetch all rows
- `POST /{table}` – insert a new row
- `PUT /{table}/{id}` – update a row by ID
- `DELETE /{table}/{id}` – delete a row by ID

The API dynamically maps requests to AST statements and executes them using the core engine. It acts as a clean bridge between the database and external clients.

---

## Laravel Client Application

The Laravel application demonstrates real-world usage of MyDB.

It:

- Consumes the MyDB API using HTTP requests
- Displays data using Blade views
- Supports creating, viewing, editing, and deleting users
- Shows that the custom database behaves like a real backend system

All user actions in the UI are reflected directly in the JSON files inside the `/data` folder.

---

## Setup & Usage

### Requirements

- .NET 7+
- PHP 8+
- Composer
- Node.js (for Laravel assets)

### Running the Database API

```bash
cd MyDBApi
dotnet run
```

- You can access http://localhost:5000/users and test CRUD actions using Postman and changes will be reflected in the /data folder

### Running the Laravel App

```bash
cd LaravelApp
composer install
php artisan serve
```

Configure the API URL in Laravel via `config/services.php`.

---

- You can access http://127.0.0.1:8000 and test CRUD actions in the web app.

## Project Goals

This project was inspired by a coding challenge to design and implement a relational database management system from scratch. Its goals are to understand how an RDBMS works internally, explore SQL tokenization, parsing, and execution, implement joins and constraint enforcement, build a persistence layer without external databases, and demonstrate how a custom database can power a real web application. The project is intentionally simple, educational, and transparent, prioritizing correctness and clarity over production optimizations.

## Tooling and Resources

During development, AI tools such as ChatGPT, Grok, and ClaudeAI were used to assist with debugging and architectural validation. Official .NET tutorials and documentation were used extensively, and Stack Overflow was consulted to resolve language-specific and runtime issues. These tools supported learning and problem-solving without replacing core implementation work.

This project demonstrates low-level systems thinking combined with full-stack integration and serves as both an educational reference and a technical portfolio piece.
