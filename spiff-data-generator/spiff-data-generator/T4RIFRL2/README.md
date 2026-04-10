# T4RIFRL2 — Statement of Income from a RRIF / Releve 2

## Formulaire fiscal

- **Federal**: T4RIF — Etat du revenu provenant d'un fonds enregistre de revenu de retraite (FERR)
- **Quebec**: Releve 2 (RL-2) — Revenus de retraite et rentes
- **codFormulaireReleve**: `"T4RIFRL2"` (QC) ou `"T4RIF"` (hors-QC) — a confirmer
- **codSousTypeDocument**: `"T4RIFR2"` (QC) ou `"T4RIF"` (hors-QC) — a confirmer

## Statut: SQUELETTE — Logique a implementer

Voir `../ARCHITECTURE.md` pour le pattern complet et `../T5Rl3/README.md` pour l'implementation de reference.

## Fichiers a completer

| Fichier | Ce qui reste a faire |
|---------|---------------------|
| `T4RIFRL2SlipContext.cs` | Ajuster les proprietes (montants FERR, minimum, exces, etc.) |
| `T4RIFRL2SlipGenerator.cs` | Implementer `BuildContext(int seq)` |
| `T4RIFRL2IndividuSlipBuilder.cs` | Implementer `Build(T4RIFRL2SlipContext context)` |
| `T4RIFRL2OrganisationSlipBuilder.cs` | Implementer `Build(T4RIFRL2SlipContext context)` |
| `T4RIFRL2CaseBuilder.cs` | Implementer `Build(T4RIFRL2SlipContext context)` |

## Cases fiscales T4RIF / RL-2

| Case | Description | Notes |
|------|-------------|-------|
| 16 | Montants imposables | Montant — `RandomDecimal` |
| 18 | Montant excedentaire | Montant optionnel |
| 20 | Revenu de rente admissible | Montant |
| 22 | Montants reputes recus | Montant |
| 24 | Exces du minimum | Montant |
| 28 | Montant de transfert admissible | Montant |
| 30 | Impot retenu a la source | Montant |
| 34 | Impot retenu | Montant federal |

### Cases RL-2 specifiques (Quebec)

| Case RL-2 | Description | Condition |
|-----------|-------------|-----------|
| A | Rentes (autres) | IsQc |
| B | Prestations au titre d'un RPDB | IsQc |
| C | Autres revenus | IsQc |

## Particularites T4RIF vs T5RL3

- **Variante Quebec**: Utilise RL-2 au Quebec, comme T4RSP. Le `codFormulaireReleve` varie selon IsQc
- **Beneficiaire individu**: Le FERR est detenu par un individu. Le rentier est la partie principale.
- **Minimum FERR**: Le montant minimum est calcule selon l'age du rentier — pour la generation, utiliser un montant aleatoire
- **Impot retenu**: Case 30/34 — retenue a la source presente contrairement au T5 qui n'en a pas necessairement

## Contexte a ajuster

Proprietes possiblement necessaires dans `T4RIFSlipContext`:
```csharp
string Case16  // Montants imposables
string Case18  // Montant excedentaire
string Case20  // Revenu de rente admissible
string Case24  // Exces du minimum
string Case30  // Impot retenu
```

## Notes d'implementation

1. Le `codFormulaireReleve` depend de IsQc: `"T4RIFRL2"` vs `"T4RIF"` (pattern identique a T5RL3)
2. Le FERR est un produit individuel — l'OrganisationSlipBuilder est pour l'emetteur institutionnel
3. Les cases RL-2 specifiques au Quebec s'ajoutent comme les cases D/Succ dans T5RL3
