# CI/CD avec GitHub Actions

RIVORA inclut un pipeline CI/CD complet base sur GitHub Actions. Ce pipeline gere la compilation, les tests, l'analyse de securite, la revue AI et la publication automatique.

## Vue d'ensemble du pipeline

Le workflow principal (`.github/workflows/ci.yml`) s'execute a chaque push et pull request sur `main` :

```
push / PR -> Build Matrix -> Tests -> Coverage -> Security -> AI Review -> Publish
```

## Build Matrix

Le pipeline compile sur plusieurs configurations pour garantir la compatibilite :

```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
        dotnet-version: ['9.0.x']

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ matrix.dotnet-version }}

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Test
        run: dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage"
```

## Tests unitaires et couverture

Les tests sont executes avec `dotnet test` et les rapports de couverture sont generes au format Cobertura :

```yaml
      - name: Test with coverage
        run: |
          dotnet test --no-build --configuration Release \
            --collect:"XPlat Code Coverage" \
            --results-directory ./coverage \
            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v4
        with:
          token: ${{ secrets.CODECOV_TOKEN }}
          files: ./coverage/**/coverage.cobertura.xml
          fail_ci_if_error: true
```

### Configuration Codecov

Ajoutez un fichier `codecov.yml` a la racine du projet :

```yaml
coverage:
  status:
    project:
      default:
        target: 80%
        threshold: 2%
    patch:
      default:
        target: 90%
  ignore:
    - "**/*.Generated.cs"
    - "**/Migrations/**"
    - "tests/**"
```

## Analyse de securite

Le pipeline integre plusieurs outils de securite :

```yaml
  security:
    runs-on: ubuntu-latest
    needs: build
    steps:
      - uses: actions/checkout@v4

      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          scan-type: 'fs'
          format: 'sarif'
          output: 'trivy-results.sarif'

      - name: Upload Trivy scan results
        uses: github/codeql-action/upload-sarif@v3
        with:
          sarif_file: 'trivy-results.sarif'

      - name: .NET Security Audit
        run: |
          dotnet list package --vulnerable --include-transitive 2>&1 | tee audit.log
          if grep -q "has the following vulnerable packages" audit.log; then
            echo "::error::Vulnerable packages detected"
            exit 1
          fi
```

## AI Review automatique

RIVORA peut executer une revue de code automatique via la CLI :

```yaml
  ai-review:
    runs-on: ubuntu-latest
    needs: build
    if: github.event_name == 'pull_request'
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Install RVR CLI
        run: dotnet tool install --global RVR.CLI

      - name: Run AI Review
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          rvr ai review --all \
            --output sarif \
            --output-file ai-review.sarif

      - name: Upload AI Review results
        uses: github/codeql-action/upload-sarif@v3
        with:
          sarif_file: ai-review.sarif
```

## Job Frontend

Si votre projet inclut le frontend React, ajoutez un job dedie :

```yaml
  frontend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./src/frontend

    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: ./src/frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Lint
        run: npm run lint

      - name: Type check
        run: npx tsc --noEmit

      - name: Unit tests
        run: npm run test -- --coverage

      - name: Build
        run: npm run build

      - name: E2E tests
        run: npx playwright test
```

## Publication NuGet

Pour publier automatiquement les packages NuGet lors d'un tag :

```yaml
  publish:
    runs-on: ubuntu-latest
    needs: [build, security]
    if: startsWith(github.ref, 'refs/tags/v')

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Pack
        run: |
          VERSION=${GITHUB_REF#refs/tags/v}
          dotnet pack --configuration Release \
            -p:PackageVersion=$VERSION \
            --output ./nupkgs

      - name: Push to NuGet
        run: |
          dotnet nuget push ./nupkgs/*.nupkg \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate
```

## Configuration des secrets

Les secrets suivants doivent etre configures dans les parametres GitHub du repository :

| Secret | Description | Requis |
|--------|-------------|--------|
| `CODECOV_TOKEN` | Token Codecov pour les rapports de couverture | Oui |
| `NUGET_API_KEY` | Cle API NuGet pour la publication | Pour publish |
| `OPENAI_API_KEY` | Cle API OpenAI pour la revue AI | Pour AI review |
| `DOCKER_USERNAME` | Identifiant Docker Hub | Pour images Docker |
| `DOCKER_PASSWORD` | Mot de passe Docker Hub | Pour images Docker |

Pour configurer un secret :

1. Allez dans **Settings** > **Secrets and variables** > **Actions**
2. Cliquez sur **New repository secret**
3. Entrez le nom et la valeur

## Bonnes pratiques

- **Branch protection** : Exigez que tous les checks passent avant le merge
- **Concurrency** : Annulez les runs precedents sur la meme branche :

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

- **Cache NuGet** : Accelerez les builds avec le cache :

```yaml
      - name: Cache NuGet
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-
```

- **Notifications** : Configurez des notifications Slack ou Teams en cas d'echec
- **Dependabot** : Activez Dependabot pour les mises a jour automatiques des dependances

## Etape suivante

- [Docker](/guide/docker) pour la configuration des conteneurs
- [Native AOT](/guide/native-aot) pour optimiser les performances de build
- [Monitoring](/guide/monitoring) pour surveiller votre application en production
