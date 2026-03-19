"""
scraper_stats.py — Scraper modulaire de statistiques football (Ligue 1).
Source : Football-Data.co.uk (CSV match-by-match).
Agrège les données par équipe et produit dataset_football.csv.
"""

import io
import requests
import pandas as pd

# ── Configuration ──────────────────────────────────────────────────────────────
SEASON = "2526"  # 2025-2026
CSV_URL = f"https://www.football-data.co.uk/mmz4281/{SEASON}/F1.csv"

HEADERS = {
    "User-Agent": (
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/124.0.0.0 Safari/537.36"
    ),
}

OUTPUT_FILE = "dataset_football.csv"


# ── Fonctions ──────────────────────────────────────────────────────────────────

def fetch_csv(url: str) -> pd.DataFrame:
    """Télécharge le CSV de résultats depuis Football-Data.co.uk."""
    print(f"  → GET {url}")
    resp = requests.get(url, headers=HEADERS, timeout=30)
    resp.raise_for_status()
    return pd.read_csv(io.StringIO(resp.text), encoding="utf-8-sig")


def aggregate_team_stats(matches: pd.DataFrame) -> pd.DataFrame:
    """Agrège les statistiques match-by-match en stats par équipe.

    Colonnes sources utilisées :
      FTHG/FTAG = buts marqués (domicile/extérieur)
      FTR       = résultat (H/D/A)
      HS/AS     = tirs (shots)
      HST/AST   = tirs cadrés (shots on target)
      HC/AC     = corners
      HF/AF     = fautes
      HY/AY     = cartons jaunes
      HR/AR     = cartons rouges
    """
    teams = sorted(set(matches["HomeTeam"]) | set(matches["AwayTeam"]))
    rows = []

    for team in teams:
        home = matches[matches["HomeTeam"] == team]
        away = matches[matches["AwayTeam"] == team]

        mp = len(home) + len(away)
        wins = (home["FTR"] == "H").sum() + (away["FTR"] == "A").sum()
        draws = (home["FTR"] == "D").sum() + (away["FTR"] == "D").sum()
        losses = mp - wins - draws
        goals_for = home["FTHG"].sum() + away["FTAG"].sum()
        goals_against = home["FTAG"].sum() + away["FTHG"].sum()
        shots = home["HS"].sum() + away["AS"].sum()
        shots_on_target = home["HST"].sum() + away["AST"].sum()
        corners = home["HC"].sum() + away["AC"].sum()
        fouls = home["HF"].sum() + away["AF"].sum()
        yellow = home["HY"].sum() + away["AY"].sum()
        red = home["HR"].sum() + away["AR"].sum()
        points = wins * 3 + draws

        rows.append({
            "Equipe": team,
            "Matchs_Joues": mp,
            "Victoires": int(wins),
            "Nuls": int(draws),
            "Defaites": int(losses),
            "Buts_Pour": int(goals_for),
            "Buts_Contre": int(goals_against),
            "Diff_Buts": int(goals_for - goals_against),
            "Tirs": int(shots),
            "Tirs_Cadres": int(shots_on_target),
            "Corners": int(corners),
            "Fautes": int(fouls),
            "Cartons_Jaunes": int(yellow),
            "Cartons_Rouges": int(red),
            "Points": int(points),
        })

    df = pd.DataFrame(rows)

    # Métriques dérivées
    df["Buts_Par_Match"] = (df["Buts_Pour"] / df["Matchs_Joues"]).round(2)
    df["Tirs_Cadres_Pct"] = ((df["Tirs_Cadres"] / df["Tirs"]) * 100).round(1)
    df["Pct_Victoires"] = ((df["Victoires"] / df["Matchs_Joues"]) * 100).round(1)

    return df.sort_values("Points", ascending=False).reset_index(drop=True)


def save_csv(df: pd.DataFrame, path: str) -> None:
    """Sauvegarde le DataFrame en CSV UTF-8."""
    df.to_csv(path, index=False, encoding="utf-8-sig")
    print(f"\n✓ {len(df)} équipes sauvegardées dans {path}")


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    print("=== Scraper Stats Football — Ligue 1 ===\n")

    print("1. Téléchargement des données …")
    matches = fetch_csv(CSV_URL)
    print(f"   {len(matches)} matchs récupérés\n")

    print("2. Agrégation par équipe …")
    df = aggregate_team_stats(matches)

    print("\n── Classement Ligue 1 ──")
    print(df.to_string(index=False))

    print("\n3. Export CSV …")
    save_csv(df, OUTPUT_FILE)


if __name__ == "__main__":
    main()
