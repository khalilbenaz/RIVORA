"""
train_model.py — Entraîne 3 modèles de prédiction football.

  1. Classifieur 1X2      (Victoire Dom / Nul / Victoire Ext)
  2. Classifieur Over/Under 2.5 buts
  3. Classifieur BTTS     (Les deux équipes marquent)

Entrée  : matchups_train.csv
Sortie  : modele_foot.pkl  (artifact contenant les 3 modèles + scaler)
"""

import joblib
import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier, GradientBoostingClassifier
from sklearn.model_selection import StratifiedKFold, cross_validate
from sklearn.metrics import classification_report, confusion_matrix
from sklearn.preprocessing import StandardScaler
from xgboost import XGBClassifier

# ── Configuration ──────────────────────────────────────────────────────────────

INPUT_FILE = "matchups_train.csv"
MODEL_FILE = "modele_foot.pkl"
RANDOM_STATE = 42
CV_FOLDS = 5

LABEL_MAP_1X2 = {-1: 0, 0: 1, 1: 2}
LABEL_NAMES_1X2 = ["Victoire Ext", "Nul", "Victoire Dom"]


# ── Helpers ────────────────────────────────────────────────────────────────────

def load_data(path: str):
    """Charge les données et prépare les features / labels pour chaque tâche."""
    df = pd.read_csv(path)
    diff_cols = [c for c in df.columns if c.startswith("diff_")]
    sum_cols = [c for c in df.columns if c.startswith("sum_")]

    # Features pour le 1X2 : uniquement les différences
    X_1x2 = df[diff_cols].values
    y_1x2 = df["Resultat"].map(LABEL_MAP_1X2).values

    # Features pour Over/Under & BTTS : différences + sommes
    all_feature_cols = diff_cols + sum_cols
    X_goals = df[all_feature_cols].values
    y_over = df["Over2_5"].values
    y_btts = df["BTTS"].values

    return {
        "X_1x2": X_1x2,
        "y_1x2": y_1x2,
        "cols_1x2": diff_cols,
        "X_goals": X_goals,
        "y_over": y_over,
        "y_btts": y_btts,
        "cols_goals": all_feature_cols,
        "n_samples": len(df),
    }


def pick_best(candidates: dict, X, y, task_name: str, stratified: bool = True):
    """Évalue les candidats en CV et retourne le meilleur (f1_macro)."""
    cv = StratifiedKFold(n_splits=CV_FOLDS, shuffle=True, random_state=RANDOM_STATE)
    scoring = ["accuracy", "f1_macro"] if len(np.unique(y)) > 2 else ["accuracy", "f1"]
    best_name, best_score, best_model = None, -1, None
    results = {}

    for name, model in candidates.items():
        scores = cross_validate(model, X, y, cv=cv, scoring=scoring, return_train_score=False)
        metric_key = "test_f1_macro" if "test_f1_macro" in scores else "test_f1"
        acc = scores["test_accuracy"]
        f1 = scores[metric_key]
        results[name] = {"acc": acc.mean(), "f1": f1.mean()}
        print(f"    {name:20s}  Acc: {acc.mean():.3f} ±{acc.std():.3f}  |  F1: {f1.mean():.3f} ±{f1.std():.3f}")
        if f1.mean() > best_score:
            best_score = f1.mean()
            best_name = name
            best_model = model

    print(f"    → Retenu : {best_name} (F1 = {best_score:.3f})\n")
    return best_name, best_model


def train_and_report(model, X, y, label_names=None):
    """Entraîne sur toutes les données et affiche le rapport."""
    model.fit(X, y)
    y_pred = model.predict(X)
    if label_names:
        print(classification_report(y, y_pred, target_names=label_names, digits=3))
    else:
        print(classification_report(y, y_pred, digits=3))

    if hasattr(model, "feature_importances_"):
        return model.feature_importances_
    return None


def print_importances(importances, feature_cols, top_n=5):
    """Affiche le top-N des features les plus influentes."""
    if importances is None:
        return
    order = np.argsort(importances)[::-1][:top_n]
    for i in order:
        bar = "█" * int(importances[i] * 40)
        print(f"    {feature_cols[i]:25s}  {importances[i]:.3f}  {bar}")
    print()


