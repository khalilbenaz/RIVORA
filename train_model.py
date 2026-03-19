"""
train_model.py — Entraîne un modèle de prédiction de résultats football.

Entrée  : matchups_train.csv (features de confrontation)
Sortie  : modele_foot.pkl    (meilleur modèle sérialisé)

Compare RandomForest vs XGBoost, sélectionne le meilleur,
et affiche les métriques détaillées (Accuracy, F1, matrice de confusion).
"""

import joblib
import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import StratifiedKFold, cross_validate
from sklearn.metrics import (
    accuracy_score,
    classification_report,
    confusion_matrix,
    f1_score,
)
from sklearn.preprocessing import StandardScaler
from xgboost import XGBClassifier

# ── Configuration ──────────────────────────────────────────────────────────────

INPUT_FILE = "matchups_train.csv"
MODEL_FILE = "modele_foot.pkl"
RANDOM_STATE = 42
CV_FOLDS = 5

LABEL_NAMES = ["Victoire Ext (-1)", "Nul (0)", "Victoire Dom (1)"]
LABEL_MAP = {-1: 0, 0: 1, 1: 2}  # XGBoost exige des labels 0..N


# ── Fonctions ──────────────────────────────────────────────────────────────────

def load_data(path: str):
    """Charge les données et sépare features / labels."""
    df = pd.read_csv(path)
    feature_cols = [c for c in df.columns if c.startswith("diff_")]
    X = df[feature_cols].values
    y = df["Resultat"].map(LABEL_MAP).values
    return X, y, feature_cols


def build_candidates():
    """Retourne les modèles candidats avec hyperparamètres ajustés."""
    return {
        "RandomForest": RandomForestClassifier(
            n_estimators=300,
            max_depth=6,
            min_samples_split=5,
            min_samples_leaf=3,
            class_weight="balanced",
            random_state=RANDOM_STATE,
        ),
        "XGBoost": XGBClassifier(
            n_estimators=300,
            max_depth=4,
            learning_rate=0.05,
            subsample=0.8,
            colsample_bytree=0.8,
            reg_alpha=1.0,
            reg_lambda=2.0,
            use_label_encoder=False,
            eval_metric="mlogloss",
            random_state=RANDOM_STATE,
            verbosity=0,
        ),
    }


def evaluate_cv(models: dict, X, y) -> dict:
    """Évalue chaque modèle en validation croisée stratifiée."""
    cv = StratifiedKFold(n_splits=CV_FOLDS, shuffle=True, random_state=RANDOM_STATE)
    scoring = ["accuracy", "f1_macro"]
    results = {}

    for name, model in models.items():
        scores = cross_validate(
            model, X, y, cv=cv, scoring=scoring, return_train_score=False
        )
        acc = scores["test_accuracy"]
        f1 = scores["test_f1_macro"]
        results[name] = {
            "accuracy_mean": acc.mean(),
            "accuracy_std": acc.std(),
            "f1_mean": f1.mean(),
            "f1_std": f1.std(),
        }
        print(f"  {name:15s}  Accuracy: {acc.mean():.3f} ±{acc.std():.3f}  |  F1 macro: {f1.mean():.3f} ±{f1.std():.3f}")

    return results


def train_final_model(model, X, y):
    """Entraîne le modèle sur l'ensemble des données."""
    model.fit(X, y)
    return model


def print_full_report(model, X, y, feature_cols):
    """Affiche le rapport complet sur le jeu d'entraînement."""
    y_pred = model.predict(X)

    print(f"\n── Rapport de classification (sur données complètes) ──\n")
    print(classification_report(y, y_pred, target_names=LABEL_NAMES, digits=3))

    print("── Matrice de confusion ──")
    cm = confusion_matrix(y, y_pred)
    cm_df = pd.DataFrame(cm, index=LABEL_NAMES, columns=LABEL_NAMES)
    print(cm_df.to_string())

    # Importance des features
    if hasattr(model, "feature_importances_"):
        importances = model.feature_importances_
        order = np.argsort(importances)[::-1]
        print(f"\n── Importance des features ──")
        for i in order:
            bar = "█" * int(importances[i] * 40)
            print(f"  {feature_cols[i]:25s}  {importances[i]:.3f}  {bar}")


# ── Main ───────────────────────────────────────────────────────────────────────

def main():
    print("=== Entraînement du modèle de prédiction football ===\n")

    print("1. Chargement des données …")
    X, y, feature_cols = load_data(INPUT_FILE)
    print(f"   {X.shape[0]} matchs, {X.shape[1]} features\n")

    # Normalisation
    scaler = StandardScaler()
    X_scaled = scaler.fit_transform(X)

    print(f"2. Validation croisée ({CV_FOLDS} folds) …")
    models = build_candidates()
    results = evaluate_cv(models, X_scaled, y)

    # Sélection du meilleur modèle
    best_name = max(results, key=lambda k: results[k]["f1_mean"])
    best_model = models[best_name]
    print(f"\n   → Meilleur modèle : {best_name}\n")

    print("3. Entraînement final sur toutes les données …")
    train_final_model(best_model, X_scaled, y)
    print_full_report(best_model, X_scaled, y, feature_cols)

    # Sauvegarde : modèle + scaler ensemble pour pouvoir prédire plus tard
    print(f"\n4. Sauvegarde …")
    artifact = {
        "model": best_model,
        "scaler": scaler,
        "feature_cols": feature_cols,
        "label_map": {v: k for k, v in LABEL_MAP.items()},
        "label_names": LABEL_NAMES,
        "model_name": best_name,
    }
    joblib.dump(artifact, MODEL_FILE)
    print(f"✓ Modèle sauvegardé dans {MODEL_FILE}")


if __name__ == "__main__":
    main()
