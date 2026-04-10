# RRSP — Regime enregistre d'epargne-retraite (Cotisations)

## Formulaire fiscal

- **Federal**: Recu de cotisation REER
- **Quebec**: Cotisation au REER
- **codFormulaireReleve**: `"RRSP"` (a confirmer selon le systeme cible)
- **codSousTypeDocument**: `"RRSP"` (a confirmer)

## Statut: SQUELETTE — Logique a implementer

Voir `../ARCHITECTURE.md` pour le pattern complet et `../T5Rl3/README.md` pour l'implementation de reference.

## Fichiers a completer

| Fichier | Ce qui reste a faire |
|---------|---------------------|
| `RRSPSlipContext.cs` | Ajuster les proprietes (cotisations, type de regime, etc.) |
| `RRSPSlipGenerator.cs` | Implementer `BuildContext(int seq)` |
| `RRSPIndividuSlipBuilder.cs` | Implementer `Build(RRSPSlipContext context)` |
| `RRSPOrganisationSlipBuilder.cs` | Implementer `Build(RRSPSlipContext context)` |
| `RRSPCaseBuilder.cs` | Implementer `Build(RRSPSlipContext context)` |

## Cases fiscales RRSP

| Case | Description | Notes |
|------|-------------|-------|
| 16 | Cotisation REER | Montant — `RandomDecimal` |
| 18 | Date de cotisation | Format AAAA-MM-JJ |
| 20 | Numero de contrat | Identifiant du regime |
| 22 | Type de cotisant | Rentier ou conjoint |
| 24 | Cotisations excedentaires | Montant optionnel |

## Particularites RRSP vs T5RL3

- **Principalement individuel**: Les cotisations REER sont faites par des individus. Les organisations emettent le recu mais le beneficiaire est toujours un individu.
- **OrganisationSlipBuilder**: Pourrait ne pas etre necessaire ou avoir une structure tres differente (emetteur institutionnel). Evaluer si un builder d'organisation est pertinent pour ce type.
- **Date de cotisation**: Champ supplementaire non present dans T5RL3
- **Saison fiscale**: Les cotisations des 60 premiers jours de l'annee peuvent s'appliquer a l'annee precedente

## Contexte a ajuster

Proprietes possiblement necessaires dans `RRSPSlipContext`:
```csharp
string MontantCotisation  // Case 16
string DateCotisation     // Case 18
string NumeroContrat      // Case 20
string TypeCotisant       // Rentier ou conjoint
```

## Notes d'implementation

1. Determiner le `codFormulaireReleve` exact selon le systeme SPIFF
2. Les recus RRSP sont emis par l'institution financiere (emetteur) pour le cotisant (beneficiaire)
3. La structure "parties" suit le meme pattern mais le contenu des cases est specifique au REER
