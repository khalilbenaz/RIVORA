"""
prepare_data.py — Prépare les données de confrontation pour le ML.

Pour chaque match réel (Équipe A vs Équipe B), calcule la différence
de profil statistique entre les deux équipes. Le modèle apprendra
à prédire le résultat à partir de ces écarts.

Entrée  : dataset_football.csv  (profil agrégé par équipe)
        + données brutes Football-Data.co.uk (résultats réels)
Sortie  : matchups_train.csv
"""

import io
import itertools
import requests
import pandas as pd
import numpy as np

# ── Configuration ──────────────────────────────────────────────────────────────

TEAM_STATS_FILE = "dataset_football.csv"
MATCH_CSV_URL = "https://www.football-data.co.uk/mmz4281/2526/F1.csv"
OUTPUT_FILE = "matchups_train.csv"

# Features sur lesquelles calculer les différences (par match pour normaliser)
DIFF_FEATURES = [
    "Buts_Par_Match",
    "Tirs_Cadres_Pct",
    "Pct_Victoires",
    "Diff_Buts",
    "Corners",
    "Fautes",
    "Cartons_Jaunes",
    "Cartons_Rouges",
    "Points",
]

# Normalisation : ces colonnes sont des totaux, on les ramène par match
PER_MATCH_COLS = ["Diff_Buts", "Corners", "Fautes", "Cartons_Jaunes", "Cartons_Rouges"]


# ── Fonctions ──────────────────────────────────────────────────────────────────

def load_team_profiles(path: str) -> pd.DataFrame:
    """Charge les stats agrégées par équipe et normalise par match."""
    df = pd.read_csv(path)
    for col in PER_MATCH_COLS:
        df[col] = (df[col] / df["Matchs_Joues"]).round(3)
    return df.set_index("Equipe")


def fetch_match_results() -> pd.DataFrame:
    """Récupère les résultats réels pour labelliser chaque confrontation.

    Labels :
        1 = victoire équipe A (domicile)
        0 = match nul
       -1 = victoire équipe B (extérieur)
    """
    print(f"  → GET {MATCH_CSV_URL}")
    resp = requests.get(
        MATCH_CSV_URL,
        headers={"User-Agent": "Mozilla/5.0"},
        timeout=30,
    )
    resp.raise_for_status()
    raw = pd.read_csv(io.StringIO(resp.text), encoding="utf-8-sig")

    matches = raw[["HomeTeam", "AwayTeam", "FTR"]].copy()
    label_map = {"H": 1, "D": 0, "A": -1}
    matches["Resultat"] = matches["FTR"].map(label_map)
    matches = matches.drop(columns=["FTR"])
    matches = matches.rename(columns={"HomeTeam": "Equipe_A", "AwayTeam": "Equipe_B"})
    return matches


def build_matchup_features(
    matches: pd.DataFrame, profiles: pd.DataFrame
) -> pd.DataFrame:
    """Pour chaque match, calcule diff = profil(A) - profil(B) sur chaque feature."""
    rows = []

    for _, m in matches.iterrows():
        team_a, team_b = m["Equipe_A"], m["Equipe_B"]
        if team_a not in profiles.index or team_b not in profiles.index:
            continue

        prof_a = profiles.loc[team_a, DIFF_FEATURES]
        prof_b = profiles.loc[team_b, DIFF_FEATURES]
        diff = (prof_a - prof_b).to_dict()

        row = {
            "Equipe_A": team_a,
            "Equipe_B": team_b,
        }
        for feat, val in diff.items():
            row[f"diff_{feat}"] = round(val, 3)
        row["Resultat"] = m["Resultat"]
        rows.append(row)

    return pd.DataFrame(rows)


def print_summary(df: pd.DataFrame) -> None:
    """Affiche un aperçu clair des données transformées."""
    print(f"\n── Aperçu des données de confrontation ({len(df)} matchs) ──\n")

    # Premières lignes
    display_cols = [c for c in df.columns if c != "Equipe_A" and c != "Equipe_B"]
    print(df.head(10).to_string(index=False))

    # Stats descriptives des features
    diff_cols = [c for c in df.columns if c.startswith("diff_")]
    print(f"\n── Statistiques descriptives des différences ──\n")
    print(df[diff_cols].describe().round(3).to_string())

    # Distribution des résultats
    print(f"\n── Distribution des résultats ──")
    counts = df["Resultat"].value_counts().sort_index()
    labels = {-1: "Victoire B (ext)", 0: "Nul", 1: "Victoire A (dom)"}
    for val, count in counts.items():
        pct = count / len(df) * 100
        print(f"  {labels[val]:>20s} : {count:3d}  ({pct:.1f}%)")


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    print("=== Préparation des données de confrontation ===\n")

    print("1. Chargement des profils d'équipe …")
    profiles = load_team_profiles(TEAM_STATS_FILE)
    print(f"   {len(profiles)} équipes chargées\n")

    print("2. Récupération des résultats réels …")
    matches = fetch_match_results()
    print(f"   {len(matches)} matchs récupérés\n")

    print("3. Calcul des différences de profil …")
    df = build_matchup_features(matches, profiles)

    print_summary(df)

    print(f"\n4. Export …")
    df.to_csv(OUTPUT_FILE, index=False, encoding="utf-8-sig")
    print(f"✓ {len(df)} lignes sauvegardées dans {OUTPUT_FILE}")


if __name__ == "__main__":
    main()
