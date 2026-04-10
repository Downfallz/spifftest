# NR4 — Statement of Amounts Paid or Credited to Non-Residents of Canada

## Formulaire fiscal

- **Federal**: NR4 — Etat des sommes payees ou creditees a des non-residents du Canada
- **Quebec**: Pas d'equivalent RL provincial (formulaire federal seulement)
- **codFormulaireReleve**: `"NR4"`
- **codSousTypeDocument**: `"NR4"`

## Statut: SQUELETTE — Logique a implementer

Voir `../ARCHITECTURE.md` pour le pattern complet et `../T5Rl3/README.md` pour l'implementation de reference.

## Fichiers a completer

| Fichier | Ce qui reste a faire |
|---------|---------------------|
| `NR4SlipContext.cs` | Ajuster les proprietes au besoin (cases specifiques NR4) |
| `NR4SlipGenerator.cs` | Implementer `BuildContext(int seq)` |
| `NR4IndividuSlipBuilder.cs` | Implementer `Build(NR4SlipContext context)` |
| `NR4OrganisationSlipBuilder.cs` | Implementer `Build(NR4SlipContext context)` |
| `NR4CaseBuilder.cs` | Implementer `Build(NR4SlipContext context)` |

## Cases fiscales NR4

| Case | Description | Notes |
|------|-------------|-------|
| 14 | Revenu brut | Montant — `RandomDecimal` |
| 15 | Taux d'impot des non-residents | Pourcentage (ex: "15", "25") |
| 16 | Impot des non-residents retenu | Montant — `RandomDecimal` |
| 17 | Code d'exemption | Code optionnel |
| 24 | Code de revenu | Code NR4 (ex: "11" = dividendes, "14" = redevances, "29" = interets) |

## Particularites NR4 vs T5RL3

- **Pas de variante Quebec**: Le `codFormulaireReleve` est toujours `"NR4"`, pas de logique IsQc pour le code formulaire
- **Code de revenu (case 24)**: Identifie la nature du paiement. Valeurs courantes: 11 (dividendes), 14 (redevances), 15 (loyer), 16 (droits), 18 (rentes), 29 (interets)
- **Taux d'impot (case 15)**: Depend du pays de residence du beneficiaire et de la convention fiscale. Valeurs typiques: 10%, 15%, 25%
- **Pays du beneficiaire**: Le champ `codPaysIso` sera typiquement un pays etranger (USA, FRA, GBR, etc.) plutot que "CAN"
- **Adresse**: Peut contenir une adresse etrangere (adapter le format postal)

## Contexte a ajuster

Proprietes possiblement necessaires dans `NR4SlipContext`:
```csharp
string CodeRevenu    // Case 24 — type de revenu
string TauxImpot     // Case 15 — taux applicable
string PaysResidence // Pays du non-resident (ex: "USA", "FRA")
```

## Notes d'implementation

1. Le `codFormulaireReleve` est toujours `"NR4"` (pas de variante QC)
2. Le `codPaysIso` dans l'adresse devrait etre un pays etranger, pas "CAN"
3. Les codes de revenu (case 24) peuvent etre randomises parmi une liste predeterminee
4. Le taux d'impot (case 15) est generalement lie au pays de residence
