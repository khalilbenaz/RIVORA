#!/usr/bin/env bash
# Install git pre-commit hooks for RIVORA development
set -e

HOOKS_DIR="$(git rev-parse --git-dir)/hooks"
mkdir -p "$HOOKS_DIR"

cat > "$HOOKS_DIR/pre-commit" << 'HOOK'
#!/usr/bin/env bash
# RIVORA pre-commit hook: scan for secrets before committing

if command -v gitleaks &> /dev/null; then
    gitleaks protect --staged --redact -v
    if [ $? -ne 0 ]; then
        echo ""
        echo "❌ gitleaks detected secrets in staged files."
        echo "   Fix the issues above or use --no-verify to bypass (not recommended)."
        exit 1
    fi
else
    echo "⚠️  gitleaks not installed. Install: brew install gitleaks (macOS) or see https://github.com/gitleaks/gitleaks"
fi
HOOK

chmod +x "$HOOKS_DIR/pre-commit"
echo "✅ Pre-commit hook installed successfully."
