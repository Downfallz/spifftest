using System.Globalization;
using System.IO.Compression;
using Bogus;
using Newtonsoft.Json;

namespace spiff_data_generator;

public class T5RI3Generator
{
    private readonly T5RI3Config cfg;
    private readonly Random rng;
    private readonly Faker faker;

    public T5RI3Generator(T5RI3Config config)
    {
        cfg = config;
        rng = new Random(cfg.Seed);
        faker = new Faker("fr_CA");
    }

    /// <summary>
    /// Génère le ZIP à l'emplacement calculé à partir de la config.
    /// </summary>
    public void GenerateToFile()
    {
        var currentDate = DateTime.Today.ToString(format: "yyyyMMdd");
        var filePrefix = $"{cfg.GetOutputPrefix()}_{currentDate}";
        var zipPath = Path.Combine(cfg.OutputDir, $"{filePrefix}.zip");
        Directory.CreateDirectory(cfg.OutputDir);

        using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        GenerateToStream(fs, currentDate);
        Console.WriteLine($"ZIP généré: {zipPath}");
    }

    /// <summary>
    /// Génère le ZIP vers un stream fourni (utile pour un API).
    /// </summary>
    public void GenerateToStream(Stream output, string? yyyymmdd = null)
    {
        var dateString = yyyymmdd ?? DateTime.Today.ToString(format: "yyyyMMdd");
        int jsonFileIndex = 1;

        using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);

        var startTime = DateTime.Now;

        for (int i = 0; i < cfg.NombreLignes; i += cfg.BatchSize)
        {
            int startSeq = i + 1;
            int endSeq = Math.Min(startSeq + cfg.BatchSize, 1 + cfg.NombreLignes);

            var entry = zip.CreateEntry($"{cfg.GetOutputPrefix()}_{jsonFileIndex}.json", CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            using var sw = new StreamWriter(entryStream);
            using var jw = new JsonTextWriter(sw) { Formatting = cfg.PrettyPrint ? Formatting.Indented : Formatting.None };

            jw.WriteStartArray();

            for (int seq = startSeq; seq < endSeq; seq++)
            {
                var obj = GenerateObject(seq);
                JsonSerializer.CreateDefault(new JsonSerializerSettings
                {
                    Culture = CultureInfo.InvariantCulture,
                    StringEscapeHandling = StringEscapeHandling.Default
                }).Serialize(jw, obj);
            }

            jw.WriteEndArray();

            jsonFileIndex++;
        }

        var endTime = DateTime.Now;
        Console.WriteLine($"Génération terminée à: {endTime:HH:mm:ss}");
        Console.WriteLine($"Durée totale: {endTime - startTime}");
    }

    private object GenerateObject(int seq)
    {
        bool isIndividu = seq <= cfg.NombreIndividus;

        // Province (QC vs Autre)
        string province = RandomUtils.WeightedChoice(rng,
            vals: new[] { "QC", "Autres" }, cfg.WeightsCodeProvince);
        if (province == "Autres")
            province = RandomUtils.RandomChoice(rng,
                new[] { "AB", "BC", "MB", "NB", "NS", "NL", "PE", "SK", "NT", "NU", "YT" });

        bool isQc = province == "QC";

        // Impression & hold mail
        string typImpression = RandomUtils.WeightedChoice(rng, vals: new[] { "O", "N" }, cfg.WeightsImpression);
        bool holdMail = RandomUtils.WeightedChoice(rng, vals: new[] { true, false }, cfg.WeightsCourrierRetenu);

        // Transit
        string pays = "CAN";
        string langue = isQc ? "FR" : "EN";
        string numTransit = RandomUtils.RandomChoice(rng,
            cfg.IndicateurOntario
                ? Constants.NUMEROS_INSTITUTION_TRANSIT_ONTARIO
                : Constants.NUMEROS_INSTITUTION_TRANSIT);

