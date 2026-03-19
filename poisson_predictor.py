"""
poisson_predictor.py — Prédiction de paris football par la Loi de Poisson
                       + Analyse de rentabilité (Value Betting).

Approche quantitative :
  1. Calcule la force d'attaque / défense de chaque équipe (dom & ext)
     relative à la moyenne de la ligue.
  2. Estime λ (espérance de buts) pour chaque équipe dans un match donné.
  3. Génère une matrice de scores exacts (0-0 → 5-5) via Poisson.
  4. Dérive tous les marchés de paris à partir de cette matrice.
  5. Compare avec les cotes bookmaker et détecte les Value Bets (EV+).

Source : Football-Data.co.uk (données match-by-match Ligue 1).
"""

import io
import numpy as np
import pandas as pd
import requests
from scipy.stats import poisson

# ── Configuration ──────────────────────────────────────────────────────────────

MATCH_CSV_URL = "https://www.football-data.co.uk/mmz4281/2526/F1.csv"
MAX_GOALS = 6
W = 62

# Codes ANSI pour couleurs terminal
GREEN = "\033[1;32m"
RED = "\033[1;31m"
YELLOW = "\033[1;33m"
CYAN = "\033[1;36m"
BOLD = "\033[1m"
DIM = "\033[2m"
RESET = "\033[0m"


# ── Chargement & calcul des forces ─────────────────────────────────────────────

def fetch_matches() -> pd.DataFrame:
    resp = requests.get(
        MATCH_CSV_URL,
        headers={"User-Agent": "Mozilla/5.0"},
        timeout=30,
    )
    resp.raise_for_status()
    df = pd.read_csv(io.StringIO(resp.text), encoding="utf-8-sig")
    return df[["HomeTeam", "AwayTeam", "FTHG", "FTAG"]].dropna()


def compute_strengths(matches: pd.DataFrame) -> tuple[pd.DataFrame, dict]:
    league = {
        "avg_home_goals": matches["FTHG"].mean(),
        "avg_away_goals": matches["FTAG"].mean(),
    }
    teams = sorted(set(matches["HomeTeam"]) | set(matches["AwayTeam"]))
    rows = []
    for team in teams:
        home = matches[matches["HomeTeam"] == team]
        away = matches[matches["AwayTeam"] == team]
        n_home, n_away = len(home), len(away)
        avg_scored_home = home["FTHG"].mean() if n_home else league["avg_home_goals"]
        avg_conceded_home = home["FTAG"].mean() if n_home else league["avg_away_goals"]
        avg_scored_away = away["FTAG"].mean() if n_away else league["avg_away_goals"]
        avg_conceded_away = away["FTHG"].mean() if n_away else league["avg_home_goals"]
        rows.append({
            "Equipe": team,
            "Att_Dom": avg_scored_home / league["avg_home_goals"],
            "Def_Dom": avg_conceded_home / league["avg_away_goals"],
            "Att_Ext": avg_scored_away / league["avg_away_goals"],
            "Def_Ext": avg_conceded_away / league["avg_home_goals"],
        })
    return pd.DataFrame(rows).set_index("Equipe"), league


# ── Modèle de Poisson ─────────────────────────────────────────────────────────

def compute_lambdas(strengths, league, team_home, team_away):
    lam_home = (strengths.loc[team_home, "Att_Dom"]
                * strengths.loc[team_away, "Def_Ext"]
                * league["avg_home_goals"])
    lam_away = (strengths.loc[team_away, "Att_Ext"]
                * strengths.loc[team_home, "Def_Dom"]
                * league["avg_away_goals"])
    return lam_home, lam_away


def score_matrix(lam_home, lam_away, max_goals=MAX_GOALS):
    home_probs = poisson.pmf(np.arange(max_goals), lam_home)
    away_probs = poisson.pmf(np.arange(max_goals), lam_away)
    return np.outer(home_probs, away_probs)


# ── Dérivation des marchés ─────────────────────────────────────────────────────

