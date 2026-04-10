# T4FHSARL32 — First Home Savings Account / Releve 32

## Formulaire fiscal

- **Federal**: T4FHSA — Compte d'epargne libre d'impot pour l'achat d'une premiere propriete (CELIAPP)
- **Quebec**: Releve 32 (RL-32) — equivalent provincial
- **codFormulaireReleve**: `"T4FHSARL32"` (QC) ou `"T4FHSA"` (hors-QC) — a confirmer
- **codSousTypeDocument**: `"T4FHSAR32"` (QC) ou `"T4FHSA"` (hors-QC) — a confirmer

## Statut: SQUELETTE — Logique a implementer

Voir `../ARCHITECTURE.md` pour le pattern complet et `../T5Rl3/README.md` pour l'implementation de reference.

## Fichiers a completer

| Fichier | Ce qui reste a faire |
|---------|---------------------|
| `T4FHSARL32SlipContext.cs` | Ajuster les proprietes (cotisations, retraits, type de compte CELIAPP) |
| `T4FHSARL32SlipGenerator.cs` | Implementer `BuildContext(int seq)` |
| `T4FHSARL32IndividuSlipBuilder.cs` | Implementer `Build(T4FHSARL32SlipContext context)` |
| `T4FHSARL32OrganisationSlipBuilder.cs` | Implementer `Build(T4FHSARL32SlipContext context)` |
| `T4FHSARL32CaseBuilder.cs` | Implementer `Build(T4FHSARL32SlipContext context)` |

## Cases fiscales T4FHSA / RL-32

| Case | Description | Notes |
|------|-------------|-------|
| 14 | Cotisations designees | Montant — `RandomDecimal` |
| 16 | Retrait admissible | Montant — retrait pour achat premiere propriete |
| 18 | Retrait imposable | Montant — retrait non admissible |
| 20 | Transfert d'un REER | Montant |
| 22 | Transfert a un REER/FERR | Montant |
| 24 | Revenu couru | Montant |
| 26 | Juste valeur marchande | Montant |
| 30 | Impot retenu a la source | Montant |

### Cases RL-32 specifiques (Quebec)

| Case RL-32 | Description | Condition |
|------------|-------------|-----------|
| A | Cotisations designees | IsQc |
| B | Retraits imposables | IsQc |

## Particularites T4FHSA vs T5RL3

- **Programme recent** (depuis 2023): Le CELIAPP/FHSA est un nouveau vehicule d'epargne. Les regles peuvent evoluer.
- **Strictement individuel**: Seuls les individus peuvent detenir un CELIAPP. Le builder d'organisation est pour l'institution financiere emettrice.
- **Types de transactions**: Cotisations, retraits admissibles (achat premiere propriete), retraits imposables (non admissibles), transferts REER↔CELIAPP
- **Plafond annuel**: 8 000 $ / an, maximum viager 40 000 $ — les montants generes devraient respecter des ordres de grandeur realistes
- **Retrait admissible**: Exonere d'impot si utilise pour achat d'une premiere propriete admissible

## Contexte a ajuster

Proprietes possiblement necessaires dans `T4FHSASlipContext`:
```csharp
string Cotisations       // Case 14
string RetraitAdmissible // Case 16
string RetraitImposable  // Case 18
string ImpotRetenu       // Case 30
string TypeTransaction   // Cotisation, retrait admissible, retrait imposable
```

## Notes d'implementation

1. Le `codFormulaireReleve` depend de IsQc: `"T4FHSARL32"` vs `"T4FHSA"`
2. Montants realistes: cotisations 1 000–8 000 $, retraits 0–40 000 $
3. Un feuillet typique aura soit une cotisation, soit un retrait — pas les deux en meme temps
4. Les transferts REER↔CELIAPP sont moins frequents, les generer pour ~10% des feuillets
