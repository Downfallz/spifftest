# T4RSPRL2 — Statement of RRSP Income / Releve 2

## Formulaire fiscal

- **Federal**: T4RSP — Etat du revenu provenant d'un REER
- **Quebec**: Releve 2 (RL-2) — Revenus de retraite et rentes
- **codFormulaireReleve**: `"T4RSPRL2"` (QC) ou `"T4RSP"` (hors-QC) — a confirmer
- **codSousTypeDocument**: `"T4RSPR2"` (QC) ou `"T4RSP"` (hors-QC) — a confirmer

## Statut: SQUELETTE — Logique a implementer

Voir `../ARCHITECTURE.md` pour le pattern complet et `../T5Rl3/README.md` pour l'implementation de reference.

## Fichiers a completer

| Fichier | Ce qui reste a faire |
|---------|---------------------|
| `T4RSPRL2SlipContext.cs` | Ajuster les proprietes (retraits REER, RAP, REEP, etc.) |
| `T4RSPRL2SlipGenerator.cs` | Implementer `BuildContext(int seq)` |
| `T4RSPRL2IndividuSlipBuilder.cs` | Implementer `Build(T4RSPRL2SlipContext context)` |
| `T4RSPRL2OrganisationSlipBuilder.cs` | Implementer `Build(T4RSPRL2SlipContext context)` |
| `T4RSPRL2CaseBuilder.cs` | Implementer `Build(T4RSPRL2SlipContext context)` |

## Cases fiscales T4RSP / RL-2

| Case | Description | Notes |
|------|-------------|-------|
| 16 | Revenu de REER | Montant imposable — `RandomDecimal` |
| 18 | Revenu couru | Montant |
| 20 | Remboursement de primes | Montant |
| 22 | Montant RAP (Regime d'accession a la propriete) | Montant |
| 26 | Montant REEP (Regime d'encouragement a l'education) | Montant |
| 28 | Autres revenus ou deductions | Montant |
| 30 | Impot retenu a la source | Montant |
| 34 | Impot retenu federal | Montant |

### Cases RL-2 specifiques (Quebec)

| Case RL-2 | Description | Condition |
|-----------|-------------|-----------|
| A | Rentes (autres) | IsQc |
| B | Prestations RPDB | IsQc |
| C | Autres revenus | IsQc |

## Particularites T4RSP vs T5RL3

- **T4RSP = retraits/revenus du REER** (a ne pas confondre avec RRSP qui est les cotisations)
- **Variante Quebec**: Partage le RL-2 avec T4RIF. Le `codFormulaireReleve` varie selon IsQc
- **RAP et REEP**: Cases 22 et 26 specifiques aux programmes federaux. Le RAP permet un retrait pour achat de maison, le REEP pour les etudes.
- **Types de retrait**: Le type de paiement peut influencer les cases (retrait normal, deces du rentier, RAP, REEP)
- **Impot retenu**: Retenue a la source obligatoire sur les retraits REER (sauf RAP/REEP)

## Contexte a ajuster

Proprietes possiblement necessaires dans `T4RSPSlipContext`:
```csharp
string Case16  // Revenu de REER
string Case22  // Montant RAP
string Case26  // Montant REEP
string Case30  // Impot retenu
string TypeRetrait  // Normal, RAP, REEP, deces
```

## Notes d'implementation

1. Le `codFormulaireReleve` depend de IsQc: `"T4RSPRL2"` vs `"T4RSP"`
2. Les retraits REER sont individuels — le beneficiaire est toujours une personne physique
3. Pour la generation, randomiser le type de retrait et les montants en consequence
4. Les cases RAP (22) et REEP (26) sont mutuellement exclusives dans la plupart des cas