def derive_markets(matrix):
    n = matrix.shape[0]
    p_home = sum(matrix[i][j] for i in range(n) for j in range(n) if i > j)
    p_draw = sum(matrix[i][i] for i in range(n))
    p_away = sum(matrix[i][j] for i in range(n) for j in range(n) if j > i)
    p_under = sum(matrix[i][j] for i in range(n) for j in range(n) if i + j <= 2)
    p_over = 1 - p_under
    p_btts_yes = sum(matrix[i][j] for i in range(1, n) for j in range(1, n))
    p_btts_no = 1 - p_btts_yes
    scores = sorted(
        [(i, j, matrix[i][j]) for i in range(n) for j in range(n)],
        key=lambda x: -x[2],
    )
    return {
        "1x2": (p_home, p_draw, p_away),
        "double_chance": (p_home + p_draw, p_home + p_away, p_draw + p_away),
        "over_under": (p_over, p_under),
        "btts": (p_btts_yes, p_btts_no),
        "exact_scores": scores[:5],
    }


# ── Value Betting ──────────────────────────────────────────────────────────────

def calculer_cote_implicite(probabilite: float) -> float:
    """Convertit une probabilité en cote décimale européenne."""
    if probabilite < 0.005:
        return 200.0
    return 1.0 / probabilite


def calculer_ev(probabilite: float, cote_bookmaker: float) -> float:
    """Expected Value = (probabilité × cote) - 1."""
    return (probabilite * cote_bookmaker) - 1.0


def ask_cote(prompt: str) -> float | None:
    """Demande une cote au format décimal. Retourne None si vide."""
    raw = input(prompt).strip()
    if not raw or raw == "-":
        return None
    try:
        val = float(raw)
        if val < 1.01:
            print(f"  {DIM}(cote ignorée — doit être ≥ 1.01){RESET}")
            return None
        return val
    except ValueError:
        print(f"  {DIM}(format invalide — ignoré){RESET}")
        return None


def display_value_analysis(selections: list[dict]):
    """Affiche le tableau comparatif Value Betting."""
    # Largeur des colonnes
    TW = 82

    print(f"\n╔{'═' * TW}╗")
    print(f"║{f'{BOLD}📊  ANALYSE VALUE BETTING{RESET}':^{TW + 8}}║")
    print(f"╠{'═' * TW}╣")

    # Header
    h = (f"  {'Marché':<16s}│{'Proba':>7s}│{'Cote Model':>11s}│"
         f"{'Cote Book':>10s}│{'EV':>8s}│ Statut")
    print(f"║{h:<{TW}}║")
    print(f"║  {'─' * 16}┼{'─' * 7}┼{'─' * 11}┼{'─' * 10}┼{'─' * 8}┼{'─' * 24}  ║")

    value_bets = []

    for sel in selections:
        label = sel["label"]
        prob = sel["prob"]
        cote_model = calculer_cote_implicite(prob)
        cote_book = sel.get("cote_book")

        prob_str = f"{prob * 100:5.1f}%"
        model_str = f"{cote_model:6.2f}"

        if cote_book is None:
            book_str = f"{DIM}   —    {RESET}"
            ev_str = f"{DIM}  —   {RESET}"
            status = f"{DIM}Non renseigné{RESET}"
            line = (f"  {label:<16s}│{prob_str:>7s}│{model_str:>11s}│"
                    f"{book_str:>19s}│{ev_str:>17s}│ {status}")
        else:
            ev = calculer_ev(prob, cote_book)
            book_str = f"{cote_book:6.2f}"
            ev_pct = ev * 100

            if ev > 0:
                ev_str = f"{GREEN}+{ev_pct:5.1f}%{RESET}"
                status = f"{GREEN}🟢 VALUE BET{RESET}"
                value_bets.append({**sel, "ev": ev, "cote_model": cote_model})
                line = (f"  {GREEN}{label:<16s}{RESET}│{prob_str:>7s}│{model_str:>11s}│"
                        f"{book_str:>10s}│{ev_str:>17s}│ {status}")
            else:
                ev_str = f"{RED}{ev_pct:+5.1f}%{RESET}"
                status = f"{RED}✗ Pas de value{RESET}"
                line = (f"  {label:<16s}│{prob_str:>7s}│{model_str:>11s}│"
                        f"{book_str:>10s}│{ev_str:>17s}│ {status}")

        print(f"║{line}{'':>{TW - len_visible(line)}}║")

    print(f"╠{'═' * TW}╣")

    # Résumé
    if value_bets:
        print(f"║{'':^{TW}}║")
        print(f"║  {GREEN}{BOLD}🟢 {len(value_bets)} VALUE BET(S) DÉTECTÉ(S) :{RESET}{'':>{TW - 35}}║")
        print(f"║{'':^{TW}}║")
        for vb in value_bets:
            ev_pct = vb["ev"] * 100
            edge = vb["cote_book"] - vb["cote_model"]
            detail = (f"  {GREEN}►{RESET} {BOLD}{vb['label']}{RESET}"
                      f"  │  Book: {BOLD}{vb['cote_book']:.2f}{RESET}"
                      f" vs Model: {vb['cote_model']:.2f}"
                      f"  │  Edge: {GREEN}+{edge:.2f}{RESET}"
                      f"  │  EV: {GREEN}+{ev_pct:.1f}%{RESET}")
            print(f"║{detail}{'':>{TW - len_visible(detail)}}║")

        print(f"║{'':^{TW}}║")
        # Kelly Criterion suggestion
        print(f"║  {CYAN}Suggestion Kelly Criterion (mise fractionnelle) :{RESET}{'':>{TW - 53}}║")
        for vb in value_bets:
            p = vb["prob"]
            b = vb["cote_book"] - 1
            kelly = ((b * p) - (1 - p)) / b
            kelly_frac = kelly * 0.25  # quart de Kelly = conservateur
            kelly_str = (f"  {vb['label']:<16s}  "
                         f"Kelly: {kelly * 100:.1f}%  │  "
                         f"¼ Kelly (conservateur): {BOLD}{kelly_frac * 100:.1f}%{RESET} du bankroll")
            print(f"║  {kelly_str}{'':>{TW - len_visible(kelly_str) - 2}}║")
    else:
        print(f"║{'':^{TW}}║")
        no_val = f"  {YELLOW}⚠  Aucun Value Bet détecté — les cotes du bookmaker sont justes ou en sa faveur.{RESET}"
        print(f"║{no_val}{'':>{TW - len_visible(no_val)}}║")

    print(f"║{'':^{TW}}║")
    print(f"╚{'═' * TW}╝")
    print()


