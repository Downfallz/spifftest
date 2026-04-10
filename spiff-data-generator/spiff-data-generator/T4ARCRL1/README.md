# T4ARCRL1 — Retirement Compensation Arrangement / Releve 1

## Formulaire fiscal

- **Federal**: T4A-RCA — Etat des montants attribues d'une convention de retraite
- **Quebec**: Releve 1 (RL-1) — Revenus d'emploi et revenus divers
- **codFormulaireReleve**: `"T4ARCRL1"` (QC) ou `"T4ARC"` (hors-QC) — a confirmer
- **codSousTypeDocument**: `"T4ARCR1"` (QC) ou `"T4ARC"` (hors-QC) — a confirmer

## Statut: SQUELETTE — Logique a implementer

Voir `../ARCHITECTURE.md` pour le pattern complet et `../T5Rl3/README.md` pour l'implementation de reference.

## Fichiers a completer

| Fichier | Ce qui reste a faire |
|---------|---------------------|
| `T4ARCRL1SlipContext.cs` | Ajuster les proprietes (distributions RCA, impot remboursable, etc.) |
| `T4ARCRL1SlipGenerator.cs` | Implementer `BuildContext(int seq)` |
| `T4ARCRL1IndividuSlipBuilder.cs` | Implementer `Build(T4ARCRL1SlipContext context)` |
| `T4ARCRL1OrganisationSlipBuilder.cs` | Implementer `Build(T4ARCRL1SlipContext context)` |
| `T4ARCRL1CaseBuilder.cs` | Implementer `Build(T4ARCRL1SlipContext context)` |

## Cases fiscales T4A-RCA / RL-1

| Case | Description | Notes |
|------|-------------|-------|
| 14 | Distributions de la convention | Montant — `RandomDecimal` |
| 16 | Cotisations de l'employe | Montant |
| 18 | Montant d'impot remboursable | Montant |
| 20 | Montant admissible au transfert | Montant |
| 22 | Impot retenu a la source | Montant |

### Cases RL-1 specifiques (Quebec)

| Case RL-1 | Description | Condition |
|-----------|-------------|-----------|
| A | Revenus d'emploi | IsQc |
| B | Cotisations au RRQ | IsQc |
| D | Regime de retraite | IsQc |
| E | Impot du Quebec retenu | IsQc |
| O | Autres revenus | IsQc — la case principale pour les distributions RCA |

## Particularites T4A-RCA vs T5RL3

- **Convention de retraite (RCA)**: Un arrangement entre employeur et employe pour une retraite supplementaire. Les montants sont generalement eleves.
- **Parties**: L'employeur est l'emetteur, l'employe/retraite est le beneficiaire. Structure similaire a T5RL3 mais avec un contexte employeur-employe.
- **Impot remboursable (case 18)**: Un mecanisme unique au RCA — l'impot paye au moment de la cotisation est partiellement remboursable lors de la distribution.
- **RL-1 partage**: Le Releve 1 au Quebec est aussi utilise pour les revenus d'emploi (T4) — le T4A-RCA y contribue via la case O principalement.
- **Montants eleves**: Les distributions RCA sont typiquement plus elevees que les revenus de placement T5 (10 000 $ — 500 000 $+)

## Contexte a ajuster

Proprietes possiblement necessaires dans `T4ARCSlipContext`:
```csharp
string Distributions     // Case 14
string CotisationsEmploye // Case 16
string ImpotRemboursable  // Case 18
string MontantTransfert   // Case 20
string ImpotRetenu        // Case 22
```

## Notes d'implementation

1. Le `codFormulaireReleve` depend de IsQc: `"T4ARCRL1"` vs `"T4ARC"`
2. Les distributions RCA sont principalement individuelles (un retraite recoit des montants)
3. Montants realistes: distributions 10 000–500 000 $, cotisations 5 000–100 000 $
4. L'impot remboursable est generalement ~50% des cotisations totales au RCA
5. Le RL-1 a une structure de cases differente (A, B, D, E, O) — la case O est la plus pertinente pour les RCA
