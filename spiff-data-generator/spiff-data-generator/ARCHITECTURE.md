# SPIFF Data Generator — Architecture

## Vue d'ensemble

Generateur de donnees de feuillets fiscaux canadiens (T5, NR4, T4RSP, etc.) en format JSON compresse en ZIP. Chaque type de feuillet a sa propre implementation mais reutilise une infrastructure commune (DI, randomisation, anomalies, export ZIP).

## Pattern d'implementation par type de feuillet

Chaque type de feuillet suit exactement le meme pattern. Voici les 5 classes a implementer, en utilisant `T5RL3` comme reference (`T5Rl3/`).

### 1. SlipContext (record)

**Role**: Contient toutes les donnees necessaires pour construire un feuillet. Immutable.

```
{Type}SlipContext : SlipContextBase(Province, Pays)
```

- Herite de `SlipContextBase` (defini dans `Common/Models/SlipContextBase.cs`, namespace `spiff_data_generator.T5Rl3.Models`)
- Doit implementer `ISlipContext` (via heritage de SlipContextBase)
- Proprietes typiques: NumTransit, NumCompte, Province, IsQc, Langue, Pays, TypImpression, HoldMail, Devise, IsIndividu
- Ajouter les proprietes specifiques au type (ex: cases fiscales propres au formulaire)

**Reference**: `T5Rl3/T5RL3SlipContext.cs`

### 2. SlipGenerator (class)

**Role**: Orchestre la generation d'un feuillet pour un numero de sequence donne.

```
{Type}SlipGenerator : ISlipGenerator
```

**Dependances injectees**:
- `GeneratorConfig` — configuration du run
- `IRandomService` — generation aleatoire deterministe
- `IEnumerable<ISlipBuilder<{Type}SlipContext>>` — builders (individu + organisation)
- `IAnomalyService` — injection d'anomalies
- `IGenerationLogger` — journalisation

**Methodes**:
- `Generate(int seq)` — methode publique, deja implementee dans les squelettes:
  1. Appelle `BuildContext(seq)` pour creer le contexte
  2. Trouve le bon builder via `CanBuild(context)`
  3. Appelle `builder.Build(context)`
  4. Ajoute l'ID unique si active dans config
  5. Applique l'anomalie si applicable
- `BuildContext(int seq)` — **A IMPLEMENTER**: construit le contexte a partir du seq

**Logique typique de BuildContext**:
```csharp
private {Type}SlipContext BuildContext(int seq)
{
    bool isIndividu = seq <= _config.NombreIndividus;

    // Province avec poids (QC vs autre)
    string province = _random.WeightedChoice(
        new[] { "QC", "Autre" }, _config.WeightsCodeProvince);
    if (province == "Autre")
    {
        province = _config.IndicateurOntario
            ? "ON"
            : _random.RandomChoice(new[] { "AB", "BC", "MB", ... });
    }

    bool isQc = province == "QC";

    // Transit number base sur index de caisse
    int transitIndex = (seq - 1) / _config.NombreFeuilletParCaisse;
    var transitArray = _config.IndicateurOntario
        ? Constants.TransitNumbersOntario : Constants.TransitNumbers;
    string numTransit = transitArray[transitIndex % transitArray.Length];

    // Numero de compte
    string numCompte = (seq % 999_999 == 0 ? 999999 : seq % 999_999)
        .ToString("D6", CultureInfo.InvariantCulture);

    // Construire le contexte avec les valeurs specifiques au type
    return new {Type}SlipContext(...);
}
```

**Reference**: `T5Rl3/T5RL3SlipGenerator.cs`

### 3. IndividuSlipBuilder (class)

**Role**: Construit le dictionnaire JSON pour un feuillet d'individu (personne physique).

```
{Type}IndividuSlipBuilder : ISlipBuilder<{Type}SlipContext>
```

- `CanBuild(context)` → `context.IsIndividu`
- `Build(context)` — **A IMPLEMENTER**: retourne `Dictionary<string, object>`

**Structure JSON attendue** (commune a tous les types):
```
{
  "information": {
    "codFormulaireReleve": "...",     // Code du formulaire (ex: "T5RL3", "NR4", etc.)
    "codLangue": "F" ou "A",
    "codDevise": "CAD",
    "typImpression": "PN" ou "N",
    "holdMail": true/false,
    "numIdentificationEmetteur": "81500008",
    "parties": [
      {
        "idCodSousTypePartie": 1,         // 1 = individu
        "idCodRoleRelevePartie": 1,
        "idCodTypeRoleRelevePartie": 1,
        "identificationPartie": [
          { "idCodTypeIdentificationPartie": 1, "numIdentificationPartie": "SIN" },
          { "idCodTypeIdentificationPartie": 4, "numIdentificationPartie": "transit+compte" }
        ],
        "prn": "Prenom",
        "nomFamille": "Nom",
        "nomInitiale": "P",
        "adresseFiscale": { ... },
        "indAdFiscalePostaleIdentique": true
      }
    ],
    "documents": [ { "metadonneesDocument": [...] } ]
  },
  "contenu": {
    "cases": [
      { "case": "13", "valeur": "1234.56" },
      ...
    ]
  }
}
```

**Reference**: `T5Rl3/T5RL3IndividuSlipBuilder.cs`

### 4. OrganisationSlipBuilder (class)

**Role**: Construit le dictionnaire JSON pour un feuillet d'organisation.

```
{Type}OrganisationSlipBuilder : ISlipBuilder<{Type}SlipContext>
```

- `CanBuild(context)` → `!context.IsIndividu`
- `Build(context)` — **A IMPLEMENTER**