def len_visible(s: str) -> int:
    """Longueur visible d'une chaîne (sans les codes ANSI)."""
    import re
    return len(re.sub(r"\033\[[0-9;]*m", "", s))


# ── Affichage dashboard ────────────────────────────────────────────────────────

def bar(pct, width=20):
    filled = int(pct / 100 * width)
    return "█" * filled + "░" * (width - filled)


def cote_str(p):
    return f"{1 / p:.2f}" if p > 0.005 else "—"


def print_matrix(matrix, team_home, team_away):
    n = matrix.shape[0]
    print(f"║  {'MATRICE DES SCORES EXACTS (Poisson)':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    header = f"  {team_away:>10s}"
    for j in range(min(n, 6)):
        header += f"   {j}"
    print(f"║  {header:<{W - 4}}  ║")
    for i in range(min(n, 6)):
        prefix = f"{team_home:>10s}" if i == 0 else " " * 10
        row = f"  {prefix}  {i}"
        for j in range(min(n, 6)):
            row += f" {matrix[i][j] * 100:4.1f}"
        print(f"║  {row:<{W - 4}}  ║")


def display_dashboard(team_home, team_away, lam_h, lam_a, matrix, markets, strengths):
    p1, px, p2 = [x * 100 for x in markets["1x2"]]
    dc_1x, dc_12, dc_x2 = [x * 100 for x in markets["double_chance"]]
    po, pu = [x * 100 for x in markets["over_under"]]
    bt_y, bt_n = [x * 100 for x in markets["btts"]]
    r1, rx, r2 = markets["1x2"]
    sh = strengths.loc[team_home]
    sa = strengths.loc[team_away]

    print()
    print(f"╔{'═' * W}╗")
    print(f"║{'POISSON PREDICTOR — ANALYSE QUANTITATIVE':^{W}}║")
    print(f"╠{'═' * W}╣")
    print(f"║{f'{team_home}  (DOM)  vs  (EXT)  {team_away}':^{W}}║")
    print(f"╠{'═' * W}╣")

    print(f"║{'':^{W}}║")
    print(f"║  {'PARAMÈTRES DU MODÈLE':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    print(f"║  {'':15s} {'Att':>7s} {'Déf':>7s} {'λ (esp. buts)':>14s}{'':>{W - 48}}  ║")
    print(f"║  {team_home:15s} {sh['Att_Dom']:7.3f} {sh['Def_Dom']:7.3f} {lam_h:14.3f}{'':>{W - 48}}  ║")
    print(f"║  {team_away:15s} {sa['Att_Ext']:7.3f} {sa['Def_Ext']:7.3f} {lam_a:14.3f}{'':>{W - 48}}  ║")

    print(f"║{'':^{W}}║")
    print_matrix(matrix, team_home, team_away)

    # 1X2
    print(f"║{'':^{W}}║")
    print(f"║  {'MARCHÉ 1X2':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    print(f"║  {'1 — ' + team_home:<22} {bar(p1):20s}  {p1:5.1f}%  @{cote_str(r1):>6s}  ║")
    print(f"║  {'X — Nul':<22} {bar(px):20s}  {px:5.1f}%  @{cote_str(rx):>6s}  ║")
    print(f"║  {'2 — ' + team_away:<22} {bar(p2):20s}  {p2:5.1f}%  @{cote_str(r2):>6s}  ║")

    # Double Chance
    print(f"║{'':^{W}}║")
    print(f"║  {'DOUBLE CHANCE':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    print(f"║  {'1X (Dom ou Nul)':<22} {bar(dc_1x):20s}  {dc_1x:5.1f}%  @{cote_str(dc_1x/100):>6s}  ║")
    print(f"║  {'12 (Pas de Nul)':<22} {bar(dc_12):20s}  {dc_12:5.1f}%  @{cote_str(dc_12/100):>6s}  ║")
    print(f"║  {'X2 (Ext ou Nul)':<22} {bar(dc_x2):20s}  {dc_x2:5.1f}%  @{cote_str(dc_x2/100):>6s}  ║")

    # Over/Under
    print(f"║{'':^{W}}║")
    print(f"║  {'OVER / UNDER 2.5 BUTS':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    print(f"║  {'Over 2.5':<22} {bar(po):20s}  {po:5.1f}%  @{cote_str(po/100):>6s}  ║")
    print(f"║  {'Under 2.5':<22} {bar(pu):20s}  {pu:5.1f}%  @{cote_str(pu/100):>6s}  ║")

    # BTTS
    print(f"║{'':^{W}}║")
    print(f"║  {'BTTS (LES DEUX MARQUENT)':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    print(f"║  {'Oui':<22} {bar(bt_y):20s}  {bt_y:5.1f}%  @{cote_str(bt_y/100):>6s}  ║")
    print(f"║  {'Non':<22} {bar(bt_n):20s}  {bt_n:5.1f}%  @{cote_str(bt_n/100):>6s}  ║")

    # Scores exacts
    print(f"║{'':^{W}}║")
    print(f"║  {'TOP 5 SCORES EXACTS':<{W - 4}}  ║")
    print(f"║  {'─' * (W - 4)}  ║")
    for rank, (gh, ga, prob) in enumerate(markets["exact_scores"], 1):
        score_s = f"{team_home} {gh} - {ga} {team_away}"
        print(f"║  {rank}.  {score_s:<30s}  {prob * 100:5.1f}%  @{cote_str(prob):>6s}  ║")

    # Verdict
    best_1x2 = ["1", "X", "2"][np.argmax(markets["1x2"])]
    best_ou = "Over" if po > pu else "Under"
    best_btts = "Oui" if bt_y > bt_n else "Non"
    top_score = markets["exact_scores"][0]

    print(f"║{'':^{W}}║")
    print(f"╠{'═' * W}╣")
    verdicts = {"1": f"Victoire {team_home}", "X": "Match Nul", "2": f"Victoire {team_away}"}
    print(f"║  {'PRÉDICTION':14s} {verdicts[best_1x2]:<{W - 18}}  ║")
    combo = f"{best_1x2} + {best_ou} 2.5 + BTTS {best_btts}"
    print(f"║  {'COMBO':14s} {combo:<{W - 18}}  ║")
    print(f"║  {'SCORE EXACT':14s} {top_score[0]}-{top_score[1]} ({top_score[2]*100:.1f}%){'':>{W - 30}}  ║")
    print(f"║  {'BUTS ATTENDUS':14s} {lam_h + lam_a:.2f}{'':>{W - 20}}  ║")
    print(f"╚{'═' * W}╝")
    print()


# ── Saisie des cotes bookmaker ─────────────────────────────────────────────────

def ask_bookmaker_odds(team_home, team_away, markets):
    """Demande les cotes du bookmaker et lance l'analyse Value."""
    p1, px, p2 = markets["1x2"]
    po, pu = markets["over_under"]
    pby, pbn = markets["btts"]
    dc_1x, dc_12, dc_x2 = markets["double_chance"]

    print(f"{CYAN}{'─' * W}{RESET}")
    print(f"{BOLD}Entrez les cotes du bookmaker{RESET} (Entrée = passer) :\n")

    # Saisie structurée
    print(f"  {BOLD}── Marché 1X2 ──{RESET}")
    c1 = ask_cote(f"  Cote 1 ({team_home}) : ")
    cx = ask_cote(f"  Cote X (Nul)     : ")
    c2 = ask_cote(f"  Cote 2 ({team_away}) : ")

    print(f"\n  {BOLD}── Over/Under 2.5 ──{RESET}")
    c_over = ask_cote("  Cote Over 2.5    : ")
    c_under = ask_cote("  Cote Under 2.5   : ")

    print(f"\n  {BOLD}── BTTS ──{RESET}")
    c_btts_y = ask_cote("  Cote BTTS Oui    : ")
    c_btts_n = ask_cote("  Cote BTTS Non    : ")

    print(f"\n  {BOLD}── Double Chance ──{RESET}")
    c_dc_1x = ask_cote("  Cote 1X          : ")
    c_dc_12 = ask_cote("  Cote 12          : ")
    c_dc_x2 = ask_cote("  Cote X2          : ")

    selections = [
        {"label": f"1 ({team_home})", "prob": p1, "cote_book": c1},
        {"label": "X (Nul)", "prob": px, "cote_book": cx},
        {"label": f"2 ({team_away})", "prob": p2, "cote_book": c2},
        {"label": "Over 2.5", "prob": po, "cote_book": c_over},
        {"label": "Under 2.5", "prob": pu, "cote_book": c_under},
        {"label": "BTTS Oui", "prob": pby, "cote_book": c_btts_y},
        {"label": "BTTS Non", "prob": pbn, "cote_book": c_btts_n},
        {"label": "DC 1X", "prob": dc_1x, "cote_book": c_dc_1x},
        {"label": "DC 12", "prob": dc_12, "cote_book": c_dc_12},
        {"label": "DC X2", "prob": dc_x2, "cote_book": c_dc_x2},
    ]

    display_value_analysis(selections)


# ── Interaction ────────────────────────────────────────────────────────────────

def fuzzy_match(name, valid):
    low = name.strip().lower()
    for v in valid:
        if v.lower() == low:
            return v
    hits = [v for v in valid if low in v.lower() or v.lower() in low]
    return hits[0] if len(hits) == 1 else None


def ask_team(prompt, valid):
    while True:
        raw = input(prompt).strip()
        if not raw:
            continue
        m = fuzzy_match(raw, valid)
        if m:
            return m
        print(f"  ✗ « {raw} » introuvable.")
        close = [v for v in valid if raw.lower()[:3] in v.lower()]
        if close:
            print(f"    Suggestions : {', '.join(close)}")
        print()


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    print()
    print(f"╔{'═' * W}╗")
    print(f"║{'⚽  POISSON PREDICTOR — LIGUE 1  ⚽':^{W}}║")
    print(f"║{'Modèle quantitatif + Value Betting':^{W}}║")
    print(f"╚{'═' * W}╝")
    print()

    print("Chargement des données …")
    matches = fetch_matches()
    strengths, league = compute_strengths(matches)
    print(f"  {len(matches)} matchs analysés")
    print(f"  Moyenne ligue : {league['avg_home_goals']:.3f} buts/match (dom), "
          f"{league['avg_away_goals']:.3f} (ext)\n")

    valid = sorted(strengths.index.tolist())
    print(f"Équipes : {', '.join(valid)}\n")

    while True:
        team_h = ask_team("Équipe à domicile   : ", valid)
        team_a = ask_team("Équipe à l'extérieur : ", valid)

        if team_h == team_a:
            print("  ✗ Une équipe ne peut pas jouer contre elle-même.\n")
            continue

        lam_h, lam_a = compute_lambdas(strengths, league, team_h, team_a)
        mat = score_matrix(lam_h, lam_a)
        markets = derive_markets(mat)

        display_dashboard(team_h, team_a, lam_h, lam_a, mat, markets, strengths)

        # Phase Value Betting
        ask_bookmaker_odds(team_h, team_a, markets)

        again = input("Autre prédiction ? (o/n) : ").strip().lower()
        if again not in ("o", "oui", "y", "yes"):
            print("\nÀ bientôt !")
            break
        print()


if __name__ == "__main__":
    main()
