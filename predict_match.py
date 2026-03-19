"""
predict_match.py — Dashboard de prédiction de paris football.

Charge les 3 modèles entraînés (1X2, Over/Under, BTTS) et affiche
les probabilités pour chaque marché comme un tableau de bord parieur.
"""

import sys
import joblib
import numpy as np
import pandas as pd

# ── Configuration ──────────────────────────────────────────────────────────────

MODEL_FILE = "modele_foot.pkl"
TEAM_STATS_FILE = "dataset_football.csv"
PER_MATCH_COLS = ["Diff_Buts", "Corners", "Fautes", "Cartons_Jaunes", "Cartons_Rouges"]

W = 60  # largeur du dashboard


# ── Fonctions utilitaires ─────────────────────────────────────────────────────

def load_artifacts():
    """Charge les modèles et les profils d'équipe."""
    artifact = joblib.load(MODEL_FILE)
    df = pd.read_csv(TEAM_STATS_FILE)
    for col in PER_MATCH_COLS:
        df[col] = (df[col] / df["Matchs_Joues"]).round(3)
    teams = df.set_index("Equipe")
    return artifact, teams


def fuzzy_match(name: str, valid_names: list[str]) -> str | None:
    """Correspondance souple : insensible à la casse."""
    name_lower = name.strip().lower()
    for valid in valid_names:
        if valid.lower() == name_lower:
            return valid
    matches = [v for v in valid_names if name_lower in v.lower() or v.lower() in name_lower]
    if len(matches) == 1:
        return matches[0]
    return None


def ask_team(prompt: str, valid_names: list[str]) -> str:
    """Demande un nom d'équipe avec validation."""
    while True:
        raw = input(prompt).strip()
        if not raw:
            continue
        match = fuzzy_match(raw, valid_names)
        if match:
            return match
        print(f"  ✗ « {raw} » introuvable.")
        close = [v for v in valid_names if raw.lower()[:3] in v.lower()]
        if close:
            print(f"    Suggestions : {', '.join(close)}")
        print()


def compute_features(profiles, team_a, team_b, cols_1x2, cols_goals):
    """Calcule les vecteurs de features pour les 3 modèles."""
    # Noms bruts (sans préfixe diff_/sum_)
    raw_diff = [c.replace("diff_", "") for c in cols_1x2]
    prof_a = profiles.loc[team_a]
    prof_b = profiles.loc[team_b]

    # Vecteur diff pour 1X2
    X_1x2 = np.array([(prof_a[f] - prof_b[f]) for f in raw_diff]).reshape(1, -1)

    # Vecteur diff + sum pour Over/Under & BTTS
    vals = []
    for col in cols_goals:
        feat = col.split("_", 1)[1]  # enlève diff_ ou sum_
        if col.startswith("diff_"):
            vals.append(prof_a[feat] - prof_b[feat])
        elif col.startswith("sum_"):
            vals.append(prof_a[feat] + prof_b[feat])
    X_goals = np.array(vals).reshape(1, -1)

    return X_1x2, X_goals


def proba_to_cote(p: float) -> str:
    """Convertit une probabilité en cote décimale."""
    if p < 0.01:
        return "99.00"
    return f"{1 / p:.2f}"


# ── Affichage dashboard ───────────────────────────────────────────────────────

def bar(pct: float, width: int = 20) -> str:
    """Génère une barre de progression."""
    filled = int(pct / 100 * width)
    return "█" * filled + "░" * (width - filled)


