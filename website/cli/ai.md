# rvr ai

AI-powered commands for code review, interactive chat, code generation, and architecture design.

## Usage

```bash
rvr ai <subcommand> [options]
```

## Subcommands

### chat

Start an interactive AI chat session scoped to your project context.

```bash
rvr ai chat
rvr ai chat --provider claude
rvr ai chat --provider ollama --model codellama
```

### generate

Generate code from a natural language description.

```bash
rvr ai generate "Create a payment service with Stripe integration"
rvr ai generate "Add email notification when order status changes" --provider openai
```

### review

Run AI-powered code analysis on your project.

```bash
# Run all analyzers
rvr ai review --all

# Run specific analyzers
rvr ai review --architecture    # Clean Architecture conformance
rvr ai review --ddd             # DDD anti-pattern detection
rvr ai review --performance     # N+1 queries, missing async, EF anti-patterns
rvr ai review --security        # OWASP vulnerability scanning
```

### design

Launch an interactive architecture design assistant.

```bash
rvr ai design
rvr ai design --provider openai
```

## Providers

| Provider | Flag | Description |
|----------|------|-------------|
| OpenAI | `--provider openai` | GPT-4 and GPT-3.5 models |
| Claude | `--provider claude` | Anthropic Claude models |
| Ollama | `--provider ollama` | Local/offline models |

Configure the default provider in `rivora.json`:

```json
{
  "ai": {
    "defaultProvider": "claude",
    "openai": { "apiKey": "${OPENAI_API_KEY}" },
    "claude": { "apiKey": "${CLAUDE_API_KEY}" },
    "ollama": { "endpoint": "http://localhost:11434" }
  }
}
```

## Review Options

| Flag | Description |
|------|-------------|
| `--all` | Run all analyzers |
| `--architecture` | Check Clean Architecture conformance |
| `--ddd` | Detect DDD anti-patterns |
| `--performance` | Find performance issues |
| `--security` | Scan for OWASP vulnerabilities |
| `--output <format>` | Output format: `console`, `json`, `sarif` |
| `--output-file <path>` | Write results to file |
| `--severity <level>` | Minimum severity: `info`, `warning`, `error` |

## SARIF Output for CI/CD

Generate SARIF reports for integration with GitHub Code Scanning or Azure DevOps:

```bash
rvr ai review --all --output sarif --output-file review.sarif
```

Use in a GitHub Actions workflow:

```yaml
- name: AI Review
  run: rvr ai review --all --output sarif --output-file results.sarif

- name: Upload SARIF
  uses: github/codeql-action/upload-sarif@v3
  with:
    sarif_file: results.sarif
```

## Examples

Offline review with Ollama:

```bash
rvr ai review --all --provider ollama
```

Generate code and review it:

```bash
rvr ai generate "Create a notification aggregate with email and SMS channels"
rvr ai review --architecture --ddd
```
