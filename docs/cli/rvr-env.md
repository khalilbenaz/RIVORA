# rvr env

Manage environments, configuration, and secrets for RIVORA projects.

## Syntax

```bash
rvr env <subcommand> [options]
```

## Subcommands

### list
List available environments detected from `appsettings.*.json` files.
```bash
rvr env list
```

### get
Read a configuration value from the current environment.
```bash
rvr env get <key>
rvr env get ConnectionStrings:Default
rvr env get Logging:LogLevel:Default
```

### set
Set a configuration value (supports nested keys with `:` separator).
```bash
rvr env set <key> <value>
rvr env set DB:Host "localhost"
rvr env set Logging:LogLevel:Default "Warning"
```

### remove
Remove a configuration key.
```bash
rvr env remove <key>
rvr env remove DB:Host
```

### switch
Change the active environment (updates `launchSettings.json`).
```bash
rvr env switch <environment>
rvr env switch Production
rvr env switch Staging
```

### diff
Compare configuration between two environments. Sensitive values are masked.
```bash
rvr env diff <env1> <env2>
rvr env diff Development Production
```

### secrets init
Initialize .NET User Secrets for the project.
```bash
rvr env secrets init
```

### secrets set
Set a secret value via .NET User Secrets.
```bash
rvr env secrets set <key> <value>
rvr env secrets set "AI:OpenAI:ApiKey" "sk-..."
```

### export
Export configuration to file (dotenv, json, yaml). Sensitive values are masked with `<REPLACE_ME>`.
```bash
rvr env export --format dotenv    # -> .env
rvr env export --format json      # -> env.development.json
rvr env export --format yaml      # -> env.development.yml
```

### import
Import configuration from a `.env` file into `appsettings.{env}.json`.
```bash
rvr env import --file .env
rvr env import --file config.env
```

## Examples

```bash
# Full workflow
rvr env secrets init
rvr env secrets set "DB:Password" "SuperSecret123!"
rvr env set DB:Host "db.production.com"
rvr env diff Development Production
rvr env export --format dotenv
```
