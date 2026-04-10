# T5008RL18 — Statement of Securities Transactions / Releve 18

## Formulaire fiscal

- **Federal**: T5008 — Etat des operations sur titres
- **Quebec**: Releve 18 (RL-18) — Transactions de titres
- **codFormulaireReleve**: `"T5008RL18"` (QC) ou `"T5008"` (hors-QC) — a confirmer
- **codSousTypeDocument**: `"T5008R18"` (QC) ou `"T5008"` (hors-QC) — a confirmer

## Statut: SQUELETTE — Logique a implementer

Voir `../ARCHITECTURE.md` pour le pattern complet et `../T5Rl3/README.md` pour l'implementation de reference.

## Fichiers a completer

| Fichier | Ce qui reste a faire |
|---------|---------------------|
| `T5008RL18SlipContext.cs` | Ajuster les proprietes (titres, produit de disposition, cout, etc.) |
| `T5008RL18SlipGenerator.cs` | Implementer `BuildContext(int seq)` |
| `T5008RL18IndividuSlipBuilder.cs` | Implementer `Build(T5008RL18SlipContext context)` |
| `T5008RL18OrganisationSlipBuilder.cs` | Implementer `Build(T5008RL18SlipContext context)` |
| `T5008RL18CaseBuilder.cs` | Implementer `Build(T5008RL18SlipContext context)` |

## Cases fiscales T5008 / RL-18

| Case | Description | Notes |
|------|-------------|-------|
| 15 | Nombre de titres | Entier — nombre d'unites/actions |
| 16 | Cout ou valeur comptable | Montant — `RandomDecimal` |
| 20 | Produit de disposition | Montant — `RandomDecimal` |
| 21 | Type de titre | Code: actions, obligations, fonds communs, options, etc. |
| 24 | Date de reglement | Format AAAA-MM-JJ |
| 25 | Description du titre | Texte libre (nom du fond, symbole boursier, etc.) |

### Cases RL-18 specifiques (Quebec)

| Case RL-18 | Description | Condition |
|------------|-------------|-----------|
| A | Produit de disposition | IsQc |
| B | Cout ou valeur comptable | IsQc |
| C | Frais ou commissions | IsQc |

## Particularites T5008 vs T5RL3

- **Transactions multiples**: Un meme individu peut avoir plusieurs T5008 (un par transaction). Pour la generation, chaque seq = une transaction.
- **Type de titre (case 21)**: Valeurs courantes — "MF" (fonds communs), "SH" (actions), "BD" (obligations), "TR" (fiducie), "OP" (options), "OT" (autres)
- **Gain/perte en capital**: Le produit (case 20) minus le cout (case 16) = gain ou perte. La generation peut creer les deux scenarios.
- **Description du titre (case 25)**: Un champ texte — generer un nom fictif de fond ou un symbole boursier
- **Volume eleve**: Ce type peut generer beaucoup de feuillets car chaque transaction est un feuillet distinct

## Contexte a ajuster

Proprietes possiblement necessaires dans `T5008SlipContext`:
```csharp
int NombreTitres        // Case 15
string CoutComptable    // Case 16
string ProduitDisposition  // Case 20
string TypeTitre        // Case 21 — "MF", "SH", "BD", etc.
string DescriptionTitre // Case 25
```

## Notes d'implementation

1. Le `codFormulaireReleve` depend de IsQc: `"T5008RL18"` vs `"T5008"`
2. Le type de titre peut etre randomise parmi les codes valides
3. Le produit de disposition devrait etre > cout ~70% du temps (pour simuler des gains), < cout ~30% du temps (pertes)
4. La description du titre peut etre generee avec `CompanyName()` + suffixe du type ("Fund", "ETF", "Corp")