def display_dashboard(team_a: str, team_b: str, p1x2, p_over, p_btts):
    """Affiche le tableau de bord parieur complet."""
    p1, px, p2 = p1x2[2] * 100, p1x2[1] * 100, p1x2[0] * 100  # Dom, Nul, Ext
    c1, cx, c2 = proba_to_cote(p1x2[2]), proba_to_cote(p1x2[1]), proba_to_cote(p1x2[0])

    # Double Chance
    dc_1x = p1 + px
    dc_12 = p1 + p2
    dc_x2 = px + p2

    # Over / Under
    po = p_over[1] * 100
    pu = p_over[0] * 100

    # BTTS
    pb_y = p_btts[1] * 100
    pb_n = p_btts[0] * 100

    # Verdict
    best_1x2_idx = np.argmax([p2, px, p1])
    verdict = [f"Victoire {team_b}", "Match Nul", f"Victoire {team_a}"][best_1x2_idx]

    print()
    print(f"╔{'═' * W}╗")
    print(f"║{'TABLEAU DE BORD — PRÉDICTION':^{W}}║")
    print(f"╠{'═' * W}╣")
    print(f"║{f'{team_a}  (DOM)  vs  (EXT)  {team_b}':^{W}}║")
    print(f"╠{'═' * W}╣")

    # ── 1X2 ──
    print(f"║{'':^{W}}║")
    print(f"║  {'MARCHÉ 1X2':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    print(f"║  {'1 — ' + team_a:<22} {bar(p1):20s}  {p1:5.1f}%   @{c1:>6s}  ║")
    print(f"║  {'X — Nul':<22} {bar(px):20s}  {px:5.1f}%   @{cx:>6s}  ║")
    print(f"║  {'2 — ' + team_b:<22} {bar(p2):20s}  {p2:5.1f}%   @{c2:>6s}  ║")

    # ── Double Chance ──
    print(f"║{'':^{W}}║")
    print(f"║  {'DOUBLE CHANCE':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    print(f"║  {'1X (Dom ou Nul)':<22} {bar(dc_1x):20s}  {dc_1x:5.1f}%   @{proba_to_cote(dc_1x / 100):>6s}  ║")
    print(f"║  {'12 (Pas de Nul)':<22} {bar(dc_12):20s}  {dc_12:5.1f}%   @{proba_to_cote(dc_12 / 100):>6s}  ║")
    print(f"║  {'X2 (Ext ou Nul)':<22} {bar(dc_x2):20s}  {dc_x2:5.1f}%   @{proba_to_cote(dc_x2 / 100):>6s}  ║")

    # ── Over/Under ──
    print(f"║{'':^{W}}║")
    print(f"║  {'OVER / UNDER 2.5 BUTS':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    print(f"║  {'Over 2.5':<22} {bar(po):20s}  {po:5.1f}%   @{proba_to_cote(po / 100):>6s}  ║")
    print(f"║  {'Under 2.5':<22} {bar(pu):20s}  {pu:5.1f}%   @{proba_to_cote(pu / 100):>6s}  ║")

    # ── BTTS ──
    print(f"║{'':^{W}}║")
    print(f"║  {'BTTS (LES DEUX MARQUENT)':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    print(f"║  {'Oui':<22} {bar(pb_y):20s}  {pb_y:5.1f}%   @{proba_to_cote(pb_y / 100):>6s}  ║")
    print(f"║  {'Non':<22} {bar(pb_n):20s}  {pb_n:5.1f}%   @{proba_to_cote(pb_n / 100):>6s}  ║")

    # ── Verdict ──
    print(f"║{'':^{W}}║")
    print(f"╠{'═' * W}╣")
    print(f"║  {'PRÉDICTION :':<14} {verdict:<{W - 18}}  ║")

    # Meilleur combo
    best_ou = "Over 2.5" if po > pu else "Under 2.5"
    best_btts = "BTTS Oui" if pb_y > pb_n else "BTTS Non"
    combo = f"{'1' if p1 > max(px, p2) else ('X' if px > p2 else '2')} + {best_ou} + {best_btts}"
    print(f"║  {'COMBO :':<14} {combo:<{W - 18}}  ║")
    print(f"╚{'═' * W}╝")
    print()


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    print()
    print(f"╔{'═' * W}╗")
    print(f"║{'⚽  PRÉDICTEUR DE PARIS — LIGUE 1  ⚽':^{W}}║")
    print(f"╚{'═' * W}╝")
    print()

    artifact, profiles = load_artifacts()
    valid_teams = sorted(profiles.index.tolist())
    print(f"Équipes : {', '.join(valid_teams)}\n")

    while True:
        team_a = ask_team("Équipe à domicile  : ", valid_teams)
        team_b = ask_team("Équipe à l'extérieur : ", valid_teams)

        if team_a == team_b:
            print("  ✗ Une équipe ne peut pas jouer contre elle-même.\n")
            continue

        X_1x2, X_goals = compute_features(
            profiles, team_a, team_b,
            artifact["cols_1x2"], artifact["cols_goals"],
        )
        X_1x2_s = artifact["scaler_1x2"].transform(X_1x2)
        X_goals_s = artifact["scaler_goals"].transform(X_goals)

        p1x2 = artifact["model_1x2"].predict_proba(X_1x2_s)[0]
        p_over = artifact["model_over"].predict_proba(X_goals_s)[0]
        p_btts = artifact["model_btts"].predict_proba(X_goals_s)[0]

        display_dashboard(team_a, team_b, p1x2, p_over, p_btts)

        again = input("Autre prédiction ? (o/n) : ").strip().lower()
        if again not in ("o", "oui", "y", "yes"):
            print("\nÀ bientôt !")
            break
        print()


if __name__ == "__main__":
    main()
