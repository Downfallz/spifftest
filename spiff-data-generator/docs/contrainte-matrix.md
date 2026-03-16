# Matrice de Contrainte × Capacité Organisationnelle

## Concept

Chaque feuillet fiscal généré possède un **profil de contrainte** qui reflète
l'effort organisationnel requis pour le traiter. Le score est calculé en
croisant 6 dimensions de contrainte avec leurs poids configurables.

Le « pain point » principal est que certaines combinaisons de caractéristiques
(province QC + organisation fiducie + devise étrangère + impression papier)
créent un feuillet exponentiellement plus coûteux à traiter qu'un simple
individu hors-QC en CAD électronique.

## Matrice de scoring

| Dimension                 | Condition faible        | Condition élevée           | Poids défaut |
|---------------------------|------------------------|----------------------------|:------------:|
| Réglementaire provincial  | Hors-QC                | QC (double décl. T5+RL-3) | 25           |
| Type de bénéficiaire      | Individu               | Organisation               | 15           |
| Complexité identification | Individu / Org hors-QC | Org QC (NEQ, NI, etc.)    | 20           |
| Devise étrangère          | CAD                    | Toute devise ≠ CAD         | 15           |
| Contrainte impression     | N (électronique)       | PN (papier + postal)       | 15           |
| Courrier retenu           | Non                    | Oui                        | 10           |
| **Total max**             |                        |                            | **100**      |

### Calcul du score

1. Chaque dimension produit un score brut : `0` (faible) ou `poids` (élevé)
2. Exception : `ComplexiteIdentification` pour une Org hors-QC = `poids / 2`
3. Le score total est normalisé sur 100 : `score = rawScore × 100 / maxPossible`

### Niveaux qualitatifs

| Niveau   | Plage score | Signification                                    |
|----------|:-----------:|--------------------------------------------------|
| Faible   | 0 – 25      | Traitement standard, peu de contraintes          |
| Modéré   | 26 – 50     | Quelques exigences supplémentaires               |
| Élevé    | 51 – 75     | Complexité significative, validation renforcée   |
| Critique | 76 – 100    | Maximum de contraintes, capacité org. sous pression |

Les seuils sont configurables via `SeuilModere`, `SeuilEleve`, `SeuilCritique`.

## Exemples de profils

### Profil minimal (Score: 0, Faible)
```
Individu + Ontario + CAD + Électronique (N) + Pas de courrier retenu
```

### Profil modéré (Score: 40, Modéré)
```
Individu + QC + USD + Électronique (N) + Pas de courrier retenu
→ Réglementaire (25) + Devise étrangère (15) = 40
```

### Profil élevé (Score: 75, Élevé)
```
Organisation + QC + EUR + Électronique (N) + Pas de courrier retenu
→ Réglementaire (25) + Type bénéficiaire (15) + Complexité ID (20) + Devise (15) = 75
```

### Profil critique (Score: 100, Critique)
```
Organisation + QC + USD + Impression PN + Courrier retenu
→ Toutes les dimensions au maximum = 100
```

## Configuration API

```json
{
  "Contraintes": {
    "Enabled": true,
    "PoidsReglementaire": 25,
    "PoidsTypeBeneficiaire": 15,
    "PoidsComplexiteIdentification": 20,
    "PoidsDeviseEtrangere": 15,
    "PoidsContrainteImpression": 15,
    "PoidsCourrierRetenu": 10,
    "SeuilModere": 26,
    "SeuilEleve": 51,
    "SeuilCritique": 76
  }
}
```

Quand `Enabled: true`, chaque feuillet généré contient un bloc `contrainte` :

```json
{
  "information": { ... },
  "contenu": { ... },
  "contrainte": {
    "score": 75,
    "niveau": "Eleve",
    "details": {
      "ReglementaireProvincial": 25,
      "TypeBeneficiaire": 15,
      "ComplexiteIdentification": 20,
      "DeviseEtrangere": 15,
      "ContrainteImpression": 0,
      "CourrierRetenu": 0
    }
  }
}
```

## Architecture

```
Common/Constraints/
├── ConstraintDimension.cs   # Enum des 6 dimensions
├── ConstraintNiveau.cs       # (dans ConstraintProfile.cs) Faible → Critique
├── ConstraintProfile.cs     # Record: Score + Niveau + Details
├── ConstraintConfig.cs      # Poids configurables + seuils
└── ConstraintMatrix.cs      # Logique de scoring (statique)
```

Le `SlipGenerator.BuildContext()` appelle `ConstraintMatrix.Evaluate()` si
`Contraintes.Enabled`, puis le profil est sérialisé dans le JSON de sortie.

## Faut-il plus de profils ?

**Non, les 6 dimensions actuelles suffisent.** Elles couvrent tous les axes
de complexité du domaine T5/RL-3 :

- **Réglementaire** : le seul vrai fork est QC vs reste du Canada
- **Type + Identification** : Individu → Société/Association → Fiducie
- **Devise** : binaire (CAD vs étranger)
- **Impression + Courrier** : contraintes logistiques

Si un nouveau besoin émerge (ex: déclaration amendée vs originale, ou
multi-caisse avec rotation complexe), il suffit d'ajouter une nouvelle
valeur à `ConstraintDimension` et un poids dans `ConstraintConfig`. Le
système est extensible sans changer l'architecture.
