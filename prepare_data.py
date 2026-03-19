"""
prepare_data.py — Prépare les données de confrontation pour le ML.

Pour chaque match réel (Équipe A vs Équipe B), calcule la différence
de profil statistique entre les deux équipes. Produit 3 labels :
  - Resultat   : 1X2  (1 = dom, 0 = nul, -1 = ext)
  - Over2_5    : 1 si total buts > 2.5, 0 sinon
  - BTTS       : 1 si les deux équipes marquent, 0 sinon

Entrée  : dataset_football.csv  (profil agrégé par équipe)
        + données brutes Football-Data.co.uk (résultats réels)
Sortie  : matchups_train.csv
"""

import io
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

# Colonnes additionnelles injectées telles quelles (somme, pas diff)
SUM_FEATURES = [
    "Buts_Par_Match",
    "Tirs_Cadres_Pct",
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
    """Récupère les résultats réels avec buts pour labelliser chaque confrontation."""
    print(f"  → GET {MATCH_CSV_URL}")
    resp = requests.get(
        MATCH_CSV_URL,
        headers={"User-Agent": "Mozilla/5.0"},
        timeout=30,
    )
    resp.raise_for_status()
    raw = pd.read_csv(io.StringIO(resp.text), encoding="utf-8-sig")

    matches = raw[["HomeTeam", "AwayTeam", "FTHG", "FTAG", "FTR"]].copy()
    label_map = {"H": 1, "D": 0, "A": -1}
    matches["Resultat"] = matches["FTR"].map(label_map)
    matches["Total_Buts"] = matches["FTHG"] + matches["FTAG"]
    matches["Over2_5"] = (matches["Total_Buts"] > 2.5).astype(int)
    matches["BTTS"] = ((matches["FTHG"] > 0) & (matches["FTAG"] > 0)).astype(int)
    matches = matches.drop(columns=["FTR", "FTHG", "FTAG"])
    matches = matches.rename(columns={"HomeTeam": "Equipe_A", "AwayTeam": "Equipe_B"})
    return matches


def build_matchup_features(
    matches: pd.DataFrame, profiles: pd.DataFrame
) -> pd.DataFrame:
    """Pour chaque match, calcule diff et somme de profils."""
    rows = []

    for _, m in matches.iterrows():
        team_a, team_b = m["Equipe_A"], m["Equipe_B"]
        if team_a not in profiles.index or team_b not in profiles.index:
            continue

        prof_a = profiles.loc[team_a, DIFF_FEATURES]
        prof_b = profiles.loc[team_b, DIFF_FEATURES]

        row = {"Equipe_A": team_a, "Equipe_B": team_b}

        # Différences (pour 1X2)
        diff = (prof_a - prof_b).to_dict()
        for feat, val in diff.items():
            row[f"diff_{feat}"] = round(val, 3)

        # Sommes (pour Over/Under & BTTS — le potentiel offensif combiné)
        for feat in SUM_FEATURES:
            row[f"sum_{feat}"] = round(prof_a[feat] + prof_b[feat], 3)

        # Labels
        row["Resultat"] = m["Resultat"]
        row["Total_Buts"] = m["Total_Buts"]
        row["Over2_5"] = m["Over2_5"]
        row["BTTS"] = m["BTTS"]
        rows.append(row)

    return pd.DataFrame(rows)


def print_summary(df: pd.DataFrame) -> None:
    """Affiche un aperçu clair des données transformées."""
    print(f"\n── Aperçu ({len(df)} matchs) ──\n")

    preview_cols = (
        ["Equipe_A", "Equipe_B"]
        + [c for c in df.columns if c.startswith("diff_")][:4]
        + [c for c in df.columns if c.startswith("sum_")]
        + ["Resultat", "Total_Buts", "Over2_5", "BTTS"]
    )
    print(df[preview_cols].head(8).to_string(index=False))

    # Distribution des labels
    print(f"\n── Distribution des labels ──")
    labels_1x2 = {-1: "Victoire Ext", 0: "Nul", 1: "Victoire Dom"}
    for val, label in sorted(labels_1x2.items()):
        c = (df["Resultat"] == val).sum()
        print(f"  {label:>15s} : {c:3d}  ({c / len(df) * 100:.1f}%)")

    ov = df["Over2_5"].sum()
    bt = df["BTTS"].sum()
    n = len(df)
    print(f"      Over 2.5    : {ov:3d}  ({ov / n * 100:.1f}%)")
    print(f"      BTTS Oui    : {bt:3d}  ({bt / n * 100:.1f}%)")
    print(f"  Moy. buts/match : {df['Total_Buts'].mean():.2f}")


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    print("=== Préparation des données de confrontation ===\n")

    print("1. Chargement des profils d'équipe …")
    profiles = load_team_profiles(TEAM_STATS_FILE)
    print(f"   {len(profiles)} équipes chargées\n")

    print("2. Récupération des résultats réels …")
    matches = fetch_match_results()
    print(f"   {len(matches)} matchs récupérés\n")

    print("3. Calcul des features de confrontation …")
    df = build_matchup_features(matches, profiles)

    print_summary(df)

    print(f"\n4. Export …")
    df.to_csv(OUTPUT_FILE, index=False, encoding="utf-8-sig")
    print(f"✓ {len(df)} lignes sauvegardées dans {OUTPUT_FILE}")


if __name__ == "__main__":
    main()
