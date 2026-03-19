"""
predict_match.py — Prédiction interactive de résultat football.

Charge le modèle entraîné (modele_foot.pkl) et les profils d'équipe
(dataset_football.csv), puis demande une confrontation à l'utilisateur.
"""

import sys
import joblib
import numpy as np
import pandas as pd

# ── Configuration ──────────────────────────────────────────────────────────────

MODEL_FILE = "modele_foot.pkl"
TEAM_STATS_FILE = "dataset_football.csv"

PER_MATCH_COLS = ["Diff_Buts", "Corners", "Fautes", "Cartons_Jaunes", "Cartons_Rouges"]


# ── Fonctions ──────────────────────────────────────────────────────────────────

def load_artifacts():
    """Charge le modèle et les profils d'équipe."""
    artifact = joblib.load(MODEL_FILE)
    df = pd.read_csv(TEAM_STATS_FILE)
    for col in PER_MATCH_COLS:
        df[col] = (df[col] / df["Matchs_Joues"]).round(3)
    teams = df.set_index("Equipe")
    return artifact, teams


def fuzzy_match(name: str, valid_names: list[str]) -> str | None:
    """Correspondance souple : insensible à la casse et aux espaces."""
    name_lower = name.strip().lower()
    for valid in valid_names:
        if valid.lower() == name_lower:
            return valid
    # Correspondance partielle
    matches = [v for v in valid_names if name_lower in v.lower() or v.lower() in name_lower]
    if len(matches) == 1:
        return matches[0]
    return None


def ask_team(prompt: str, valid_names: list[str]) -> str:
    """Demande un nom d'équipe avec validation et suggestions."""
    while True:
        raw = input(prompt).strip()
        if not raw:
            continue
        match = fuzzy_match(raw, valid_names)
        if match:
            return match
        print(f"  ✗ Équipe « {raw} » introuvable.")
        close = [v for v in valid_names if raw.lower()[:3] in v.lower()]
        if close:
            print(f"    Suggestions : {', '.join(close)}")
        print()


def compute_diff(profiles: pd.DataFrame, team_a: str, team_b: str, feature_cols: list[str]) -> np.ndarray:
    """Calcule le vecteur de différences entre deux équipes."""
    prof_a = profiles.loc[team_a, feature_cols]
    prof_b = profiles.loc[team_b, feature_cols]
    return (prof_a - prof_b).values.reshape(1, -1)


def display_prediction(team_a: str, team_b: str, probas: np.ndarray, label_names: list[str]):
    """Affiche les probabilités de manière visuelle."""
    # probas : [Victoire Ext, Nul, Victoire Dom]
    p_dom = probas[2] * 100
    p_nul = probas[1] * 100
    p_ext = probas[0] * 100

    winner_idx = np.argmax(probas)
    outcomes = [f"Victoire {team_b}", "Match Nul", f"Victoire {team_a}"]
    verdict = outcomes[winner_idx]

    width = max(len(team_a), len(team_b), 10)

    print(f"\n{'─' * 52}")
    print(f"  {team_a:>{width}}  (DOM)  vs  (EXT)  {team_b}")
    print(f"{'─' * 52}\n")

    bars = [
        (f"🏠 {team_a}", p_dom),
        ("🤝 Nul", p_nul),
        (f"✈️  {team_b}", p_ext),
    ]

    for label, pct in bars:
        bar_len = int(pct / 100 * 30)
        bar = "█" * bar_len + "░" * (30 - bar_len)
        marker = " ◀" if pct == max(p_dom, p_nul, p_ext) else ""
        print(f"  {label:<20s}  {bar}  {pct:5.1f}%{marker}")

    print(f"\n  ➤ Prédiction : {verdict}")
    print(f"{'─' * 52}\n")


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    print("╔══════════════════════════════════════════════════╗")
    print("║       ⚽  Prédiction de Match — Ligue 1  ⚽      ║")
    print("╚══════════════════════════════════════════════════╝\n")

    artifact, profiles = load_artifacts()
    model = artifact["model"]
    scaler = artifact["scaler"]
    feature_cols = artifact["feature_cols"]
    # Extraire les noms bruts des features (sans prefix diff_)
    raw_features = [c.replace("diff_", "") for c in feature_cols]

    valid_teams = sorted(profiles.index.tolist())
    print(f"Équipes disponibles :\n  {', '.join(valid_teams)}\n")

    while True:
        team_a = ask_team("Équipe à domicile : ", valid_teams)
        team_b = ask_team("Équipe à l'extérieur : ", valid_teams)

        if team_a == team_b:
            print("  ✗ Une équipe ne peut pas jouer contre elle-même.\n")
            continue

        X_diff = compute_diff(profiles, team_a, team_b, raw_features)
        X_scaled = scaler.transform(X_diff)
        probas = model.predict_proba(X_scaled)[0]

        display_prediction(team_a, team_b, probas, artifact["label_names"])

        again = input("Autre prédiction ? (o/n) : ").strip().lower()
        if again not in ("o", "oui", "y", "yes"):
            print("\nÀ bientôt !")
            break
        print()


if __name__ == "__main__":
    main()