        // Compte
        string numCompte = RandomUtils.GenerateAccount(rng);

        // Montants (max 8 entiers + 2 décimales)
        string case13 = RandomUtils.RandomDecimal(rng, minLeft: 1, maxLeft: 8, decimals: 2);
        string caseD = RandomUtils.RandomDecimal(rng, minLeft: 1, maxLeft: 8, decimals: 2);
        string devise = RandomUtils.RandomChoice(rng, cfg.Devises);

        if (isIndividu)
            return BuildIndividu(numTransit, numCompte, province, isQc, langue, pays, typImpression, holdMail, devise,
                case13, caseD);

        return BuildOrganisation(numTransit, numCompte, province, isQc, langue, pays, typImpression, holdMail, devise,
            case13, caseD);
    }

    private object BuildIndividu(
        string numTransit, string numCompte, string province, bool isQc, string langue, string pays, string typImpression, bool holdMail, string devise,
        string case13, string caseD)
    {
        string prenom = faker.Name.FirstName().ToUpperInvariant();
        string nom = faker.Name.LastName().ToUpperInvariant();

        var root = new Dictionary<string, object>
        {
            ["information"] = new Dictionary<string, object>
            {
                ["codeFormulaire"] = isQc ? "T5RL3" : "T5",
                ["codeAnnee"] = cfg.AnneeProduction,
                ["codeSuppression"] = "N",
                ["typImpression"] = typImpression,
                ["holdMailIndicateur"] = holdMail,
                ["numIdentification"] = RandomUtils.GenerateSIN(rng),
                ["parties"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["codeSpeLevelCode"] = 1,
                        ["identTypeIdentificationParts"] = new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                ["numIdentificationParts"] = 1,
                            },
                            new Dictionary<string, object>
                            {
                                ["numIdentificationParts"] = 4,
                            }
                        }
                    }
                }
            },
            ["nom"] = nom,
            ["prenom"] = prenom,
            ["nomRue"] = faker.Address.StreetName().ToUpperInvariant(),
            ["nomMunicipalite"] = faker.Address.City().ToUpperInvariant(),
            ["secondaryAddress"] = faker.Address.SecondaryAddress().ToUpperInvariant(),
            ["codeProvince"] = province,
            ["codePaysIso"] = pays,
            ["numCodePostal"] = RandomUtils.GenerateCanadianPostalCode(rng, province).Replace(" ", ""),
            ["indicateurFiscalPostalIdentique"] = true,
            ["documents"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["metadonneesDocument"] = new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "numIdentifiantPDO",
                            ["valMetadonneeDocument"] = RandomUtils.FixedDigits(rng, length: 11)
                        },
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "numIdentifiantInstitution",
                            ["valMetadonneeDocument"] = numTransit
                        },
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "numIdentifiantTransit",
                            ["valMetadonneeDocument"] = numTransit.Substring(startIndex: 0, length: 3)
                        },
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "numIdentifiantFolio",
                            ["valMetadonneeDocument"] = numTransit.Substring(3)
                        },
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "codSousTypeDocument",
                            ["valMetadonneeDocument"] = isQc ? "T5RL3" : "T5"
                        }
                    }
                }
            },
            ["content"] = BuildCases(numTransit, numCompte, case13, caseD, isQc)
        };

        return root;
    }

    private object BuildOrganisation(
        string numTransit, string numCompte, string province, bool isQc, string langue, string pays, string typImpression, bool holdMail, string devise,
        string case13, string caseD)
    {
        int genre = RandomUtils.RandomChoice(rng, vals: Constants.TYPES_ORGANISATION);
        string neq = RandomUtils.GenerateNEQ(rng, genre);
        string fid = RandomUtils.FixedDigits(rng, length: 9);
        string nl = RandomUtils.GenerateAccount(rng);

        string nom = faker.Company.CompanyName().ToUpperInvariant().Replace(",", "");

        var identification = new List<object>
        {
            new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 4,
                ["numIdentificationPartie"] = numTransit + numCompte
            }
        };

        var documents = new List<object>
        {
            new Dictionary<string, object>
            {
                ["metadonneesDocument"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantInstitution",
                        ["valMetadonneeDocument"] = numTransit.Substring(startIndex: 0, length: 3)
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantTransit",
                        ["valMetadonneeDocument"] = numTransit.Substring(3)
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantFolio",
                        ["valMetadonneeDocument"] = numCompte.PadLeft(totalWidth: 7, paddingChar: '0')
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "codSousTypeDocument",
                        ["valMetadonneeDocument"] = isQc ? "T5RL3" : "T5"
                    },
                }
            }
        };

        if (genre == 3 || genre == 5)
        {
            identification.Add(new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 2, // NE
                ["numIdentificationPartie"] = neq
            });
        }

        if (isQc)
        {
            identification.Add(new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 6, // NEQ
                ["numIdentificationPartie"] = neq
            });
        }
        else if (genre == 4)
        {
            // Ajoute PDO pour fichier
            ((List<object>)((Dictionary<string, object>)documents[0])["metadonneesDocument"])
                .Add(new Dictionary<string, object>
                {
                    ["codTypeMetadonneeDocument"] = "numIdentifiantPDO",
                    ["valMetadonneeDocument"] = RandomUtils.FixedDigits(rng, length: 11)
                });
        }

        identification.Add(new Dictionary<string, object>
        {
            ["idCodTypeIdentificationPartie"] = 8, // FID
            ["numIdentificationPartie"] = fid
        });

        if (isQc)
        {
            identification.Add(new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 7, // NZ
                ["numIdentificationPartie"] = nl
            });
        }

        var root = new Dictionary<string, object>
        {
            ["information"] = new Dictionary<string, object>
            {
                ["codeFormulaire"] = isQc ? "T5RL3" : "T5",
                ["codeAnnee"] = cfg.AnneeProduction,
                ["codeSuppression"] = "N",
                ["typImpression"] = typImpression,
                ["holdMailIndicateur"] = holdMail,
                ["numIdentification"] = numTransit,
                ["identificationLegale"] = nom,
                ["parties"] = identification,
            },
            new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 2,
                ["adressePostale"] = new Dictionary<string, object>
                {
                    ["nomRue"] = faker.Address.StreetName().ToUpperInvariant(),
                    ["nomMunicipalite"] = faker.Address.City().ToUpperInvariant(),
                    ["secondaryAddress"] = faker.Address.SecondaryAddress().ToUpperInvariant(),
                    ["codeProvince"] = province,
                    ["codePaysIso"] = pays,
                    ["numCodePostal"] = RandomUtils.GenerateCanadianPostalCode(rng, province).Replace(" ", ""),
                }
            },
            ["indicateurAdresseFiscalePostalIdentique"] = true,
            ["documents"] = documents,
            ["content"] = BuildCases(numTransit, numCompte, case13, caseD, isQc),
        };

        return root;
    }

    private List<object> BuildCases(string transit, string compte, string case13, string caseD, bool isQc)
    {
        var cases = new List<object>
        {
            new Dictionary<string, object> { ["case"] = "13", ["valeur"] = case13 },
            new Dictionary<string, object> { ["case"] = "2B", ["valeur"] = transit },
            new Dictionary<string, object> { ["case"] = "28", ["valeur"] = compte },
        };

        if (isQc)
        {
            cases.Add(new Dictionary<string, object> { ["case"] = "D", ["valeur"] = caseD });
            cases.Add(new Dictionary<string, object> { ["case"] = "Succ", ["valeur"] = transit });
        }

        return cases;
    }

    private void PrintProgress(int current, int total)
    {
        double pct = current / (double)total * 100.0;
        Console.WriteLine($"Progress: {current}/{total} ({pct:F2}%)");
    }
}
