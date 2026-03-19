# rvr generate

Generate code scaffolds for aggregates, CRUD operations, commands, queries, API clients, tests, and seeders.

## Usage

```bash
rvr generate <subcommand> <name> [options]
```

## Subcommands

### aggregate

Generate a DDD aggregate root with value objects and domain events.

```bash
rvr generate aggregate Order \
  --props "OrderNumber:string,Total:decimal,Status:OrderStatus"
```

### crud

Generate a full CRUD stack: entity, command handlers, query handlers, controller, and tests.

```bash
rvr generate crud Invoice \
  --props "Reference:string,Amount:decimal,DueDate:DateTime"
```

This creates files across all layers:

- `Domain/Entities/Invoice.cs`
- `Application/Commands/CreateInvoiceCommand.cs`
- `Application/Commands/UpdateInvoiceCommand.cs`
- `Application/Commands/DeleteInvoiceCommand.cs`
- `Application/Queries/GetInvoiceQuery.cs`
- `Application/Queries/GetInvoicesQuery.cs`
- `Api/Controllers/InvoicesController.cs`
- `Tests/InvoiceTests.cs`

### command

Generate a single CQRS command with handler and validator.

```bash
rvr generate command ApproveOrder --aggregate Order
```

### query

Generate a single CQRS query with handler.

```bash
rvr generate query GetOverdueInvoices --returns "List<InvoiceDto>"
```

### client

Generate a typed HTTP client for an API endpoint.

```bash
rvr generate client InvoiceApi --base-url "https://api.example.com"
```

### test

Generate unit and integration test scaffolds for an entity or feature.

```bash
rvr generate test Invoice --type unit
rvr generate test Invoice --type integration
```

### seed

Generate a data seeder class for an entity.

```bash
rvr generate seed Product
```

## Common Options

| Flag | Description | Default |
|------|-------------|---------|
| `--props` | Comma-separated property definitions (`Name:Type`) | None |
| `--module` | Target module name | Default module |
| `--output`, `-o` | Output directory override | Auto-detected |
| `--force` | Overwrite existing files | `false` |
| `--dry-run` | Preview generated files without writing | `false` |

## Examples

Generate CRUD with dry-run to preview:

```bash
rvr generate crud Customer \
  --props "Name:string,Email:string,IsActive:bool" \
  --dry-run
```

Generate a command in a specific module:

```bash
rvr generate command ProcessPayment \
  --aggregate Payment \
  --module Billing
```

Run a seeder after generation:

```bash
rvr generate seed Product
rvr seed --profile demo
```