# ── Candidats par tâche ────────────────────────────────────────────────────────

def candidates_1x2():
    return {
        "RandomForest": RandomForestClassifier(
            n_estimators=300, max_depth=6, min_samples_split=5,
            min_samples_leaf=3, class_weight="balanced", random_state=RANDOM_STATE,
        ),
        "XGBoost": XGBClassifier(
            n_estimators=300, max_depth=4, learning_rate=0.05,
            subsample=0.8, colsample_bytree=0.8, reg_alpha=1.0, reg_lambda=2.0,
            eval_metric="mlogloss", random_state=RANDOM_STATE, verbosity=0,
        ),
    }


def candidates_binary():
    return {
        "RandomForest": RandomForestClassifier(
            n_estimators=300, max_depth=5, min_samples_split=4,
            min_samples_leaf=3, class_weight="balanced", random_state=RANDOM_STATE,
        ),
        "GradientBoosting": GradientBoostingClassifier(
            n_estimators=200, max_depth=3, learning_rate=0.05,
            subsample=0.8, random_state=RANDOM_STATE,
        ),
        "XGBoost": XGBClassifier(
            n_estimators=200, max_depth=3, learning_rate=0.05,
            subsample=0.8, colsample_bytree=0.8, reg_alpha=0.5, reg_lambda=1.5,
            eval_metric="logloss", random_state=RANDOM_STATE, verbosity=0,
        ),
    }


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    print("=" * 60)
    print("  Entraînement multi-modèles — Marchés de paris football")
    print("=" * 60, "\n")

    data = load_data(INPUT_FILE)
    print(f"Données : {data['n_samples']} matchs\n")

    scaler_1x2 = StandardScaler()
    X_1x2 = scaler_1x2.fit_transform(data["X_1x2"])

    scaler_goals = StandardScaler()
    X_goals = scaler_goals.fit_transform(data["X_goals"])

    # ── Modèle 1 : 1X2 ────────────────────────────────────────────────────────
    print("━" * 60)
    print("  MODÈLE 1 — Résultat 1X2")
    print("━" * 60)
    name_1x2, model_1x2 = pick_best(candidates_1x2(), X_1x2, data["y_1x2"], "1X2")
    imp = train_and_report(model_1x2, X_1x2, data["y_1x2"], LABEL_NAMES_1X2)
    print_importances(imp, data["cols_1x2"])

    # ── Modèle 2 : Over/Under 2.5 ─────────────────────────────────────────────
    print("━" * 60)
    print("  MODÈLE 2 — Over/Under 2.5 buts")
    print("━" * 60)
    name_ov, model_over = pick_best(candidates_binary(), X_goals, data["y_over"], "Over2.5")
    imp = train_and_report(model_over, X_goals, data["y_over"], ["Under 2.5", "Over 2.5"])
    print_importances(imp, data["cols_goals"])

    # ── Modèle 3 : BTTS ───────────────────────────────────────────────────────
    print("━" * 60)
    print("  MODÈLE 3 — BTTS (Les deux équipes marquent)")
    print("━" * 60)
    name_bt, model_btts = pick_best(candidates_binary(), X_goals, data["y_btts"], "BTTS")
    imp = train_and_report(model_btts, X_goals, data["y_btts"], ["Non", "Oui"])
    print_importances(imp, data["cols_goals"])

    # ── Sauvegarde ─────────────────────────────────────────────────────────────
    artifact = {
        "model_1x2": model_1x2,
        "model_over": model_over,
        "model_btts": model_btts,
        "scaler_1x2": scaler_1x2,
        "scaler_goals": scaler_goals,
        "cols_1x2": data["cols_1x2"],
        "cols_goals": data["cols_goals"],
        "label_map_1x2": {v: k for k, v in LABEL_MAP_1X2.items()},
        "model_names": {
            "1x2": name_1x2,
            "over": name_ov,
            "btts": name_bt,
        },
    }
    joblib.dump(artifact, MODEL_FILE)
    print(f"✓ 3 modèles sauvegardés dans {MODEL_FILE}")
    print(f"  1X2 : {name_1x2}  |  Over/Under : {name_ov}  |  BTTS : {name_bt}")


if __name__ == "__main__":
    main()
