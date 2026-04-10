# T5RL3 — Implementation de reference

## Formulaire fiscal

- **Federal**: T5 — Etat des revenus de placements (dividendes, interets, redevances)
- **Quebec**: Releve 3 (RL-3) — equivalent provincial du T5
- **codFormulaireReleve**: `"T5RL3"` (QC) ou `"T5"` (hors-QC)
- **codSousTypeDocument**: `"T5R3"` (QC) ou `"T5"` (hors-QC)

## Statut: COMPLETE (implementation de reference)

Ce dossier contient l'implementation complete qui sert de modele pour tous les autres types.

## Fichiers

| Fichier | Role | Statut |
|---------|------|--------|
| `T5RL3SlipContext.cs` | Contexte immutable avec toutes les donnees du feuillet | Complet |
| `T5RL3SlipGenerator.cs` | Orchestre la generation: BuildContext → Build → anomalies | Complet |
| `T5RL3IndividuSlipBuilder.cs` | Structure JSON pour individus (SIN, nom, prenom) | Complet |
| `T5RL3OrganisationSlipBuilder.cs` | Structure JSON pour organisations (NE, NEQ, FID, NI) | Complet |
| `T5RL3CaseBuilder.cs` | Cases fiscales: 13, 28, 29, D (QC), Succ (QC) | Complet |

## Cases fiscales T5/RL-3

| Case | Description | Condition |
|------|-------------|-----------|
| 13 | Revenu de placement (montant) | Toujours |
| 28 | Numero de transit | Toujours |
| 29 | Numero de compte | Toujours |
| D | Montant (provincial QC) | `IsQc` seulement |
| Succ | Succursale (transit) | `IsQc` seulement |

## Proprietes specifiques du contexte

```csharp
public sealed record T5RL3SlipContext(
    string NumTransit,
    string NumCompte,
    string Province,
    bool IsQc,
    string Langue,        // "F" (QC) ou "A" (hors-QC)
    string Pays,          // "CAN"
    string TypImpression, // "PN" ou "N" (weighted)
    bool HoldMail,        // weighted
    string Devise,        // aleatoire parmi config.Devises
    string Case13,        // RandomDecimal(1, 8, 2)
    string CaseD,         // RandomDecimal(1, 8, 2)
    bool IsIndividu       // seq <= NombreIndividus
) : SlipContextBase(Province, Pays);
```

## Points cles de l'implementation

### BuildContext
- `IsIndividu` determine par `seq <= config.NombreIndividus`
- Province selectionnee avec `WeightedChoice(["QC", "Autre"], config.WeightsCodeProvince)`
- Si "Autre" et `IndicateurOntario` = true → "ON", sinon province aleatoire
- Transit number = `transitArray[(seq-1) / NombreFeuilletParCaisse % transitArray.Length]`
- Compte = `(seq % 999999).ToString("D6")`

### IndividuSlipBuilder
- `idCodSousTypePartie` = 1
- Identifications: SIN (type 1) + transit+compte (type 4)
- Champs: `prn`, `nomFamille`, `nomInitiale`
- Documents avec 5 metadonnees: PDO, institution, transit, folio, sousTypeDocument

### OrganisationSlipBuilder
- `idCodSousTypePartie` = 2
- `idCodTypeRoleRelevePartie` = genre (Societe=3, Fiducie=4, Association=5)
- Identifications complexes selon genre + province (voir code)
- Champs: `nomOrganisationLign1`, `nomOrganisationLign2`
- Documents avec PDO conditionnel (Fiducie seulement)

### Emetteur fourni (optionnel, dans Generator)
Ajoute une identification type 5 avec `context.NumTransit` si `config.AjouterEmetteurFourni`