**Differences vs individu**:
- `idCodSousTypePartie` = 2 (organisation)
- `idCodTypeRoleRelevePartie` = genre d'organisation (3=Societe, 4=Fiducie, 5=Association)
- Identifications: NE (type 2), NEQ (type 6), NI (type 7), FID (type 8) selon le genre
- Champs: `nomOrganisationLign1`, `nomOrganisationLign2` au lieu de `prn`, `nomFamille`

**Reference**: `T5Rl3/T5RL3OrganisationSlipBuilder.cs`

### 5. CaseBuilder (static class)

**Role**: Construit la liste des cases fiscales specifiques au type de feuillet.

```
static {Type}CaseBuilder
```

- `Build(context)` → `List<object>` de `Dictionary<string, object>` avec `{ "case": "XX", "valeur": "..." }`
- Les cases sont specifiques a chaque formulaire fiscal
- Certaines cases s'appliquent seulement au Quebec (IsQc)

**Reference**: `T5Rl3/T5RL3CaseBuilder.cs`

---

## Services communs (reutilisables tels quels)

| Service | Interface | Implementation | Role |
|---------|-----------|----------------|------|
| Random | `IRandomService` | `RandomService` | SIN, NEQ, NI, FID, noms, adresses, choix ponderes |
| Anomalies | `IAnomalyService` | `AnomalyService` | Injection d'erreurs sur les derniers feuillets |
| Export | `IZipExporter` | `ZipExporter` | Serialisation JSON → ZIP par batch |
| Logger | `IGenerationLogger` | `FileGenerationLogger` | Log des anomalies et progression |
| Adresse | `AdresseHelper` (static) | — | Construction d'adresse canadienne |
| Config | `GeneratorConfig` | — | Toute la config partagee |

### IRandomService — methodes disponibles

```
RandomChoice<T>(vals)              // Choix aleatoire uniforme
WeightedChoice<T>(vals, weights)   // Choix avec poids
FixedDigits(length)                // Ex: "00412753"
RandomDecimal(minLeft, maxLeft, decimals)  // Ex: "12345.67"
GenerateSIN()                      // NAS valide (Luhn)
GenerateNEQ(genre)                 // NEQ valide avec prefixe selon genre
GenerateNI()                       // NI valide (facteurs ponderes)
GenerateAccount()                  // Numero de compte
GenerateCanadianPostalCode(province) // Code postal valide pour la province
FirstName(), LastName()            // Noms (Bogus en_CA)
CompanyName(), CompanySuffix()     // Noms d'entreprise
StreetName(), City(), BuildingNumber(), SecondaryAddress()
```

---

## Enregistrement DI (ServiceProviderFactory.cs)

Chaque type a un case dans le switch:
```csharp
case "{TYPE_CODE}":
    services.AddSingleton<ISlipBuilder<{Type}SlipContext>, {Type}IndividuSlipBuilder>();
    services.AddSingleton<ISlipBuilder<{Type}SlipContext>, {Type}OrganisationSlipBuilder>();
    services.AddSingleton<ISlipGenerator, {Type}SlipGenerator>();
    break;
```

## Configuration (GeneratorConfigLoader.cs + datagenerator-config.json)

Chaque type a besoin de:
1. Un case dans `GeneratorConfigLoader.Load()` mappant le code vers la section JSON
2. Une section dans `datagenerator-config.json` avec la meme structure que T5RL3

---

## Codes d'identification des parties

| Code | Type | Utilise pour |
|------|------|--------------|
| 1 | NAS (SIN) | Individus |
| 2 | NE (Numero d'entreprise) | Societes, Associations |
| 4 | Transit + Compte | Tous |
| 5 | Emetteur fourni | Optionnel (config) |
| 6 | NEQ | Organisations QC |
| 7 | NI | Fiducies QC |
| 8 | FID | Fiducies hors-QC |

## Types d'organisation

| Enum | Code | Genre |
|------|------|-------|
| `OrganisationType.Societe` | 3 | Societe |
| `OrganisationType.Fiducie` | 4 | Fiducie |
| `OrganisationType.Association` | 5 | Association |

## Anomalies

Le systeme d'anomalies est generique et s'applique a tous les types de feuillets. Il modifie le dictionnaire JSON en vidant des champs specifiques. Les anomalies sont appliquees aux **derniers** feuillets du run (fin de sequence).

Le `AnomalyService.Apply()` s'attend a la structure standard:
- `root["information"]["parties"][0]` — la partie principale
- `root["information"]["parties"][0]["adresseFiscale"]` — l'adresse
- `root["information"]["parties"][0]["identificationPartie"]` — les identifications
- `root["contenu"]["cases"]` — les cases fiscales

**Important**: Tant que les builders produisent cette structure, les anomalies fonctionnent automatiquement.

## Flow complet

```
Program.Main()
  → ConsoleDataGenerator.Run()
    → ConsoleUi.PromptTypeFeuillet(Constants.TypesFeuillet)
    → GeneratorConfigLoader.Load(typeFeuillet)
    → ConsoleUi.DisplayConfig(typeFeuillet, config)
    → GeneratorRunner.Run(typeFeuillet, config)
      → ServiceProviderFactory.Build(typeFeuillet, config, logger)
      → ZipExporter.ExportToFile()
        → Pour chaque seq 1..NombreLignes:
          → {Type}SlipGenerator.Generate(seq)
            → BuildContext(seq)
            → builder.Build(context)
            → anomalyService.Apply(root, kind, isIndividu)
        → Serialise en JSON → ZIP
      → ConsoleUi.ShowSummary(...)
```
