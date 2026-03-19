#!/usr/bin/env bash
set -euo pipefail

API_URL="${API_URL:-http://localhost:5220}"
OUT="src/api/generated.d.ts"

echo "Fetching OpenAPI spec from $API_URL/swagger/v1/swagger.json..."
npx openapi-typescript "$API_URL/swagger/v1/swagger.json" -o "$OUT"
echo "Types generated at $OUT"
